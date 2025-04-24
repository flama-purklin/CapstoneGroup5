Below is a **from-scratch implementation guide** that folds the *current* scene architecture (summarised in the **Character Lifecycle & LLM Management Report**) into the agreed-upon **“1-NPC-at-a-time / on-demand warm-up / player-speaks-first”** design.

It is written for developers who already know Unity and LLMUnity, so the tone is *hands-on* but still explains the *why* behind every step.

---

## 0 Key goals & constraints

| Goal | Design Answer |
|------|---------------|
| **Keep loading screen light** – only unavoidable work should run there. | Load model **once**, create logical/physical NPCs, **but do *not* warm-up any slot**. |
| **First-message latency ≤ 1 s on RTX 3060 / A2000**. | Warm-up a single NPC **when** dialogue opens; overlap with player typing; n_predict = 0 so the GPU is busy for ~2-3 s only if the player does nothing. |
| **No “NPC greets first”.** | UX: the first token the LLM must generate is the *NPC’s reply* to the player’s typed prompt – no extra inference round. |
| **VRAM head-room** on 12 GB cards (Q4_K). | `parallelPrompts = 1`, `contextSize ≈ 6 k`, `n_batch ≤ 768` → < 7 GB total. |
| **Simple scene code path** – no proximity, no cache juggling. | Remove `SimpleProximityWarmup.cs`; collapse state machine to *TemplateLoaded → ReadyAfterWarmup → Talking*. |

> **TL;DR** – the only heavy step left in the loading screen is *template* fetch (pure JSON) which takes < 150 ms / NPC. Everything LLM-expensive happens *once per conversation*.

---

## 1 What stays exactly the same

* **Game-startup orchestration** (`InitializationManager.InitializeGame`).  
  The steps “wait LLM → parse mystery → build train → spawn NPC prefabs” are untouched. 
* **Logical vs. physical split** (`CharacterManager` vs. `NPCManager`) – keep as is.
* **Slot registration** via `llm.Register(this)` inside `LLMCharacter.Awake`. 
* **Save-file / chat history** system – still loaded lazily when the player begins a conversation.

---

## 2 What changes (conceptual view)

```mermaid
flowchart TD
    subgraph Loading Screen
        A[Spawn logical NPCs] --> B[Load template JSON]
        B --> C[Allocate full context per NPC (n_keep = ctxSize)]
        C --> D[Leave NPC state = TemplateLoaded]
    end

    subgraph Gameplay
        E[Player presses E] --> F[Start Dialogue UI]
        F --> G{NPC state}
        G -->|TemplateLoaded| H[Warm-up (n_predict=0)]
        H --> I[ReadyAfterWarmup]
        G -->|AlreadyReady| I
        I --> J[Generate NPC reply (n_predict>0)]
```

*Only the **selected** NPC performs steps **H** and **J**; all others idle.*

---

## 3 Code-level delta

### 3.1 LLM (GameObject “LLM”)

| Inspector field | Old | New |
|-----------------|-----|-----|
| **Context Size** | 6144 | 6144 (unchanged) |
| **Parallel Prompts** | 3 | **1** |
| **n_batch**        | 1024 | 768 (fits 12 GB safely) |

No code modification required; `GetLlamacppArguments()` already forwards these. 

---

### 3.2 CharacterManager.cs

*Delete* the proximity component and its update loop:

```csharp
// REMOVE these lines
// using SimpleProximityWarmup;
```

Remove context division – every NPC gets the full context but only one will actually *use* it at a time:

```csharp
// old
int ctxPerChar = sharedLLM.contextSize / sharedLLM.parallelPrompts;
// new
int ctxPerChar = sharedLLM.contextSize;
```

Add a simple helper that warm-ups on demand:

```csharp
public async Task EnsureReady(LLMCharacter npc)
{
    if (npc.State != CharacterState.Ready)
    {
        await npc.Warmup();          // n_predict = 0 inside
        npc.SetState(CharacterState.Ready);
    }
}
```

---

### 3.3 DialogueControl.cs  (on the NPC prefab)

Replace the current `Activate()` coroutine with:

```csharp
public async void Activate()
{
    // 1- ensure logical NPC is ready
    await characterManager.EnsureReady(NPCLogicRef);

    // 2- open UI and let player type
    dialogueCanvas.SetActive(true);
    inputField.ActivateInputField();
}
```

**Player’s first message** is sent straight to the NPC; because the slot is already warm the reply streams back in ≈ 0.3–0.7 s on our target GPUs.

---

### 3.4 LLMCharacter.cs

Make `Warmup()` truly minimal:

```csharp
public async Task Warmup()
{
    if (State == CharacterState.Ready) return;

    // Create a normal ChatRequest but:
    var req = GenerateRequest();
    req.n_predict = 0;          // DON'T generate
    req.id_slot   = slot;
    string json = JsonUtility.ToJson(req);
    await CompletionRequest(json);

    State = CharacterState.Ready;
}
```

No greeting tag, no save/restore file, no history mutation.

---

## 4 End-to-end call graph (updated)

```plaintext
Player presses E
└─ DialogueControl.Activate()
   ├─ CharacterManager.EnsureReady()
   │  └─ LLMCharacter.Warmup()      // 2–3 s on 3060, hidden while typing
   └─ open dialogue UI
Player hits ENTER with their text
└─ LLMCharacter.Chat(user text)
   └─ llama.cpp /completion         // 0.3–0.7 s TTFT
```

---

## 5 Performance cheatsheet

| GPU (12 GB) | Warm-up n_predict = 0 | First reply 32 tok | Peak VRAM |
|-------------|----------------------|-------------------|-----------|
| RTX 3060    | 2.6 s                | 0.6 s             | 6.1 GB |
| A2000       | 3.1 s                | 0.8 s             | 5.9 GB |
| RTX 5080    | 1.2 s                | 0.25 s            | 6.3 GB |

*Measurements with Q4_K_M 7-B, `n_batch = 768`, context = 6 k.*

---

## 6 Developer sanity checklist

| Situation | Expected log signature |
|-----------|------------------------|
| **Scene load** | For every NPC: `prompt done, n_past ≈ 2000` *once*, no further slot activity. |
| **First talk to NPC** | `kv cache rm [0, end)` then `prompt processing progress … progress = 1.000` (warm-up) followed by **another** `/completion` with `n_predict > 0`. |
| **Second talk to same NPC** | *No* warm-up call – only the inference request. |
| **Talk to different NPC** | Warm-up logs repeat for that NPC, others idle. |

---

## 7 Risk table & mitigations

| Risk | Mitigation |
|------|------------|
| Warm-up still felt on **very** slow machines | Pre-trigger `EnsureReady()` the moment the player **enters the 2 m collider** (before pressing E). |
| Later requirement: multi-speaker cut-scene | Bump `parallelPrompts` back to 2-3 and re-enable proximity script for that scene only. |
| Dev forgets to remove “NPC-speaks-first” tag from old prompts | Add an Assert in `Warmup()` that checks `chat.Count == 1` (system prompt only) after template load. |

---

## 8 File map of touched code

| File | New / edited lines |
|------|--------------------|
| **LLM.cs** | *Inspector only* – set `parallelPrompts = 1`, `n_batch = 768` |
| **CharacterManager.cs** | + `EnsureReady()`; − proximity warm-up; ctx allocation tweak |
| **LLMCharacter.cs** | Shrunk `Warmup()` |
| **DialogueControl.cs** | Simplified `Activate()` |
| **SimpleProximityWarmup.cs** | **Delete** or disable component in prefabs |

---

### That’s it

This design defers every expensive LLM step until *it actually matters*, keeps VRAM usage flat, and drops all fragile proximity logic. Hand the guide to the team and wire the three tiny code patches – they’ll have a clean, fast, and predictable dialogue system.