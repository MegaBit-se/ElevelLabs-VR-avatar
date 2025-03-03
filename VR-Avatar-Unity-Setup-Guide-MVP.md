# ElevenLabs VR Avatar - Unity Setup Guide (MVP)

This guide provides essential setup instructions for the ElevenLabs VR Avatar project targeting Meta Quest 3 using PCVR streaming with Virtual Desktop.

## Prerequisites

- Unity 2022.3 LTS or newer
- Meta Quest 3 with Virtual Desktop installed
- ElevenLabs API key (sign up at https://elevenlabs.io)
- Git (for cloning the repository)

## Project Setup

1. **Create or Open the Project**
   - Open the project in Unity
   - Ensure you're using the correct Unity version

2. **Configure for PCVR Development**
   - Go to **Edit > Project Settings > Player**
   - Select the **PC, Mac & Linux** tab
   - Under **XR Plug-in Management**:
     - Install XR Plugin Management if not already installed
     - Enable **OpenXR**
     - In OpenXR Feature Groups, enable **Meta Quest Support**

3. **Install Required Packages**
   - Open **Window > Package Manager**
   - Install the following packages:
     - XR Interaction Toolkit (1.0.0 or newer)
     - OpenXR Plugin
     - TextMeshPro

## Scene Setup

1. **Create a New Scene**
   - Use **File > New Scene** or open the existing scene

2. **Set Up the XR Origin**
   - Right-click in Hierarchy > XR > XR Origin
   - This creates the camera rig with controllers

3. **Add Required Components**
   - Create an empty GameObject named "Managers"
   - Add the following components to it:
     - AppManager
     - ConfigManager
     - ConversationManager
     - **ErrorManager (new)**

4. **Configure the Avatar**
   - Create an empty GameObject named "Avatar"
   - Add the AvatarController component
   - Set up references to:
     - The avatar model
     - LipSync component

5. **Add Audio Components**
   - Create an empty GameObject named "AudioSystem"
   - Add the following components:
     - AudioPlayer
     - MicrophoneInput
   - Configure the AudioSource component with proper spatial settings

## Essential Configuration

1. **API Key Setup**
   - Create the directory: `Assets/StreamingAssets/Settings/`
   - Create a new file: `AppSettings.json` with the content:
   ```json
   {
     "ElevenLabsApiKey": "YOUR_API_KEY_HERE",
     "VoiceId": "default",
     "SpeechRate": 1.0,
     "Stability": 0.5,
     "Similarity": 0.75,
     "MicrophoneSensitivity": 1.0,
     "ResponseVolume": 1.0,
     "AutoDetectSpeech": true,
     "SilenceThreshold": 0.05,
     "DefaultAvatarModel": "default",
     "LipSyncSensitivity": 1.0,
     "EnableIdleAnimations": true,
     "ShowSubtitles": true,
     "MaxConversationHistory": 10,
     "UiTheme": "default"
   }
   ```
   - Replace `YOUR_API_KEY_HERE` with your actual ElevenLabs API key

2. **ErrorManager Integration**
   - Ensure the ErrorManager script is added to the Managers GameObject
   - In the ConversationManager script, add a reference to ErrorManager:
   ```csharp
   [SerializeField] private ErrorManager errorManager;
   ```
   - In the ConversationManager's Initialize method, find the ErrorManager if not assigned:
   ```csharp
   if (errorManager == null) errorManager = FindObjectOfType<ErrorManager>();
   ```

## Component Connections

1. **Wire Up the Components**
   - In the Inspector, connect the components with their required references:
     - ConversationManager needs references to:
       - MicrophoneInput
       - AudioPlayer
       - AvatarController
       - ErrorManager
     - AudioPlayer needs reference to:
       - LipSync (for lip synchronization)

2. **Scene Hierarchy Example**
   ```
   Scene
   ├── XR Origin
   │   ├── Camera Offset
   │   │   └── Main Camera
   │   └── Left/Right Hand Controllers
   ├── Managers
   │   ├── AppManager
   │   ├── ConfigManager
   │   ├── ConversationManager
   │   └── ErrorManager
   ├── Avatar
   │   ├── AvatarController
   │   └── LipSync
   └── AudioSystem
       ├── AudioPlayer
       └── MicrophoneInput
   ```

## Testing Your Setup

1. **Quick Verification**
   - Enter Play mode in the Unity Editor
   - Check console for any initialization errors
   - Verify all managers are properly initialized

2. **Test PCVR with Virtual Desktop**
   - Build the project (**File > Build Settings**)
   - Select PC, Mac & Linux as the platform
   - Build to a folder
   - Run the built executable
   - Connect your Meta Quest 3 to your PC using Virtual Desktop
   - Start the PCVR application through Virtual Desktop

## Troubleshooting Common Issues

- **API Authentication Errors**: Double-check your API key in AppSettings.json
- **Microphone Not Working**: Ensure proper microphone permissions and device selection
- **Avatar Not Responding**: Check the ConversationManager connections and error logs
- **Rate Limit Errors**: The ErrorManager will handle rate limits, but you may need to wait if limits are reached

---

This guide covers the minimal setup required for an MVP version of the VR Avatar with ElevenLabs integration. For more detailed configuration and advanced features, refer to the full documentation.