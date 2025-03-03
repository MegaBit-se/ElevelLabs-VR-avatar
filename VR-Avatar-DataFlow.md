# VR Conversational AI Avatar - Data Flow Diagram

## Main Data Flow

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

## Detailed Process Flows

### 1. Voice Input Processing
1. Microphone captures user's voice
2. Audio is processed and converted to digital format
3. Speech is detected and isolated from background noise
4. Audio data is sent to ConversationManager

### 2. API Communication
1. ConversationManager sends audio to ElevenLabsAPI
2. ElevenLabsAPI converts speech to text
3. Text is processed by Conversational AI
4. Response text is converted to speech
5. Audio response is returned to ConversationManager

### 3. Avatar Response
1. ConversationManager sends audio to AudioPlayer
2. AudioPlayer plays the audio response
3. LipSync analyzes audio for phonemes
4. Avatar's mouth and expressions are animated
5. ConversationUI displays the text transcript