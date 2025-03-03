# Unity VR Conversational Avatar - Project Structure Blueprint

## 1. Project Overview

This blueprint outlines the structure for a VR conversational AI avatar application with these specifications:

- **Platform:** Unity 2022.3 LTS targeting Meta Quest 3 (with PCVR via Virtual Desktop)
- **Core Functionality:** 
  - Conversational AI avatar using ElevenLabs Conversational AI API
  - Always-listening voice input for natural conversation
  - Minimalist UI displaying conversation history
- **Development Approach:** 
  - Primary development via VS Code
  - Minimal manual Unity Editor work required
  - Code-first approach with clean architecture

## 2. Complete Folder Structure

```
ElevelLabs-VR-avatar/
├── Assets/
│   ├── Animations/                 # Avatar animations and movement sequences
│   ├── Materials/                  # Materials for avatar and environment
│   ├── Models/                     # 3D models including avatar meshes
│   ├── Prefabs/                    # Reusable game objects and components
│   ├── Scenes/                     # Unity scenes
│   │   └── Main.unity              # Main application scene
│   ├── Scripts/                    # C# scripts organized by component
│   │   ├── API/                    # API integration scripts
│   │   │   ├── ElevenLabsAPI.cs    # ElevenLabs API client implementation
│   │   │   └── APIModels.cs        # Data models for API requests/responses
│   │   ├── Audio/                  # Audio processing scripts
│   │   │   ├── MicrophoneInput.cs  # Microphone capture and processing
│   │   │   └── AudioPlayer.cs      # Audio playback for avatar responses
│   │   ├── Avatar/                 # Avatar control scripts
│   │   │   ├── AvatarController.cs # Main avatar controller
│   │   │   └── LipSync.cs          # Lip synchronization with audio
│   │   ├── Core/                   # Core application scripts
│   │   │   ├── AppManager.cs       # Main application manager
│   │   │   ├── ConversationManager.cs # Manages conversation state
│   │   │   └── ConfigManager.cs    # Configuration and settings
│   │   └── UI/                     # UI scripts
│   │       ├── ConversationUI.cs   # UI for displaying conversation
│   │       └── SettingsUI.cs       # Settings interface
│   ├── Settings/                   # Configuration files
│   │   └── AppSettings.json        # Application settings including API keys
│   └── Plugins/                    # Third-party plugins and libraries
├── Packages/                       # Unity packages
│   └── manifest.json               # Package manifest
├── ProjectSettings/                # Unity project settings
└── README.md                       # Project documentation
```

## 3. Required Unity Packages

### 3.1 Core Packages (Essential)

| Package Name | Purpose | Version |
|--------------|---------|---------|
| XR Plugin Management | VR device support and management | 4.3.3+ |
| OpenXR Plugin | Cross-platform VR support | 1.8.2+ |
| XR Interaction Toolkit | VR interactions framework | 2.4.3+ |
| TextMeshPro | High-quality text rendering for UI | 3.0.6+ |
| Newtonsoft JSON | JSON serialization/deserialization for API | 3.2.1+ |

### 3.2 Recommended Packages (Optional but Beneficial)

| Package Name | Purpose | Version |
|--------------|---------|---------|
| Oculus XR Plugin | Meta Quest specific features | 3.3.0+ |
| ProBuilder | Simple environment creation tools | 5.1.1+ |
| Shader Graph | Custom avatar materials | 14.0.8+ |
| Unity Recorder | Testing and debugging | 4.0.1+ |

## 4. Project Configuration Settings

### 4.1 Unity Project Settings

| Setting Category | Configuration |
|------------------|---------------|
| **Build Settings** | Platform: Android<br>Texture Compression: ASTC |
| **Player Settings** | Product Name: ElevenLabs VR Avatar<br>Company Name: [Your Company]<br>Minimum API Level: Android 10 (API 29)<br>Target API Level: Android 12 (API 31)<br>Scripting Backend: IL2CPP<br>API Compatibility Level: .NET Standard 2.1 |
| **XR Plug-in Management** | Initialize XR on Startup: Checked<br>Plug-in Providers:<br>- OpenXR (enabled)<br>- Oculus (enabled) |
| **Quality Settings** | Pixel Light Count: 2<br>Texture Quality: Full Res<br>Anisotropic Textures: Per Texture<br>Anti Aliasing: 4x Multi Sampling<br>Soft Particles: Disabled<br>Realtime Reflection Probes: Disabled |
| **Physics Settings** | Gravity: Default (0, -9.81, 0)<br>Default Solver Iterations: 6<br>Default Solver Velocity Iterations: 1 |
| **Time Settings** | Fixed Timestep: 0.02<br>Maximum Allowed Timestep: 0.33 |
| **Graphics Settings** | Rendering Path: Forward<br>Color Space: Linear |

### 4.2 Required Permissions

- Microphone Access (for voice input)
- Internet Access (for API communication)

## 5. Initial Scene Setup

### 5.1 Main Scene Structure

```
Main Scene Hierarchy
├── --- CORE ---
│   ├── AppManager                  # Contains AppManager.cs script
│   ├── ConversationManager         # Contains ConversationManager.cs
│   └── ConfigManager               # Contains ConfigManager.cs 
├── --- AVATAR ---
│   ├── AvatarRoot                  # Parent for avatar model
│   │   ├── AvatarModel             # Imported avatar 3D model
│   │   ├── AvatarController        # Contains AvatarController.cs
│   │   └── LipSyncController       # Contains LipSync.cs
├── --- AUDIO ---
│   ├── MicrophoneInput             # Contains MicrophoneInput.cs
│   └── AudioPlayer                 # Contains AudioPlayer.cs
├── --- ENVIRONMENT ---
│   ├── Floor                       # Simple floor plane
│   ├── Lighting                    # Scene lighting
│   │   ├── DirectionalLight        # Main directional light
│   │   └── AvatarSpotlight         # Focused light on avatar
├── --- XR ---
│   ├── XRRig                       # XR Rig for player position
│   │   ├── Camera Offset           # Camera offset for VR
│   │   │   └── Main Camera         # Main camera (stereo for VR)
│   │   ├── LeftController          # Left hand controller
│   │   └── RightController         # Right hand controller
└── --- UI ---
    └── Canvas (World Space)        # Main UI canvas
        ├── ConversationPanel       # Shows conversation history
        │   └── ConversationText    # TextMeshPro component
        ├── StatusIndicator         # Shows system status
        └── SettingsPanel           # Settings UI (hidden by default)
```

### 5.2 Default Positioning

- **Avatar:** Positioned at (0, 0, 1.5) facing the player
- **XR Rig:** Positioned at (0, 0, 0) as the player's starting position
- **Conversation UI:** Positioned at (0, 1.7, 1.3) - slightly above avatar's head
- **Environment:** Simple floor plane at (0, -0.1, 0) with scale (10, 1, 10)

### 5.3 Initial Camera Settings

- Field of View: 60 degrees
- Near Clip Plane: 0.01
- Far Clip Plane: 100
- Stereo Convergence: 1
- Occlusion Culling: Enabled

### 5.4 Lighting Configuration

- Rendering Mode: Forward
- Main Light: Directional light with soft shadows
- Light color: Warm white (#FFF4E0)
- Avatar Spotlight: Soft spotlight to highlight avatar
- Ambient Light: Subtle ambient light to prevent dark areas

## 6. Getting Started Steps

1. Create new Unity 2022.3 LTS project
2. Import required packages via Package Manager
3. Configure project settings as specified in Section 4
4. Create folder structure as outlined in Section 2
5. Set up Main scene according to Section 5
6. Create empty C# script files following the structure in Section 2
7. Implement the ConfigManager first to establish configuration framework
8. Test XR setup in editor before proceeding with implementation

Note: Detailed implementation of individual components will be covered in future documentation.