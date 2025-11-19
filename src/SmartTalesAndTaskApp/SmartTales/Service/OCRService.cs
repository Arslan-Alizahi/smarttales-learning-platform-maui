using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tesseract;
using Microsoft.Extensions.Logging;

namespace SmartTales.Service
{
    public class OCRService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OCRService> _logger;
        private readonly string _tessDataPath;

        public OCRService(IServiceProvider serviceProvider, ILogger<OCRService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Set tessdata path for MAUI app
            try
            {
                _tessDataPath = Path.Combine(FileSystem.Current.CacheDirectory, "tessdata");
                // Copy tessdata files to cache directory if they don't exist
                EnsureTessDataExists();
            }
            catch
            {
                // Fallback to current directory tessdata
                _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
            }
        }

        public async Task<string> ExtractTextFromImage(Stream imageStream, string fileName)
        {
            try
            {
                // Save the stream to a temporary file for Tesseract processing
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"ocr_temp_{Guid.NewGuid()}.jpg");

                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    imageStream.Position = 0;
                    await imageStream.CopyToAsync(fileStream);
                }

                // Process with Tesseract
                string extractedText = await Task.Run(() => ProcessImageWithTesseract(tempFilePath));

                // Clean up temp file
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from image: {Message}", ex.Message);
                return $"Error: {ex.Message}";
            }
        }

        public async Task<byte[]> ConvertImageToSpeech(Stream imageStream, string fileName)
        {
            try
            {
                // First extract text from image
                var extractedText = await ExtractTextFromImage(imageStream, fileName);

                if (extractedText.StartsWith("Error:"))
                {
                    throw new InvalidOperationException(extractedText);
                }

                // Then convert text to speech using local TTS service
                var ttsService = _serviceProvider.GetRequiredService<TTSService>();
                return await ttsService.SynthesizeText(extractedText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting image to speech: {Message}", ex.Message);
                throw;
            }
        }

        private string ProcessImageWithTesseract(string imagePath)
        {
            try
            {
                _logger.LogInformation($"Attempting OCR with tessdata path: {_tessDataPath}");
                _logger.LogInformation($"Processing image: {imagePath}");
                
                // Check if tessdata directory exists
                if (!Directory.Exists(_tessDataPath))
                {
                    _logger.LogError($"Tessdata directory does not exist: {_tessDataPath}");
                    return $"Error: Tessdata directory not found at {_tessDataPath}";
                }
                
                // Check if eng.traineddata exists
                var engDataPath = Path.Combine(_tessDataPath, "eng.traineddata");
                if (!File.Exists(engDataPath))
                {
                    _logger.LogError($"English training data not found: {engDataPath}");
                    return $"Error: English training data not found at {engDataPath}";
                }
                
                _logger.LogInformation($"Tessdata files verified. File size: {new FileInfo(engDataPath).Length} bytes");
                _logger.LogInformation($"Initializing Tesseract engine...");
                
                // Try different engine modes if default fails
                var engineModes = new[] { EngineMode.Default, EngineMode.TesseractOnly, EngineMode.LstmOnly };
                
                foreach (var mode in engineModes)
                {
                    try
                    {
                        _logger.LogInformation($"Trying engine mode: {mode}");
                        using var engine = new TesseractEngine(_tessDataPath, "eng", mode);
                         _logger.LogInformation($"Tesseract engine initialized successfully with mode: {mode}");
                         
                         using var img = Pix.LoadFromFile(imagePath);
                         _logger.LogInformation("Image loaded successfully");
                         
                         using var page = engine.Process(img);
                         var text = page.GetText();
                        
                        _logger.LogInformation($"OCR completed. Extracted {text.Length} characters");
                        return text;
                    }
                    catch (Exception engineEx)
                    {
                        _logger.LogWarning($"Engine mode {mode} failed: {engineEx.Message}");
                        if (mode == engineModes.Last())
                        {
                            throw; // Re-throw if this was the last attempt
                        }
                    }
                }
                
                return "Error: All engine modes failed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tesseract processing failed: {Message}", ex.Message);
                _logger.LogError($"Exception type: {ex.GetType().Name}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                // Provide more specific error messages
                if (ex.Message.Contains("Specified method is not supported"))
                {
                    return "Error: Tesseract library compatibility issue. This may be due to missing native dependencies or platform incompatibility.";
                }
                
                return $"OCR processing failed: {ex.Message}";
            }
        }

        private void EnsureTessDataExists()
        {
            try
            {
                _logger.LogInformation($"Ensuring tessdata exists at: {_tessDataPath}");
                
                if (!Directory.Exists(_tessDataPath))
                {
                    _logger.LogInformation($"Creating tessdata directory: {_tessDataPath}");
                    Directory.CreateDirectory(_tessDataPath);
                }

                var engDataPath = Path.Combine(_tessDataPath, "eng.traineddata");
                if (!File.Exists(engDataPath))
                {
                    _logger.LogInformation("English training data not found, attempting to copy from app package");
                    
                    // Try to copy from app package
                    var sourceStream = FileSystem.Current.OpenAppPackageFileAsync("tessdata/eng.traineddata").Result;
                    if (sourceStream == null)
                    {
                        _logger.LogError("Could not open tessdata/eng.traineddata from app package");
                        throw new FileNotFoundException("tessdata/eng.traineddata not found in app package");
                    }
                    
                    using var destinationStream = File.Create(engDataPath);
                    sourceStream.CopyTo(destinationStream);
                    
                    _logger.LogInformation($"Successfully copied tessdata files to cache directory. File size: {new FileInfo(engDataPath).Length} bytes");
                }
                else
                {
                    _logger.LogInformation($"English training data already exists. File size: {new FileInfo(engDataPath).Length} bytes");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not copy tessdata files: {Message}", ex.Message);
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to indicate initialization failure
            }
        }
    }
}
