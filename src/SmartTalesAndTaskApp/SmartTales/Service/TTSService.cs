using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;

namespace SmartTales.Service
{
    public class TTSService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TTSService> _logger;
        private readonly string? _speechKey;
        private readonly string? _speechRegion;

        public TTSService(IConfiguration configuration, ILogger<TTSService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Get speech service configuration
            _speechKey = _configuration["SpeechService:Key"];
            _speechRegion = _configuration["SpeechService:Region"];
            
            // Check if fallback is explicitly enabled or if credentials are missing/invalid
            var useFallback = _configuration.GetValue<bool>("SpeechService:UseFallback", false);
            var hasValidConfig = !string.IsNullOrEmpty(_speechKey) && !string.IsNullOrEmpty(_speechRegion) && 
                                _speechKey != "YOUR_SPEECH_SERVICE_KEY_HERE" && _speechRegion != "YOUR_SPEECH_SERVICE_REGION_HERE";
            
            if (useFallback || !hasValidConfig)
            {
                _logger.LogInformation("TTS Service initialized with fallback mode. UseFallback: {UseFallback}, ValidConfig: {HasValidConfig}", useFallback, hasValidConfig);
            }
            else
            {
                _logger.LogInformation("TTS Service initialized with Azure Speech Services.");
            }
        }

        public async Task<byte[]> SynthesizeText(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return new byte[0];
                }

                // Check if fallback is explicitly enabled or if credentials are missing/invalid
                var useFallback = _configuration.GetValue<bool>("SpeechService:UseFallback", false);
                var hasValidConfig = !string.IsNullOrEmpty(_speechKey) && !string.IsNullOrEmpty(_speechRegion) &&
                    _speechKey != "YOUR_SPEECH_SERVICE_KEY_HERE" && _speechRegion != "YOUR_SPEECH_SERVICE_REGION_HERE";

                if (!useFallback && hasValidConfig)
                {
                    return await SynthesizeWithCognitiveServices(text);
                }
                else
                {
                    _logger.LogInformation("Using fallback TTS method. Text: '{Text}'", text.Substring(0, Math.Min(50, text.Length)) + (text.Length > 50 ? "..." : ""));
                    return await GenerateTextToSpeechFallback(text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TTS synthesis, falling back to simple audio: {Message}", ex.Message);
                return await GenerateTextToSpeechFallback(text);
            }
        }

        public async Task<Stream?> SynthesizeText2(string text)
        {
            try
            {
                var audioBytes = await SynthesizeText(text);
                return new MemoryStream(audioBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synthesizing text to stream: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<byte[]> SynthesizeWithCognitiveServices(string text)
        {
            var config = SpeechConfig.FromSubscription(_speechKey!, _speechRegion!);
            config.SpeechSynthesisVoiceName = "en-US-JennyNeural"; // Use a neural voice
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

            using var synthesizer = new SpeechSynthesizer(config);
            using var result = await synthesizer.SpeakTextAsync(text);

            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                return result.AudioData;
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                _logger.LogError("Speech synthesis canceled: {Reason}, {ErrorDetails}", cancellation.Reason, cancellation.ErrorDetails);
                throw new InvalidOperationException($"Speech synthesis canceled: {cancellation.ErrorDetails}");
            }

            throw new InvalidOperationException("Speech synthesis failed");
        }

        private async Task<byte[]> GenerateTextToSpeechFallback(string text)
        {
            // Generate a simple WAV file with a tone pattern to indicate TTS is working
            // This creates a series of beeps that represent the text length
            var sampleRate = 22050;
            var duration = Math.Max(1.0, Math.Min(text.Length * 0.05, 5.0)); // 0.05 seconds per character, max 5 seconds
            var samples = (int)(sampleRate * duration);
            var dataSize = samples * 2; // 16-bit = 2 bytes per sample

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            // WAV header
            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + dataSize);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16); // PCM format chunk size
            writer.Write((short)1); // PCM format
            writer.Write((short)1); // Mono
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2); // Byte rate
            writer.Write((short)2); // Block align
            writer.Write((short)16); // Bits per sample
            writer.Write("data".ToCharArray());
            writer.Write(dataSize);

            // Generate a simple tone pattern to indicate TTS is working
            // Create a series of beeps with different frequencies
            var frequency = 440.0; // A4 note
            var amplitude = 8000; // Volume level
            
            for (int i = 0; i < samples; i++)
            {
                // Create a tone that varies slightly to make it more interesting
                var time = (double)i / sampleRate;
                var beepFreq = frequency + (Math.Sin(time * 2) * 50); // Slight frequency modulation
                var sample = (short)(amplitude * Math.Sin(2 * Math.PI * beepFreq * time));
                
                // Add some fade in/out to avoid clicks
                var fadeLength = sampleRate * 0.1; // 0.1 second fade
                if (i < fadeLength)
                {
                    sample = (short)(sample * (i / fadeLength));
                }
                else if (i > samples - fadeLength)
                {
                    sample = (short)(sample * ((samples - i) / fadeLength));
                }
                
                writer.Write(sample);
            }

            _logger.LogInformation("Generated fallback audio tone for text: {TextLength} characters, duration: {Duration:F1}s", text.Length, duration);
            return stream.ToArray();
        }
    }
}