using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElevelLabs.VRAvatar.Core;
using ElevelLabs.VRAvatar.Audio;

namespace ElevelLabs.VRAvatar.Avatar
{
    /// <summary>
    /// Controls the avatar model, animations, and behaviors.
    /// Manages state transitions and coordinates with other components.
    /// </summary>
    public class AvatarController : MonoBehaviour
    {
        [Header("Avatar Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform headBone;
        [SerializeField] private SkinnedMeshRenderer faceRenderer;
        
        [Header("References")]
        [SerializeField] private AudioPlayer audioPlayer;
        [SerializeField] private LipSync lipSync;
        [SerializeField] private Transform playerHead; // Camera/head transform to look at
        
        [Header("Look At Settings")]
        [SerializeField] private bool enableLookAt = true;
        [SerializeField] private float lookAtWeight = 0.7f;
        [SerializeField] private float lookAtSpeed = 2.0f;
        [SerializeField] private Vector2 horizontalLookLimit = new Vector2(-30f, 30f);
        [SerializeField] private Vector2 verticalLookLimit = new Vector2(-20f, 20f);
        
        [Header("Animation Settings")]
        [SerializeField] private bool enableIdleAnimations = true;
        [SerializeField] private float blendSpeed = 0.2f;
        
        // Animation parameter names
        private const string PARAM_IDLE = "IsIdle";
        private const string PARAM_LISTENING = "IsListening";
        private const string PARAM_THINKING = "IsThinking";
        private const string PARAM_TALKING = "IsTalking";
        
        // Current state
        private enum AvatarState
        {
            Idle,
            Listening,
            Thinking,
            Talking,
            Error
        }
        
        [Header("State")]
        [SerializeField] private AvatarState currentState = AvatarState.Idle;
        
        // Target look position
        private Vector3 targetLookPosition;
        private Quaternion defaultHeadRotation;
        
        // Random idle timing
        private float nextIdleActionTime;
        private float idleActionInterval = 3f;
        
        // Events
        public event Action<AvatarState> OnAvatarStateChanged;

        private void Start()
        {
            // Store default head rotation if head bone is assigned
            if (headBone != null)
            {
                defaultHeadRotation = headBone.localRotation;
            }
            
            // Set initial state
            SetIdleState();
            
            // Start idle behavior coroutine if enabled
            if (enableIdleAnimations)
            {
                StartCoroutine(PerformIdleBehaviors());
            }
        }
        
        /// <summary>
        /// Initializes the avatar controller and its components.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("Initializing AvatarController...");
            
            // Find components if not assigned
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null)
                {
                    Debug.LogError("Animator component not found!");
                }
            }
            
            if (faceRenderer == null)
            {
                faceRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
                if (faceRenderer == null)
                {
                    Debug.LogWarning("SkinnedMeshRenderer not found for face!");
                }
            }
            
            if (audioPlayer == null)
            {
                audioPlayer = FindObjectOfType<AudioPlayer>();
            }
            
            if (lipSync == null)
            {
                lipSync = GetComponent<LipSync>();
                if (lipSync == null)
                {
                    lipSync = gameObject.AddComponent<LipSync>();
                }
            }
            
            // Look for player camera if not assigned
            if (playerHead == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    playerHead = mainCamera.transform;
                }
            }
            
            // Load settings
            if (ConfigManager.Instance != null)
            {
                enableIdleAnimations = ConfigManager.Instance.Settings.EnableIdleAnimations;
            }
            
            // Initialize lip sync
            if (lipSync != null)
            {
                lipSync.SetFaceRenderer(faceRenderer);
                lipSync.SetAudioPlayer(audioPlayer);
                yield return lipSync.Initialize();
            }
            
            yield return null;
            Debug.Log("AvatarController initialized");
        }
        
        private void Update()
        {
            // Handle look-at behavior
            if (enableLookAt && playerHead != null && headBone != null)
            {
                UpdateLookAt();
            }
            
            // Update animation state
            UpdateAnimationState();
        }
        
        /// <summary>
        /// Updates the look-at behavior to face the player naturally.
        /// </summary>
        private void UpdateLookAt()
        {
            // Calculate target position (player's head)
            targetLookPosition = playerHead.position;
            
            // Get direction to target
            Vector3 directionToTarget = targetLookPosition - headBone.position;
            directionToTarget.Normalize();
            
            // Calculate look rotation
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            
            // Apply limits to rotation
            Vector3 targetEuler = targetRotation.eulerAngles;
            targetEuler.x = ClampAngle(targetEuler.x, verticalLookLimit.x, verticalLookLimit.y);
            targetEuler.y = ClampAngle(targetEuler.y, horizontalLookLimit.x, horizontalLookLimit.y);
            targetRotation = Quaternion.Euler(targetEuler);
            
            // Smoothly interpolate to target rotation
            headBone.rotation = Quaternion.Slerp(headBone.rotation, targetRotation, Time.deltaTime * lookAtSpeed);
        }
        
        /// <summary>
        /// Updates the animation parameters based on current state.
        /// </summary>
        private void UpdateAnimationState()
        {
            if (animator == null) return;
            
            // Update animation parameters based on state
            switch (currentState)
            {
                case AvatarState.Idle:
                    SetAnimatorBool(PARAM_IDLE, true);
                    SetAnimatorBool(PARAM_LISTENING, false);
                    SetAnimatorBool(PARAM_THINKING, false);
                    SetAnimatorBool(PARAM_TALKING, false);
                    break;
                
                case AvatarState.Listening:
                    SetAnimatorBool(PARAM_IDLE, false);
                    SetAnimatorBool(PARAM_LISTENING, true);
                    SetAnimatorBool(PARAM_THINKING, false);
                    SetAnimatorBool(PARAM_TALKING, false);
                    break;
                
                case AvatarState.Thinking:
                    SetAnimatorBool(PARAM_IDLE, false);
                    SetAnimatorBool(PARAM_LISTENING, false);
                    SetAnimatorBool(PARAM_THINKING, true);
                    SetAnimatorBool(PARAM_TALKING, false);
                    break;
                
                case AvatarState.Talking:
                    SetAnimatorBool(PARAM_IDLE, false);
                    SetAnimatorBool(PARAM_LISTENING, false);
                    SetAnimatorBool(PARAM_THINKING, false);
                    SetAnimatorBool(PARAM_TALKING, true);
                    break;
                
                case AvatarState.Error:
                    // Could add specific error animation
                    SetAnimatorBool(PARAM_IDLE, true);
                    SetAnimatorBool(PARAM_LISTENING, false);
                    SetAnimatorBool(PARAM_THINKING, false);
                    SetAnimatorBool(PARAM_TALKING, false);
                    break;
            }
        }
        
        /// <summary>
        /// Sets an animator bool parameter with smooth transitions.
        /// </summary>
        private void SetAnimatorBool(string paramName, bool value)
        {
            if (animator == null) return;
            
            // Check if parameter exists
            if (HasParameter(paramName, animator))
            {
                animator.SetBool(paramName, value);
            }
        }
        
        /// <summary>
        /// Checks if an Animator has a specific parameter.
        /// </summary>
        private bool HasParameter(string paramName, Animator animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Sets the avatar to idle state.
        /// </summary>
        public void SetIdleState()
        {
            SetState(AvatarState.Idle);
        }
        
        /// <summary>
        /// Sets the avatar to listening state.
        /// </summary>
        public void SetListeningState()
        {
            SetState(AvatarState.Listening);
        }
        
        /// <summary>
        /// Sets the avatar to thinking state.
        /// </summary>
        public void SetThinkingState()
        {
            SetState(AvatarState.Thinking);
        }
        
        /// <summary>
        /// Sets the avatar to talking state.
        /// </summary>
        public void SetTalkingState()
        {
            SetState(AvatarState.Talking);
        }
        
        /// <summary>
        /// Sets the avatar to error state.
        /// </summary>
        public void SetErrorState()
        {
            SetState(AvatarState.Error);
        }
        
        /// <summary>
        /// Sets the avatar state and notifies listeners.
        /// </summary>
        private void SetState(AvatarState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnAvatarStateChanged?.Invoke(newState);
                
                // Update lip sync based on state
                if (lipSync != null)
                {
                    lipSync.SetActive(newState == AvatarState.Talking);
                }
                
                Debug.Log($"Avatar state changed to: {newState}");
            }
        }
        
        /// <summary>
        /// Performs random idle behaviors when in idle state.
        /// </summary>
        private IEnumerator PerformIdleBehaviors()
        {
            while (true)
            {
                // Only perform idle behaviors in idle state
                if (currentState == AvatarState.Idle && enableIdleAnimations)
                {
                    // Wait for next idle action
                    if (Time.time > nextIdleActionTime)
                    {
                        // Choose a random idle animation/behavior
                        int randomIdle = UnityEngine.Random.Range(0, 3);
                        
                        switch (randomIdle)
                        {
                            case 0:
                                // Small head movement
                                if (headBone != null)
                                {
                                    Vector3 randomLook = new Vector3(
                                        UnityEngine.Random.Range(-10f, 10f),
                                        UnityEngine.Random.Range(-15f, 15f),
                                        0f
                                    );
                                    
                                    StartCoroutine(PerformHeadMovement(randomLook));
                                }
                                break;
                                
                            case 1:
                                // Blink or small facial gesture
                                if (lipSync != null)
                                {
                                    StartCoroutine(lipSync.PerformBlink());
                                }
                                break;
                                
                            case 2:
                                // Small fidget or posture shift
                                if (animator != null && HasParameter("TriggerFidget", animator))
                                {
                                    animator.SetTrigger("TriggerFidget");
                                }
                                break;
                        }
                        
                        // Set next idle action time
                        nextIdleActionTime = Time.time + UnityEngine.Random.Range(idleActionInterval * 0.7f, idleActionInterval * 1.3f);
                    }
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        /// <summary>
        /// Performs a smooth head movement for idle behavior.
        /// </summary>
        private IEnumerator PerformHeadMovement(Vector3 targetEulerOffset)
        {
            if (headBone == null) yield break;
            
            Quaternion startRotation = headBone.localRotation;
            Quaternion targetRotation = Quaternion.Euler(targetEulerOffset) * defaultHeadRotation;
            
            float duration = 1.0f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                headBone.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Hold for a moment
            yield return new WaitForSeconds(0.5f);
            
            // Return to default
            elapsed = 0f;
            startRotation = headBone.localRotation;
            
            while (elapsed < duration)
            {
                headBone.localRotation = Quaternion.Slerp(startRotation, defaultHeadRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            headBone.localRotation = defaultHeadRotation;
        }
        
        /// <summary>
        /// Clamps an angle between min and max, handling 360-degree wraparound.
        /// </summary>
        private float ClampAngle(float angle, float min, float max)
        {
            angle = NormalizeAngle(angle);
            if (angle > 180) angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
        
        /// <summary>
        /// Normalizes an angle to be between 0 and 360 degrees.
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle < 0) angle += 360;
            while (angle > 360) angle -= 360;
            return angle;
        }
        
        /// <summary>
        /// Toggles idle animations on or off.
        /// </summary>
        public void SetIdleAnimationsEnabled(bool enabled)
        {
            enableIdleAnimations = enabled;
        }
        
        /// <summary>
        /// Sets the look at target for the avatar.
        /// </summary>
        public void SetLookAtTarget(Transform target)
        {
            playerHead = target;
        }
        
        /// <summary>
        /// Enables or disables look-at behavior.
        /// </summary>
        public void SetLookAtEnabled(bool enabled)
        {
            enableLookAt = enabled;
        }
    }
}