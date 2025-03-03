using System.Collections;
using UnityEngine;
using ElevelLabs.VRAvatar.Core.Interfaces;

namespace ElevelLabs.VRAvatar.Core.Base
{
    /// <summary>
    /// Base class for all major components in the system.
    /// Provides common initialization and lifecycle functionality.
    /// </summary>
    public abstract class BaseComponent : MonoBehaviour, IInitializable, ILifecycleManager
    {
        [SerializeField] protected bool isInitialized = false;
        [SerializeField] protected bool debugLogging = false;
        
        /// <summary>
        /// Checks if the component has been initialized.
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public virtual IEnumerator Initialize()
        {
            if (isInitialized)
            {
                LogDebug($"{GetType().Name} is already initialized.");
                yield break;
            }
            
            LogDebug($"Initializing {GetType().Name}...");
            
            yield return OnInitialize();
            
            isInitialized = true;
            LogDebug($"{GetType().Name} initialized successfully.");
        }
        
        /// <summary>
        /// Override this method to perform component-specific initialization.
        /// </summary>
        protected virtual IEnumerator OnInitialize()
        {
            yield return null;
        }
        
        /// <summary>
        /// Pauses the component operations.
        /// </summary>
        public virtual void Pause()
        {
            if (!isInitialized) return;
            
            LogDebug($"Pausing {GetType().Name}...");
            OnPause();
        }
        
        /// <summary>
        /// Override this method to perform component-specific pause logic.
        /// </summary>
        protected virtual void OnPause() { }
        
        /// <summary>
        /// Resumes the component operations.
        /// </summary>
        public virtual void Resume()
        {
            if (!isInitialized) return;
            
            LogDebug($"Resuming {GetType().Name}...");
            OnResume();
        }
        
        /// <summary>
        /// Override this method to perform component-specific resume logic.
        /// </summary>
        protected virtual void OnResume() { }
        
        /// <summary>
        /// Shuts down the component and cleans up resources.
        /// </summary>
        public virtual void Shutdown()
        {
            if (!isInitialized) return;
            
            LogDebug($"Shutting down {GetType().Name}...");
            OnShutdown();
            
            isInitialized = false;
        }
        
        /// <summary>
        /// Override this method to perform component-specific shutdown logic.
        /// </summary>
        protected virtual void OnShutdown() { }
        
        /// <summary>
        /// Logs a debug message if debug logging is enabled.
        /// </summary>
        protected void LogDebug(string message)
        {
            if (debugLogging)
            {
                Debug.Log($"[{GetType().Name}] {message}");
            }
        }
        
        /// <summary>
        /// Logs an error message.
        /// </summary>
        protected void LogError(string message)
        {
            Debug.LogError($"[{GetType().Name}] {message}");
        }
        
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[{GetType().Name}] {message}");
        }
    }
}