# ElevenLabs VR Avatar - Manual Testing Checklist (MVP)

This document provides a structured testing approach for verifying the core functionality of the ElevenLabs VR Avatar MVP. Use this checklist during development and before releases to ensure essential features are working correctly.

## Basic Functionality Tests

### System Initialization
- [ ] Project loads without errors in Unity Editor
- [ ] All required components initialize properly
- [ ] API connection can be established with ElevenLabs
- [ ] Avatar model loads and displays correctly
- [ ] No console errors during startup

### API Configuration
- [ ] API key can be successfully loaded from settings
- [ ] API key validation works as expected
- [ ] Voice ID is correctly configured and applied
- [ ] Default conversation settings are properly loaded

### Input Systems
- [ ] Microphone can be successfully initialized
- [ ] Microphone input is correctly detected
- [ ] Speech detection thresholds function appropriately
- [ ] Speech-to-text conversion works accurately

### Output Systems
- [ ] Text-to-speech conversion functions correctly
- [ ] Audio playback works with proper volume
- [ ] LipSync matches audio output
- [ ] Avatar animations trigger appropriately

## Conversation Flow Testing

### Basic Conversation
- [ ] User speech is accurately captured
- [ ] Speech is correctly converted to text
- [ ] AI response is generated within reasonable time
- [ ] Response is properly converted to speech
- [ ] Avatar responds with appropriate animations
- [ ] Conversation feels natural and interactive

### Conversation States
- [ ] Idle state is correctly displayed
- [ ] Listening state activates when speaking
- [ ] Processing state shows during API requests
- [ ] Responding state activates during speech playback
- [ ] Transitions between states are smooth

### Conversation History
- [ ] Conversation entries are properly recorded
- [ ] History maintains the correct number of entries
- [ ] System correctly manages conversation context
- [ ] Token usage is accurately tracked

## Error Handling Verification

### API Error Management
- [ ] Rate limit errors are properly detected
- [ ] Error messages are user-friendly
- [ ] Retry logic works with exponential backoff
- [ ] Cooldown periods activate appropriately

### Connection Issues
- [ ] System gracefully handles network interruptions
- [ ] Appropriate error messages display on connection failure
- [ ] Recovery attempts are made when possible
- [ ] System returns to functional state after connection restored

### Input/Output Errors
- [ ] Microphone errors are properly detected and reported
- [ ] Audio playback failures are handled gracefully
- [ ] Empty or invalid responses are managed appropriately
- [ ] System remains stable during error conditions

## Performance Testing

### Resource Usage
- [ ] CPU usage remains at acceptable levels
- [ ] Memory usage is stable over time
- [ ] No memory leaks during extended usage
- [ ] Performance remains consistent in VR mode

### VR Performance
- [ ] Frame rate remains stable (72+ FPS for Quest 3)
- [ ] No stuttering during conversation processing
- [ ] Avatar animations run smoothly
- [ ] Audio remains synchronized with visuals

### Response Times
- [ ] Speech recognition completes in < 2 seconds
- [ ] AI response generation completes in < 3 seconds
- [ ] Text-to-speech conversion completes in < 2 seconds
- [ ] Total response time feels natural (< 7 seconds)

## Troubleshooting Guide

### Common Issues and Solutions

#### API Connection Issues
- Verify internet connection
- Check API key validity in AppSettings.json
- Ensure firewall is not blocking connections
- Verify ElevenLabs service status

#### Microphone Problems
- Check microphone permissions
- Verify correct microphone is selected
- Adjust microphone sensitivity
- Restart the application

#### Avatar Response Issues
- Check ConversationManager connections
- Verify AudioPlayer is properly configured
- Check LipSync component settings
- Restart the conversation

#### Performance Problems
- Check CPU/GPU usage with Task Manager
- Close other resource-intensive applications
- Verify Quest has good battery level
- Check Virtual Desktop settings for optimal performance

## Meta Quest 3 PCVR Verification

### Virtual Desktop Setup
- [ ] Application launches correctly through Virtual Desktop
- [ ] VR tracking works properly
- [ ] Audio passes through to headset correctly
- [ ] Performance remains stable in PCVR mode

### Interaction Verification
- [ ] User position is tracked correctly
- [ ] Avatar maintains eye contact appropriately
- [ ] Spatial audio works correctly
- [ ] Overall experience feels immersive

---

**Testing Notes:**

- Complete all high-priority items (Basic Functionality, Conversation Flow, Error Handling)
- Document any failures or issues with specific steps to reproduce
- Include screenshots or recordings of issues when possible
- Prioritize fixing critical path issues that prevent core functionality