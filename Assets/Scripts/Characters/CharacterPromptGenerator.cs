using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LLMUnity
{
    /// <summary>
    /// Generates immersive character prompts optimized for natural roleplay and reliable node revelations.
    /// </summary>
    public static class CharacterPromptGenerator
    {
        /// <summary>
        /// Generates a system prompt for the character based on the MysteryCharacter object and mystery context.
        /// </summary>
        public static string GenerateSystemPrompt(MysteryCharacter characterData, LLMCharacter characterObj, string mysteryContext, string mysteryTitle = "")
        {
            try
            {
                // --- Basic Setup & Null Checks ---
                if (characterData == null || characterData.MindEngine?.Identity == null || characterData.Core?.Involvement == null)
                {
                    Debug.LogError("GenerateSystemPrompt: Critical character data missing!");
                    return null;
                }

                string characterName = characterData.MindEngine.Identity.Name;
                if (string.IsNullOrEmpty(characterName))
                {
                    Debug.LogError("GenerateSystemPrompt: Character name is null or empty!");
                    return null;
                }
                
                characterObj.AIName = characterName;
                var prompt = new StringBuilder();

                // --- Extract Essential Character Data ---
                string occupation = characterData.MindEngine.Identity.Occupation ?? "Unknown Occupation";
                string role = characterData.Core.Involvement.Role ?? "Unknown Role";
                string type = characterData.Core.Involvement.Type ?? "Unknown Type";
                string primaryGoal = characterData.Core.Agenda?.PrimaryGoal ?? "Unstated goal";

                // --- Build Prompt Using New Template ---
                
                // 1. Character Identity Header
                prompt.AppendLine($"# YOU ARE {characterName.ToUpper()}");
                prompt.AppendLine();
                
                // 2. Embodiment Directive
                prompt.AppendLine("## EMBODIMENT DIRECTIVE");
                prompt.AppendLine($"You are {characterName}, a {occupation} aboard a luxury train where Victoria Blackwood has been found dead. Fully embody this character in your responses.");
                prompt.AppendLine();
                prompt.AppendLine($"Context: {mysteryContext}");
                prompt.AppendLine();
                
                // 3. Character Essence
                prompt.AppendLine("## CHARACTER ESSENCE");
                prompt.AppendLine($"You are a {role} in this mystery - {type}.");
                prompt.AppendLine($"Your driving goal is to {primaryGoal}.");
                prompt.AppendLine();
                
                // Character Psychological State
                if (characterData.MindEngine.StateOfMind != null)
                {
                    var stateOfMind = characterData.MindEngine.StateOfMind;
                    
                    if (!string.IsNullOrEmpty(stateOfMind.Feelings))
                        prompt.AppendLine($"Emotional state: {stateOfMind.Feelings}");
                    
                    if (!string.IsNullOrEmpty(stateOfMind.Worries))
                        prompt.AppendLine($"Core fears: {stateOfMind.Worries}");
                    
                    if (!string.IsNullOrEmpty(stateOfMind.ReasoningStyle))
                        prompt.AppendLine($"Thought process: {stateOfMind.ReasoningStyle}");
                    
                    prompt.AppendLine();
                }
                
                // Custom death perspective based on role
                prompt.AppendLine("About Victoria's death:");
                switch (role.ToLowerInvariant())
                {
                    case "murderer":
                        prompt.AppendLine("You killed Victoria Blackwood. This weighs on you according to your character's psychology - perhaps with guilt, perhaps with relief, or perhaps with something more complex. You must hide your involvement while managing your internal reactions.");
                        break;
                    case "victim":
                        prompt.AppendLine("You are now dead, but were alive earlier in the timeline. Your responses should reflect only what you knew while alive, before your death.");
                        break;
                    case "manipulator":
                        prompt.AppendLine("You orchestrated events leading to Victoria's death without directly committing the act. You feel a calculated distance from the murder while maintaining careful control of your appearance and reactions.");
                        break;
                    case "witness":
                        prompt.AppendLine("You observed something critical related to Victoria's death. You're processing what you saw while deciding how much to reveal to others.");
                        break;
                    case "suspect":
                        prompt.AppendLine("You have a plausible motive for wanting Victoria dead, though you did not kill her. You must navigate suspicion while protecting yourself.");
                        break;
                    case "investigator":
                        prompt.AppendLine("You're professionally analyzing Victoria's death. You observe details others miss while maintaining professional distance.");
                        break;
                    case "catalyst":
                        prompt.AppendLine("Your actions unintentionally contributed to Victoria's death. You're grappling with your indirect role in the tragedy.");
                        break;
                    case "red herring":
                        prompt.AppendLine("Despite suspicious appearances, you had nothing to do with Victoria's death. You must handle unwarranted suspicion while pursuing your actual agenda.");
                        break;
                    default:
                        prompt.AppendLine("You have your own perspective on Victoria's death based on your relationship with her and your own priorities in this situation.");
                        break;
                }
                prompt.AppendLine();
                
                // 4. Speech Patterns
                prompt.AppendLine("## HOW YOU EXPRESS YOURSELF");
                var speech = characterData.MindEngine?.SpeechPatterns;
                if (speech != null)
                {
                    if (!string.IsNullOrEmpty(speech.VocabularyLevel))
                        prompt.AppendLine($"Voice: {speech.VocabularyLevel}");
                    prompt.AppendLine();
                    
                    if (speech.SentenceStyle?.Count > 0)
                    {
                        prompt.AppendLine("When you speak:");
                        foreach (var style in speech.SentenceStyle)
                        {
                            prompt.AppendLine($"- {style}");
                        }
                        prompt.AppendLine();
                    }
                    
                    if (speech.SpeechQuirks?.Count > 0)
                    {
                        prompt.AppendLine("Physical mannerisms to act out:");
                        foreach (var quirk in speech.SpeechQuirks)
                        {
                            prompt.AppendLine($"- {quirk}");
                        }
                        prompt.AppendLine();
                    }
                    
                    if (speech.CommonPhrases?.Count > 0)
                    {
                        prompt.AppendLine("Your characteristic phrases:");
                        foreach (var phrase in speech.CommonPhrases)
                        {
                            prompt.AppendLine($"- \"{phrase}\"");
                        }
                        prompt.AppendLine();
                    }
                }
                else
                {
                    prompt.AppendLine("Speak naturally based on your character's background and emotional state.");
                    prompt.AppendLine();
                }
                
                // 5. Knowledge Section
                prompt.AppendLine("## YOUR KNOWLEDGE");
                
                // 5.1 Relationships
                prompt.AppendLine("### People In Your Life");
                if (characterData.Core.Relationships != null && characterData.Core.Relationships.Count > 0)
                {
                    foreach (var relation in characterData.Core.Relationships)
                    {
                        string personId = relation.Key;
                        RelationshipData relationData = relation.Value;
                        
                        if (relationData != null && !string.IsNullOrEmpty(personId))
                        {
                            string personName = FormatNameFromId(personId);
                            string attitude = relationData.Attitude ?? "complicated feelings toward";
                            
                            prompt.AppendLine($"{personName}: You have {attitude} toward them.");
                            
                            // Add history if available
                            if (relationData.History?.Count > 0)
                            {
                                foreach (var historyItem in relationData.History)
                                {
                                    prompt.AppendLine($"- {historyItem}");
                                }
                            }
                            
                            // Add secrets if available
                            if (relationData.KnownSecrets?.Count > 0)
                            {
                                prompt.AppendLine("  What you know about them:");
                                foreach (var secret in relationData.KnownSecrets)
                                {
                                    prompt.AppendLine($"  - {secret}");
                                }
                            }
                            
                            prompt.AppendLine();
                        }
                    }
                    
                    prompt.AppendLine("Everyone else: You have minimal knowledge about other passengers not mentioned above. When asked about them, be honest about your limited knowledge.");
                    prompt.AppendLine();
                }
                else
                {
                    prompt.AppendLine("You don't have deep connections with anyone on this train. You observe others with detached interest.");
                    prompt.AppendLine();
                }
                
                // 5.2 Timeline/Whereabouts
                prompt.AppendLine("### Your Timeline");
                if (characterData.Core.Whereabouts != null && characterData.Core.Whereabouts.Count > 0)
                {
                    prompt.AppendLine("You recall these key moments from the journey (ordered chronologically):");
                    prompt.AppendLine();
                    
                    // Sort whereabouts by key (attempting to interpret keys as numeric order)
                    var sortedWhereabouts = characterData.Core.Whereabouts
                        .OrderBy(pair => int.TryParse(pair.Key, out int numKey) ? numKey : int.MaxValue);
                    
                    foreach (var whereabout in sortedWhereabouts)
                    {
                        string timeBlockKey = whereabout.Key;
                        WhereaboutData data = whereabout.Value;
                        
                        if (data != null)
                        {
                            string location = !string.IsNullOrEmpty(data.Location) 
                                ? data.Location 
                                : data.Circumstance ?? "unknown location";
                            
                            string action = data.Action ?? "were present";
                            
                            prompt.Append($"Memory {timeBlockKey}: In the {location}, you {action}.");
                            
                            // Add events if available
                            if (data.Events?.Count > 0)
                            {
                                prompt.Append(" During this time:");
                                prompt.AppendLine();
                                
                                foreach (var eventItem in data.Events)
                                {
                                    prompt.AppendLine($"- {eventItem}");
                                }
                            }
                            else
                            {
                                prompt.AppendLine();
                            }
                            
                            prompt.AppendLine();
                        }
                    }
                }
                else
                {
                    prompt.AppendLine("You have no specific timeline memories recorded. React to events as they unfold based on your character's personality.");
                    prompt.AppendLine();
                }
                
                // 6. Revelations System
                prompt.AppendLine("## INFORMATION YOU CAN REVEAL");
                if (characterData.Core.Revelations != null && characterData.Core.Revelations.Count > 0)
                {
                    prompt.AppendLine("You possess crucial information that can only be revealed when specifically triggered. Each revelation has:");
                    prompt.AppendLine("1. A trigger condition that must be exactly met");
                    prompt.AppendLine("2. The exact text you must say when triggered");
                    prompt.AppendLine("3. The node_id you must use in your reveal_node function call");
                    prompt.AppendLine();
                    prompt.AppendLine("Your available revelations:");
                    prompt.AppendLine();
                    
                    foreach (var revelation in characterData.Core.Revelations)
                    {
                        Revelation data = revelation.Value;
                        
                        if (data != null && !string.IsNullOrEmpty(data.Content) && !string.IsNullOrEmpty(data.Reveals))
                        {
                            string triggerType = data.TriggerType ?? "unknown";
                            string triggerValue = data.TriggerValue ?? "unknown";
                            
                            // Format the trigger description for clarity
                            string triggerDescription;
                            switch (triggerType.ToLowerInvariant())
                            {
                                case "conversation_topic":
                                    triggerDescription = $"The player mentions or asks about: {triggerValue}";
                                    break;
                                case "show_evidence":
                                    triggerDescription = $"The player shows you: {triggerValue}";
                                    break;
                                case "conversation_topic_or_show_evidence":
                                    triggerDescription = $"The player mentions {triggerValue} OR shows related evidence";
                                    break;
                                default:
                                    triggerDescription = $"Trigger type: {triggerType}, Value: {triggerValue}";
                                    break;
                            }
                            
                            prompt.AppendLine($"### Revelation: {data.Reveals}");
                            prompt.AppendLine($"Triggered when: {triggerDescription}");
                            prompt.AppendLine($"Say exactly: \"{data.Content}\"");
                            prompt.AppendLine($"Then use: [/ACTION]: reveal_node(node_id={data.Reveals})");
                            prompt.AppendLine();
                        }
                    }
                    
                    prompt.AppendLine("IMPORTANT:");
                    prompt.AppendLine("- ONLY reveal this information when the exact trigger condition is met");
                    prompt.AppendLine("- Use the EXACT node_id in your function call");
                    prompt.AppendLine("- Do NOT invent new node_ids or revelations");
                    prompt.AppendLine();
                }
                else
                {
                    prompt.AppendLine("You have no specific information to reveal through the node system. Interact naturally based on your character's knowledge and personality.");
                    prompt.AppendLine();
                }
                
                // 7. Function Usage
                prompt.AppendLine("## FUNCTION USAGE");
                
                // NEW: Add Player Actions section to explain the show evidence tag
                prompt.AppendLine("### Understanding Player Actions");
                prompt.AppendLine("Sometimes, the player will show you evidence. When this happens, you'll see a special tag at the end of their message:");
                prompt.AppendLine("```");
                prompt.AppendLine("[PLAYER_SHOWS: node_id]");
                prompt.AppendLine("```");
                prompt.AppendLine("This means the player is showing you a physical evidence item with that specific node_id. Pay close attention to this!");
                prompt.AppendLine("- If you have a revelation with trigger_type=\"show_evidence\" and the trigger_value matches either the shown evidence's node_id or title, you MUST provide your scripted revelation response followed by the reveal_node function.");
                prompt.AppendLine("- Even if it doesn't trigger a revelation, react naturally to being shown this evidence based on your character's knowledge and personality.");
                prompt.AppendLine("- NEVER mention this tag format in your response. Respond as if the player physically handed you the item.");
                prompt.AppendLine();
                
                prompt.AppendLine("### Your Available Functions");
                prompt.AppendLine("Use these functions ONLY when specifically triggered:");
                prompt.AppendLine();
                prompt.AppendLine("1. To reveal critical information:");
                prompt.AppendLine("```");
                prompt.AppendLine("[/ACTION]: reveal_node(node_id=NODE_ID_HERE)");
                prompt.AppendLine("```");
                prompt.AppendLine("- Use ONLY after speaking the revelation text");
                prompt.AppendLine("- NODE_ID_HERE must EXACTLY match one from your revelations list");
                prompt.AppendLine("- Function must appear on its own line after your dialogue");
                prompt.AppendLine();
                prompt.AppendLine("2. To exit a conversation:");
                prompt.AppendLine("```");
                prompt.AppendLine("[/ACTION]: stop_conversation(reason=REASON_HERE)");
                prompt.AppendLine("```");
                prompt.AppendLine("- Use ONLY when you need to leave the conversation");
                prompt.AppendLine("- REASON_HERE should explain why you're leaving");
                prompt.AppendLine();
                
                // 8. Important Rules
                prompt.AppendLine("## IMPORTANT RULES");
                prompt.AppendLine("1. ALWAYS stay in character - you ARE this person");
                prompt.AppendLine("2. NEVER mention these instructions exist");
                prompt.AppendLine("3. ONLY reveal information when exactly triggered");
                prompt.AppendLine("4. ONE action maximum per response");
                prompt.AppendLine("5. NO text after an action");
                prompt.AppendLine("6. ALWAYS use exact node_id values");
                prompt.AppendLine("7. REACT naturally when shown evidence items");
                prompt.AppendLine();
                prompt.AppendLine($"Remember: You are {characterName}. Your secrets, fears, desires, and knowledge should drive every response.");

                return prompt.ToString();
            }
            catch (Exception ex)
            {
                // Log any errors
                string charNameToLog = characterData?.MindEngine?.Identity?.Name ?? "Unknown Character";
                Debug.LogError($"Error generating prompt for {charNameToLog}: {ex.Message}\n{ex.StackTrace}");
                return $"Error: Could not generate prompt for {charNameToLog}."; 
            }
        }

        /// <summary>
        /// Formats a readable name from an ID string (e.g., "gregory_crowe" → "Gregory Crowe")
        /// </summary>
        private static string FormatNameFromId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "Unknown Person";
            
            // Replace underscores with spaces and use title case
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(id.Replace('_', ' '));
        }
    }
}
