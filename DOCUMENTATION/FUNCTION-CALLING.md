## **One-Way Function Calling Guide: Ministral-8B & LLMUnity**

This guide provides a practical overview for implementing **one-way function calling** in Unity using the **LLMUnity** plugin with the **Ministral-8B-Instruct** language model. One-way means the LLM signals an action, your game performs it, but the LLM doesn't get feedback on the action's result \[cite: Unity Function Calling Guide\_.txt\].

### **1\. Core Concept: Instruction-Based Signaling**

Instead of having innate function-calling abilities, models like Ministral-8B-Instruct follow instructions given in their prompt \[cite: 36-42\]. We leverage this by:

1. **Instructing:** Telling the LLM *when* and *how* to signal that a specific game function should be called.  
2. **Formatting:** Defining a clear, specific format for the LLM to use when signaling a function call, separate from its regular dialogue.  
3. **Parsing:** Writing code in Unity (C\#) to detect this signal in the LLM's response, extract the necessary information (function name, parameters), and trigger the corresponding game logic.

**The Flow:**

\[Game (LLMUnity)\] \--\> Sends Prompt (Instructions \+ History \+ Player Input) \--\> \[Ministral-8B\]  
\[Ministral-8B\] \--\> Generates Response (Dialogue \+ Optional Function Signal) \--\> \[Game (LLMUnity)\]  
\[Game (C\# Script)\] \--\> Parses Response \--\> Separates Dialogue & Signal \--\> Executes Game Function (if signaled)

### **2\. Ministral-8B-Instruct Considerations**

* **Instruction Following:** This model is designed to follow instructions well, which is key for this technique \[cite: Guide to Tool Usage (Function Calling) with Ministral-8B-Instruct-2410.txt\]. However, its ability to perfectly adhere to complex, multi-part instructions (like generating dialogue *and* specific JSON/formatted text) might be less consistent than larger models \[cite: 40-42\].  
* **Prompt Sensitivity:** Clear, unambiguous instructions are crucial. The quality of your prompt directly impacts the reliability of function triggering and formatting \[cite: 133-135\].  
* **Formatting Reliability:** While you can instruct it to use JSON, simpler custom formats (like ACTION: function\_name(param=value)) might be more reliably generated alongside dialogue \[cite: Function calling · Issue \#130 · undreamai/LLMUnity \- GitHub\]. Experimentation is needed.  
* **Grammar Constraints (Advanced):** LLMUnity leverages llama.cpp which often supports grammar files (like GBNF). If LLMUnity exposes this, defining a grammar to force the output into your exact desired function call format can significantly improve reliability over relying solely on prompt instructions \[cite: Function calling · Issue \#130 · undreamai/LLMUnity \- GitHub\]. Check LLMUnity documentation/samples for grammar support.

### **3\. Implementation Steps (Conceptual)**

#### **3.1. Prompt Engineering (LLMCharacter Component)**

The primary control mechanism is the **System Prompt** you define within the LLMCharacter component's Prompt field in the Unity Inspector.

* **Structure:**  
  * Define the character's role and personality.  
  * Clearly describe available functions/actions, including *when* they should be used (triggering criteria).  
  * Provide explicit instructions on the **exact output format** for signaling a function, including a unique separator.  
* **Example Prompt Snippet (Illustrative):**  
  You are \[Character Name\], a \[description\]. Respond naturally to the player.

  Available Actions:  
  \- stop\_conversation: Use this if the player is rude, the conversation is over, or you feel annoyed.  
  \- disclose\_evidence: Use this ONLY if the player asks about the note AND you trust them. The evidence ID is 'suicide\_note'.

  Output Format Rule:  
  If you use an action, first write your dialogue. Then, on a NEW LINE, write 'ACTION:' followed by the action details.  
  Examples:  
  ACTION: stop\_conversation()  
  ACTION: disclose\_evidence(evidence\_id=suicide\_note)  
  Do NOT add any other text after the ACTION line. If no action is needed, just provide dialogue.

#### **3.2. Parsing the Response (C\#)**

In your C\# script that handles the LLM's reply (likely in the callback function passed to llmCharacter.Chat), you need to parse the raw string response.

* **Logic:**  
  1. Define the exact separator string used in your prompt (e.g., \\nACTION:).  
  2. Search the response string for the separator.  
  3. If found, split the string: the part before is dialogue, the part after is the function call data.  
  4. Parse the function call data string to extract the function name and any parameters based on the format you defined (e.g., using string manipulation or regex).  
  5. Use try-catch blocks for robustness against potential LLM formatting errors \[cite: 218\].  
* **Example Parsing Snippet (Conceptual):**  
  void ProcessLLMResponse(string fullReply)  
  {  
      string dialogueToShow \= fullReply;  
      string functionCallData \= null;  
      string separator \= "\\nACTION:"; // Must match prompt instruction

      int separatorIndex \= fullReply.IndexOf(separator);  
      if (separatorIndex \!= \-1)  
      {  
          dialogueToShow \= fullReply.Substring(0, separatorIndex).Trim();  
          // Extract functionCallData from the part after the separator...  
          functionCallData \= fullReply.Substring(separatorIndex \+ separator.Length).Trim();  
      }

      // Display dialogueToShow in UI...

      if (\!string.IsNullOrEmpty(functionCallData))  
      {  
          ParseAndExecuteFunction(functionCallData);  
      }  
  }

  void ParseAndExecuteFunction(string data)  
  {  
      // Example for "functionName(param=value)"  
      try  
      {  
          // Use string manipulation (IndexOf, Substring) or Regex  
          // to extract functionName and parameters...  
          string functionName \= /\* ... extract name ... \*/;  
          string args \= /\* ... extract arguments ... \*/;

          // Call game logic based on functionName and args  
          // e.g., if (functionName \== "stop\_conversation") StopGameConversation();  
      }  
      catch (System.Exception e)  
      {  
          Debug.LogError($"Failed to parse action: {data} \- {e.Message}");  
      }  
  }

#### **3.3. Triggering Game Logic (C\#)**

Once parsed, map the function name to your actual C\# game logic methods.

* **Mapping:** Use a switch statement or a Dictionary\<string, System.Action\<string\>\> to link the parsed function name to the method that performs the action.  
* **Execution:** Call the relevant C\# method, passing any extracted parameters.  
* **Example Trigger Snippet (Conceptual):**  
  void ExecuteParsedFunction(string functionName, string arguments)  
  {  
      switch (functionName)  
      {  
          case "stop\_conversation":  
              MyGameManager.Instance.EndDialogue();  
              break;  
          case "disclose\_evidence":  
              // Extract evidenceId from arguments string  
              string evidenceId \= ParseEvidenceId(arguments);  
              MyGameState.Instance.MarkEvidenceRevealed(evidenceId);  
              break;  
          default:  
              Debug.LogWarning($"Unknown action: {functionName}");  
              break;  
      }  
  }

### **4\. LLMUnity's Role**

* **Integration:** Provides the LLM and LLMCharacter components to easily load models (like Ministral-8B in .gguf format) and manage basic interaction settings within the Unity Editor \[cite: undreamai/LLMUnity: Create characters in Unity with LLMs\! \- GitHub\].  
* **Interaction:** Offers the llmCharacter.Chat(message, replyCallback, completionCallback) method to send prompts and receive responses asynchronously \[cite: undreamai/LLMUnity: Create characters in Unity with LLMs\! \- GitHub\].  
* **Prompting:** Uses the Prompt field in the LLMCharacter component as the primary place to define the system prompt, including your function calling instructions.  
* **Potential Features:** May offer advanced features like grammar support (GBNF) or specific samples for structured output/function calling (check its documentation and included samples thoroughly\!) \[cite: Function calling · Issue \#130 · undreamai/LLMUnity \- GitHub, CHANGELOG.md \- undreamai/LLMUnity \- GitHub\].

### **5\. Example Functions (stop\_conversation, disclose\_evidence)**

* **stop\_conversation**  
  * **Purpose:** Allows the NPC to end the conversation based on dialogue triggers (annoyance, finality).  
  * **Parameters:** None typically needed.  
  * **Prompt Trigger:** "Use stop\_conversation if the player is insulting or the topic is finished."  
  * **Signal Format:** ACTION: stop\_conversation()  
  * **Game Logic:** Close dialogue UI, potentially disable further interaction temporarily.  
* **disclose\_evidence**  
  * **Purpose:** Signals that the NPC is revealing a specific piece of evidence ('suicide\_note').  
  * **Parameters:** evidence\_id (string, likely always 'suicide\_note' in this case).  
  * **Prompt Trigger:** "Use disclose\_evidence with evidence\_id=suicide\_note ONLY if the player asks about a note AND you trust them."  
  * **Signal Format:** ACTION: disclose\_evidence(evidence\_id=suicide\_note)  
  * **Game Logic:** Update game state (e.g., GameState.isSuicideNoteFound \= true), potentially unlock new dialogue or objectives.

### **6\. Key Takeaways**

* **Prompt is King:** Success hinges on clear, explicit instructions in the LLMCharacter prompt.  
* **Parsing is Crucial:** Implement robust C\# parsing logic to handle the LLM's output reliably, including potential errors.  
* **Keep it Simple:** Especially with models like Ministral-8B, simpler output formats might be more reliable than complex JSON when mixed with dialogue.  
* **Iterate:** Test extensively and refine your prompts and parsing based on observed LLM behavior \[cite: 271-273\].  
* **Check LLMUnity Docs/Samples:** Look for specific guidance or examples related to structured output or function calling within the plugin itself.

This approach provides a flexible way to add agency and game state interaction to your LLM-powered characters in Unity.