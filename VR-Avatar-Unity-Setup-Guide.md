# VR Conversational Avatar - Unity Setup Guide

This guide outlines the minimum required Unity project configuration to work with the implemented VR conversational avatar components.

## 1. Unity Project Setup

### Required Unity Version
- **Unity 2022.3 LTS** (Long Term Support)
- Unity Hub recommended for installation and management

### New Project Creation
```
1. Open Unity Hub
2. Click "New Project"
3. Select "3D Core" template
4. Set Project name: "ElevelLabs-VR-avatar"
5. Select Unity 2022.3 LTS version
6. Click "Create project"
```

## 2. Required Packages

### Core Packages
| Package | Version | Purpose |
|---------|---------|---------|
| XR Plugin Management | 4.3.3+ | XR device configuration |
| OpenXR Plugin | 1.8.2+ | Cross-platform XR support |
| XR Interaction Toolkit | 2.4.3+ | VR interaction framework |
| TextMeshPro | 3.0.6+ | High-quality text rendering |
| Newtonsoft JSON | 3.2.1+ | JSON serialization for API |

### VR-Specific Packages
| Package | Version | Purpose |
|---------|---------|---------|
| Oculus XR Plugin | 3.3.0+ | Meta Quest support |

### Installation Steps
```
1. Window > Package Manager
2. Click "+" button > "Add package by name"
3. Enter package name and version (e.g., "com.unity.xr.management@4.3.3")
4. Click "Add"
5. Repeat for all required packages
```

## 3. Project Configuration

### XR Configuration
```
1. Edit > Project Settings > XR Plugin Management
   - Install XR Plugin Management if prompted
   - Check "Initialize XR on Startup"
   - Select "OpenXR" and "Oculus" providers in the Android tab

2. Edit > Project Settings > XR Plugin Management > OpenXR
   - Add "Hand Tracking" feature group if needed
   - Enable "Quest Features" for Meta Quest support
```

### Build Settings
```
1. File > Build Settings
   - Switch Platform to Android
   - Set Texture Compression: ASTC

2. Player Settings (from Build Settings)
   - Company Name: Your company
   - Product Name: ElevenLabs VR Avatar
   
   Under Android settings:
   - Minimum API Level: Android 10 (API 29)
   - Target API Level: Android 12 (API 31)
   - Scripting Backend: IL2CPP
   - API Compatibility Level: .NET Standard 2.1
   - Target Architectures: ARM64
```

### Player Settings
```
1. Edit > Project Settings > Player
   
   Under Other Settings:
   - Rendering: Color Space: Linear
   - Config: Scripting Backend: IL2CPP
   - Config: API Compatibility Level: .NET Standard 2.1
   
   Under Publishing Settings:
   - Build: Custom Main Manifest, Gradle & Proguard: Add permissions
     - android.permission.INTERNET
     - android.permission.RECORD_AUDIO
```

### Quality Settings
```
1. Edit > Project Settings > Quality
   - Set default quality level for Android to "Medium"
   
   Under Medium settings:
   - Pixel Light Count: 2
   - Texture Quality: Half Res
   - Anisotropic Textures: Per Texture
   - Anti Aliasing: 2x Multi Sampling
   - Soft Particles: Disabled
   - Realtime Reflection Probes: Disabled
```

## 4. Initial Scene Setup

### Required Scene Structure
```
Hierarchy:
- --- CORE ---
  - SystemManager
    • AppManager.cs
    • ConfigManager.cs
  - ConversationManager
    • ConversationManager.cs
  - ElevenLabsAPI
    • ElevenLabsAPI.cs
  
- --- AUDIO ---
  - MicrophoneInput
    • MicrophoneInput.cs
  - AudioPlayer
    • AudioPlayer.cs
  
- --- AVATAR ---
  - AvatarRoot
    - AvatarModel (placeholder model)
      • AvatarController.cs
      • LipSync.cs
  
- --- XR ---
  - XROrigin
    - Camera Offset
      - Main Camera
    - LeftHand Controller
    - RightHand Controller
  
- --- UI ---
  - Canvas (World Space)
    - ConversationPanel
      • TextMeshPro text element
    - StatusIndicator
  
- --- ENVIRONMENT ---
  - Directional Light
  - Floor (simple plane)
```

### Scene Configuration Steps
```
1. Create > Scene
   - Name it "Main"
   - Save to Assets/Scenes/

2. Create all required hierarchy objects
   - Use empty GameObjects for managers
   - Configure each with appropriate scripts
   
3. Configure XR Origin
   - Add Component > XR > XR Origin
   - Set Tracking Origin Mode: Floor
   
4. Configure Canvas
   - Set Render Mode: World Space
   - Position at (0, 1.7, 1.3) relative to avatar
   - Scale: (0.01, 0.01, 0.01) for readable text in VR
   
5. Add placeholder for avatar
   - Create cube as temporary avatar
   - Position at (0, 0, 1.5) facing player
```

## 5. Streaming Assets Setup

### AppSettings.json Configuration
```
1. Create "StreamingAssets" folder in Assets
2. Create "Settings" folder inside StreamingAssets
3. Create AppSettings.json:

{
  "ElevenLabsApiKey": "",
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

## 6. Script Compilation Configuration

### Assembly Definition Files
For better organization and compilation speed, create Assembly Definition files for each module:

```
1. Create Assets/Scripts/ElevelLabs.VRAvatar.Core.asmdef:
   References:
   - Unity.TextMeshPro
   - Unity.XR.Management
   - Newtonsoft.Json

2. Create Assets/Scripts/ElevelLabs.VRAvatar.API.asmdef:
   References:
   - ElevelLabs.VRAvatar.Core
   - Newtonsoft.Json

3. Create Assets/Scripts/ElevelLabs.VRAvatar.Audio.asmdef:
   References:
   - ElevelLabs.VRAvatar.Core

4. Create Assets/Scripts/ElevelLabs.VRAvatar.Avatar.asmdef:
   References:
   - ElevelLabs.VRAvatar.Core
   - ElevelLabs.VRAvatar.Audio

5. Create Assets/Scripts/ElevelLabs.VRAvatar.UI.asmdef:
   References:
   - ElevelLabs.VRAvatar.Core
   - Unity.TextMeshPro
```

## 7. Testing the Setup

### Quick Validation Test
```
1. Add all created scripts to their appropriate GameObjects
2. Ensure no console errors appear
3. Enter Play mode to verify initialization
   - Check console for initialization logs
   - Verify no script errors
```

### Minimal VR Testing Flow
```
1. Connect Meta Quest via USB
2. Enable developer mode on device
3. Enable USB debugging
4. In Unity:
   - File > Build Settings > Build and Run
5. On headset:
   - Allow USB debugging access
   - Test if scene loads properly
   - Check if XR rig is properly configured
```

## 8. Development Environment

### VS Code Integration
```
1. Edit > Preferences > External Tools
   - Enable "Generate .csproj files for:
     - Registry packages
     - Git packages
     - Local tarball
     - Built-in packages
   
2. Install VS Code Unity Debugger extension
   - In VS Code: Extensions > Search "Unity Debugger"
   - Install "Unity Tools" extension
   
3. Attach VS Code Debugger:
   - In VS Code: Run > Attach to Unity
   - Select your Unity instance
```

## 9. Editor Tools Setup

For easier development, create these editor scripts (when needed):

```
Create Assets/Scripts/Editor/AvatarDebugTools.cs:
- Add buttons to test audio & speech
- Add visualization for microphone input
- Add LipSync testing tools
```

## 10. Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| Missing references in scene | Add prefab references to respective scripts |
| XR not initializing | Check XR Plugin Management configuration |
| Microphone access denied | Ensure proper permissions in Player Settings |
| Scripts not finding each other | Check script execution order in Project Settings |
| Performance issues in VR | Reduce quality settings and optimize rendering |