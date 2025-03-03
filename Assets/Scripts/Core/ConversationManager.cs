using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElevelLabs.VRAvatar.API;
using ElevelLabs.VRAvatar.Audio;
using ElevelLabs.VRAvatar.Avatar;
using ElevelLabs.VRAvatar.UI;

namespace ElevelLabs.VRAvatar.Core
{
    /// <summary>
    /// Manages the conversation flow between the user and the AI avatar.
    /// Acts as the central coordinator for conversation state and processing.
    /// Handles conversation history, context management, and the integration
    /// between audio input, AI processing, and audio output.
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MicrophoneInput microphoneInput;
        [SerializeField] private AudioPlayer audioPlayer;
        [SerializeField] private AvatarController avatarController;
        [SerializeField] private ConversationUI conversationUI;
        [SerializeField] private ErrorManager errorManager;
        
        // Reference to the ElevenLabs API client
        private ElevenLabsAPIClient apiClient;

        [Header("Conversation Settings")]
        [SerializeField] private float responseDelay = 0.2f; // Short delay before responding for natural feel
        [SerializeField] private int maxHistoryItems = 10;
        [SerializeField] private string initialSystemPrompt = "You are a helpful AI assistant in a VR environment. Provide concise and informative responses.";
        
        // Conversation state
        public enum ConversationState
        {
            Idle,           // Waiting for user input
            Listening,      // Actively listening to user
            Processing,     // Processing user input and generating response
            Responding,     // Avatar is speaking response
            Error           // Error state
        }
        
        [Header("State")]
        [SerializeField] private ConversationState currentState = ConversationState.Idle;
        
        // Conversation history
        private List<ConversationEntry> conversationHistory = new List<ConversationEntry>();
        
        // API conversation history (formatted for the API)
        private List<APIModels.ConversationMessage> apiConversationHistory = new List<APIModels.ConversationMessage>();
        
        // Conversation context management
        private string currentTopic = "";
        private int totalTokensUsed = 0;
        private DateTime conversationStartTime;
        private bool hasInitialPrompt = false;
        
        // Events
        public event Action<ConversationState> OnConversationStateChanged;
        public event Action<string> OnUserInputProcessed;
        public event Action<string> OnAvatarResponseReceived;
        public event Action<ConversationEntry> OnConversationEntryAdded;
        public event Action<string> OnConversationError;
        public event Action<int> OnTokensUsed;

        /// <summary>
        /// Initializes the conversation manager and sets up event subscriptions.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("Initializing ConversationManager...");
            
            // Find dependencies if not assigned
            if (microphoneInput == null) microphoneInput = FindObjectOfType<MicrophoneInput>();
            if (audioPlayer == null) audioPlayer = FindObjectOfType<AudioPlayer>();
            if (avatarController == null) avatarController = FindObjectOfType<AvatarController>();
            if (conversationUI == null) conversationUI = FindObjectOfType<ConversationUI>();
            if (errorManager == null) errorManager = FindObjectOfType<ErrorManager>();
            
            // Create ElevenLabs API client
            apiClient = new ElevenLabsAPIClient();
            
            // Subscribe to API client events
            apiClient.OnApiError += HandleApiError;
            
            // Subscribe to ErrorManager events if available
            if (errorManager != null)
            {
                errorManager.OnErrorOccurred += HandleApiError;
                errorManager.OnRateLimitChanged += HandleRateLimitChanged;
                errorManager.OnTokenUsageUpdated += HandleTokenUsageUpdated;
                
                Debug.Log("ErrorManager connected to ConversationManager");
            }
            else
            {
                Debug.LogWarning("ErrorManager not found - advanced error handling disabled");
            }
            
            // Subscribe to input/output events
            if (microphoneInput != null)
            {
                microphoneInput.OnSpeechDetected += HandleSpeechDetected;
                microphoneInput.OnSpeechEnded += HandleSpeechEnded;
                microphoneInput.OnAudioDataReceived += HandleAudioDataReceived;
            }
            else
            {
                Debug.LogError("MicrophoneInput reference missing!");
            }
            
            if (audioPlayer != null)
            {
                audioPlayer.OnPlaybackStarted += HandlePlaybackStarted;
                audioPlayer.OnPlaybackCompleted += HandlePlaybackCompleted;
            }
            else
            {
                Debug.LogError("AudioPlayer reference missing!");
            }
            
            // Load settings
            if (ConfigManager.Instance != null)
            {
                maxHistoryItems = ConfigManager.Instance.Settings.MaxConversationHistory;
                
                // Load custom system prompt if available
                if (!string.IsNullOrEmpty(ConfigManager.Instance.Settings.SystemPrompt))
                {
                    initialSystemPrompt = ConfigManager.Instance.Settings.SystemPrompt;
                }
            }
            
            // Initialize conversation state
            ResetConversation();
            
            // Start in idle state
            SetConversationState(ConversationState.Idle);
            
            yield return null;
            Debug.Log("ConversationManager initialized");
        }

        /// <summary>
        /// Resets the conversation, clearing history and setting up initial system prompt.
        /// </summary>
        public void ResetConversation()
        {
            // Clear conversation histories
            conversationHistory.Clear();
            apiConversationHistory.Clear();
            
            // Add system prompt as the initial message in API history
            if (!string.IsNullOrEmpty(initialSystemPrompt))
            {
                apiConversationHistory.Add(new APIModels.ConversationMessage
                {
                    Role = "system",
                    Content = initialSystemPrompt
                });
                
                hasInitialPrompt = true;
            }
            else
            {
                hasInitialPrompt = false;
            }
            
            // Reset conversation metrics
            totalTokensUsed = 0;
            conversationStartTime = DateTime.Now;
            currentTopic = "";
            
            // Update UI
            if (conversationUI != null)
            {
                conversationUI.ClearConversation();
            }
            
            Debug.Log("Conversation has been reset");
        }

        /// <summary>
        /// Sets the conversation state and triggers the state change event.
        /// </summary>
        private void SetConversationState(ConversationState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnConversationStateChanged?.Invoke(currentState);
                
                // Update UI and avatar based on state
                UpdateUIForState(currentState);
                UpdateAvatarForState(currentState);
                
                Debug.Log($"Conversation state changed to: {currentState}");
            }
        }

        /// <summary>
        /// Updates the UI based on the current conversation state.
        /// </summary>
        private void UpdateUIForState(ConversationState state)
        {
            if (conversationUI == null) return;
            
            switch (state)
            {
                case ConversationState.Idle:
                    conversationUI.ShowIdleState();
                    break;
                case ConversationState.Listening:
                    conversationUI.ShowListeningState();
                    break;
                case ConversationState.Processing:
                    conversationUI.ShowProcessingState();
                    break;
                case ConversationState.Responding:
                    conversationUI.ShowRespondingState();
                    break;
                case ConversationState.Error:
                    conversationUI.ShowErrorState();
                    break;
            }
        }

        /// <summary>
        /// Updates the avatar based on the current conversation state.
        /// </summary>
        private void UpdateAvatarForState(ConversationState state)
        {
            if (avatarController == null) return;
            
            switch (state)
            {
                case ConversationState.Idle:
                    avatarController.SetIdleState();
                    break;
                case ConversationState.Listening:
                    avatarController.SetListeningState();
                    break;
                case ConversationState.Processing:
                    avatarController.SetThinkingState();
                    break;
                case ConversationState.Responding:
                    avatarController.SetTalkingState();
                    break;
                case ConversationState.Error:
                    avatarController.SetErrorState();
                    break;
            }
        }

        #region Event Handlers
        
        /// <summary>
        /// Handles speech detection from the microphone input.
        /// </summary>
        private void HandleSpeechDetected()
        {
            SetConversationState(ConversationState.Listening);
        }
        
        /// <summary>
        /// Handles the end of speech from the microphone input.
        /// </summary>
        private void HandleSpeechEnded()
        {
            SetConversationState(ConversationState.Processing);
        }
        
        /// <summary>
        /// Handles audio data received from the microphone input.
        /// </summary>
        private void HandleAudioDataReceived(byte[] audioData)
        {
            // When we receive complete audio, start processing it
            StartCoroutine(ProcessAudioInput(audioData));
        }
        
        /// <summary>
        /// Handles the start of response playback.
        /// </summary>
        private void HandlePlaybackStarted()
        {
            SetConversationState(ConversationState.Responding);
        }
        
        /// <summary>
        /// Handles the completion of response playback.
        /// </summary>
        private void HandlePlaybackCompleted()
        {
            SetConversationState(ConversationState.Idle);
        }
        
        /// <summary>
        /// Handles API errors.
        /// </summary>
        private void HandleApiError(string errorMessage)
        {
            Debug.LogError($"API Error: {errorMessage}");
            
            // Get user-friendly error message if possible
            if (errorManager != null)
            {
                string friendlyMessage = errorManager.GetUserFriendlyErrorMessage(errorMessage);
                OnConversationError?.Invoke(friendlyMessage);
            }
            else
            {
                OnConversationError?.Invoke(errorMessage);
            }
            
            SetConversationState(ConversationState.Error);
        }
        
        /// <summary>
        /// Handles rate limit state changes from the ErrorManager.
        /// </summary>
        private void HandleRateLimitChanged(bool isRateLimited)
        {
            if (isRateLimited)
            {
                Debug.LogWarning("Rate limit applied - pausing conversation");
                string message = "You're talking too quickly. Please wait a moment before continuing.";
                OnConversationError?.Invoke(message);
                SetConversationState(ConversationState.Error);
            }
            else
            {
                Debug.Log("Rate limit lifted - conversation can continue");
                SetConversationState(ConversationState.Idle);
            }
        }
        
        /// <summary>
        /// Handles token usage updates from the ErrorManager.
        /// </summary>
        private void HandleTokenUsageUpdated(int currentUsage, int maxAllowed)
        {
            // Update token usage metric
            totalTokensUsed = currentUsage;
            OnTokensUsed?.Invoke(currentUsage);
            
            // Warn if approaching limit
            if (currentUsage > maxAllowed * 0.8f)
            {
                Debug.LogWarning($"Approaching token limit: {currentUsage}/{maxAllowed}");
            }
        }
        
        #endregion

        /// <summary>
        /// Processes audio input from the user.
        /// </summary>
        private IEnumerator ProcessAudioInput(byte[] audioData)
        {
            if (apiClient == null)
            {
                Debug.LogError("ElevenLabs API client not initialized");
                SetConversationState(ConversationState.Error);
                yield break;
            }
            
            try
            {
                // Process audio to text using ElevenLabs Speech Recognition API
                bool recognitionComplete = false;
                string userText = "";
                
                // Call speech recognition API
                yield return apiClient.RecognizeSpeech(
                    audioData,
                    (text) => {
                        userText = text;
                        recognitionComplete = true;
                    },
                    (error) => {
                        Debug.LogError($"Speech recognition error: {error}");
                        OnConversationError?.Invoke("Speech recognition failed: " + error);
                        SetConversationState(ConversationState.Error);
                        recognitionComplete = true;
                    }
                );
                
                // Wait for recognition to complete
                while (!recognitionComplete)
                {
                    yield return null;
                }
                
                // Check if we have valid text
                if (string.IsNullOrEmpty(userText))
                {
                    Debug.LogWarning("No text recognized from speech");
                    SetConversationState(ConversationState.Idle);
                    yield break;
                }
                
                // Add user message to history
                AddConversationEntry(true, userText);
                OnUserInputProcessed?.Invoke(userText);
                
                // Generate response 
                yield return StartCoroutine(GenerateAIResponse(userText));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing audio input: {ex.Message}");
                OnConversationError?.Invoke("Error processing audio: " + ex.Message);
                SetConversationState(ConversationState.Error);
            }
        }

        /// <summary>
        /// Generates an AI response based on user input.
        /// </summary>
        private IEnumerator GenerateAIResponse(string userInput)
        {
            // Small delay for more natural conversation
            yield return new WaitForSeconds(responseDelay);
            
            try
            {
                // Add user message to API conversation history
                apiConversationHistory.Add(new APIModels.ConversationMessage
                {
                    Role = "user",
                    Content = userInput
                });
                
                // Generate response using API
                bool responseComplete = false;
                string responseText = "";
                
                // Call Conversational AI API
                yield return apiClient.GenerateConversation(
                    apiConversationHistory,
                    null, // Use default model
                    0.7f, // Default temperature
                    (response) => {
                        responseText = response.Message.Content;
                        
                        // Add assistant response to API history
                        apiConversationHistory.Add(new APIModels.ConversationMessage
                        {
                            Role = "assistant",
                            Content = responseText
                        });
                        
                        // Track token usage
                        if (response.Usage != null)
                        {
                            // Let the ErrorManager track token usage if available
                            if (errorManager != null)
                            {
                                errorManager.TrackTokenUsage(response.Usage.TotalTokens);
                            }
                            else
                            {
                                // Otherwise track it locally
                                totalTokensUsed += response.Usage.TotalTokens;
                                OnTokensUsed?.Invoke(response.Usage.TotalTokens);
                            }
                        }
                        
                        responseComplete = true;
                    },
                    (error) => {
                        Debug.LogError($"Conversation API error: {error}");
                        OnConversationError?.Invoke("Failed to generate response: " + error);
                        SetConversationState(ConversationState.Error);
                        responseComplete = true;
                    }
                );
                
                // Wait for response to complete
                while (!responseComplete)
                {
                    yield return null;
                }
                
                // Check if we have a valid response
                if (string.IsNullOrEmpty(responseText))
                {
                    Debug.LogWarning("No response text generated");
                    SetConversationState(ConversationState.Idle);
                    yield break;
                }
                
                // Add AI response to conversation history
                AddConversationEntry(false, responseText);
                OnAvatarResponseReceived?.Invoke(responseText);
                
                // Limit conversation history size
                TrimConversationHistory();
                
                // Generate audio for the response
                yield return StartCoroutine(GenerateAudioResponse(responseText));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error generating AI response: {ex.Message}");
                OnConversationError?.Invoke("Error generating response: " + ex.Message);
                SetConversationState(ConversationState.Error);
            }
        }

        /// <summary>
        /// Generates audio for the AI response text.
        /// </summary>
        private IEnumerator GenerateAudioResponse(string responseText)
        {
            try
            {
                // Generate audio using ElevenLabs Text-to-Speech API
                bool audioComplete = false;
                byte[] audioData = null;
                
                // Call Text-to-Speech API
                yield return apiClient.GenerateSpeech(
                    responseText,
                    null, // Use default voice
                    null, // Use default voice settings
                    null, // Use default model
                    (data) => {
                        audioData = data;
                        audioComplete = true;
                    },
                    (error) => {
                        Debug.LogError($"Text-to-speech error: {error}");
                        OnConversationError?.Invoke("Failed to generate audio: " + error);
                        SetConversationState(ConversationState.Error);
                        audioComplete = true;
                    }
                );
                
                // Wait for audio generation to complete
                while (!audioComplete)
                {
                    yield return null;
                }
                
                // Play the response audio
                if (audioPlayer != null && audioData != null && audioData.Length > 0)
                {
                    audioPlayer.PlayAudioData(audioData);
                }
                else
                {
                    // If no audio data or player, just go back to idle
                    Debug.LogWarning("No audio data generated or audio player not available");
                    SetConversationState(ConversationState.Idle);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error generating audio response: {ex.Message}");
                OnConversationError?.Invoke("Error generating audio: " + ex.Message);
                SetConversationState(ConversationState.Error);
            }
        }

        /// <summary>
        /// Processes text input directly (instead of from speech).
        /// </summary>
        public void ProcessTextInput(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            // Only proceed if we're in an idle state
            if (currentState != ConversationState.Idle)
            {
                Debug.LogWarning("Cannot process text input while in " + currentState + " state");
                return;
            }
            
            SetConversationState(ConversationState.Processing);
            
            // Add to history and generate response
            AddConversationEntry(true, text);
            OnUserInputProcessed?.Invoke(text);
            
            StartCoroutine(GenerateAIResponse(text));
        }

        /// <summary>
        /// Adds an entry to the conversation history.
        /// </summary>
        private void AddConversationEntry(bool isUser, string text)
        {
            ConversationEntry entry = new ConversationEntry
            {
                IsUserMessage = isUser,
                Text = text,
                Timestamp = DateTime.Now
            };
            
            conversationHistory.Add(entry);
            
            // Notify listeners
            OnConversationEntryAdded?.Invoke(entry);
            
            // Update UI
            if (conversationUI != null)
            {
                conversationUI.AddConversationEntry(entry);
            }
        }

        /// <summary>
        /// Trims the conversation history to stay within limits.
        /// </summary>
        private void TrimConversationHistory()
        {
            // Trim UI conversation history
            while (conversationHistory.Count > maxHistoryItems)
            {
                conversationHistory.RemoveAt(0);
            }
            
            // Trim API conversation history, but preserve system prompt if present
            int startIndex = hasInitialPrompt ? 1 : 0;
            int maxApiHistoryItems = maxHistoryItems * 2; // Each exchange has user + assistant message
            
            while (apiConversationHistory.Count > maxApiHistoryItems + startIndex)
            {
                // Remove the oldest user-assistant exchange (2 messages)
                apiConversationHistory.RemoveAt(startIndex); // Remove user message
                if (apiConversationHistory.Count > startIndex)
                {
                    apiConversationHistory.RemoveAt(startIndex); // Remove assistant message
                }
            }
        }

        /// <summary>
        /// Sets a custom system prompt to guide the AI's behavior.
        /// </summary>
        public void SetSystemPrompt(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                return;
            }
            
            initialSystemPrompt = prompt;
            
            // If we have an existing system message, update it
            if (apiConversationHistory.Count > 0 && apiConversationHistory[0].Role == "system")
            {
                apiConversationHistory[0].Content = prompt;
            }
            else
            {
                // Otherwise insert it at the beginning
                apiConversationHistory.Insert(0, new APIModels.ConversationMessage
                {
                    Role = "system",
                    Content = prompt
                });
            }
            
            hasInitialPrompt = true;
            Debug.Log("System prompt updated");
        }

        /// <summary>
        /// Returns the current conversation history.
        /// </summary>
        public List<ConversationEntry> GetConversationHistory()
        {
            return new List<ConversationEntry>(conversationHistory);
        }

        /// <summary>
        /// Returns metrics about the current conversation.
        /// </summary>
        public (int messageCount, int tokenCount, TimeSpan duration, string topic) GetConversationMetrics()
        {
            int messageCount = conversationHistory.Count;
            TimeSpan duration = DateTime.Now - conversationStartTime;
            
            return (messageCount, totalTokensUsed, duration, currentTopic);
        }

        /// <summary>
        /// Clears the conversation history.
        /// </summary>
        public void ClearConversationHistory()
        {
            ResetConversation();
        }

        /// <summary>
        /// Gets the current conversation state.
        /// </summary>
        public ConversationState GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Manually set the conversation topic.
        /// </summary>
        public void SetConversationTopic(string topic)
        {
            currentTopic = topic;
        }
    }

    /// <summary>
    /// Represents an entry in the conversation history.
    /// </summary>
    [Serializable]
    public class ConversationEntry
    {
        public bool IsUserMessage;
        public string Text;
        public DateTime Timestamp;
    }
}