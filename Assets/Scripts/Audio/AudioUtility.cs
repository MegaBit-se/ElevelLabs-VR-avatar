using System;
using System.IO;
using UnityEngine;

namespace ElevelLabs.VRAvatar.Audio
{
    /// <summary>
    /// Provides utility methods for audio processing, format conversion, and analysis.
    /// </summary>
    public static class AudioUtility
    {
        /// <summary>
        /// Audio format for WAV files.
        /// </summary>
        public enum AudioFormat
        {
            Mono8,
            Mono16,
            Stereo8,
            Stereo16
        }
        
        /// <summary>
        /// Converts a Unity AudioClip to a WAV file byte array.
        /// </summary>
        /// <param name="clip">The AudioClip to convert.</param>
        /// <param name="format">The target audio format.</param>
        /// <returns>Byte array containing WAV file data.</returns>
        public static byte[] AudioClipToWav(AudioClip clip, AudioFormat format = AudioFormat.Mono16)
        {
            int sampleRate = clip.frequency;
            int channels = clip.channels;
            int bitsPerSample = (format == AudioFormat.Mono8 || format == AudioFormat.Stereo8) ? 8 : 16;
            
            // Override channels based on format
            if (format == AudioFormat.Mono8 || format == AudioFormat.Mono16)
            {
                channels = 1;
            }
            else
            {
                channels = 2;
            }
            
            // Get audio samples
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            // Convert to mono if needed
            if ((format == AudioFormat.Mono8 || format == AudioFormat.Mono16) && clip.channels > 1)
            {
                samples = ConvertToMono(samples, clip.channels);
            }
            
            // Convert to byte array based on format
            byte[] audioData;
            if (bitsPerSample == 8)
            {
                audioData = ConvertTo8Bit(samples);
            }
            else
            {
                audioData = ConvertTo16Bit(samples);
            }
            
            // Create WAV file header
            byte[] wavHeader = CreateWavHeader(audioData.Length, channels, sampleRate, bitsPerSample);
            
            // Combine header and audio data
            byte[] wavFile = new byte[wavHeader.Length + audioData.Length];
            Buffer.BlockCopy(wavHeader, 0, wavFile, 0, wavHeader.Length);
            Buffer.BlockCopy(audioData, 0, wavFile, wavHeader.Length, audioData.Length);
            
            return wavFile;
        }
        
        /// <summary>
        /// Converts WAV file data to a Unity AudioClip.
        /// </summary>
        /// <param name="wavData">The WAV file data as byte array.</param>
        /// <param name="clipName">The name to give the AudioClip.</param>
        /// <returns>A new AudioClip containing the audio data.</returns>
        public static AudioClip WavToAudioClip(byte[] wavData, string clipName = "AudioClip")
        {
            // Parse WAV header to get format info
            int channels, sampleRate, bitsPerSample;
            int dataStartIndex = ParseWavHeader(wavData, out channels, out sampleRate, out bitsPerSample);
            
            if (dataStartIndex <= 0)
            {
                Debug.LogError("Failed to parse WAV header");
                return null;
            }
            
            // Get audio data (skip header)
            int dataLength = wavData.Length - dataStartIndex;
            byte[] audioData = new byte[dataLength];
            Buffer.BlockCopy(wavData, dataStartIndex, audioData, 0, dataLength);
            
            // Convert to float samples
            float[] samples;
            if (bitsPerSample == 8)
            {
                samples = ConvertFrom8Bit(audioData);
            }
            else
            {
                samples = ConvertFrom16Bit(audioData);
            }
            
            // Create the AudioClip
            AudioClip clip = AudioClip.Create(clipName, samples.Length / channels, channels, sampleRate, false);
            clip.SetData(samples, 0);
            
            return clip;
        }
        
        /// <summary>
        /// Converts a multi-channel audio buffer to mono by averaging channels.
        /// </summary>
        /// <param name="input">Multi-channel audio buffer.</param>
        /// <param name="channels">Number of channels in input.</param>
        /// <returns>Mono audio buffer.</returns>
        public static float[] ConvertToMono(float[] input, int channels)
        {
            int samples = input.Length / channels;
            float[] output = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float sum = 0f;
                for (int channel = 0; channel < channels; channel++)
                {
                    sum += input[i * channels + channel];
                }
                output[i] = sum / channels;
            }
            
            return output;
        }
        
        /// <summary>
        /// Creates a WAV file header.
        /// </summary>
        /// <param name="dataSize">Size of the audio data in bytes.</param>
        /// <param name="channels">Number of audio channels.</param>
        /// <param name="sampleRate">Audio sample rate in Hz.</param>
        /// <param name="bitsPerSample">Bits per sample (8 or 16).</param>
        /// <returns>Byte array containing WAV header.</returns>
        public static byte[] CreateWavHeader(int dataSize, int channels, int sampleRate, int bitsPerSample)
        {
            byte[] header = new byte[44];
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
        /// Parses a WAV file header to extract audio format information.
        /// </summary>
        /// <param name="wavData">The WAV file data as byte array.</param>
        /// <param name="channels">Number of audio channels.</param>
        /// <param name="sampleRate">Audio sample rate in Hz.</param>
        /// <param name="bitsPerSample">Bits per sample (8 or 16).</param>
        /// <returns>Start index of audio data, or -1 if header is invalid.</returns>
        public static int ParseWavHeader(byte[] wavData, out int channels, out int sampleRate, out int bitsPerSample)
        {
            channels = 0;
            sampleRate = 0;
            bitsPerSample = 0;
            
            if (wavData.Length < 44)
            {
                Debug.LogError("WAV data too short to contain valid header");
                return -1;
            }
            
            // Verify RIFF header
            if (wavData[0] != 'R' || wavData[1] != 'I' || wavData[2] != 'F' || wavData[3] != 'F')
            {
                Debug.LogError("Missing RIFF header in WAV data");
                return -1;
            }
            
            // Verify WAVE format
            if (wavData[8] != 'W' || wavData[9] != 'A' || wavData[10] != 'V' || wavData[11] != 'E')
            {
                Debug.LogError("Invalid WAVE format in WAV data");
                return -1;
            }
            
            // Get format info
            channels = wavData[22];
            sampleRate = wavData[24] | (wavData[25] << 8) | (wavData[26] << 16) | (wavData[27] << 24);
            bitsPerSample = wavData[34];
            
            // Find data chunk
            int pos = 12; // Start after RIFF and WAVE
            while (pos < wavData.Length - 8)
            {
                if (wavData[pos] == 'd' && wavData[pos + 1] == 'a' && wavData[pos + 2] == 't' && wavData[pos + 3] == 'a')
                {
                    // Data chunk found, skip chunk header (8 bytes) to get to data
                    return pos + 8;
                }
                
                // Skip this chunk (chunk size is at pos+4)
                int chunkSize = wavData[pos + 4] | (wavData[pos + 5] << 8) | (wavData[pos + 6] << 16) | (wavData[pos + 7] << 24);
                pos += 8 + chunkSize;
            }
            
            Debug.LogError("Data chunk not found in WAV data");
            return -1;
        }
        
        /// <summary>
        /// Converts float audio samples to 8-bit PCM format.
        /// </summary>
        private static byte[] ConvertTo8Bit(float[] samples)
        {
            byte[] output = new byte[samples.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                // Convert to range 0-255
                output[i] = (byte)((samples[i] * 0.5f + 0.5f) * 255f);
            }
            return output;
        }
        
        /// <summary>
        /// Converts float audio samples to 16-bit PCM format.
        /// </summary>
        private static byte[] ConvertTo16Bit(float[] samples)
        {
            byte[] output = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                // Convert to 16-bit range (-32768 to 32767)
                short value = (short)(samples[i] * 32767f);
                
                // Write as little-endian
                output[i * 2] = (byte)(value & 0xFF);
                output[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
            }
            return output;
        }
        
        /// <summary>
        /// Converts 8-bit PCM audio data to float samples.
        /// </summary>
        private static float[] ConvertFrom8Bit(byte[] data)
        {
            float[] output = new float[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                // Convert from range 0-255 to -1.0 to 1.0
                output[i] = (data[i] / 255f) * 2f - 1f;
            }
            return output;
        }
        
        /// <summary>
        /// Converts 16-bit PCM audio data to float samples.
        /// </summary>
        private static float[] ConvertFrom16Bit(byte[] data)
        {
            int samples = data.Length / 2;
            float[] output = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                // Read as little-endian 16-bit
                short value = (short)(data[i * 2] | (data[i * 2 + 1] << 8));
                
                // Convert to float range -1.0 to 1.0
                output[i] = value / 32768f;
            }
            
            return output;
        }
        
        /// <summary>
        /// Calculates the RMS amplitude of an audio buffer.
        /// </summary>
        /// <param name="samples">The audio samples to analyze.</param>
        /// <returns>The RMS amplitude (0.0 to 1.0).</returns>
        public static float CalculateRMS(float[] samples)
        {
            float sum = 0;
            
            for (int i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }
            
            return Mathf.Sqrt(sum / samples.Length);
        }
        
        /// <summary>
        /// Detects if there is significant audio in a sample buffer.
        /// </summary>
        /// <param name="samples">The audio samples to analyze.</param>
        /// <param name="threshold">Threshold for speech detection (0.0 to 1.0).</param>
        /// <returns>True if speech is detected, false otherwise.</returns>
        public static bool DetectSpeech(float[] samples, float threshold)
        {
            float rms = CalculateRMS(samples);
            return rms > threshold;
        }
    }
}