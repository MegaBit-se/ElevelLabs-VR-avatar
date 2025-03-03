using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using ElevelLabs.VRAvatar.Core;

namespace ElevelLabs.VRAvatar.API
{
    /// <summary>
    /// Client for handling API communication with the ElevenLabs API.
    /// Provides robust error handling, authentication, and methods for accessing
    /// the Conversational AI, speech recognition, and text-to-speech endpoints.
    /// Integrates with ErrorManager for rate limiting and retry logic.
    /// </summary>
    public class ElevenLabsAPIClient
    {
        // API Base URLs and endpoints
        private const string API_BASE_URL = "https://api.elevenlabs.io";
        private const string TTS_ENDPOINT = "/v1/text-to-speech";
        private const string SPEECH_RECOGNITION_ENDPOINT = "/v1/speech-recognition";
        private const string CONVERSATION_ENDPOINT = "/v1/conversation";
        private const string VOICES_ENDPOINT = "/v1/voices";

        // Authentication
        private string apiKey;
        
        // Configuration
        private string defaultVoiceId;
        private string defaultModelId = "eleven_monolingual_v1";
        
        // Voice settings
        private float stability = 0.5f;
        private float similarity = 0.75f;
        private float speechRate = 1.0f;
        
        // Reference to the ErrorManager
        private ErrorManager errorManager;

        // Events
        public event Action<string> OnApiError;
        public event Action<string> OnRequestStarted;
        public event Action<string> OnRequestCompleted;

        /// <summary>
        /// Initializes the API client with settings from the config manager.
        /// </summary>
        public ElevenLabsAPIClient()
        {
            // Find the ErrorManager if it exists
            errorManager = GameObject.FindObjectOfType<ErrorManager>();
            if (errorManager == null)
            {
                Debug.LogWarning("ErrorManager not found. Error handling and retry logic will be limited.");
            }
            
            LoadConfiguration();
        }

        /// <summary>
        /// Initializes the API client with the provided API key and voice ID.
        /// </summary>
        /// <param name="apiKey">ElevenLabs API key</param>
        /// <param name="voiceId">Default voice ID to use for TTS</param>
        public ElevenLabsAPIClient(string apiKey, string voiceId)
        {
            this.apiKey = apiKey;
            this.defaultVoiceId = voiceId;
            
            // Find the ErrorManager if it exists
            errorManager = GameObject.FindObjectOfType<ErrorManager>();
            if (errorManager == null)
            {
                Debug.LogWarning("ErrorManager not found. Error handling and retry logic will be limited.");
            }
        }

        /// <summary>
        /// Loads configuration settings from the ConfigManager.
        /// </summary>
        private void LoadConfiguration()
        {
            if (ConfigManager.Instance != null)
            {
                AppSettings settings = ConfigManager.Instance.Settings;
                apiKey = settings.ElevenLabsApiKey;
                defaultVoiceId = settings.VoiceId;
                stability = settings.Stability;
                similarity = settings.Similarity;
                speechRate = settings.SpeechRate;
            }
            else
            {
                Debug.LogWarning("ConfigManager not found. Using default API settings.");
                apiKey = "";
                defaultVoiceId = "default";
            }
        }

        /// <summary>
        /// Validates the API key exists and is not empty.
        /// </summary>
        private bool ValidateApiKey()
        {
            bool isValid = !string.IsNullOrEmpty(apiKey);
            if (!isValid)
            {
                string errorMessage = "ElevenLabs API key is not set. Please set it in the settings.";
                Debug.LogError(errorMessage);
                OnApiError?.Invoke(errorMessage);
            }
            return isValid;
        }

        #region Speech Recognition API

        /// <summary>
        /// Converts speech audio to text using ElevenLabs Speech Recognition API.
        /// </summary>
        /// <param name="audioData">Raw audio data in WAV format</param>
        /// <param name="onSuccess">Callback for successful response</param>
        /// <param name="onError">Callback for error</param>
        /// <returns>Coroutine for async execution</returns>
        public IEnumerator RecognizeSpeech(byte[] audioData, Action<string> onSuccess, Action<string> onError)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            if (audioData == null || audioData.Length == 0)
            {
                onError?.Invoke("Audio data is empty");
                yield break;
            }

            string url = API_BASE_URL + SPEECH_RECOGNITION_ENDPOINT;
            OnRequestStarted?.Invoke("Speech Recognition");

            yield return ExecuteWithRetry(
                () => CreateSpeechRecognitionRequest(url, audioData),
                www =>
                {
                    try
                    {
                        APIModels.SpeechRecognitionResponse response = 
                            JsonConvert.DeserializeObject<APIModels.SpeechRecognitionResponse>(www.downloadHandler.text);
                        
                        OnRequestCompleted?.Invoke("Speech Recognition");
                        onSuccess?.Invoke(response.Text);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error parsing speech recognition response: {ex.Message}";
                        Debug.LogError(errorMessage);
                        onError?.Invoke(errorMessage);
                    }
                },
                error =>
                {
                    string errorMessage = $"Speech recognition request failed: {error}";
                    Debug.LogError(errorMessage);
                    onError?.Invoke(errorMessage);
                }
            );
        }

        /// <summary>
        /// Creates a web request for speech recognition.
        /// </summary>
        private UnityWebRequest CreateSpeechRecognitionRequest(string url, byte[] audioData)
        {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            
            // Set headers
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "audio/wav");
            
            // Set up the request body with audio data
            www.uploadHandler = new UploadHandlerRaw(audioData);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            return www;
        }

        #endregion

        #region Conversational AI API

        /// <summary>
        /// Generates a conversational response based on input text and conversation history.
        /// </summary>
        /// <param name="messages">List of conversation messages</param>
        /// <param name="modelId">Optional model ID to use</param>
        /// <param name="temperature">Model temperature (0.0-1.0)</param>
        /// <param name="onSuccess">Callback for successful response</param>
        /// <param name="onError">Callback for error</param>
        /// <returns>Coroutine for async execution</returns>
        public IEnumerator GenerateConversation(
            List<APIModels.ConversationMessage> messages, 
            string modelId = null, 
            float temperature = 0.7f,
            Action<APIModels.ConversationResponse> onSuccess = null, 
            Action<string> onError = null)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            if (messages == null || messages.Count == 0)
            {
                onError?.Invoke("Conversation messages are empty");
                yield break;
            }

            string url = API_BASE_URL + CONVERSATION_ENDPOINT;
            OnRequestStarted?.Invoke("Conversation Generation");

            // Create request payload
            APIModels.ConversationRequest request = new APIModels.ConversationRequest
            {
                Messages = messages,
                Model = string.IsNullOrEmpty(modelId) ? "default" : modelId,
                Temperature = temperature,
                MaxTokens = 150 // Default value, could be made configurable
            };

            string jsonPayload = JsonConvert.SerializeObject(request);

            yield return ExecuteWithRetry(
                () => CreateJsonRequest(url, jsonPayload),
                www =>
                {
                    try
                    {
                        APIModels.ConversationResponse response = 
                            JsonConvert.DeserializeObject<APIModels.ConversationResponse>(www.downloadHandler.text);
                        
                        OnRequestCompleted?.Invoke("Conversation Generation");
                        onSuccess?.Invoke(response);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error parsing conversation response: {ex.Message}";
                        Debug.LogError(errorMessage);
                        onError?.Invoke(errorMessage);
                    }
                },
                error =>
                {
                    string errorMessage = $"Conversation request failed: {error}";
                    Debug.LogError(errorMessage);
                    onError?.Invoke(errorMessage);
                }
            );
        }

        /// <summary>
        /// Simplified version of GenerateConversation that takes a single text input.
        /// </summary>
        /// <param name="inputText">User input text</param>
        /// <param name="conversationHistory">Previous conversation history</param>
        /// <param name="onSuccess">Callback with the response text</param>
        /// <param name="onError">Callback for error</param>
        /// <returns>Coroutine for async execution</returns>
        public IEnumerator GenerateResponse(
            string inputText, 
            List<APIModels.ConversationMessage> conversationHistory,
            Action<string> onSuccess, 
            Action<string> onError)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                onError?.Invoke("Input text is empty");
                yield break;
            }

            // Add user message to history
            List<APIModels.ConversationMessage> messages = new List<APIModels.ConversationMessage>(conversationHistory);
            messages.Add(new APIModels.ConversationMessage
            {
                Role = "user",
                Content = inputText
            });

            yield return GenerateConversation(
                messages,
                null,
                0.7f,
                response =>
                {
                    string responseText = response.Message.Content;
                    onSuccess?.Invoke(responseText);
                },
                error =>
                {
                    onError?.Invoke(error);
                }
            );
        }

        #endregion

        #region Text-to-Speech API

        /// <summary>
        /// Converts text to speech using ElevenLabs Text-to-Speech API.
        /// </summary>
        /// <param name="text">Text to convert to speech</param>
        /// <param name="voiceId">Voice ID to use (optional, uses default if not specified)</param>
        /// <param name="voiceSettings">Custom voice settings (optional, uses default if not specified)</param>
        /// <param name="modelId">Model ID to use (optional, uses default if not specified)</param>
        /// <param name="onSuccess">Callback for successful response with audio data</param>
        /// <param name="onError">Callback for error</param>
        /// <returns>Coroutine for async execution</returns>
        public IEnumerator GenerateSpeech(
            string text, 
            string voiceId = null,
            APIModels.VoiceSettings voiceSettings = null,
            string modelId = null,
            Action<byte[]> onSuccess = null, 
            Action<string> onError = null)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            if (string.IsNullOrEmpty(text))
            {
                onError?.Invoke("Text is empty");
                yield break;
            }

            // Use default voice ID if not specified
            string useVoiceId = voiceId ?? defaultVoiceId;
            if (string.IsNullOrEmpty(useVoiceId))
            {
                onError?.Invoke("Voice ID is not set");
                yield break;
            }

            string url = $"{API_BASE_URL}{TTS_ENDPOINT}/{useVoiceId}";
            OnRequestStarted?.Invoke("Text-to-Speech");

            // Create request payload
            APIModels.TextToSpeechRequest request = new APIModels.TextToSpeechRequest
            {
                Text = text,
                ModelId = modelId ?? defaultModelId,
                VoiceSettings = voiceSettings ?? new APIModels.VoiceSettings
                {
                    Stability = stability,
                    SimilarityBoost = similarity
                }
            };

            string jsonPayload = JsonConvert.SerializeObject(request);

            yield return ExecuteWithRetry(
                () => CreateTTSRequest(url, jsonPayload),
                www =>
                {
                    try
                    {
                        byte[] audioData = www.downloadHandler.data;
                        OnRequestCompleted?.Invoke("Text-to-Speech");
                        onSuccess?.Invoke(audioData);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error processing text-to-speech response: {ex.Message}";
                        Debug.LogError(errorMessage);
                        onError?.Invoke(errorMessage);
                    }
                },
                error =>
                {
                    string errorMessage = $"Text-to-speech request failed: {error}";
                    Debug.LogError(errorMessage);
                    onError?.Invoke(errorMessage);
                }
            );
        }

        /// <summary>
        /// Creates a web request for text-to-speech.
        /// </summary>
        private UnityWebRequest CreateTTSRequest(string url, string jsonPayload)
        {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            
            // Set headers
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");
            
            // Set up the request body
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            return www;
        }

        /// <summary>
        /// Gets available voices from the ElevenLabs API.
        /// </summary>
        /// <param name="onSuccess">Callback for successful response</param>
        /// <param name="onError">Callback for error</param>
        /// <returns>Coroutine for async execution</returns>
        public IEnumerator GetVoices(Action<List<APIModels.Voice>> onSuccess, Action<string> onError)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            string url = API_BASE_URL + VOICES_ENDPOINT;
            OnRequestStarted?.Invoke("Get Voices");

            yield return ExecuteWithRetry(
                () => CreateGetRequest(url),
                www =>
                {
                    try
                    {
                        APIModels.VoicesResponse response = 
                            JsonConvert.DeserializeObject<APIModels.VoicesResponse>(www.downloadHandler.text);
                        
                        OnRequestCompleted?.Invoke("Get Voices");
                        onSuccess?.Invoke(response.Voices);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error parsing voices response: {ex.Message}";
                        Debug.LogError(errorMessage);
                        onError?.Invoke(errorMessage);
                    }
                },
                error =>
                {
                    string errorMessage = $"Get voices request failed: {error}";
                    Debug.LogError(errorMessage);
                    onError?.Invoke(errorMessage);
                }
            );
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Creates a GET request with authentication.
        /// </summary>
        private UnityWebRequest CreateGetRequest(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("xi-api-key", apiKey);
            return www;
        }

        /// <summary>
        /// Creates a JSON POST request with authentication.
        /// </summary>
        private UnityWebRequest CreateJsonRequest(string url, string jsonPayload)
        {
            UnityWebRequest www = new UnityWebRequest(url, "POST");
            
            // Set headers
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            
            // Set up the request body
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            return www;
        }

        /// <summary>
        /// Executes a web request with retry logic and error handling.
        /// Uses ErrorManager for rate limiting and exponential backoff.
        /// </summary>
        private IEnumerator ExecuteWithRetry(
            Func<UnityWebRequest> createRequest,
            Action<UnityWebRequest> onSuccess,
            Action<string> onError)
        {
            int retryCount = 0;
            bool success = false;
            int maxRetries = errorManager != null ? errorManager.maxRetryAttempts : 3;

            // Check if we're currently rate limited before starting
            if (errorManager != null && errorManager.isRateLimited)
            {
                string errorMessage = "Rate limit in effect. Please wait before making more requests.";
                Debug.LogWarning(errorMessage);
                onError(errorMessage);
                yield break;
            }

            while (!success && retryCount <= maxRetries)
            {
                if (retryCount > 0)
                {
                    float delay = 0;
                    if (errorManager != null)
                    {
                        delay = errorManager.GetExponentialBackoffDelay(retryCount - 1);
                        Debug.Log($"Retrying request (attempt {retryCount}/{maxRetries}) after {delay:F2}s...");
                    }
                    else
                    {
                        delay = Mathf.Pow(2, retryCount - 1) * 0.5f; // Simple exponential backoff
                        Debug.Log($"Retrying request (attempt {retryCount}/{maxRetries}) after {delay:F2}s...");
                    }
                    
                    yield return new WaitForSeconds(delay);
                }

                using (UnityWebRequest www = createRequest())
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        // Check for token usage in response for Conversational AI
                        if (www.url.Contains("/conversation") && !string.IsNullOrEmpty(www.downloadHandler.text))
                        {
                            try
                            {
                                APIModels.ConversationResponse response =
                                    JsonConvert.DeserializeObject<APIModels.ConversationResponse>(www.downloadHandler.text);
                                
                                // Track token usage if ErrorManager exists
                                if (errorManager != null && response?.Usage != null)
                                {
                                    errorManager.TrackTokenUsage(response.Usage.TotalTokens);
                                }
                            }
                            catch {}
                        }

                        onSuccess(www);
                        success = true;
                    }
                    else
                    {
                        string errorDetails = GetErrorDetails(www);
                        string errorMessage = errorDetails ?? www.error;
                        Debug.LogWarning($"Request failed: {errorMessage}. Status code: {www.responseCode}");
                        
                        // Check for rate limit error
                        bool isRateLimitError = www.responseCode == 429;
                        if (errorManager != null)
                        {
                            isRateLimitError = isRateLimitError || errorManager.IsRateLimitError(errorMessage);
                            
                            // Handle rate limit
                            if (isRateLimitError)
                            {
                                Debug.LogWarning("Rate limit detected. Starting cooldown period.");
                                errorManager.StartCoroutine(errorManager.StartRateLimitCooldown());
                                
                                // Get user-friendly error message
                                string friendlyMessage = errorManager.GetUserFriendlyErrorMessage(errorMessage);
                                onError(friendlyMessage);
                                yield break;
                            }
                        }
                        
                        // Check if the error is retryable
                        bool isRetryable = IsRetryableError(www);
                        if (errorManager != null)
                        {
                            isRetryable = isRetryable || errorManager.IsRetryableError(errorMessage);
                        }
                        
                        if (isRetryable && retryCount < maxRetries)
                        {
                            retryCount++;
                        }
                        else
                        {
                            // Get user-friendly error message if possible
                            if (errorManager != null)
                            {
                                string friendlyMessage = errorManager.GetUserFriendlyErrorMessage(errorMessage);
                                onError(friendlyMessage);
                            }
                            else
                            {
                                onError(errorMessage);
                            }
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if an error is retryable (e.g., network issues, server errors).
        /// </summary>
        private bool IsRetryableError(UnityWebRequest www)
        {
            // 5xx status codes indicate server errors that may be temporary
            bool isServerError = www.responseCode >= 500 && www.responseCode < 600;
            
            // Certain 4xx errors might also be retryable
            bool isRetryable4xx = www.responseCode == 429; // Too Many Requests
            
            // Network-level errors
            bool isNetworkError = www.result == UnityWebRequest.Result.ConnectionError;
            
            return isServerError || isRetryable4xx || isNetworkError;
        }

        /// <summary>
        /// Extracts detailed error information from the response if available.
        /// </summary>
        private string GetErrorDetails(UnityWebRequest www)
        {
            if (string.IsNullOrEmpty(www.downloadHandler?.text))
            {
                return null;
            }

            try
            {
                APIModels.ErrorResponse errorResponse = 
                    JsonConvert.DeserializeObject<APIModels.ErrorResponse>(www.downloadHandler.text);
                
                if (errorResponse?.Detail != null)
                {
                    return $"{errorResponse.Detail.Status}: {errorResponse.Detail.Message}";
                }
            }
            catch
            {
                // If we can't parse the error response, just return the raw text
                return $"Error ({www.responseCode}): {www.downloadHandler.text}";
            }

            return null;
        }

        /// <summary>
        /// Updates the API key.
        /// </summary>
        public void UpdateApiKey(string newApiKey)
        {
            apiKey = newApiKey;
        }

        /// <summary>
        /// Updates voice settings.
        /// </summary>
        public void UpdateVoiceSettings(string newVoiceId, float newStability, float newSimilarity, float newSpeechRate)
        {
            defaultVoiceId = newVoiceId;
            stability = newStability;
            similarity = newSimilarity;
            speechRate = newSpeechRate;
        }

        /// <summary>
        /// Updates the default model ID.
        /// </summary>
        public void UpdateDefaultModel(string newModelId)
        {
            defaultModelId = newModelId;
        }

        #endregion
    }
}