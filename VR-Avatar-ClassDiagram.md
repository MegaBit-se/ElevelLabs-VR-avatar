# VR Conversational AI Avatar - Detailed Class Diagram

## Core Components

```
┌───────────────────────────┐
│        AppManager         │
├───────────────────────────┤
│ - conversationManager     │
│ - avatarController        │
│ - configManager           │
│ - uiManager               │
├───────────────────────────┤
│ + Initialize()            │
│ + Update()                │
│ + Shutdown()              │
└───────────┬───────────────┘
            │
            │ manages
            ▼
┌───────────────────────────┐
│    ConversationManager    │
├───────────────────────────┤
│ - elevenLabsAPI           │
│ - microphoneInput         │
│ - audioPlayer             │
│ - conversationHistory     │
│ - isListening             │
├───────────────────────────┤
│ + StartListening()        │
│ + StopListening()         │
│ + ProcessUserInput(audio) │
│ + GetResponse(text)       │
│ + PlayResponse(audio)     │
└─────────┬─────────────────┘
          │
    ┌─────┴─────┐
    │           │
    ▼           ▼
┌───────────────────────────┐ ┌───────────────────────────┐
│      ElevenLabsAPI        │ │     MicrophoneInput       │
├───────────────────────────┤ ├───────────────────────────┤
│ - apiKey                  │ │ - isRecording             │
│ - baseUrl                 │ │ - audioSource             │
│ - httpClient              │ │ - audioClip               │
├───────────────────────────┤ │ - sampleRate              │
│ + Authenticate()          │ ├───────────────────────────┤
│ + RecognizeSpeech(audio)  │ │ + StartRecording()        │
│ + GetAIResponse(text)     │ │ + StopRecording()         │
│ + TextToSpeech(text)      │ │ + GetAudioData()          │
└───────────────────────────┘ │ + DetectSpeech()          │
                              └───────────────────────────┘

┌───────────────────────────┐ ┌───────────────────────────┐
│     AvatarController      │ │       AudioPlayer         │
├───────────────────────────┤ ├───────────────────────────┤
│ - avatarModel             │ │ - audioSource             │
│ - animator                │ │ - audioQueue              │
│ - lipSync                 │ │ - isPlaying               │
│ - currentState            │ ├───────────────────────────┤
├───────────────────────────┤ │ + PlayAudio(clip)         │
│ + Initialize(model)       │ │ + StopAudio()             │
│ + SetState(state)         │ │ + QueueAudio(clip)        │
│ + UpdateLipSync(audio)    │ │ + GetPlaybackPosition()   │
│ + PlayAnimation(name)     │ └───────────────────────────┘
└───────────────────────────┘

┌───────────────────────────┐ ┌───────────────────────────┐
│         LipSync           │ │      ConversationUI       │
├───────────────────────────┤ ├───────────────────────────┤
│ - blendShapes             │ │ - conversationText        │
│ - audioAnalyzer           │ │ - userInputDisplay        │
│ - phonemeMap              │ │ - statusIndicator         │
├───────────────────────────┤ ├───────────────────────────┤
│ + ProcessAudio(audio)     │ │ + DisplayMessage(msg)     │
│ + UpdateBlendShapes()     │ │ + ShowUserInput(text)     │
│ + MapPhonemes(data)       │ │ + SetStatus(status)       │
│ + Reset()                 │ │ + ClearConversation()     │
└───────────────────────────┘ └───────────────────────────┘

┌───────────────────────────┐
│      ConfigManager        │
├───────────────────────────┤
│ - configData              │
│ - configPath              │
├───────────────────────────┤
│ + LoadConfig()            │
│ + SaveConfig()            │
│ + GetSetting(key)         │
│ + SetSetting(key, value)  │
└───────────────────────────┘
```

## Data Models

```
┌───────────────────────────┐
│     ConversationEntry     │
├───────────────────────────┤
│ - id                      │
│ - speaker                 │
│ - text                    │
│ - timestamp               │
│ - audioClipPath           │
└───────────────────────────┘

┌───────────────────────────┐
│      APIRequestModel      │
├───────────────────────────┤
│ - requestId               │
│ - inputText               │
│ - conversationHistory     │
│ - settings                │
└───────────────────────────┘

┌───────────────────────────┐
│     APIResponseModel      │
├───────────────────────────┤
│ - responseId              │
│ - responseText            │
│ - audioUrl                │
│ - emotions                │
└───────────────────────────┘
```

## Interfaces

```
┌───────────────────────────┐
│     IAudioProcessor       │
├───────────────────────────┤
│ + ProcessAudio(audio)     │
│ + GetProcessedData()      │
└───────────────────────────┘

┌───────────────────────────┐
│      IAnimatable          │
├───────────────────────────┤
│ + PlayAnimation(name)     │
│ + StopAnimation(name)     │
│ + SetAnimationParam(p, v) │
└───────────────────────────┘

┌───────────────────────────┐
│    IConversationHandler   │
├───────────────────────────┤
│ + HandleUserInput(input)  │
│ + HandleResponse(response)│
└───────────────────────────┘
```

## Enums and Constants

```
enum AvatarState {
    Idle,
    Listening,
    Thinking,
    Speaking,
    Reacting
}

enum ConversationStatus {
    Ready,
    Listening,
    Processing,
    Responding,
    Error
}

static class APIEndpoints {
    const string SpeechRecognition = "/v1/speech-recognition";
    const string Conversation = "/v1/conversation";
    const string TextToSpeech = "/v1/text-to-speech";
}
```

## Relationships and Dependencies

1. **AppManager** initializes and coordinates all other components
   - Creates instances of ConversationManager, AvatarController, ConfigManager
   - Handles application lifecycle events

2. **ConversationManager** depends on:
   - ElevenLabsAPI for API communication
   - MicrophoneInput for user voice capture
   - AudioPlayer for response playback
   - Maintains conversation state and history

3. **AvatarController** depends on:
   - LipSync for mouth animation
   - Receives events from ConversationManager to trigger animations

4. **LipSync** depends on:
   - AudioPlayer for timing information
   - Processes audio data to drive avatar mouth movements

5. **ConversationUI** depends on:
   - ConversationManager for conversation data
   - Displays conversation history and status

6. **ElevenLabsAPI** depends on:
   - ConfigManager for API keys and settings
   - Makes HTTP requests to ElevenLabs services

7. **MicrophoneInput** implements:
   - IAudioProcessor interface
   - Provides processed audio data to ConversationManager

8. **AudioPlayer** depends on:
   - Unity's AudioSource component
   - Manages audio playback queue