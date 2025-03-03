using System;
using System.Collections;
using UnityEngine;
using ElevelLabs.VRAvatar.Core;

namespace ElevelLabs.VRAvatar.Audio
{
    /// <summary>
    /// Handles audio playback for the avatar's responses.
    /// Provides events and timing information for lip synchronization.
    /// </summary>
    public class AudioPlayer : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float volume = 1.0f;
        [SerializeField] private bool spatialAudio = true;
        
        [Header("Playback State")]
        [SerializeField] private bool isPlaying = false;
        [SerializeField] private float playbackPosition = 0f;
        [SerializeField] private float playbackDuration = 0f;
        
        [Header("Spectrum Analysis")]
        [SerializeField] private bool analyzeAudio = true;
        [SerializeField] private int spectrumSize = 256;
        [SerializeField] private float[] spectrumData;
        [SerializeField] private float currentAmplitude = 0f;
        
        // Cache for currently loaded audio clip
        private AudioClip currentClip;
        
        // Events
        public event Action OnPlaybackStarted;
        public event Action OnPlaybackCompleted;
        public event Action<float> OnPlaybackProgress;
        
        private void Awake()
        {
            // Create audio source if not assigned
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Initialize spectrum array
            spectrumData = new float[spectrumSize];
        }
        
        /// <summary>
        /// Initializes the audio player with settings.
        /// </summary>
        public void Initialize()
        {
            Debug.Log("Initializing AudioPlayer...");
            
            // Load settings
            if (ConfigManager.Instance != null)
            {
                volume = ConfigManager.Instance.Settings.ResponseVolume;
            }
            
            // Configure audio source
            audioSource.volume = volume;
            audioSource.spatialize = spatialAudio;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            
            Debug.Log("AudioPlayer initialized");
        }
        
        private void Update()
        {
            if (!isPlaying || audioSource == null) return;
            
            // Update playback position
            playbackPosition = audioSource.time;
            
            // Calculate playback progress (0-1)
            float progress = playbackPosition / playbackDuration;
            
            // Notify listeners of progress
            OnPlaybackProgress?.Invoke(progress);
            
            // Check if playback has completed
            if (!audioSource.isPlaying && isPlaying)
            {
                isPlaying = false;
                OnPlaybackCompleted?.Invoke();
                
                Debug.Log("Audio playback completed");
            }
            
            // Perform spectrum analysis for lip sync
            if (analyzeAudio)
            {
                AnalyzeAudioSpectrum();
            }
        }
        
        /// <summary>
        /// Plays audio data from a byte array (MP3 format).
        /// </summary>
        public void PlayAudioData(byte[] audioData)
        {
            if (audioData == null || audioData.Length == 0)
            {
                Debug.LogWarning("Attempted to play empty audio data");
                return;
            }
            
            StartCoroutine(LoadAndPlayAudio(audioData));
        }
        
        /// <summary>
        /// Loads and plays audio from a byte array.
        /// </summary>
        private IEnumerator LoadAndPlayAudio(byte[] audioData)
        {
            Debug.Log($"Loading audio data ({audioData.Length} bytes)");
            
            // Stop any current playback
            StopPlayback();
            
            // Create WAV AudioClip if audioData contains WAV header
            if (IsWavFile(audioData))
            {
                currentClip = CreateClipFromWav(audioData);
            }
            else
            {
                // For MP3, we need to convert to AudioClip
                // This is a simplified implementation and may need to be expanded
                // For production use, you might want to use a proper audio conversion library
                Debug.LogWarning("MP3 format detected. Using temporary conversion method.");
                
                // Create temporary AudioClip - in production, use proper MP3 decoding
                currentClip = AudioClip.Create("TempClip", 44100 * 5, 1, 44100, false);
            }
            
            if (currentClip == null)
            {
                Debug.LogError("Failed to create AudioClip from data");
                yield break;
            }
            
            // Set clip on audio source
            audioSource.clip = currentClip;
            playbackDuration = currentClip.length;
            
            // Start playback
            audioSource.Play();
            isPlaying = true;
            playbackPosition = 0f;
            
            // Notify playback started
            OnPlaybackStarted?.Invoke();
            
            Debug.Log($"Started audio playback (Duration: {playbackDuration:F2}s)");
        }
        
        /// <summary>
        /// Stops the current audio playback.
        /// </summary>
        public void StopPlayback()
        {
            if (!isPlaying) return;
            
            // Stop audio source
            audioSource.Stop();
            isPlaying = false;
            
            // Clear references
            audioSource.clip = null;
            currentClip = null;
            
            Debug.Log("Stopped audio playback");
        }
        
        /// <summary>
        /// Creates an AudioClip from WAV data.
        /// </summary>
        private AudioClip CreateClipFromWav(byte[] wavData)
        {
            // Check for valid WAV format
            if (!IsWavFile(wavData))
            {
                Debug.LogError("Invalid WAV format");
                return null;
            }
            
            try
            {
                // Parse WAV header
                int channels = wavData[22];
                int sampleRate = BitConverter.ToInt32(wavData, 24);
                int bitsPerSample = wavData[34];
                
                // Find data chunk
                int dataIndex = FindDataChunk(wavData);
                if (dataIndex < 0)
                {
                    Debug.LogError("Could not find data chunk in WAV file");
                    return null;
                }
                
                // Get data size
                int dataSize = BitConverter.ToInt32(wavData, dataIndex + 4);
                int dataStart = dataIndex + 8;
                
                // Calculate number of samples
                int bytesPerSample = bitsPerSample / 8;
                int sampleCount = dataSize / (channels * bytesPerSample);
                
                // Create audio clip
                AudioClip clip = AudioClip.Create("Response", sampleCount, channels, sampleRate, false);
                
                // Convert WAV data to float samples
                float[] samples = new float[sampleCount * channels];
                for (int i = 0; i < sampleCount * channels; i++)
                {
                    int sampleIndex = dataStart + (i * bytesPerSample);
                    
                    if (sampleIndex + bytesPerSample > wavData.Length)
                    {
                        Debug.LogWarning("WAV data parsing went out of bounds");
                        break;
                    }
                    
                    // Parse sample based on bit depth
                    if (bitsPerSample == 16)
                    {
                        // 16-bit samples
                        short sample = BitConverter.ToInt16(wavData, sampleIndex);
                        samples[i] = sample / 32768f; // Convert to -1.0 to 1.0 range
                    }
                    else if (bitsPerSample == 8)
                    {
                        // 8-bit samples
                        samples[i] = (wavData[sampleIndex] - 128) / 128f;
                    }
                    else
                    {
                        Debug.LogWarning($"Unsupported bit depth: {bitsPerSample}");
                        samples[i] = 0f;
                    }
                }
                
                // Set the samples in the AudioClip
                clip.SetData(samples, 0);
                
                return clip;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating AudioClip from WAV: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Checks if the data is a valid WAV file.
        /// </summary>
        private bool IsWavFile(byte[] data)
        {
            if (data.Length < 44) return false; // WAV header is 44 bytes minimum
            
            // Check RIFF header
            if (data[0] != 'R' || data[1] != 'I' || data[2] != 'F' || data[3] != 'F')
                return false;
            
            // Check WAVE format
            if (data[8] != 'W' || data[9] != 'A' || data[10] != 'V' || data[11] != 'E')
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Finds the position of the data chunk in a WAV file.
        /// </summary>
        private int FindDataChunk(byte[] wavData)
        {
            // Find the 'data' chunk
            for (int i = 12; i < wavData.Length - 4; i++)
            {
                if (wavData[i] == 'd' && wavData[i + 1] == 'a' && wavData[i + 2] == 't' && wavData[i + 3] == 'a')
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        /// <summary>
        /// Analyzes the audio spectrum for lip sync.
        /// </summary>
        private void AnalyzeAudioSpectrum()
        {
            if (audioSource == null || !isPlaying) return;
            
            // Get spectrum data
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);
            
            // Calculate average amplitude in the speech range (300Hz-3kHz)
            // This is a simplified approach - in a real implementation, you might
            // want to use more sophisticated frequency analysis
            float sum = 0f;
            int startBin = 3; // ~300Hz
            int endBin = 30;  // ~3kHz
            
            for (int i = startBin; i < endBin && i < spectrumData.Length; i++)
            {
                sum += spectrumData[i];
            }
            
            currentAmplitude = sum / (endBin - startBin);
        }
        
        /// <summary>
        /// Gets the current audio amplitude for lip sync.
        /// </summary>
        public float GetCurrentAmplitude()
        {
            return currentAmplitude;
        }
        
        /// <summary>
        /// Gets the entire spectrum data for visualization.
        /// </summary>
        public float[] GetSpectrumData()
        {
            return spectrumData;
        }
        
        /// <summary>
        /// Sets the volume for audio playback.
        /// </summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            audioSource.volume = volume;
        }
        
        /// <summary>
        /// Gets the current playback position in seconds.
        /// </summary>
        public float GetPlaybackPosition()
        {
            return playbackPosition;
        }
        
        /// <summary>
        /// Gets the total duration of the current clip in seconds.
        /// </summary>
        public float GetPlaybackDuration()
        {
            return playbackDuration;
        }
        
        /// <summary>
        /// Checks if audio is currently playing.
        /// </summary>
        public bool IsPlaying()
        {
            return isPlaying;
        }
    }
}