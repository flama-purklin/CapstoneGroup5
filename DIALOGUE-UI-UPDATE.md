# DIALOGUE SYSTEM IMPROVEMENTS (April/May 2025)

## 0. Previous Issues

| Component | Issue |
|------|---------|
| `BaseDialogueManager.HandleReply()` | Text duplication due to appending each LLM chunk instead of replacing. |
| `BeepSpeak` and `BaseDialogueManager` | Conflicting timeout mechanisms causing premature animation termination. |
| `DialogueControl.Activate()` | Deactivating ALL HUD elements, including critical ones needed for node notifications. |
| `BaseDialogueManager` | Function call handling accumulated redundant text across chunks. |

---

## 1. Implemented Improvements

```
                     /- BaseDialogueManager (replaced chunk handling)
LLM ---- Chunks ---<
                     \- BeepSpeak (improved text processing & removed conflicting timeout)
                            |
                            v
                         Dynamic timing
                       (based on text length)
                            |
                            v
                  Selective HUD Management
                  (keep critical UI elements)
```

### 1.1 Text Duplication Fix

```csharp
// OLD - in BaseDialogueManager.HandleReply()
currentResponse.Append(reply);

// NEW - Each chunk contains the complete response so far, not just new content
currentResponse.Clear();
currentResponse.Append(reply);
```

### 1.2 Function Call Handling

```csharp
// OLD - Keeping redundant accumulated text across chunks
if (isAccumulatingAction)
{
    actionBuffer.Append(reply); // Causes "revealreveal_node..." accumulation
}

// NEW - Only keep the most recent/complete version of function calls
if (isAccumulatingAction)
{
    actionBuffer.Clear();
    actionBuffer.Append(reply); // Only the latest chunk is preserved
}
```

### 1.3 Dynamic Timing for Animation

```csharp
// OLD - Fixed timeout regardless of text length
float maxWaitTime = 5.0f;

// NEW - Dynamically calculated based on actual typing speed
float estimatedCharTime = characterSpeed + speedVariance;
float punctuationPauseEstimate = Mathf.Min(textLength * 0.2f, 4.0f);
float calculatedWaitTime = Mathf.Max(3.0f, (textLength * estimatedCharTime) + punctuationPauseEstimate);
float maxWaitTime = Mathf.Min(calculatedWaitTime, 20.0f);
```

### 1.4 Selective HUD Management

```csharp
// OLD - All HUD elements deactivated during dialogue
if (defaultHud) defaultHud.SetActive(false);

// NEW - Only deactivate non-essential HUD elements
if (defaultHud) {
    for (int i = 0; i < defaultHud.transform.childCount; i++) {
        Transform childTransform = defaultHud.transform.GetChild(i);
        GameObject childObject = childTransform.gameObject;
        
        // Skip our critical HUD elements
        bool isNodeUnlockNotif = nodeUnlockNotifHud != null && childObject == nodeUnlockNotifHud;
        bool isPowerControl = powerControlHud != null && childObject == powerControlHud;
        
        if (!isNodeUnlockNotif && !isPowerControl) {
            childObject.SetActive(false);
        } else {
            childObject.SetActive(true);
        }
    }
}
```

### 1.5 Single Timeout System

```csharp
// REMOVED from BeepSpeak.cs - Redundant timeout mechanism that caused premature text animation termination
private IEnumerator EnsureTypingCompletesWithLongDelay()
{
    yield return new WaitForSeconds(8.0f);
    if (typingCoroutine != null)
    {
        // Force completion code...
    }
}
```

---

## 2. Behavior After Improvements

• **Consistent Text Display:** No more duplication or flashing between states thanks to proper chunk handling.

• **Smooth Animation:** BeepSpeak now runs at its configured speed without premature termination.

• **Appropriate Timing:** Wait times before input re-enabling are dynamically calculated based on actual text length and typing speed.

• **Reliable Function Calling:** Node revelations and conversation ending work consistently.

• **Visible Critical UI:** NodeUnlockNotif and PowerControl remain visible during dialogue, ensuring function calls can properly trigger UI feedback.

• **Simplified State Management:** Single timeout system in BaseDialogueManager ensures consistent behavior.

_Total code touched_: **3 files**, ~100 LOC modified.
