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
    /// Handles communication with the ElevenLabs Conversational AI API.
    /// Provides methods for speech recognition, conversation, and text-to-speech.
    /// This class uses ElevenLabsAPIClient for actual API communication.
    /// </summary>
    public class ElevenLabsAPI
    {
        // API Base URLs
        private const string API_BASE_URL = "https://api.elevenlabs.io";
        private const string TTS_ENDPOINT = "/v1/text-to-speech";
        private const string SPEECH_RECOGNITION_ENDPOINT = "/v1/speech-recognition";
        private const string CONVERSATION_ENDPOINT = "/v1/conversation"; // Note: This is a placeholder, actual endpoint may differ

        // API Key
        private string apiKey;
        private string voiceId;

        // Voice settings
        private float stability = 0.5f;
        private float similarity = 0.75f;
        private float speechRate = 1.0f;

        // Cached conversation history for context
        private List<APIModels.ConversationMessage> conversationHistory = new List<APIModels.ConversationMessage>();
        private int maxHistoryItems = 10;

        // Internal API client for actual API communication
        private ElevenLabsAPIClient apiClient;

        // Events
        public event Action<string> OnApiError;

        /// <summary>
        /// Initializes the ElevenLabs API client.
        /// </summary>
        public ElevenLabsAPI()
        {
            LoadConfiguration();
            
            // Create the API client
            apiClient = new ElevenLabsAPIClient(apiKey, voiceId);
            
            // Forward API client errors to this class's error handler
            apiClient.OnApiError += (errorMessage) => OnApiError?.Invoke(errorMessage);
        }

        /// <summary>
        /// Loads API configuration from ConfigManager.
        /// </summary>
        private void LoadConfiguration()
        {
            if (ConfigManager.Instance != null)
            {
                AppSettings settings = ConfigManager.Instance.Settings;
                apiKey = settings.ElevenLabsApiKey;
                voiceId = settings.VoiceId;
                stability = settings.Stability;
                similarity = settings.Similarity;
                speechRate = settings.SpeechRate;
                maxHistoryItems = settings.MaxConversationHistory;
            }
            else
            {
                Debug.LogWarning("ConfigManager not found. Using default API settings.");
                apiKey = "";
                voiceId = "default";
            }
        }

        /// <summary>
        /// Validates that the API key is set.
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

        /// <summary>
        /// Converts speech audio to text using ElevenLabs Speech Recognition API.
        /// </summary>
        public IEnumerator RecognizeSpeech(byte[] audioData, Action<string> onSuccess, Action<string> onError)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            // Delegate to API client
            yield return apiClient.RecognizeSpeech(audioData, onSuccess, onError);
        }

        /// <summary>
        /// Generates a conversational response based on the input text and conversation history.
        /// </summary>
        public IEnumerator GenerateConversationalResponse(string inputText, Action<string> onSuccess, Action<string> onError)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            // Add user message to history
            conversationHistory.Add(new APIModels.ConversationMessage
            {
                Role = "user",
                Content = inputText
            });

            // Limit history size
            while (conversationHistory.Count > maxHistoryItems)
            {
                conversationHistory.RemoveAt(0);
            }
            
            // Use API client to generate response
            yield return apiClient.GenerateConversation(
                conversationHistory,
                null, // Use default model
                0.7f, // Default temperature
                (response) => {
                    string responseText = response.Message.Content;
                    
                    // Add assistant response to history
                    conversationHistory.Add(new APIModels.ConversationMessage
                    {
                        Role = "assistant",
                        Content = responseText
                    });
                    
                    // Return the response text
                    onSuccess?.Invoke(responseText);
                },
                onError
            );
        }

        /// <summary>
        /// Converts text to speech using ElevenLabs Text-to-Speech API.
        /// </summary>
        public IEnumerator GenerateSpeech(string text, Action<byte[]> onSuccess, Action<string> onError)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }

            // Create voice settings
            APIModels.VoiceSettings voiceSettings = new APIModels.VoiceSettings
            {
                Stability = stability,
                SimilarityBoost = similarity
            };
            
            // Delegate to API client
            yield return apiClient.GenerateSpeech(
                text,
                voiceId,
                voiceSettings,
                "eleven_monolingual_v1",
                onSuccess,
                onError
            );
        }

        /// <summary>
        /// Clears the conversation history.
        /// </summary>
        public void ClearConversationHistory()
        {
            conversationHistory.Clear();
        }

        /// <summary>
        /// Updates the API key.
        /// </summary>
        public void UpdateApiKey(string newApiKey)
        {
            apiKey = newApiKey;
            apiClient.UpdateApiKey(newApiKey);
        }

        /// <summary>
        /// Updates voice settings.
        /// </summary>
        public void UpdateVoiceSettings(string newVoiceId, float newStability, float newSimilarity, float newSpeechRate)
        {
            voiceId = newVoiceId;
            stability = newStability;
            similarity = newSimilarity;
            speechRate = newSpeechRate;
            
            apiClient.UpdateVoiceSettings(newVoiceId, newStability, newSimilarity, newSpeechRate);
        }
        
        /// <summary>
        /// Gets the number of messages in the conversation history.
        /// </summary>
        public int GetConversationHistoryCount()
        {
            return conversationHistory.Count;
        }
        
        /// <summary>
        /// Gets a copy of the current conversation history.
        /// </summary>
        public List<APIModels.ConversationMessage> GetConversationHistory()
        {
            return new List<APIModels.ConversationMessage>(conversationHistory);
        }
        
        /// <summary>
        /// Gets available voices from the ElevenLabs API.
        /// </summary>
        public IEnumerator GetVoices(Action<List<APIModels.Voice>> onSuccess, Action<string> onError)
        {
            if (!ValidateApiKey())
            {
                onError?.Invoke("API key not set");
                yield break;
            }
            
            // Delegate to API client
            yield return apiClient.GetVoices(onSuccess, onError);
        }
        
        /// <summary>
        /// Sets a model ID to use for conversation.
        /// </summary>
        public void SetModelId(string modelId)
        {
            apiClient.UpdateDefaultModel(modelId);
        }
    }
}