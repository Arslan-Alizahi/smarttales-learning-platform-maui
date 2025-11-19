using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace SmartTales.Service
{
    public class StoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IConfiguration _configuration;
        private readonly Random _random = new Random();
        private string _lastStory = string.Empty;
        private string _currentStoryContext = string.Empty;

        // Azure OpenAI Configuration
        private string _endpoint;
        private string _apiKey;
        private string _deploymentName;
        private string _apiVersion;
        private int _maxTokens;
        private double _temperature;
        private bool _enableLogging;

        public StoryService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Load Azure OpenAI configuration
            _endpoint = _configuration["AzureOpenAI:Endpoint"] ?? "";
            _apiKey = _configuration["AzureOpenAI:ApiKey"] ?? "";
            _deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";
            _apiVersion = _configuration["AzureOpenAI:ApiVersion"] ?? "2024-12-01-preview";
            _maxTokens = int.Parse(_configuration["AzureOpenAI:MaxTokens"] ?? "1000");
            _temperature = double.Parse(_configuration["AzureOpenAI:Temperature"] ?? "0.7");
            _enableLogging = bool.Parse(_configuration["AzureOpenAI:EnableLogging"] ?? "true");

            // Debug logging to check configuration
            Console.WriteLine($"DEBUG: Azure OpenAI Configuration:");
            Console.WriteLine($"  Endpoint: '{_endpoint}'");
            Console.WriteLine($"  ApiKey: '{(_apiKey.Length > 0 ? _apiKey.Substring(0, Math.Min(10, _apiKey.Length)) + "..." : "EMPTY")}'");
            Console.WriteLine($"  DeploymentName: '{_deploymentName}'");
            Console.WriteLine($"  ApiVersion: '{_apiVersion}'");

            // Configure HTTP client for Azure OpenAI
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            }
        }

        public async Task<(string story, List<string> questions)> GenerateStoryAsync(string prompt)
        {
            return await GenerateWithContextAsync(prompt, false);
        }

        public async Task<(string story, List<string> questions)> AnswerQuestionAboutStoryAsync(string question)
        {
            Console.WriteLine($"Answering question: '{question}' about story context: '{_currentStoryContext.Substring(0, Math.Min(50, _currentStoryContext.Length))}...'");

            // Make sure we have a story context to work with
            if (string.IsNullOrEmpty(_currentStoryContext))
            {
                Console.WriteLine("No story context found, generating new story instead");
                return await GenerateStoryAsync(question);
            }

            return await GenerateWithContextAsync(question, true);
        }

        private async Task<(string story, List<string> questions)> GenerateWithContextAsync(string input, bool isFollowupQuestion)
        {
            try
            {
                // Check if this is a story-related request
                if (!IsStoryRelatedRequest(input))
                {
                    return (GetOutOfScopeResponse(), GetClarificationQuestions());
                }

                // Ensure API configuration is available - use hardcoded values for now
                if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_endpoint))
                {
                    Console.WriteLine("ERROR: Azure OpenAI configuration missing from appsettings.json!");
                    Console.WriteLine($"  _apiKey is null/empty: {string.IsNullOrEmpty(_apiKey)}");
                    Console.WriteLine($"  _endpoint is null/empty: {string.IsNullOrEmpty(_endpoint)}");

                    // Use hardcoded configuration as fallback
                    Console.WriteLine("Using hardcoded configuration...");
                    _endpoint = "testapi";
                    _apiKey = "test key";
                    _deploymentName = "gpt-4o";
                    _apiVersion = "2024-12-01-preview";
                    _maxTokens = 1000;
                    _temperature = 0.7;
                    _enableLogging = true;

                    // Update HTTP client headers
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
                }

                // Create the Azure OpenAI API request
                var messages = new List<AzureMessage>();

                // System message to define the AI's role as a story teller
                string systemPrompt = isFollowupQuestion
                    ? @"You are a creative storyteller AI assistant for children. Your role is ONLY to:
1. Answer questions about previously told stories
2. Create new stories based on user prompts
3. Help with story-related discussions

If users ask about anything unrelated to stories (like math, science, general questions), politely redirect them back to storytelling.

When answering story questions, format your response as:
ANSWER: (your detailed answer about the story)
QUESTIONS: (3-4 numbered follow-up questions about the story)

Stay focused on storytelling and creative writing only."
                    : @"You are a creative storyteller AI assistant for children. Your role is ONLY to:
1. Create engaging, age-appropriate stories based on user prompts
2. Generate follow-up questions about the stories you create
3. Help with story-related discussions

If users ask about anything unrelated to stories (like math, science, general questions), politely redirect them back to storytelling.

If the user's request is unclear or too vague, ask them to be more specific about what kind of story they want.

Format your response as:
STORY: (create an engaging, detailed story based on their prompt)
QUESTIONS: (3-4 numbered questions about the story to encourage discussion)

Keep stories appropriate for children and focus on positive themes, adventure, friendship, and learning.";

                messages.Add(new AzureMessage
                {
                    Role = "system",
                    Content = systemPrompt
                });

                // If this is a follow-up question and we have context, add it
                if (isFollowupQuestion && !string.IsNullOrEmpty(_currentStoryContext))
                {
                    messages.Add(new AzureMessage
                    {
                        Role = "assistant",
                        Content = $"STORY: {_currentStoryContext}"
                    });
                }

                // Add the user's input
                messages.Add(new AzureMessage
                {
                    Role = "user",
                    Content = isFollowupQuestion
                        ? $"Question about the story: {input}"
                        : $"Create a story about: {input}"
                });

                var request = new AzureOpenAIRequest
                {
                    Messages = messages,
                    Temperature = _temperature,
                    MaxTokens = _maxTokens
                };

                // Build the Azure OpenAI endpoint URL
                var apiUrl = $"{_endpoint.TrimEnd('/')}/openai/deployments/{_deploymentName}/chat/completions?api-version={_apiVersion}";

                if (_enableLogging)
                {
                    Console.WriteLine($"Calling Azure OpenAI: {apiUrl}");
                    Console.WriteLine($"Request: {JsonSerializer.Serialize(request, _jsonOptions)}");
                }

                // Call the Azure OpenAI API
                var response = await _httpClient.PostAsJsonAsync(apiUrl, request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Azure OpenAI API Error: {response.StatusCode} - {errorContent}");
                    throw new Exception($"API call failed: {response.StatusCode}");
                }

                var azureResponse = await response.Content.ReadFromJsonAsync<AzureOpenAIResponse>(_jsonOptions);
                var content = azureResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;

                if (_enableLogging)
                {
                    Console.WriteLine($"Azure OpenAI Response: {content}");
                }

                // Process the response to extract story/answer and questions
                return isFollowupQuestion
                    ? ParseAnswerAndQuestions(content)
                    : ParseStoryAndQuestions(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Azure OpenAI API: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return ($"I apologize, but I encountered an error while generating your story: {ex.Message}. Please try again.", new List<string>());
            }
        }

        private bool IsStoryRelatedRequest(string input)
        {
            var inputLower = input.ToLower().Trim();

            // Check for obvious non-story requests
            var nonStoryKeywords = new[]
            {
                "math", "calculate", "solve", "equation", "number", "add", "subtract", "multiply", "divide",
                "science", "physics", "chemistry", "biology", "formula", "experiment",
                "history", "when did", "who was", "what year", "date",
                "geography", "capital", "country", "continent", "ocean",
                "weather", "temperature", "forecast",
                "news", "current events", "politics", "government",
                "technology", "computer", "programming", "code", "software",
                "medical", "health", "doctor", "medicine", "symptom",
                "recipe", "cooking", "ingredients", "how to cook",
                "homework", "assignment", "test", "exam", "grade"
            };

            // If it contains non-story keywords, it's likely not story-related
            if (nonStoryKeywords.Any(keyword => inputLower.Contains(keyword)))
            {
                return false;
            }

            // Check for story-related keywords
            var storyKeywords = new[]
            {
                "story", "tale", "adventure", "character", "hero", "princess", "dragon", "magic",
                "once upon", "fairy", "wizard", "knight", "castle", "forest", "journey",
                "tell me about", "create", "imagine", "fantasy", "fiction", "narrative",
                "plot", "ending", "beginning", "chapter", "book", "novel"
            };

            // If it contains story keywords, it's likely story-related
            if (storyKeywords.Any(keyword => inputLower.Contains(keyword)))
            {
                return true;
            }

            // Check if it's too vague or unclear
            if (inputLower.Length < 5 || inputLower.Split(' ').Length < 2)
            {
                return false; // Too short/vague
            }

            // If none of the above, assume it could be story-related
            return true;
        }

        private string GetOutOfScopeResponse()
        {
            var responses = new[]
            {
                "I'm sorry, but that's outside my area of expertise! I'm your friendly storyteller AI, and I specialize in creating amazing stories and adventures. How about we create an exciting story instead? What kind of adventure would you like to hear about?",

                "That's not something I can help with - I'm here to tell stories and spark your imagination! Let's focus on creating wonderful tales together. Would you like a story about brave heroes, magical creatures, or exciting adventures?",

                "I'm afraid that's beyond my storytelling abilities! My specialty is crafting engaging stories and adventures for you. How about we dive into a fantastic story instead? What theme or setting interests you most?",

                "That question is outside my scope as your story companion! I'm here to create amazing stories, answer questions about tales I've told, and help you explore the world of imagination. What kind of story would you like me to create for you?",

                "I can't help with that, but I'd love to tell you a story! I'm your dedicated storytelling AI, here to create adventures, fairy tales, and exciting narratives. What story elements would you like me to include in a new tale?"
            };

            return responses[_random.Next(responses.Length)];
        }

        private List<string> GetClarificationQuestions()
        {
            return new List<string>
            {
                "What kind of story would you like to hear? (Adventure, fantasy, mystery, etc.)",
                "Would you like a story about animals, people, or magical creatures?",
                "Do you prefer stories set in modern times, the past, or a fantasy world?",
                "What's your favorite type of character? (Heroes, princesses, wizards, etc.)"
            };
        }

        // Mock methods removed - using only Azure OpenAI API

        // All mock story generation methods removed - using only Azure OpenAI API

        private (string story, List<string> questions) ParseStoryAndQuestions(string content)
        {
            var story = "";
            var questions = new List<string>();

            // Split by "STORY:" and "QUESTIONS:" markers
            if (content.Contains("STORY:") && content.Contains("QUESTIONS:"))
            {
                var storyStart = content.IndexOf("STORY:");
                var questionsStart = content.IndexOf("QUESTIONS:");

                if (storyStart >= 0 && questionsStart > storyStart)
                {
                    story = content.Substring(storyStart + 6, questionsStart - storyStart - 6).Trim();
                    var questionsPart = content.Substring(questionsStart + 10).Trim();

                    // Parse questions
                    var lines = questionsPart.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            // Remove numbered prefixes like "1.", "2.", etc.
                            if (trimmedLine.Length > 2 && char.IsDigit(trimmedLine[0]) && trimmedLine[1] == '.')
                            {
                                trimmedLine = trimmedLine.Substring(2).Trim();
                            }
                            questions.Add(trimmedLine);
                        }
                    }
                }
            }
            else
            {
                // If no markers are found, treat everything as story
                story = content.Trim();
            }

            // Generate default questions if none were extracted
            if (questions.Count == 0)
            {
                questions.Add("What did you think about this story?");
                questions.Add("What might happen next in this story?");
                questions.Add("Which character did you find most interesting?");
            }

            // Save the story as context for follow-up questions
            _currentStoryContext = story;

            return (story, questions);
        }

        private (string answer, List<string> questions) ParseAnswerAndQuestions(string content)
        {
            var answer = "";
            var questions = new List<string>();

            // Split by "ANSWER:" and "QUESTIONS:" markers
            if (content.Contains("ANSWER:") && content.Contains("QUESTIONS:"))
            {
                var answerStart = content.IndexOf("ANSWER:");
                var questionsStart = content.IndexOf("QUESTIONS:");

                if (answerStart >= 0 && questionsStart > answerStart)
                {
                    answer = content.Substring(answerStart + 7, questionsStart - answerStart - 7).Trim();
                    var questionsPart = content.Substring(questionsStart + 10).Trim();

                    // Parse questions
                    var lines = questionsPart.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (!string.IsNullOrEmpty(trimmedLine))
                        {
                            // Remove numbered prefixes like "1.", "2.", etc.
                            if (trimmedLine.Length > 2 && char.IsDigit(trimmedLine[0]) && trimmedLine[1] == '.')
                            {
                                trimmedLine = trimmedLine.Substring(2).Trim();
                            }
                            questions.Add(trimmedLine);
                        }
                    }
                }
            }
            else
            {
                // If no markers are found, treat everything as answer
                answer = content.Trim();
            }

            // Generate default questions if none were extracted
            if (questions.Count == 0)
            {
                questions.Add("Do you have any other questions about the story?");
                questions.Add("What else would you like to know about the characters?");
                questions.Add("Is there a different part of the story you'd like me to explain?");
            }

            return (answer, questions);
        }

        // Azure OpenAI API Models
        private class AzureOpenAIRequest
        {
            [JsonPropertyName("messages")]
            public List<AzureMessage> Messages { get; set; } = new List<AzureMessage>();

            [JsonPropertyName("temperature")]
            public double Temperature { get; set; } = 0.7;

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; } = 1000;
        }

        private class AzureMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private class AzureOpenAIResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("object")]
            public string Object { get; set; } = string.Empty;

            [JsonPropertyName("created")]
            public long Created { get; set; }

            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("choices")]
            public List<AzureChoice> Choices { get; set; } = new List<AzureChoice>();

            [JsonPropertyName("usage")]
            public AzureUsage Usage { get; set; } = new AzureUsage();
        }

        private class AzureChoice
        {
            [JsonPropertyName("index")]
            public int Index { get; set; }

            [JsonPropertyName("message")]
            public AzureMessage Message { get; set; } = new AzureMessage();

            [JsonPropertyName("finish_reason")]
            public string FinishReason { get; set; } = string.Empty;
        }

        private class AzureUsage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}
