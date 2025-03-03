using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using ElevelLabs.VRAvatar.API;
using ElevelLabs.VRAvatar.Audio;
using ElevelLabs.VRAvatar.Avatar;

namespace ElevelLabs.VRAvatar.Core
{
    /// <summary>
    /// Main application manager that coordinates and initializes all components.
    /// Acts as the central coordinator for the application.
    /// </summary>
    public class AppManager : MonoBehaviour
    {
        private static AppManager _instance;
        public static AppManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("AppManager is not initialized. Make sure it exists in the scene.");
                }
                return _instance;
            }
        }

        [Header("Component References")]
        [SerializeField] private ConversationManager conversationManager;
        [SerializeField] private MicrophoneInput microphoneInput;
        [SerializeField] private AudioPlayer audioPlayer;
        [SerializeField] private AvatarController avatarController;

        [Header("Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private bool isVREnabled = false;

        // Events
        public event Action OnApplicationInitialized;
        public event Action<bool> OnVRStatusChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(InitializeApplication());
        }

        /// <summary>
        /// Initializes all application components in the correct order.
        /// </summary>
        private IEnumerator InitializeApplication()
        {
            Debug.Log("Starting application initialization...");

            // Check if we're running in VR mode
            isVREnabled = XRSettings.isDeviceActive;
            Debug.Log($"VR Status: {(isVREnabled ? "Enabled" : "Disabled")}");
            OnVRStatusChanged?.Invoke(isVREnabled);

            // Wait for ConfigManager to be ready
            yield return new WaitUntil(() => ConfigManager.Instance != null && ConfigManager.Instance.Settings != null);
            Debug.Log("Configuration loaded");

            // Initialize components in sequence
            yield return StartCoroutine(InitializeElevenLabsAPI());
            yield return StartCoroutine(InitializeAudioSystem());
            yield return StartCoroutine(InitializeAvatarSystem());
            yield return StartCoroutine(InitializeConversationSystem());

            isInitialized = true;
            Debug.Log("Application initialization complete");
            OnApplicationInitialized?.Invoke();
        }

        private IEnumerator InitializeElevenLabsAPI()
        {
            Debug.Log("Initializing ElevenLabs API...");
            // ElevenLabsAPI initialization will be implemented here
            yield return null;
            Debug.Log("ElevenLabs API initialized");
        }

        private IEnumerator InitializeAudioSystem()
        {
            Debug.Log("Initializing Audio System...");
            // Check if microphone references are assigned
            if (microphoneInput == null)
            {
                microphoneInput = FindObjectOfType<MicrophoneInput>();
                if (microphoneInput == null)
                {
                    Debug.LogError("MicrophoneInput component not found!");
                }
            }

            if (audioPlayer == null)
            {
                audioPlayer = FindObjectOfType<AudioPlayer>();
                if (audioPlayer == null)
                {
                    Debug.LogError("AudioPlayer component not found!");
                }
            }

            // Initialize microphone and audio player
            if (microphoneInput != null) yield return microphoneInput.Initialize();
            if (audioPlayer != null) audioPlayer.Initialize();

            yield return null;
            Debug.Log("Audio System initialized");
        }

        private IEnumerator InitializeAvatarSystem()
        {
            Debug.Log("Initializing Avatar System...");
            // Check if avatar controller is assigned
            if (avatarController == null)
            {
                avatarController = FindObjectOfType<AvatarController>();
                if (avatarController == null)
                {
                    Debug.LogError("AvatarController component not found!");
                }
            }

            // Initialize avatar controller
            if (avatarController != null) yield return avatarController.Initialize();

            yield return null;
            Debug.Log("Avatar System initialized");
        }

        private IEnumerator InitializeConversationSystem()
        {
            Debug.Log("Initializing Conversation System...");
            // Check if conversation manager is assigned
            if (conversationManager == null)
            {
                conversationManager = FindObjectOfType<ConversationManager>();
                if (conversationManager == null)
                {
                    Debug.LogError("ConversationManager component not found!");
                }
            }

            // Initialize conversation manager
            if (conversationManager != null) yield return conversationManager.Initialize();

            yield return null;
            Debug.Log("Conversation System initialized");
        }

        /// <summary>
        /// Handles application pause state (for mobile devices).
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"Application {(pauseStatus ? "paused" : "resumed")}");
            
            if (!pauseStatus && isInitialized)
            {
                // Resume operations when application comes back from pause
                if (microphoneInput != null) microphoneInput.Resume();
            }
            else if (pauseStatus && isInitialized)
            {
                // Pause operations when application is paused
                if (microphoneInput != null) microphoneInput.Pause();
            }
        }

        /// <summary>
        /// Handles application quit to clean up resources.
        /// </summary>
        private void OnApplicationQuit()
        {
            Debug.Log("Application shutting down, cleaning up resources...");
            
            // Ensure settings are saved before quitting
            if (ConfigManager.Instance != null)
            {
                ConfigManager.Instance.SaveSettings();
            }
            
            // Stop microphone input
            if (microphoneInput != null)
            {
                microphoneInput.StopRecording();
            }
        }
    }
}