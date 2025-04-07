Configured LLM for 9 parallel prompts to handle all characters

Deploy server command: undreamai_server.exe -m "C:/Users/charl/Downloads/Ministral-3b-instruct.Q4_K_M.gguf" -c 40000 -b 1024 --log-disable -np 9 -ngl 32 --template "mistral chat"

MysteryCharacterExtractor initializing with output path: C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets\Characters

Using existing characters directory at: C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets\Characters

MysteryCharacterExtractor initialization complete

Added MysteryCharacterExtractor component as none was assigned

Loading C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets/undreamai-v1.2.3-llamacpp\windows-cuda-cu12.2.0-full/cudart64_12.dll

Found mystery file at: C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets\MysteryStorage\transformed-mystery.json

Loading C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets/undreamai-v1.2.3-llamacpp\windows-cuda-cu12.2.0-full/cublasLt64_12.dll

Loading C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets/undreamai-v1.2.3-llamacpp\windows-cuda-cu12.2.0-full/cublas64_12.dll

Starting character extraction process

Backing up 9 character files...

Backed up 9 character files to C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets\CharacterBackups

Clearing 9 existing character files...

Cleared 9 character files from C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets\Characters

Starting extraction of 9 characters from mystery data

Successfully extracted 9 characters to C:/Users/charl/Documents/capstonecpi411/CapstoneGroup5/Assets/StreamingAssets\Characters

Character extraction complete. 9 characters processed.

Parsing complete - firing OnParsingComplete event

Assigning LLM LLM to LLMUnity.LLMCharacter eleanor_verne

Set prompt for eleanor_verne: You are Eleanor Verne, a Former authenticator with damaged reputation Art authenticator currently wo...

Successfully created character object: eleanor_verne

Removing duplicate AudioListener in LoadingScreen

Set LLM parallelPrompts to 9 to handle all characters

Starting game initialization sequence...

INITIALIZATION STEP 1: Waiting for LLM to start...

Assigning LLM LLM to LLMUnity.LLMCharacter gideon_marsh

Set prompt for gideon_marsh: You are Gideon Marsh, a Private detective with key information Private investigator specializing in ...

Successfully created character object: gideon_marsh

Using architecture: cuda-cu12.2.0-full

LLM service created

Assigning LLM LLM to LLMUnity.LLMCharacter gregory_crowe

Set prompt for gregory_crowe: You are Gregory Crowe, a Mastermind who influenced the murderer Art dealer with a renowned gallery a...

Successfully created character object: gregory_crowe

LLM started successfully in 0.8 seconds

INITIALIZATION STEP 2: Mystery parsing and character extraction

Waiting for mystery parsing and character extraction to complete...

Mystery parsing and character extraction complete in 0.0 seconds

Found 9 character files:

  ✓ eleanor_verne.json - Valid structure

  ✓ gideon_marsh.json - Valid structure

  ✓ gregory_crowe.json - Valid structure

  ✓ maxwell_porter.json - Valid structure

  ✓ mira_sanchez.json - Valid structure

  ✓ nova_winchester.json - Valid structure

Nova's distinctive speech patterns are preserved correctly in nova_winchester.json.

  ✓ penelope_valor.json - Valid structure

  ✓ timmy_seol.json - Valid structure

  ✓ victoria_blackwood.json - Valid structure

Character file validation: 9/9 files have valid structure

INITIALIZATION STEP 3: Character Manager initialization

Initializing character manager...

Initializing NPCs with character data...

Starting NPCManager initialization...

NVIDIA GeForce RTX 5080 GPU Memory: 15.52GB, Offloading 32 GPU Layers for LM: 

Assigning LLM LLM to LLMUnity.LLMCharacter maxwell_porter

Set prompt for maxwell_porter: You are Maxwell Porter, a Paranoid killer manipulated by another Reclusive artist and master forger....

Successfully created character object: maxwell_porter

Assigning LLM LLM to LLMUnity.LLMCharacter mira_sanchez

Set prompt for mira_sanchez: You are Mira Sanchez, a Opportunistic observer with evidence Tabloid journalist known for celebrity ...

Successfully created character object: mira_sanchez

Assigning LLM LLM to LLMUnity.LLMCharacter nova_winchester

Set prompt for nova_winchester: You are Nova Winchester, a Uninterested participant who observed key moments Fashion writer known fo...

Successfully created character object: nova_winchester

Assigning LLM LLM to LLMUnity.LLMCharacter penelope_valor

Set prompt for penelope_valor: You are Penelope Valor, a Rival with strong motive Creative Director at Millennium magazine.
Suspec...

Successfully created character object: penelope_valor

Assigning LLM LLM to LLMUnity.LLMCharacter timmy_seol

Set prompt for timmy_seol: You are Timmy Seol, a Unwitting instigator of events Assistant to Penelope Valor at Millennium magaz...

Successfully created character object: timmy_seol

Assigning LLM LLM to LLMUnity.LLMCharacter victoria_blackwood

Set prompt for victoria_blackwood: You are Victoria Blackwood, a Murder victim staged as suicide Owner and editor-in-chief of Couture E...

Successfully created character object: victoria_blackwood

Successfully created 9 character objects

Loading template for eleanor_verne...

Warming up eleanor_verne...

Successfully initialized eleanor_verne

Loading template for gideon_marsh...

Warming up gideon_marsh...

Successfully initialized gideon_marsh

Loading template for gregory_crowe...

Warming up gregory_crowe...

Successfully initialized gregory_crowe

Loading template for maxwell_porter...

Warming up maxwell_porter...

Successfully initialized maxwell_porter

Loading template for mira_sanchez...

Warming up mira_sanchez...

Successfully initialized mira_sanchez

Loading template for nova_winchester...

Warming up nova_winchester...

Successfully initialized nova_winchester

Loading template for penelope_valor...

Warming up penelope_valor...

Successfully initialized penelope_valor

Loading template for timmy_seol...

Warming up timmy_seol...

Successfully initialized timmy_seol

Loading template for victoria_blackwood...

Warming up victoria_blackwood...

Successfully initialized victoria_blackwood

Attempting to switch to: eleanor_verne

Available characters: eleanor_verne, gideon_marsh, gregory_crowe, maxwell_porter, mira_sanchez, nova_winchester, penelope_valor, timmy_seol, victoria_blackwood

Character eleanor_verne: Slot 0, Context Used: 1

Character gideon_marsh: Slot 1, Context Used: 1

Character gregory_crowe: Slot 2, Context Used: 1

Character maxwell_porter: Slot 3, Context Used: 1

Character mira_sanchez: Slot 4, Context Used: 1

Character nova_winchester: Slot 5, Context Used: 1

Character penelope_valor: Slot 6, Context Used: 1

Character timmy_seol: Slot 7, Context Used: 1

Character victoria_blackwood: Slot 8, Context Used: 1

Cached LLMCharacter for: eleanor_verne

Attempting to switch to: gideon_marsh

Cached LLMCharacter for: gideon_marsh

Attempting to switch to: gregory_crowe

Cached LLMCharacter for: gregory_crowe

Attempting to switch to: maxwell_porter

Cached LLMCharacter for: maxwell_porter

Attempting to switch to: mira_sanchez

Cached LLMCharacter for: mira_sanchez

Attempting to switch to: nova_winchester

Cached LLMCharacter for: nova_winchester

Attempting to switch to: penelope_valor

Cached LLMCharacter for: penelope_valor

Attempting to switch to: timmy_seol

Cached LLMCharacter for: timmy_seol

Attempting to switch to: victoria_blackwood

Cached LLMCharacter for: victoria_blackwood

NPCManager initialization complete

NPC initialization complete

Character initialization complete in 8.0 seconds

INITIALIZATION STEP 4: Loading main game scene

Main scene load requested

There can be only one active Event System.
UnityEngine.EventSystems.EventSystem:OnEnable () (at ./Library/PackageCache/com.unity.ugui/Runtime/UGUI/EventSystem/EventSystem.cs:452)

Mystery Enabled

Removing duplicate EventSystem in SystemsTest

Removing duplicate AudioListener in SystemsTest

Mystery Disabled

Visual Node fact-murder Updated

Visual Node fact-forgery-ring Updated

Visual Node fact-magazine-trouble Updated

Visual Node evidence-suicide-note Updated

Visual Node evidence-sedative Updated

Visual Node evidence-mira-photos Updated

Visual Node evidence-financial-records Updated

Visual Node evidence-incinerator-ash Updated

Visual Node testimony-two-men Updated

Visual Node testimony-maxwell-artistic-ability Updated

Visual Node testimony-gregory-forgery-interest Updated

Visual Node testimony-victoria-penelope-argument Updated

Visual Node testimony-victoria-threats Updated

Visual Node testimony-timmy-hired-gideon Updated

Visual Node testimony-gideon-role Updated

Visual Node lead-suicide-question Updated

Visual Node lead-two-men Updated

Visual Node lead-forgery-connection Updated

Visual Node lead-victoria-desperation Updated

Visual Node lead-investigator-purpose Updated

Visual Node barrier-camera Updated

Visual Node deduction-forged-note Updated

Visual Node deduction-maxwell-paranoia Updated

Visual Node revelation-forgery-ring Updated

Visual Node revelation-victoria-desperate Updated

Visual Node revelation-murder-solution Updated

26 visual nodes created

MysteryGenRunner: Test generator is disabled. Using only the standard mystery pipeline.

=== ALL CHARACTER DEMO MODE ACTIVATED - Global character spawning enabled ===

Found existing Characters container in SystemsTest

Found 2 valid train cars for character spawning

Reset used characters list

Pre-marked car Train Car (diningCarDataManual) Variant(Clone) as visited before global spawning

Pre-marked car Train Car (passengerCarDataManual) Variant(Clone) as visited before global spawning

ALL CHARACTER DEMO: Triggering global character spawning from AllCharacterDemo

Distributing 9 characters across 2 train cars

Fallback to GameObject name: npcSprite

Character 'npcSprite' assigned animation set #1 (based on name hash)

Created NPC GameObject with name: NPC_eleanor_verne

Assigning LLM LLM to LLMUnity.LLMCharacter LLMCharacter

Set parent GameObject name to NPC_eleanor_verne

Character eleanor_verne initialized successfully with LLMCharacter

Initialized Character component with name: eleanor_verne

Placed eleanor_verne on NavMesh at (-2.47, 0.25, -2.70)

Successfully spawned NPC eleanor_verne at position (-2.95, 0.10, -2.70)

Successfully spawned eleanor_verne in Train Car (diningCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_gideon_marsh

Set parent GameObject name to NPC_gideon_marsh

Character gideon_marsh initialized successfully with LLMCharacter

Initialized Character component with name: gideon_marsh

Placed gideon_marsh on NavMesh at (-7.59, 0.96, 2.12)

Successfully spawned NPC gideon_marsh at position (-7.59, 0.10, 2.12)

Successfully spawned gideon_marsh in Train Car (diningCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_gregory_crowe

Set parent GameObject name to NPC_gregory_crowe

Character gregory_crowe initialized successfully with LLMCharacter

Initialized Character component with name: gregory_crowe

Placed gregory_crowe on NavMesh at (-4.25, 0.99, 1.12)

Successfully spawned NPC gregory_crowe at position (-4.25, 0.10, 1.12)

Successfully spawned gregory_crowe in Train Car (diningCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_maxwell_porter

Set parent GameObject name to NPC_maxwell_porter

Character maxwell_porter initialized successfully with LLMCharacter

Initialized Character component with name: maxwell_porter

Placed maxwell_porter on NavMesh at (0.34, 0.92, 2.17)

Successfully spawned NPC maxwell_porter at position (0.34, 0.10, 2.17)

Successfully spawned maxwell_porter in Train Car (diningCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_mira_sanchez

Set parent GameObject name to NPC_mira_sanchez

Character mira_sanchez initialized successfully with LLMCharacter

Initialized Character component with name: mira_sanchez

Placed mira_sanchez on NavMesh at (-10.78, 0.96, -2.07)

Successfully spawned NPC mira_sanchez at position (-10.78, 0.10, -2.07)

Successfully spawned mira_sanchez in Train Car (diningCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_nova_winchester

Set parent GameObject name to NPC_nova_winchester

Character nova_winchester initialized successfully with LLMCharacter

Initialized Character component with name: nova_winchester

Placed nova_winchester on NavMesh at (-22.97, 0.25, -2.53)

Successfully spawned NPC nova_winchester at position (-22.97, 0.10, -2.65)

Successfully spawned nova_winchester in Train Car (passengerCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_penelope_valor

Set parent GameObject name to NPC_penelope_valor

Character penelope_valor initialized successfully with LLMCharacter

Initialized Character component with name: penelope_valor

Placed penelope_valor on NavMesh at (-35.28, 0.25, 2.37)

Successfully spawned NPC penelope_valor at position (-35.28, 0.10, 2.37)

Successfully spawned penelope_valor in Train Car (passengerCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_timmy_seol

Set parent GameObject name to NPC_timmy_seol

Character timmy_seol initialized successfully with LLMCharacter

Initialized Character component with name: timmy_seol

Placed timmy_seol on NavMesh at (-30.97, 0.43, 1.34)

Successfully spawned NPC timmy_seol at position (-31.31, 0.10, 0.86)

Successfully spawned timmy_seol in Train Car (passengerCarDataManual) Variant(Clone)

Created NPC GameObject with name: NPC_victoria_blackwood

Set parent GameObject name to NPC_victoria_blackwood

Character victoria_blackwood initialized successfully with LLMCharacter

Initialized Character component with name: victoria_blackwood

Placed victoria_blackwood on NavMesh at (-29.06, 0.25, -2.44)

Successfully spawned NPC victoria_blackwood at position (-29.47, 0.10, -2.85)

Successfully spawned victoria_blackwood in Train Car (passengerCarDataManual) Variant(Clone)

GLOBAL CHARACTER SPAWN: completed, 9 characters distributed

BoxCollider does not support negative scale or size.
The effective box size has been forced positive and is likely to give unexpected collision geometry.
If you absolutely need to use negative scaling you can use the convex MeshCollider. Scene hierarchy path "TrainManager/Rail Cars/Train Car (diningCarDataManual) Variant(Clone)/RailCarPartitionWall (1)"

BoxCollider does not support negative scale or size.
The effective box size has been forced positive and is likely to give unexpected collision geometry.
If you absolutely need to use negative scaling you can use the convex MeshCollider. Scene hierarchy path "TrainManager/Rail Cars/Train Car (diningCarDataManual) Variant(Clone)/RailCarPartitionWall (1)"

BoxCollider does not support negative scale or size.
The effective box size has been forced positive and is likely to give unexpected collision geometry.
If you absolutely need to use negative scaling you can use the convex MeshCollider. Scene hierarchy path "TrainManager/Rail Cars/Train Car (passengerCarDataManual) Variant(Clone)/RailCarPartitionWall (3)"

BoxCollider does not support negative scale or size.
The effective box size has been forced positive and is likely to give unexpected collision geometry.
If you absolutely need to use negative scaling you can use the convex MeshCollider. Scene hierarchy path "TrainManager/Rail Cars/Train Car (passengerCarDataManual) Variant(Clone)/RailCarPartitionWall (3)"

TargetParameterCountException: Number of parameters specified does not match the expected number.
System.Reflection.RuntimeMethodInfo.ConvertValues (System.Reflection.Binder binder, System.Object[] args, System.Reflection.ParameterInfo[] pinfo, System.Globalization.CultureInfo culture, System.Reflection.BindingFlags invokeAttr) (at <ed969b0e627d471da4848289f9c322df>:0)
System.Reflection.RuntimeMethodInfo.Invoke (System.Object obj, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) (at <ed969b0e627d471da4848289f9c322df>:0)
System.Reflection.MethodBase.Invoke (System.Object obj, System.Object[] parameters) (at <ed969b0e627d471da4848289f9c322df>:0)
NPCManager+<DelayedAnimContainerAssign>d__12.MoveNext () (at Assets/Scripts/NPCs/NPCManager.cs:183)
UnityEngine.SetupCoroutine.InvokeMoveNext (System.Collections.IEnumerator enumerator, System.IntPtr returnValueAddress) (at <84f0d810adef4e6c8deab33e4ae93f7c>:0)

Could not find valid NavMesh position for NPC_mira_sanchez
UnityEngine.Debug:LogWarning (object)
NPCMovement/<MovementState>d__16:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:180)
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
NPCMovement/<IdleState>d__15:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:116)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Could not find valid NavMesh position for NPC_penelope_valor
UnityEngine.Debug:LogWarning (object)
NPCMovement/<MovementState>d__16:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:180)
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
NPCMovement/<IdleState>d__15:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:116)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Character Check Results:

- Characters active in scene: 9

- Cars with characters: 2/2

- Total characters in all cars: 9

- Character names: eleanor_verne, penelope_valor, nova_winchester, timmy_seol, mira_sanchez, maxwell_porter, gideon_marsh, victoria_blackwood, gregory_crowe

- Available characters in NPCManager: 9

- Names: eleanor_verne, gideon_marsh, gregory_crowe, maxwell_porter, mira_sanchez, nova_winchester, penelope_valor, timmy_seol, victoria_blackwood

- Character name issues detected: 8
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:327)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * eleanor_verne (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * penelope_valor (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * nova_winchester (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * timmy_seol (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * mira_sanchez (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * maxwell_porter (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * gideon_marsh (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * gregory_crowe (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Found 9 NPCAnimManager components

Sprite Winchester1_0 is used by multiple characters: NPC_gregory_crowe, NPC_nova_winchester, NPC_penelope_valor, NPC_victoria_blackwood, NPC_mira_sanchez, NPC_eleanor_verne
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:InspectCharacterSprites () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:407)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:353)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Sprite Winchester1Walk_0 is used by multiple characters: NPC_timmy_seol, NPC_maxwell_porter
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:InspectCharacterSprites () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:407)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:353)
AllCharacterDemo/<TriggerGlobalSpawning>d__8:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:110)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Could not find valid NavMesh position for NPC_gregory_crowe
UnityEngine.Debug:LogWarning (object)
NPCMovement/<MovementState>d__16:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:180)
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
NPCMovement/<IdleState>d__15:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:116)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Could not find valid NavMesh position for NPC_timmy_seol
UnityEngine.Debug:LogWarning (object)
NPCMovement/<MovementState>d__16:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:180)
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
NPCMovement/<IdleState>d__15:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:116)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

- Character name issues detected: 8
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:327)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * eleanor_verne (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * penelope_valor (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * nova_winchester (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * timmy_seol (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * mira_sanchez (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * maxwell_porter (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * gideon_marsh (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

  * gregory_crowe (parent name mismatch: NPC_victoria_blackwood)
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:330)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Sprite Winchester1_0 is used by multiple characters: NPC_gregory_crowe, NPC_timmy_seol
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:InspectCharacterSprites () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:407)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:353)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Sprite Winchester1Back_0 is used by multiple characters: NPC_penelope_valor, NPC_mira_sanchez
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:InspectCharacterSprites () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:407)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:353)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Sprite Winchester1BackWalk_0 is used by multiple characters: NPC_victoria_blackwood, NPC_gideon_marsh, NPC_maxwell_porter
UnityEngine.Debug:LogWarning (object)
AllCharacterDemo:InspectCharacterSprites () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:407)
AllCharacterDemo:CheckCharacterStatus () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:353)
AllCharacterDemo/<CheckCharactersAfterDelay>d__12:MoveNext () (at Assets/Mystery/Myst_Play/Dialogue/LLM/AllCharacterDemo.cs:202)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

Could not find valid NavMesh position for NPC_victoria_blackwood
UnityEngine.Debug:LogWarning (object)
NPCMovement/<MovementState>d__16:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:180)
UnityEngine.MonoBehaviour:StartCoroutine (System.Collections.IEnumerator)
NPCMovement/<IdleState>d__15:MoveNext () (at Assets/Scripts/NPCs/NPCMovement.cs:116)
UnityEngine.SetupCoroutine:InvokeMoveNext (System.Collections.IEnumerator,intptr)

