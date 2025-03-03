using System;
using System.Collections;
using UnityEngine;
using ElevelLabs.VRAvatar.Core;

namespace ElevelLabs.VRAvatar.API
{
    /// <summary>
    /// Manages error handling for API calls, including rate limits, retries with exponential backoff,
    /// and user-friendly error messages in the VR environment.
    /// </summary>
    public class ErrorManager : MonoBehaviour
    {
        private static ErrorManager _instance;
        public static ErrorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("ErrorManager is not initialized. Make sure it exists in the scene.");
                }
                return _instance;
            }
        }

        [Header("Rate Limiting")]
        [Tooltip("Maximum tokens allowed per minute (ElevenLabs limit is 8,000)")]
        [SerializeField] private int maxTokensPerMinute = 8000;
        
        [Tooltip("Cooldown period in seconds when rate limit is reached")]
        [SerializeField] private float rateLimitCooldownPeriod = 60f;

        [Header("Retry Configuration")]
        [Tooltip("Maximum number of retry attempts")]
        [SerializeField] public int maxRetryAttempts = 5;
        
        [Tooltip("Base delay for exponential backoff (in seconds)")]
        [SerializeField] private float baseRetryDelay = 0.5f;
        
        [Tooltip("Maximum delay for exponential backoff (in seconds)")]
        [SerializeField] private float maxRetryDelay = 8f;

        // Token tracking
        private int tokensUsedInLastMinute = 0;
        private float tokenResetTime = 0f;
        
        // Public properties
        public bool isRateLimited { get; private set; } = false;

        // Events
        public event Action<string> OnErrorOccurred;
        public event Action<bool> OnRateLimitChanged;
        public event Action<int, int> OnTokenUsageUpdated; // current, max

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize token reset timer
            tokenResetTime = Time.time + 60f;
        }

        private void Update()
        {
            // Reset token counter every minute
            if (Time.time >= tokenResetTime)
            {
                ResetTokenCounter();
            }

            // Check if we should end the rate limit cooldown
            if (isRateLimited && tokensUsedInLastMinute < maxTokensPerMinute)
            {
                isRateLimited = false;
                OnRateLimitChanged?.Invoke(false);
                Debug.Log("Rate limit cooldown ended");
            }
        }

        /// <summary>
        /// Resets the token usage counter for the next minute.
        /// </summary>
        private void ResetTokenCounter()
        {
            int previousUsage = tokensUsedInLastMinute;
            tokensUsedInLastMinute = 0;
            tokenResetTime = Time.time + 60f;
            
            // Notify about token reset
            OnTokenUsageUpdated?.Invoke(0, maxTokensPerMinute);
            
            Debug.Log($"Token counter reset. Previous usage: {previousUsage}/{maxTokensPerMinute}");
        }

        /// <summary>
        /// Tracks token usage to prevent rate limit errors.
        /// </summary>
        /// <param name="tokenCount">Number of tokens used</param>
        /// <returns>True if operation can proceed, false if rate limited</returns>
        public bool TrackTokenUsage(int tokenCount)
        {
            // Add to token count
            tokensUsedInLastMinute += tokenCount;
            
            // Notify about updated token usage
            OnTokenUsageUpdated?.Invoke(tokensUsedInLastMinute, maxTokensPerMinute);
            
            // Check if we're approaching the rate limit
            if (tokensUsedInLastMinute >= maxTokensPerMinute * 0.95f)
            {
                Debug.LogWarning($"Approaching token rate limit: {tokensUsedInLastMinute}/{maxTokensPerMinute}");
            }
            
            // Check if we've exceeded the rate limit
            if (tokensUsedInLastMinute >= maxTokensPerMinute && !isRateLimited)
            {
                isRateLimited = true;
                string errorMessage = $"Rate limit reached ({maxTokensPerMinute} tokens/minute). Please wait a moment before making more requests.";
                OnErrorOccurred?.Invoke(errorMessage);
                OnRateLimitChanged?.Invoke(true);
                Debug.LogError(errorMessage);
                return false;
            }
            
            return !isRateLimited;
        }

        /// <summary>
        /// Determines if an error is caused by rate limiting.
        /// </summary>
        /// <param name="errorMessage">Error message to analyze</param>
        /// <returns>True if the error is a rate limit error</returns>
        public bool IsRateLimitError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return false;
            
            // Common rate limit error indicators
            return errorMessage.Contains("429") || 
                   errorMessage.Contains("Too Many Requests") || 
                   errorMessage.Contains("rate limit") ||
                   errorMessage.Contains("too many tokens") ||
                   errorMessage.Contains("tokens per minute");
        }

        /// <summary>
        /// Handles API errors and provides user-friendly messages.
        /// </summary>
        /// <param name="errorMessage">Original error message</param>
        /// <returns>User-friendly error message</returns>
        public string GetUserFriendlyErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return "Unknown error occurred";
            
            // Rate limit errors
            if (IsRateLimitError(errorMessage))
            {
                return "You're talking too quickly. Please wait a moment before continuing.";
            }
            
            // Authentication errors
            if (errorMessage.Contains("401") || 
                errorMessage.Contains("Unauthorized") || 
                errorMessage.Contains("API key"))
            {
                return "Authentication failed. Please check your ElevenLabs API key in settings.";
            }
            
            // Network errors
            if (errorMessage.Contains("network") || 
                errorMessage.Contains("connection") || 
                errorMessage.Contains("timeout"))
            {
                return "Network connection issue. Please check your internet connection.";
            }
            
            // Voice-related errors
            if (errorMessage.Contains("voice") || errorMessage.Contains("Voice"))
            {
                return "There was an issue with the voice settings. Please try again or select a different voice.";
            }
            
            // Default error message
            return "Sorry, an error occurred. Please try again in a moment.";
        }

        /// <summary>
        /// Calculates an exponential backoff delay for retries.
        /// </summary>
        /// <param name="retryAttempt">Current retry attempt (0-based)</param>
        /// <returns>Time to wait in seconds before next retry</returns>
        public float GetExponentialBackoffDelay(int retryAttempt)
        {
            float delay = baseRetryDelay * Mathf.Pow(2, retryAttempt);
            
            // Add a small random jitter (±10%) to prevent synchronized retries
            float jitter = UnityEngine.Random.Range(-0.1f, 0.1f);
            delay *= (1f + jitter);
            
            // Cap the delay at the maximum
            return Mathf.Min(delay, maxRetryDelay);
        }

        /// <summary>
        /// Initiates a cooldown period after a rate limit is reached.
        /// </summary>
        /// <returns>Coroutine for handling the cooldown</returns>
        public IEnumerator StartRateLimitCooldown()
        {
            if (!isRateLimited)
            {
                isRateLimited = true;
                OnRateLimitChanged?.Invoke(true);
                
                Debug.Log($"Starting rate limit cooldown for {rateLimitCooldownPeriod} seconds");
                
                // Wait for the cooldown period
                yield return new WaitForSeconds(rateLimitCooldownPeriod);
                
                // Reset token counter after cooldown
                ResetTokenCounter();
                
                isRateLimited = false;
                OnRateLimitChanged?.Invoke(false);
                
                Debug.Log("Rate limit cooldown ended");
            }
        }

        /// <summary>
        /// Determines if a retry should be attempted based on the error type.
        /// </summary>
        /// <param name="errorMessage">Error message to analyze</param>
        /// <returns>True if the error is retryable</returns>
        public bool IsRetryableError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return false;
            
            // Rate limit errors should be retried after a cooldown
            if (IsRateLimitError(errorMessage)) return true;
            
            // Server errors (5xx) are retryable
            if (errorMessage.Contains("500") || 
                errorMessage.Contains("502") || 
                errorMessage.Contains("503") || 
                errorMessage.Contains("504"))
            {
                return true;
            }
            
            // Network errors are retryable
            if (errorMessage.Contains("network") || 
                errorMessage.Contains("connection") || 
                errorMessage.Contains("timeout"))
            {
                return true;
            }
            
            // Default to non-retryable for other error types
            return false;
        }
    }
}