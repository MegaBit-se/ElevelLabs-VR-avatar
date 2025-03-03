using UnityEngine;

namespace ElevelLabs.VRAvatar.Core.Base
{
    /// <summary>
    /// Base class for singleton MonoBehaviour components.
    /// Ensures only one instance exists and provides global access to it.
    /// </summary>
    /// <typeparam name="T">The type of the component.</typeparam>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isInitialized = false;
        private static object _lock = new object();
        
        /// <summary>
        /// Gets the singleton instance of the component.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance = FindObjectOfType<T>();
                        
                        if (_instance == null)
                        {
                            Debug.LogError($"[Singleton] An instance of {typeof(T)} is needed in the scene, but none was found.");
                        }
                    }
                }
                
                return _instance;
            }
        }
        
        /// <summary>
        /// Check if the singleton is initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Called when the component is created. Ensures singleton pattern.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[Singleton] Another instance of {typeof(T)} already exists. Destroying this duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this as T;
            _isInitialized = true;
            
            OnSingletonAwake();
        }
        
        /// <summary>
        /// Called after singleton initialization is complete.
        /// Override this method to perform additional initialization.
        /// </summary>
        protected virtual void OnSingletonAwake() { }
        
        /// <summary>
        /// Called when the component is being destroyed. Cleans up singleton reference.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _isInitialized = false;
            }
        }
    }
}