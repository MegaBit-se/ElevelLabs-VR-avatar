# VR Conversational Avatar - Retrospective Architecture Documentation

## 1. Project Structure Overview

### Namespace Structure
The project follows a clean, hierarchical namespace pattern to organize code into logical functional areas:

```
ElevelLabs.VRAvatar
├── Core        // Core application framework and system management
├── API         // ElevenLabs API integration
├── Audio       // Voice input and audio output
├── Avatar      // Avatar control and animation
└── UI          // User interface components (not yet implemented)
```

### Implemented Components

#### Core Components
- **ConfigManager**: Handles application configuration, API keys, and settings
- **AppManager**: Central coordinator that initializes and manages all system components
- **ConversationManager**: Manages conversation state and flow between user and AI

#### API Components
- **ElevenLabsAPI**: Client for ElevenLabs services (speech recognition, conversational AI, TTS)
- **APIModels**: Data structures for API requests/responses and serialization helpers

#### Audio Components
- **MicrophoneInput**: Captures and processes voice input, with speech detection
- **AudioPlayer**: Handles avatar response playback and audio analysis for lip sync

#### Avatar Components
- **AvatarController**: Manages the avatar model, animations, and state transitions
- **LipSync**: Synchronizes avatar mouth movements with audio output

### Current Folder Structure
```
Assets/
├── Animations/        // Avatar animations (empty)
├── Materials/         // Materials for avatar/environment (empty)
├── Models/            // 3D models including avatar (empty)
├── Plugins/           // Third-party plugins (empty)
├── Prefabs/           // Reusable game objects (empty)
├── Scenes/            // Unity scenes (empty)
├── Scripts/           // C# scripts organized by component
│   ├── API/           // API integration scripts
│   ├── Audio/         // Audio processing scripts
│   ├── Avatar/        // Avatar control scripts
│   ├── Core/          // Core application scripts
│   └── UI/            // UI scripts (empty)
└── Settings/          // Configuration files (empty)
```

### Folder Structure Recommendations

1. **Asset Organization**:
   - Add an `AppSettings.json` file in the `Settings/` folder for configuration
   - Create a placeholder avatar model in `Models/` folder
   - Add basic materials in `Materials/` folder for testing
   - Create a `Main.unity` scene in the `Scenes/` folder

2. **Editor Extensions**:
   - Add `Scripts/Editor/` folder for custom editor scripts and tools

3. **Resource Management**:
   - Add `Resources/` folder for runtime-loadable assets
   - Consider adding `StreamingAssets/` for platform-specific content

## 2. Component Relationships

### Dependency Diagram
```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   AppManager    │◄────►│ConversationManager◄────►│  ElevenLabsAPI  │
└────────┬────────┘      └────────┬────────┘      └─────────────────┘
         │                        │                        ▲
         ▼                        ▼                        │
┌─────────────────┐              │                ┌─────────────────┐
│ ConfigManager   │              │                │ MicrophoneInput │
└─────────────────┘              │                └────────┬────────┘
         ▲                       │                         │
         │                       ▼                         │
         │               ┌─────────────────┐              │
         └───────────────│ AvatarController│◄─────────────┘
                         └────────┬────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │    LipSync      │◄────── AudioPlayer
                         └─────────────────┘
```

### Key Dependencies

1. **Initialization Flow**:
   - `AppManager` initializes all components in sequence
   - `ConfigManager` is initialized first to provide settings to other components
   - Other components are initialized after configuration is loaded

2. **Runtime Communication**:
   - `MicrophoneInput` captures audio and sends it to `ConversationManager`
   - `ConversationManager` uses `ElevenLabsAPI` to process speech and generate responses
   - `ConversationManager` triggers `AudioPlayer` to play responses
   - `AvatarController` coordinates with `LipSync` to animate the avatar

3. **Event-Based Communication**:
   - Components use C# events to communicate state changes
   - `MicrophoneInput` raises events for speech detection
   - `AudioPlayer` raises events for playback status
   - `AvatarController` receives state updates from other components

## 3. Implementation Gaps Analysis

### Missing Core Components

1. **UI Components** (High Priority):
   - `ConversationUI`: For displaying conversation history and status
   - `SettingsUI`: For configuring API keys and other settings

2. **VR Integration** (Medium Priority):
   - VR-specific interaction components
   - Hand tracking and gesture recognition

3. **Supporting Components** (Medium Priority):
   - Proper resource loading and management
   - Error handling and recovery system

### API Integration Gaps

1. **ElevenLabs API Implementation** (Highest Priority):
   - The current ElevenLabs API client has placeholder methods
   - Need to implement actual HTTP requests to the ElevenLabs endpoints
   - Need to handle authentication, rate limiting, and error recovery
   - Need to implement audio streaming for lower latency

2. **API Validation and Caching** (High Priority):
   - Add API request validation
   - Implement caching for API responses to reduce latency and costs
   - Add offline fallback mechanisms

### Implementation Priority Order

1. **Complete API Integration**:
   - Finalize `ElevenLabsAPI` implementation with actual HTTP requests
   - Test API connectivity and response handling
   - Implement proper error handling and retry logic

2. **UI Implementation**:
   - Create conversation display UI
   - Implement settings interface
   - Add status indicators

3. **Unity Scene Setup**:
   - Configure XR components
   - Set up avatar model and environment
   - Configure lighting and camera

4. **Testing and Optimization**:
   - Performance testing in VR
   - Audio latency optimization
   - Memory usage optimization

## 4. Required Unity Setup

### Project Configuration

1. **Unity Version**:
   - Unity 2022.3 LTS (or newer)

2. **Build Settings**:
   - Platform: Android (for Meta Quest)
   - API Level: Android 10 (API 29) minimum
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64

3. **Player Settings**:
   - XR Plugin Management enabled
   - Required device capabilities:
     - Microphone
     - Internet access

4. **Quality Settings**:
   - Optimize for mobile VR
   - Reduce pixel light count
   - Adjust texture quality based on target device

### Required Packages

1. **Core Packages**:
   - XR Plugin Management
   - OpenXR Plugin
   - XR Interaction Toolkit
   - TextMeshPro (for UI)
   - Newtonsoft JSON (for API serialization)

2. **VR-Specific Packages**:
   - Oculus XR Plugin (for Meta Quest)

3. **Optional Packages**:
   - ProBuilder (for simple environment creation)
   - Shader Graph (for avatar materials)
   - Unity Recorder (for testing and debugging)

### Initial Scene Setup Requirements

1. **Scene Hierarchy**:
   - Core system objects (AppManager, ConfigManager, etc.)
   - XR Rig for player position
   - Avatar model with components attached
   - UI canvas (world space) for conversation display
   - Audio sources properly configured

2. **Avatar Requirements**:
   - Humanoid rig with blend shapes for lip sync
   - Material with proper shader support
   - Animation controller with idle, listening, and speaking states

3. **Environment Setup**:
   - Simple environment for testing
   - Proper lighting for avatar visibility
   - Spatial audio configuration

### Development Tools Configuration

1. **VS Code Integration**:
   - C# extension
   - Unity debugger extension
   - Auto-formatting according to project standards

2. **Testing Framework**:
   - Unity Test Framework for component testing
   - Mock implementations for API testing

---

## 5. Next Steps Recommendation

1. Complete LipSync implementation (was interrupted)
2. Implement ConversationUI and SettingsUI classes
3. Create Main scene with proper XR setup
4. Finalize the ElevenLabs API integration
5. Implement proper error handling across components
6. Set up a test environment for iterative development