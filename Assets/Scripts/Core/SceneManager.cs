using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ElevelLabs.VRAvatar.Core.Base;

namespace ElevelLabs.VRAvatar.Core
{
    /// <summary>
    /// Manages scene loading, unloading, and transitions.
    /// Coordinates scene-specific initialization and setup.
    /// </summary>
    public class SceneManager : SingletonBehaviour<SceneManager>
    {
        [Header("Scene Settings")]
        [Tooltip("Prefab containing all essential scene components")]
        [SerializeField] private GameObject scenePrefab;
        
        [Tooltip("Whether to initialize automatically on Start")]
        [SerializeField] private bool autoInitialize = true;
        
        [Header("Transition")]
        [SerializeField] private GameObject loadingScreenPrefab;
        [SerializeField] private float minLoadingTime = 0.5f;
        
        // Events
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        public event Action OnSceneSetupCompleted;
        
        // Scene state
        private bool isInitialized = false;
        private bool isTransitioning = false;
        private GameObject loadingScreenInstance;
        
        private void Start()
        {
            if (autoInitialize)
            {
                StartCoroutine(InitializeCurrentScene());
            }
        }
        
        /// <summary>
        /// Initializes the current scene and sets up all required components.
        /// </summary>
        public IEnumerator InitializeCurrentScene()
        {
            if (isInitialized)
            {
                Debug.LogWarning("Scene is already initialized.");
                yield break;
            }
            
            Debug.Log("Initializing current scene...");
            
            // Wait for AppManager to be ready
            yield return new WaitUntil(() => AppManager.Instance != null);
            
            // Instantiate scene prefab if it exists and not already in scene
            if (scenePrefab != null && GameObject.FindObjectOfType<AppManager>() == null)
            {
                Instantiate(scenePrefab);
                Debug.Log("Instantiated scene prefab with core components");
            }
            
            // Wait for essential managers to initialize
            yield return new WaitUntil(() => 
                ConfigManager.Instance != null && 
                AppManager.Instance != null);
            
            // Wait for AppManager initialization to complete
            yield return new WaitUntil(() => AppManager.Instance.IsInitialized);
            
            isInitialized = true;
            Debug.Log("Scene initialization complete");
            
            OnSceneSetupCompleted?.Invoke();
        }
        
        /// <summary>
        /// Loads a new scene with transition effects.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <param name="mode">The scene loading mode (Additive or Single).</param>
        public IEnumerator LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("Scene transition already in progress. Ignoring request.");
                yield break;
            }
            
            isTransitioning = true;
            OnSceneLoadStarted?.Invoke(sceneName);
            
            // Show loading screen
            ShowLoadingScreen();
            
            // Track loading time
            float loadingStartTime = Time.time;
            
            // Start scene loading operation
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            asyncLoad.allowSceneActivation = false;
            
            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                // Check if loading is almost complete
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum loading time
                    if (Time.time - loadingStartTime >= minLoadingTime)
                    {
                        asyncLoad.allowSceneActivation = true;
                    }
                }
                
                yield return null;
            }
            
            // Hide loading screen
            HideLoadingScreen();
            
            // Reset state
            isInitialized = false;
            isTransitioning = false;
            
            // Initialize the new scene
            StartCoroutine(InitializeCurrentScene());
            
            OnSceneLoadCompleted?.Invoke(sceneName);
            
            Debug.Log($"Scene '{sceneName}' loaded successfully");
        }
        
        /// <summary>
        /// Shows the loading screen overlay.
        /// </summary>
        private void ShowLoadingScreen()
        {
            if (loadingScreenPrefab != null && loadingScreenInstance == null)
            {
                loadingScreenInstance = Instantiate(loadingScreenPrefab);
                DontDestroyOnLoad(loadingScreenInstance);
            }
        }
        
        /// <summary>
        /// Hides the loading screen overlay.
        /// </summary>
        private void HideLoadingScreen()
        {
            if (loadingScreenInstance != null)
            {
                Destroy(loadingScreenInstance);
                loadingScreenInstance = null;
            }
        }
        
        /// <summary>
        /// Gets the name of the currently active scene.
        /// </summary>
        public string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// Reloads the current scene.
        /// </summary>
        public IEnumerator ReloadCurrentScene()
        {
            string currentScene = GetCurrentSceneName();
            yield return LoadScene(currentScene);
        }
    }
}