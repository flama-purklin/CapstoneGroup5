# Dialogue UI Update - Goals and Implementation Plan

## Overview

This document outlines a plan to replace the current dialogue system with a more modern chat-style interface that better supports conversation flow with NPCs. The update aims to improve user experience while maintaining compatibility with existing LLM integration.

## Implementation Tiers

The dialogue UI will be implemented using a tiered approach, starting with a Minimum Viable Product (MVP) and advancing to more sophisticated features with each tier.

### Tier 1: MVP Implementation (Minimal Intrusion)
* Basic chat-style interface with message bubbles
* Chat history visible within the dialogue session
* Simple visuals with native Unity UI components
* Event-based communication with existing dialogue systems
* Toggle mechanism for easy fallback to the original system

### Tier 2: Enhanced Experience
* Basic animations for messages appearing
* Visual polish (shadows, rounded corners on message bubbles)
* Responsive layout that adjusts to message length
* Character portrait/expression indicators

### Tier 3: Refined Interface
* Typing indicators when NPC is responding
* Message delivery states (sent, seen, etc.)
* Sound effects for sending/receiving messages
* Customized input field behaviors

### Tier 4: Advanced Features
* Custom rendering for different message types
* Animated transitions between dialogue states
* Deep integration with other game systems (evidence, clues, etc.)
* Platform-specific optimizations

## Core Components

The dialogue UI update will require the following key components:

1. **ChatDialogueManager**: Core script that manages the chat interface, message display, and communication with LLMDialogueManager
   
2. **MessageBubble**: Component for individual message bubbles, handling text display, sizing, and visual appearance

3. **UI Prefabs**:
   - PlayerMessagePrefab: For player messages (right-aligned, distinct color)
   - NPCMessagePrefab: For NPC responses (left-aligned, different color)

4. **UI Structure**:
   - DialogueWindow: Main container for the chat interface
   - Header: Contains character name and portrait
   - MessageArea: Scrollable area containing message bubbles
   - InputArea: Input field and send button

## Integration Points

The new UI system will connect to the existing dialogue infrastructure at these key points:

1. **LLMDialogueManager**: Add events to notify when player sends message and NPC responds
   
2. **DialogueControl**: Add reference to the chat UI and a toggle to enable/disable it

3. **Character Component**: Connect character data (name, portrait) to the chat UI header

## Design Guidelines

### Visual Style
- Modern, clean interface with clear visual hierarchy
- Distinct color coding for player vs NPC messages
- Proper spacing and padding for readability
- Support for both text and potential rich content (images, evidence items)

### Interaction Design
- Intuitive scrolling behavior for message history
- Clear visual feedback when sending messages
- Appropriate transitions and animations (subtle, not distracting)
- Ensure keyboard and controller support for input

### Technical Considerations
- Use Unity's recommended UI practices (Content Size Fitters, Layout Groups)
- Implement proper anchoring for responsive scaling
- Use TextMeshPro for all text elements
- Consider performance with long conversations (object pooling)
- Maintain backward compatibility option

## Implementation Plan

1. Create core scripts
2. Design and implement UI prefabs
3. Integrate with existing dialogue systems
4. Add toggle for switching between UI modes
5. Test with various conversation scenarios
6. Refine visuals and user experience
7. Implement higher-tier features incrementally

## Future Considerations

- Support for special message types (system notifications, evidence reveals)
- Chat history persistence between game sessions
- Accessibility features (text size options, high contrast mode)
- Mobile-specific UI adjustments if needed

---

**Note**: This document outlines the high-level goals and approach. Implementation details will evolve as development progresses.