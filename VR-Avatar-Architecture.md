# VR Conversational AI Avatar - Architecture Blueprint

## 1. Project Overview

This document outlines the architecture for a VR conversational AI avatar application built in Unity 2022.3 LTS for Meta Quest 3 (with PCVR via Virtual Desktop). The application will feature:

- A conversational AI avatar powered by ElevenLabs Conversational AI API
- Always-listening voice input for natural interaction
- Minimalist UI for displaying conversation history
- Development primarily through VS Code with minimal Unity Editor work

## 2. Project Structure

```
ElevelLabs-VR-avatar/
├── Assets/
│   ├── Animations/                 # Avatar animations
│   ├── Materials/                  # Materials for avatar and environment
│   ├── Models/                     # 3D models including avatar
│   ├── Prefabs/                    # Reusable game objects
│   ├── Scenes/                     # Unity scenes
│   │   └── Main.unity              # Main application scene
│   ├── Scripts/                    # C# scripts organized by component
│   │   ├── API/                    # API integration scripts
│   │   │   ├── ElevenLabsAPI.cs    # ElevenLabs API client
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
│   │   └── AppSettings.json        # Application settings
│   └── Plugins/                    # Third-party plugins and libraries
├── Packages/                       # Unity packages
│   └── manifest.json               # Package manifest
├── ProjectSettings/                # Unity project settings
└── README.md                       # Project documentation
```

## 3. Component Architecture

### 3.1 Class Diagram

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   AppManager    │◄────►│ConversationManager◄────►│  ElevenLabsAPI  │
└────────┬────────┘      └────────┬────────┘      └─────────────────┘
         │                        │                        ▲
         ▼                        ▼                        │
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│AvatarController │◄────►│  ConversationUI │      │ MicrophoneInput │
└────────┬────────┘      └─────────────────┘      └────────┬────────┘
         │                                                  │
         ▼                                                  │
┌─────────────────┐                                         │
│     LipSync     │◄────────────────────────────────────────┘
└─────────────────┘
```

### 3.2 Component Descriptions

#### 3.2.1 Core Components

**AppManager**
- Central coordinator for the application
- Initializes all other components
- Manages application lifecycle
- Handles scene transitions and state management

**ConversationManager**
- Manages the conversation state and flow
- Processes user input from MicrophoneInput
- Coordinates with ElevenLabsAPI to get responses
- Updates conversation history
- Notifies UI and Avatar components of changes

**ConfigManager**
- Loads and manages application configuration
- Stores API keys and other settings
- Provides access to user preferences

#### 3.2.2 API Integration

**ElevenLabsAPI**
- Handles authentication with ElevenLabs API
- Sends voice input to the Conversational AI API
- Processes API responses
- Downloads and caches audio responses
- Implements error handling and retry logic

**APIModels**
- Data models for API requests and responses
- Serialization/deserialization helpers
- Conversation history model

#### 3.2.3 Audio Processing

**MicrophoneInput**
- Manages microphone access and permissions
- Continuously captures audio input
- Processes audio for speech recognition
- Detects speech start/end for better conversation flow
- Provides processed audio data to ConversationManager

**AudioPlayer**
- Handles playback of avatar responses
- Manages audio queuing and interruption
- Provides audio timing information for lip sync

#### 3.2.4 Avatar Control

**AvatarController**
- Manages the 3D avatar model
- Controls avatar animations and behaviors
- Coordinates with LipSync for mouth movements
- Implements idle behaviors and attention focus

**LipSync**
- Synchronizes avatar mouth movements with audio
- Processes audio data to extract phoneme information
- Maps phonemes to blend shapes or animation parameters

#### 3.2.5 UI System

**ConversationUI**
- Displays conversation history
- Shows transcription of user input
- Indicates when the avatar is "thinking" or speaking
- Provides minimal feedback on system status

**SettingsUI**
- Allows configuration of API keys
- Provides options for avatar and voice settings
- Controls for audio input/output settings

## 4. Data Flow Diagram

```
┌─────────────┐    Audio    ┌─────────────┐   Text    ┌─────────────┐
│  User Voice │───────────►│Microphone   │──────────►│Conversation │
└─────────────┘            │Input        │           │Manager      │
                           └─────────────┘           └──────┬──────┘
                                                            │
                                                            │ API Request
                                                            ▼
┌─────────────┐    Audio    ┌─────────────┐   Response ┌─────────────┐
│  Avatar     │◄───────────│AudioPlayer  │◄───────────│ElevenLabs   │
│  (Speaker)  │            │             │            │API          │
└──────┬──────┘            └─────────────┘            └─────────────┘
       │
       │ Visual Feedback
       ▼
┌─────────────┐    Text     ┌─────────────┐
│  LipSync    │◄───────────│ConversationUI│
│  Animation  │            │(Display)     │
└─────────────┘            └─────────────┘
```

## 5. Required Unity Packages

### 5.1 Core Packages
- **XR Plugin Management** - For VR device support
- **OpenXR Plugin** - For cross-platform VR support
- **XR Interaction Toolkit** - For VR interactions
- **TextMeshPro** - For high-quality text rendering
- **Newtonsoft JSON** - For JSON serialization/deserialization

### 5.2 Recommended Packages
- **Oculus XR Plugin** - For Meta Quest specific features
- **ProBuilder** - For simple environment creation
- **Shader Graph** - For custom avatar materials
- **Unity Recorder** - For testing and debugging

## 6. Implementation Strategy

### 6.1 Development Phases

#### Phase 1: Project Setup and Core Infrastructure
1. Create Unity project with correct settings
2. Import required packages
3. Implement ConfigManager for settings
4. Set up basic scene structure
5. Implement AppManager

#### Phase 2: API Integration
1. Implement ElevenLabsAPI client
2. Create API models and data structures
3. Implement authentication and error handling
4. Test API connectivity and response parsing

#### Phase 3: Audio System
1. Implement MicrophoneInput for voice capture
2. Create AudioPlayer for response playback
3. Test audio recording and playback
4. Implement basic speech detection

#### Phase 4: Conversation Management
1. Implement ConversationManager
2. Create conversation state machine
3. Connect API and audio components
4. Test end-to-end conversation flow

#### Phase 5: Avatar Integration
1. Import and set up avatar model
2. Implement AvatarController
3. Create basic animations
4. Implement LipSync
5. Test avatar response to conversation

#### Phase 6: UI Implementation
1. Design and implement ConversationUI
2. Create SettingsUI
3. Connect UI to conversation system
4. Test user interaction flow

#### Phase 7: VR Integration
1. Configure XR settings
2. Implement VR-specific interactions
3. Optimize for performance
4. Test in VR environment

### 6.2 Dependencies Between Components

- **AppManager** must be implemented first as it initializes other components
- **ConfigManager** is required for ElevenLabsAPI to access API keys
- **MicrophoneInput** and **ElevenLabsAPI** are prerequisites for **ConversationManager**
- **ConversationManager** must be functional before implementing **AvatarController** and **ConversationUI**
- **AudioPlayer** is required for **LipSync** to function properly

## 7. Manual Unity Editor Steps

While most development can be done in VS Code, some operations require the Unity Editor:

### 7.1 Initial Project Setup
1. Create new Unity 2022.3 LTS project
2. Set build target to Android
3. Configure XR Plugin Management for Oculus
4. Import required packages via Package Manager
5. Set up initial scene structure

### 7.2 Avatar Setup
1. Import avatar model (FBX or similar format)
2. Configure avatar rig and animations
3. Set up materials and shaders
4. Configure blend shapes for lip sync

### 7.3 Scene Configuration
1. Position avatar and camera
2. Set up lighting
3. Configure canvas for UI elements
4. Add necessary game objects for managers

### 7.4 Build Configuration
1. Configure player settings for Meta Quest
2. Set up appropriate quality settings
3. Configure input system for VR
4. Set up appropriate permissions (microphone, internet)

## 8. ElevenLabs API Integration Details

### 8.1 Required API Endpoints

- **Speech Recognition** - For converting user speech to text
- **Conversational AI** - For generating contextual responses
- **Text-to-Speech** - For converting responses to audio

### 8.2 Authentication

- API key-based authentication
- Store API key in ConfigManager
- Consider implementing a secure storage solution

### 8.3 Request/Response Flow

1. Capture audio from microphone
2. Send audio to Speech Recognition API
3. Process recognized text through Conversational AI
4. Convert AI response to speech using Text-to-Speech API
5. Play audio response through AudioPlayer
6. Animate avatar using LipSync

## 9. Performance Considerations

- Implement audio buffering to reduce latency
- Consider streaming responses when possible
- Optimize avatar rendering for VR
- Implement quality settings for different hardware capabilities
- Use coroutines for asynchronous operations
- Consider implementing a thread pool for API requests

## 10. Future Enhancements

- Emotion detection and avatar expression matching
- Gesture recognition for more natural interaction
- Environment customization
- Multiple avatar options
- Offline mode with limited functionality
- Voice customization options
- Conversation history saving and loading