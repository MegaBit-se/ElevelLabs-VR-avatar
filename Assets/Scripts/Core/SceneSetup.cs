using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElevelLabs.VRAvatar.Audio;
using ElevelLabs.VRAvatar.Avatar;
using ElevelLabs.VRAvatar.UI;

namespace ElevelLabs.VRAvatar.Core
{
    /// <summary>
    /// Responsible for setting up a complete VR Avatar scene, including creating
    /// and connecting all necessary components. This serves as a one-click solution
    /// for initializing the entire system.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private bool createMissingComponents = true;
        [SerializeField] private Transform componentsParent;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject configManagerPrefab;
        [SerializeField] private GameObject appManagerPrefab;
        [SerializeField] private GameObject conversationManagerPrefab;
        [SerializeField] private GameObject sceneManagerPrefab;
        [SerializeField] private GameObject avatarControllerPrefab;
        [SerializeField] private GameObject microphoneInputPrefab;
        [SerializeField] private GameObject audioPlayerPrefab;
        [SerializeField] private GameObject conversationUIPrefab;
        
        private void Awake()
        {
            if (componentsParent == null)
            {
                componentsParent = transform;
            }
        }
        
        private void Start()
        {
            StartCoroutine(SetupScene());
        }
        
        /// <summary>
        /// Sets up all required components for a complete VR Avatar scene.
        /// </summary>
        public IEnumerator SetupScene()
        {
            Debug.Log("Setting up VR Avatar scene...");
            
            // Create and set up components in correct order
            yield return CreateAllComponents();
            
            // Connect components
            ConnectComponentReferences();
            
            // Let SceneManager take over initialization
            if (SceneManager.Instance != null)
            {
                yield return SceneManager.Instance.InitializeCurrentScene();
            }
            else
            {
                Debug.LogError("SceneManager not found. Scene initialization may be incomplete.");
            }
            
            Debug.Log("VR Avatar scene setup complete");
        }
        
        /// <summary>
        /// Creates all required components if they don't already exist.
        /// </summary>
        private IEnumerator CreateAllComponents()
        {
            if (!createMissingComponents) yield break;

            // Create components in dependency order
            CreateComponentIfMissing<ConfigManager>(configManagerPrefab, "ConfigManager");
            yield return new WaitForEndOfFrame(); // Wait a frame to ensure Awake is called
            
            CreateComponentIfMissing<SceneManager>(sceneManagerPrefab, "SceneManager");
            yield return new WaitForEndOfFrame();
            
            CreateComponentIfMissing<AppManager>(appManagerPrefab, "AppManager");
            yield return new WaitForEndOfFrame();
            
            CreateComponentIfMissing<AudioPlayer>(audioPlayerPrefab, "AudioPlayer");
            CreateComponentIfMissing<MicrophoneInput>(microphoneInputPrefab, "MicrophoneInput");
            CreateComponentIfMissing<AvatarController>(avatarControllerPrefab, "AvatarController");
            yield return new WaitForEndOfFrame();
            
            CreateComponentIfMissing<ConversationManager>(conversationManagerPrefab, "ConversationManager");
            CreateComponentIfMissing<ConversationUI>(conversationUIPrefab, "ConversationUI");
            yield return new WaitForEndOfFrame();
        }
        
        /// <summary>
        /// Creates a component if it doesn't already exist in the scene.
        /// </summary>
        private T CreateComponentIfMissing<T>(GameObject prefab, string componentName) where T : MonoBehaviour
        {
            T existingComponent = FindObjectOfType<T>();
            
            if (existingComponent != null)
            {
                Debug.Log($"{componentName} already exists in scene");
                return existingComponent;
            }
            
            if (prefab == null)
            {
                Debug.LogWarning($"{componentName} prefab not assigned. Cannot create component.");
                return null;
            }
            
            GameObject instance = Instantiate(prefab, componentsParent);
            instance.name = componentName;
            
            T component = instance.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"Created {componentName} prefab but it doesn't have a {typeof(T).Name} component!");
            }
            else
            {
                Debug.Log($"Created {componentName} component");
            }
            
            return component;
        }
        
        /// <summary>
        /// Connects references between components.
        /// </summary>
        private void ConnectComponentReferences()
        {
            AppManager appManager = FindObjectOfType<AppManager>();
            if (appManager == null) return;
            
            // Find all required components
            ConversationManager conversationManager = FindObjectOfType<ConversationManager>();
            MicrophoneInput microphoneInput = FindObjectOfType<MicrophoneInput>();
            AudioPlayer audioPlayer = FindObjectOfType<AudioPlayer>();
            AvatarController avatarController = FindObjectOfType<AvatarController>();
            ConversationUI conversationUI = FindObjectOfType<ConversationUI>();
            
            // Connect references using reflection to avoid modifying existing components
            SetFieldIfPublic(appManager, "conversationManager", conversationManager);
            SetFieldIfPublic(appManager, "microphoneInput", microphoneInput);
            SetFieldIfPublic(appManager, "audioPlayer", audioPlayer);
            SetFieldIfPublic(appManager, "avatarController", avatarController);
            
            if (conversationManager != null)
            {
                SetFieldIfPublic(conversationManager, "microphoneInput", microphoneInput);
                SetFieldIfPublic(conversationManager, "audioPlayer", audioPlayer);
                SetFieldIfPublic(conversationManager, "avatarController", avatarController);
                SetFieldIfPublic(conversationManager, "conversationUI", conversationUI);
            }
            
            if (avatarController != null && audioPlayer != null)
            {
                SetFieldIfPublic(avatarController, "audioPlayer", audioPlayer);
            }
            
            Debug.Log("Component references connected");
        }
        
        /// <summary>
        /// Sets a field on a component using reflection if it's publicly accessible.
        /// </summary>
        private void SetFieldIfPublic(object target, string fieldName, object value)
        {
            if (target == null || value == null) return;
            
            var field = target.GetType().GetField(fieldName);
            if (field != null && field.IsPublic)
            {
                field.SetValue(target, value);
            }
        }
    }
}