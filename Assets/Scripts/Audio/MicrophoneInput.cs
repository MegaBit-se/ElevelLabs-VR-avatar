using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElevelLabs.VRAvatar.Core;

namespace ElevelLabs.VRAvatar.Audio
{
    /// <summary>
    /// Handles microphone access and continuous voice input.
    /// Includes speech detection and audio processing.
    /// </summary>
    public class MicrophoneInput : MonoBehaviour
    {
        [Header("Recording Settings")]
        [SerializeField] private int sampleRate = 44100;
        [SerializeField] private int recordingLength = 10; // Maximum length in seconds
        [SerializeField] private bool autoStart = true;
        [SerializeField] private string deviceName = null; // null = default device
        
        [Header("Speech Detection")]
        [SerializeField] private bool autoDetectSpeech = true;
        [SerializeField] private float silenceThreshold = 0.05f;
        [SerializeField] private float minSpeechDuration = 0.5f; // Minimum duration to be considered speech (in seconds)
        [SerializeField] private float silenceBeforeEndSpeech = 0.7f; // Silence duration to consider speech ended (in seconds)
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool isRecording = false;
        [SerializeField] private bool isSpeechDetected = false;
        [SerializeField] private float currentAudioLevel = 0f;
        
        // Audio clip for recording
        private AudioClip microphoneClip;
        
        // Speech detection
        private float silenceTimer = 0f;
        private float speechTimer = 0f;
        
        // Audio processing
        private List<float> audioSamples = new List<float>();
        private int lastSamplePosition = 0;
        
        // Events
        public event Action OnSpeechDetected;
        public event Action OnSpeechEnded;
        public event Action<byte[]> OnAudioDataReceived;
        
        /// <summary>
        /// Initializes the microphone input and checks for microphone access.
        /// </summary>
        public IEnumerator Initialize()
        {
            Debug.Log("Initializing MicrophoneInput...");
            
            // Load settings
            if (ConfigManager.Instance != null)
            {
                silenceThreshold = ConfigManager.Instance.Settings.SilenceThreshold;
                autoDetectSpeech = ConfigManager.Instance.Settings.AutoDetectSpeech;
            }
            
            // Check for microphone
            if (Microphone.devices.Length <= 0)
            {
                Debug.LogError("No microphone device detected!");
                yield break;
            }
            
            // Log available microphones
            string availableMics = "Available microphones: ";
            foreach (string device in Microphone.devices)
            {
                availableMics += device + ", ";
            }
            Debug.Log(availableMics);
            
            // Use default device if none specified
            if (string.IsNullOrEmpty(deviceName) && Microphone.devices.Length > 0)
            {
                deviceName = Microphone.devices[0];
                Debug.Log($"Using default microphone: {deviceName}");
            }
            
            // Start recording if auto-start is enabled
            if (autoStart)
            {
                StartRecording();
            }
            
            yield return null;
            Debug.Log("MicrophoneInput initialized");
        }
        
        private void Update()
        {
            if (!isRecording) return;
            
            // Process audio data and detect speech
            ProcessAudioData();
            
            if (autoDetectSpeech)
            {
                DetectSpeech();
            }
        }
        
        /// <summary>
        /// Starts recording from the microphone.
        /// </summary>
        public void StartRecording()
        {
            if (isRecording) return;
            
            try
            {
                // Create a new AudioClip for recording
                microphoneClip = Microphone.Start(deviceName, true, recordingLength, sampleRate);
                
                if (microphoneClip == null)
                {
                    Debug.LogError("Failed to create microphone AudioClip");
                    return;
                }
                
                // Reset state
                lastSamplePosition = 0;
                audioSamples.Clear();
                silenceTimer = 0f;
                speechTimer = 0f;
                isSpeechDetected = false;
                
                isRecording = true;
                Debug.Log($"Started recording from microphone: {deviceName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error starting microphone: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Stops recording from the microphone.
        /// </summary>
        public void StopRecording()
        {
            if (!isRecording) return;
            
            Microphone.End(deviceName);
            isRecording = false;
            isSpeechDetected = false;
            
            Debug.Log("Stopped recording from microphone");
        }
        
        /// <summary>
        /// Pauses microphone processing, used when app is paused.
        /// </summary>
        public void Pause()
        {
            if (!isRecording) return;
            
            // Just stop the microphone, we'll restart it on resume
            Microphone.End(deviceName);
            isRecording = false;
            
            Debug.Log("Paused microphone recording");
        }
        
        /// <summary>
        /// Resumes microphone processing after app pause.
        /// </summary>
        public void Resume()
        {
            if (isRecording) return;
            
            // Restart the microphone
            StartRecording();
            
            Debug.Log("Resumed microphone recording");
        }
        
        /// <summary>
        /// Processes incoming audio data from the microphone.
        /// </summary>
        private void ProcessAudioData()
        {
            if (microphoneClip == null) return;
            
            // Get current position in the audio clip
            int currentPosition = Microphone.GetPosition(deviceName);
            
            // If position hasn't changed, nothing to process
            if (currentPosition == lastSamplePosition) return;
            
            // Calculate how many samples to read
            int samplesToRead;
            if (currentPosition < lastSamplePosition)
            {
                // Wrapped around
                samplesToRead = (microphoneClip.samples - lastSamplePosition) + currentPosition;
            }
            else
            {
                samplesToRead = currentPosition - lastSamplePosition;
            }
            
            // If no samples to read, skip
            if (samplesToRead <= 0) return;
            
            // Read audio samples
            float[] samples = new float[samplesToRead];
            if (lastSamplePosition + samplesToRead <= microphoneClip.samples)
            {
                // Read in one go
                microphoneClip.GetData(samples, lastSamplePosition);
            }
            else
            {
                // Read in two parts due to wrap-around
                int firstPartSize = microphoneClip.samples - lastSamplePosition;
                int secondPartSize = samplesToRead - firstPartSize;
                
                float[] firstPart = new float[firstPartSize];
                float[] secondPart = new float[secondPartSize];
                
                microphoneClip.GetData(firstPart, lastSamplePosition);
                microphoneClip.GetData(secondPart, 0);
                
                // Combine the parts
                Array.Copy(firstPart, 0, samples, 0, firstPartSize);
                Array.Copy(secondPart, 0, samples, firstPartSize, secondPartSize);
            }
            
            // Calculate current audio level for speech detection
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }
            currentAudioLevel = sum / samples.Length;
            
            // Store audio samples if speech is active
            if (isSpeechDetected)
            {
                audioSamples.AddRange(samples);
            }
            
            // Update position for next frame
            lastSamplePosition = currentPosition;
        }
        
        /// <summary>
        /// Detects speech based on audio levels and silence duration.
        /// </summary>
        private void DetectSpeech()
        {
            // Check if audio level exceeds threshold
            bool isSpeaking = currentAudioLevel > silenceThreshold;
            
            if (isSpeaking)
            {
                // Reset silence timer when speech is detected
                silenceTimer = 0f;
                
                if (!isSpeechDetected)
                {
                    // Increment speech timer to ensure it's not just a brief noise
                    speechTimer += Time.deltaTime;
                    
                    // If speech duration exceeds minimum, consider it actual speech
                    if (speechTimer >= minSpeechDuration)
                    {
                        isSpeechDetected = true;
                        audioSamples.Clear(); // Clear any previous samples
                        OnSpeechDetected?.Invoke();
                        
                        if (showDebugInfo)
                        {
                            Debug.Log("Speech detected");
                        }
                    }
                }
            }
            else // Not speaking
            {
                // Reset speech timer if below threshold
                speechTimer = 0f;
                
                // If we were in speech mode, increment silence timer
                if (isSpeechDetected)
                {
                    silenceTimer += Time.deltaTime;
                    
                    // If silence duration exceeds threshold, end speech
                    if (silenceTimer >= silenceBeforeEndSpeech)
                    {
                        isSpeechDetected = false;
                        
                        // Process the collected audio
                        ProcessCollectedAudio();
                        
                        // Reset state
                        silenceTimer = 0f;
                        audioSamples.Clear();
                        
                        OnSpeechEnded?.Invoke();
                        
                        if (showDebugInfo)
                        {
                            Debug.Log("Speech ended");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Processes the collected audio samples and sends them for further processing.
        /// </summary>
        private void ProcessCollectedAudio()
        {
            if (audioSamples.Count == 0) return;
            
            // Convert to 16-bit PCM
            byte[] audioData = ConvertFloatToPCM16(audioSamples.ToArray());
            
            // Create a WAV file with header
            byte[] wavFile = CreateWavFile(audioData, 1, sampleRate);
            
            // Send audio data to listeners
            OnAudioDataReceived?.Invoke(wavFile);
            
            if (showDebugInfo)
            {
                Debug.Log($"Processed audio: {audioSamples.Count} samples, {wavFile.Length} bytes");
            }
        }
        
        /// <summary>
        /// Converts float audio samples to 16-bit PCM format.
        /// </summary>
        private byte[] ConvertFloatToPCM16(float[] samples)
        {
            byte[] pcmData = new byte[samples.Length * 2]; // 16-bit = 2 bytes per sample
            
            for (int i = 0; i < samples.Length; i++)
            {
                // Convert float to 16-bit and clamp to range
                short pcm = (short)(samples[i] * 32767f);
                
                // Write bytes in little-endian format
                pcmData[i * 2] = (byte)(pcm & 0xFF);
                pcmData[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
            }
            
            return pcmData;
        }
        
        /// <summary>
        /// Creates a WAV file with the proper header for the PCM data.
        /// </summary>
        private byte[] CreateWavFile(byte[] pcmData, int channels, int sampleRate)
        {
            // WAV file header is 44 bytes
            byte[] wavFile = new byte[pcmData.Length + 44];
            
            // RIFF header
            byte[] header = CreateWavHeader(pcmData.Length, channels, sampleRate);
            Array.Copy(header, 0, wavFile, 0, 44);
            
            // Audio data
            Array.Copy(pcmData, 0, wavFile, 44, pcmData.Length);
            
            return wavFile;
        }
        
        /// <summary>
        /// Creates a WAV file header.
        /// </summary>
        private byte[] CreateWavHeader(int dataSize, int channels, int sampleRate)
        {
            byte[] header = new byte[44];
            int bitsPerSample = 16;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            
            // RIFF header
            header[0] = (byte)'R';
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            
            // File size - 8 (total file size minus 8 bytes for the RIFF and size fields)
            int fileSize = dataSize + 36; // 36 + data size
            header[4] = (byte)(fileSize & 0xFF);
            header[5] = (byte)((fileSize >> 8) & 0xFF);
            header[6] = (byte)((fileSize >> 16) & 0xFF);
            header[7] = (byte)((fileSize >> 24) & 0xFF);
            
            // WAVE header
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            
            // fmt chunk
            header[12] = (byte)'f';
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            
            // fmt chunk size (16 for PCM)
            header[16] = 16;
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            
            // Audio format (1 for PCM)
            header[20] = 1;
            header[21] = 0;
            
            // Number of channels
            header[22] = (byte)channels;
            header[23] = 0;
            
            // Sample rate
            header[24] = (byte)(sampleRate & 0xFF);
            header[25] = (byte)((sampleRate >> 8) & 0xFF);
            header[26] = (byte)((sampleRate >> 16) & 0xFF);
            header[27] = (byte)((sampleRate >> 24) & 0xFF);
            
            // Byte rate
            header[28] = (byte)(byteRate & 0xFF);
            header[29] = (byte)((byteRate >> 8) & 0xFF);
            header[30] = (byte)((byteRate >> 16) & 0xFF);
            header[31] = (byte)((byteRate >> 24) & 0xFF);
            
            // Block align
            header[32] = (byte)blockAlign;
            header[33] = 0;
            
            // Bits per sample
            header[34] = (byte)bitsPerSample;
            header[35] = 0;
            
            // data chunk
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            
            // data size
            header[40] = (byte)(dataSize & 0xFF);
            header[41] = (byte)((dataSize >> 8) & 0xFF);
            header[42] = (byte)((dataSize >> 16) & 0xFF);
            header[43] = (byte)((dataSize >> 24) & 0xFF);
            
            return header;
        }
        
        /// <summary>
        /// Returns the current audio level for visualization.
        /// </summary>
        public float GetCurrentAudioLevel()
        {
            return currentAudioLevel;
        }
        
        /// <summary>
        /// Returns whether speech is currently detected.
        /// </summary>
        public bool IsSpeechDetected()
        {
            return isSpeechDetected;
        }
        
        /// <summary>
        /// Updates the silence threshold for speech detection.
        /// </summary>
        public void SetSilenceThreshold(float threshold)
        {
            silenceThreshold = Mathf.Clamp01(threshold);
        }
        
        /// <summary>
        /// Toggles automatic speech detection.
        /// </summary>
        public void SetAutoDetectSpeech(bool enabled)
        {
            autoDetectSpeech = enabled;
        }
    }
}