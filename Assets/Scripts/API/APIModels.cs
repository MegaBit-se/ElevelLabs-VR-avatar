using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElevelLabs.VRAvatar.API
{
    /// <summary>
    /// Contains data models for API requests and responses when interacting with the ElevenLabs API.
    /// </summary>
    public static class APIModels
    {
        #region Speech Recognition Models

        /// <summary>
        /// Response from the Speech Recognition API.
        /// </summary>
        [Serializable]
        public class SpeechRecognitionResponse
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("confidence")]
            public float Confidence { get; set; }
        }

        #endregion

        #region Conversational AI Models

        /// <summary>
        /// Represents a message in a conversation.
        /// </summary>
        [Serializable]
        public class ConversationMessage
        {
            [JsonProperty("role")]
            public string Role { get; set; } // "user" or "assistant"

            [JsonProperty("content")]
            public string Content { get; set; }
        }

        /// <summary>
        /// Request to the Conversational AI API.
        /// </summary>
        [Serializable]
        public class ConversationRequest
        {
            [JsonProperty("messages")]
            public List<ConversationMessage> Messages { get; set; }

            [JsonProperty("model")]
            public string Model { get; set; } = "default";

            [JsonProperty("temperature")]
            public float Temperature { get; set; } = 0.7f;

            [JsonProperty("max_tokens")]
            public int MaxTokens { get; set; } = 150;
        }

        /// <summary>
        /// Response from the Conversational AI API.
        /// </summary>
        [Serializable]
        public class ConversationResponse
        {
            [JsonProperty("message")]
            public ConversationMessage Message { get; set; }

            [JsonProperty("usage")]
            public TokenUsage Usage { get; set; }
        }

        /// <summary>
        /// Token usage information.
        /// </summary>
        [Serializable]
        public class TokenUsage
        {
            [JsonProperty("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonProperty("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonProperty("total_tokens")]
            public int TotalTokens { get; set; }
        }

        #endregion

        #region Text-to-Speech Models

        /// <summary>
        /// Voice settings for text-to-speech generation.
        /// </summary>
        [Serializable]
        public class VoiceSettings
        {
            [JsonProperty("stability")]
            public float Stability { get; set; } = 0.5f;

            [JsonProperty("similarity_boost")]
            public float SimilarityBoost { get; set; } = 0.75f;
        }

        /// <summary>
        /// Request to the Text-to-Speech API.
        /// </summary>
        [Serializable]
        public class TextToSpeechRequest
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("model_id")]
            public string ModelId { get; set; } = "eleven_monolingual_v1";

            [JsonProperty("voice_settings")]
            public VoiceSettings VoiceSettings { get; set; }
        }

        /// <summary>
        /// Response from getting available voices.
        /// </summary>
        [Serializable]
        public class VoicesResponse
        {
            [JsonProperty("voices")]
            public List<Voice> Voices { get; set; }
        }

        /// <summary>
        /// Voice information.
        /// </summary>
        [Serializable]
        public class Voice
        {
            [JsonProperty("voice_id")]
            public string VoiceId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("category")]
            public string Category { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("preview_url")]
            public string PreviewUrl { get; set; }
        }

        #endregion

        #region Error Models

        /// <summary>
        /// Error response from the API.
        /// </summary>
        [Serializable]
        public class ErrorResponse
        {
            [JsonProperty("detail")]
            public ErrorDetail Detail { get; set; }
        }

        /// <summary>
        /// Detailed error information.
        /// </summary>
        [Serializable]
        public class ErrorDetail
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Converts a byte array to a Base64 string.
        /// </summary>
        public static string BytesToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts a Base64 string to a byte array.
        /// </summary>
        public static byte[] Base64ToBytes(string base64)
        {
            return Convert.FromBase64String(base64);
        }

        #endregion
    }
}