using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElevelLabs.VRAvatar.Core.Models
{
    /// <summary>
    /// Represents a single message in a conversation, either from the user or the AI avatar.
    /// </summary>
    [Serializable]
    public class ConversationMessage
    {
        /// <summary>
        /// Unique identifier for the message.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Whether this message is from the user (true) or the AI avatar (false).
        /// </summary>
        public bool IsUserMessage { get; set; }
        
        /// <summary>
        /// The text content of the message.
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Timestamp when the message was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Optional audio data associated with the message.
        /// </summary>
        [NonSerialized] public byte[] AudioData;
        
        /// <summary>
        /// Duration of the audio in seconds, if available.
        /// </summary>
        public float AudioDuration { get; set; }
        
        /// <summary>
        /// Whether the message has been processed by the AI.
        /// </summary>
        public bool IsProcessed { get; set; }
        
        /// <summary>
        /// Any metadata associated with this message (e.g., sentiment, topics).
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Creates a new conversation message from the user.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="audioData">Optional audio data.</param>
        /// <returns>A new user message.</returns>
        public static ConversationMessage CreateUserMessage(string text, byte[] audioData = null)
        {
            return new ConversationMessage
            {
                IsUserMessage = true,
                Text = text,
                AudioData = audioData
            };
        }
        
        /// <summary>
        /// Creates a new conversation message from the AI avatar.
        /// </summary>
        /// <param name="text">The message text.</param>
        /// <param name="audioData">Optional audio data.</param>
        /// <returns>A new AI avatar message.</returns>
        public static ConversationMessage CreateAvatarMessage(string text, byte[] audioData = null)
        {
            return new ConversationMessage
            {
                IsUserMessage = false,
                Text = text,
                AudioData = audioData
            };
        }
        
        /// <summary>
        /// Creates a system message.
        /// </summary>
        /// <param name="text">The system instruction text.</param>
        /// <returns>A new system message.</returns>
        public static ConversationMessage CreateSystemMessage(string text)
        {
            ConversationMessage message = new ConversationMessage
            {
                IsUserMessage = false,
                Text = text
            };
            
            message.Metadata["role"] = "system";
            return message;
        }
    }
    
    /// <summary>
    /// Represents a complete conversation with message history between the user and AI avatar.
    /// </summary>
    [Serializable]
    public class Conversation
    {
        /// <summary>
        /// Unique identifier for the conversation.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Title of the conversation, often derived from content.
        /// </summary>
        public string Title { get; set; } = "New Conversation";
        
        /// <summary>
        /// When the conversation was started.
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// When the conversation was last updated.
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// The list of messages in the conversation.
        /// </summary>
        public List<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
        
        /// <summary>
        /// Maximum number of messages to keep in the history.
        /// </summary>
        public int MaxHistoryItems { get; set; } = 10;
        
        /// <summary>
        /// System prompt for the conversation.
        /// </summary>
        public string SystemPrompt { get; set; } = "";
        
        /// <summary>
        /// Main topic or intent of the conversation.
        /// </summary>
        public string Topic { get; set; } = "";
        
        /// <summary>
        /// Total number of tokens used in the conversation.
        /// </summary>
        public int TokensUsed { get; set; } = 0;
        
        /// <summary>
        /// Settings for the conversation.
        /// </summary>
        public ConversationSettings Settings { get; set; } = new ConversationSettings();
        
        /// <summary>
        /// Adds a message to the conversation and manages history size.
        /// </summary>
        /// <param name="message">The message to add.</param>
        public void AddMessage(ConversationMessage message)
        {
            Messages.Add(message);
            LastUpdateTime = DateTime.Now;
            
            // Limit history size
            while (Messages.Count > MaxHistoryItems)
            {
                Messages.RemoveAt(0);
            }
            
            // Update title based on first few messages if not set
            if (Title == "New Conversation" && Messages.Count > 0)
            {
                UpdateTitle();
            }
        }
        
        /// <summary>
        /// Clears all messages from the conversation.
        /// </summary>
        public void Clear()
        {
            Messages.Clear();
            LastUpdateTime = DateTime.Now;
            TokensUsed = 0;
        }
        
        /// <summary>
        /// Updates the conversation title based on the content.
        /// </summary>
        private void UpdateTitle()
        {
            if (Messages.Count == 0) return;
            
            // Use first message or first few words as the title
            string firstMessage = Messages[0].Text;
            if (string.IsNullOrEmpty(firstMessage)) return;
            
            // Limit to first 50 chars or first sentence
            int endIndex = Mathf.Min(50, firstMessage.Length);
            int periodIndex = firstMessage.IndexOf('.');
            if (periodIndex > 0 && periodIndex < endIndex)
            {
                endIndex = periodIndex;
            }
            
            Title = firstMessage.Substring(0, endIndex).Trim() + (endIndex < firstMessage.Length ? "..." : "");
        }
        
        /// <summary>
        /// Gets the conversation history formatted for the AI API.
        /// </summary>
        /// <returns>A list of message pairs suitable for API context.</returns>
        public List<KeyValuePair<string, string>> GetFormattedHistory()
        {
            List<KeyValuePair<string, string>> formattedMessages = new List<KeyValuePair<string, string>>();
            
            // Add system prompt if available
            if (!string.IsNullOrEmpty(SystemPrompt))
            {
                formattedMessages.Add(new KeyValuePair<string, string>("system", SystemPrompt));
            }
            
            foreach (var message in Messages)
            {
                string role = "user";
                if (!message.IsUserMessage)
                {
                    // Check if it's a special role like system
                    if (message.Metadata.ContainsKey("role"))
                    {
                        role = message.Metadata["role"];
                    }
                    else
                    {
                        role = "assistant";
                    }
                }
                
                formattedMessages.Add(new KeyValuePair<string, string>(role, message.Text));
            }
            
            return formattedMessages;
        }
        
        /// <summary>
        /// Converts the conversation history to ElevenLabs API format.
        /// </summary>
        /// <returns>Conversation history in ElevenLabs format.</returns>
        public List<API.APIModels.ConversationMessage> ToElevenLabsFormat()
        {
            List<API.APIModels.ConversationMessage> result = new List<API.APIModels.ConversationMessage>();
            
            // Add system prompt if available
            if (!string.IsNullOrEmpty(SystemPrompt))
            {
                result.Add(new API.APIModels.ConversationMessage
                {
                    Role = "system",
                    Content = SystemPrompt
                });
            }
            
            // Add conversation messages
            foreach (var message in Messages)
            {
                string role = "user";
                if (!message.IsUserMessage)
                {
                    // Check if it's a special role like system
                    if (message.Metadata.ContainsKey("role"))
                    {
                        role = message.Metadata["role"];
                    }
                    else
                    {
                        role = "assistant";
                    }
                }
                
                result.Add(new API.APIModels.ConversationMessage
                {
                    Role = role,
                    Content = message.Text
                });
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Represents the settings for a conversation.
    /// </summary>
    [Serializable]
    public class ConversationSettings
    {
        /// <summary>
        /// Maximum number of messages to keep in history.
        /// </summary>
        public int MaxHistoryItems = 10;
        
        /// <summary>
        /// Whether to automatically detect speech.
        /// </summary>
        public bool AutoDetectSpeech = true;
        
        /// <summary>
        /// Delay between user input and avatar response in seconds.
        /// </summary>
        public float ResponseDelay = 0.2f;
        
        /// <summary>
        /// Whether to save conversations to disk.
        /// </summary>
        public bool SaveConversations = true;
        
        /// <summary>
        /// Directory to save conversations in.
        /// </summary>
        public string SaveDirectory = "Conversations";
        
        /// <summary>
        /// Temperature for AI response generation (0.0-1.0).
        /// Higher values make output more random, lower values more deterministic.
        /// </summary>
        public float Temperature = 0.7f;
        
        /// <summary>
        /// Maximum number of tokens to generate in responses.
        /// </summary>
        public int MaxTokens = 150;
        
        /// <summary>
        /// Model ID to use for conversation (null = use default).
        /// </summary>
        public string ModelId = null;
        
        /// <summary>
        /// Voice ID to use for text-to-speech (null = use default).
        /// </summary>
        public string VoiceId = null;
    }
    
    /// <summary>
    /// Represents performance and usage statistics for the conversation.
    /// </summary>
    [Serializable]
    public class ConversationMetrics
    {
        /// <summary>
        /// Total number of tokens used in the conversation.
        /// </summary>
        public int TotalTokensUsed { get; set; } = 0;
        
        /// <summary>
        /// Number of user messages in the conversation.
        /// </summary>
        public int UserMessageCount { get; set; } = 0;
        
        /// <summary>
        /// Number of AI responses in the conversation.
        /// </summary>
        public int AIResponseCount { get; set; } = 0;
        
        /// <summary>
        /// Total duration of the conversation.
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// Average response time for AI generation in seconds.
        /// </summary>
        public float AverageResponseTime { get; set; } = 0f;
        
        /// <summary>
        /// Average tokens per response.
        /// </summary>
        public float AverageTokensPerResponse { get; set; } = 0f;
        
        /// <summary>
        /// Records token usage for a response.
        /// </summary>
        public void RecordTokenUsage(int promptTokens, int completionTokens)
        {
            int totalTokens = promptTokens + completionTokens;
            TotalTokensUsed += totalTokens;
            
            AIResponseCount++;
            
            // Update average tokens per response
            AverageTokensPerResponse = (AverageTokensPerResponse * (AIResponseCount - 1) + completionTokens) / AIResponseCount;
        }
        
        /// <summary>
        /// Records a user message.
        /// </summary>
        public void RecordUserMessage()
        {
            UserMessageCount++;
        }
        
        /// <summary>
        /// Records response time.
        /// </summary>
        public void RecordResponseTime(float responseTimeSeconds)
        {
            // Update average response time
            AverageResponseTime = (AverageResponseTime * (AIResponseCount - 1) + responseTimeSeconds) / AIResponseCount;
        }
        
        /// <summary>
        /// Updates the conversation duration.
        /// </summary>
        public void UpdateDuration(DateTime startTime)
        {
            Duration = DateTime.Now - startTime;
        }
    }
}