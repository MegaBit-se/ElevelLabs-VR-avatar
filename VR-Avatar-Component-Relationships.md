# VR Conversational Avatar - Component Relationships

## Component Interaction Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                            USER                                     │
└───────────────────────────────┬─────────────────────────────────────┘
                                │ Speech
                                ▼
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ MicrophoneInput │──────► ConversationManager───► ElevenLabsAPI     │
└─────────────────┘      └─────────┬───────┘      └────────┬────────┘
                                   │                       │
                                   │                       │ Response
                                   │                       ▼
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│ ConfigManager   │◄─────► AppManager      │      │ AudioPlayer     │
└─────────────────┘      └─────────┬───────┘      └────────┬────────┘
                                   │                       │
                                   │                       │
                                   ▼                       ▼
                          ┌─────────────────┐      ┌─────────────────┐
                          │ AvatarController│◄─────► LipSync         │
                          └─────────────────┘      └─────────────────┘
                                   │
                                   │
                                   ▼
                          ┌─────────────────┐
                          │ [Missing]       │
                          │ ConversationUI  │
                          └─────────────────┘
```

## Data Flow During a Conversation

1. **Speech Input Flow:**
   ```
   User Speech → MicrophoneInput → Audio Data → ConversationManager → ElevenLabsAPI (Speech Recognition)
   ```

2. **AI Processing Flow:**
   ```
   Recognized Text → ElevenLabsAPI (Conversational AI) → Response Text → ElevenLabsAPI (Text-to-Speech) → Audio Data
   ```

3. **Response Output Flow:**
   ```
   Response Audio → AudioPlayer → LipSync → AvatarController → Avatar Animation
   ```

4. **Parallel UI Flow:**
   ```
   ConversationManager → [Missing] ConversationUI → Display to User
   ```

## Component Initialization Sequence

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  AppManager     │────►│  ConfigManager  │────►│  ElevenLabsAPI  │
└────────┬────────┘     └─────────────────┘     └─────────────────┘
         │
         │
         ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│MicrophoneInput  │     │  AudioPlayer    │     │AvatarController │
└─────────────────┘     └─────────────────┘     └────────┬────────┘
                                                         │
                                                         │
                                                         ▼
                                                ┌─────────────────┐
                                                │    LipSync      │
                                                └─────────────────┘
```

## Event-Based Communication

| Component | Events Raised | Listened By |
|-----------|--------------|-------------|
| MicrophoneInput | OnSpeechDetected<br>OnSpeechEnded<br>OnAudioDataReceived | ConversationManager |
| AudioPlayer | OnPlaybackStarted<br>OnPlaybackCompleted<br>OnPlaybackProgress | AvatarController<br>LipSync |
| ConversationManager | OnConversationStateChanged<br>OnUserInputProcessed<br>OnAvatarResponseReceived | AvatarController<br>[Missing] ConversationUI |
| AvatarController | OnAvatarStateChanged | LipSync |
| ElevenLabsAPI | OnApiError | ConversationManager |

## Missing Component Relationships

1. **ConversationUI Component:**
   - Should receive updates from ConversationManager
   - Should display conversation history
   - Should show system status (listening, thinking, speaking)

2. **SettingsUI Component:**
   - Should interact with ConfigManager
   - Should allow updating API keys
   - Should provide audio/avatar settings controls

3. **VR Interaction Components:**
   - Should provide input to AppManager
   - Should handle user gestures and controls
   - Should position UI elements in 3D space

## Implementation Progress and Next Steps

| Component | Implementation Status | Next Steps |
|-----------|----------------------|------------|
| ConfigManager | ✅ Complete | Add validation for settings |
| AppManager | ✅ Complete | Add error recovery mechanisms |
| ConversationManager | ✅ Complete | Connect to UI components |
| ElevenLabsAPI | ⚠️ Placeholder | Implement actual API calls |
| MicrophoneInput | ✅ Complete | Add noise filtering |
| AudioPlayer | ✅ Complete | Improve audio loading efficiency |
| AvatarController | ✅ Complete | Add more animation states |
| LipSync | ⚠️ Incomplete | Complete implementation |
| ConversationUI | ❌ Missing | Create and implement |
| SettingsUI | ❌ Missing | Create and implement |