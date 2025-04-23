using System.Text;
using UnityEngine;

namespace Dialogue
{
    /// <summary>
    /// Simple state machine that detects and extracts function calls from text stream
    /// </summary>
    public class FunctionTagParser
    {
        private bool inTag;
        private StringBuilder tag = new StringBuilder();
        private System.Action<string> functionCallAction;

        public FunctionTagParser(System.Action<string> onFunctionCall)
        {
            this.functionCallAction = onFunctionCall;
        }

        /// <summary>
        /// Process a character from the text stream
        /// </summary>
        /// <param name="c">Character to process</param>
        /// <returns>True if the character should be displayed, false if it's part of a function tag</returns>
        public bool TryConsume(char c)
        {
            // Check for function tag start
            if (!inTag)
            {
                if (c == '<')
                { 
                    inTag = true;
                    tag.Clear();
                    tag.Append(c); // Include the opening '<' in the tag
                    return false;   // Don't display tag characters
                }
                return true;        // Display all non-tag characters
            }

            // Already in a tag, append character and check for closing
            tag.Append(c);
            
            if (c == '>')
            {
                inTag = false;
                string fullTag = tag.ToString();
                
                // Check if this is a function tag
                if (fullTag.StartsWith("<func ") || fullTag.StartsWith("<ACTION:") || 
                    fullTag.StartsWith("<action:"))
                {
                    ExecuteTag(fullTag);
                }
                
                return false;   // Don't display the closing '>' of the tag
            }
            
            return false;       // Don't display any characters inside tags
        }

        private void ExecuteTag(string rawTag)
        {
            Debug.Log($"[FunctionTagParser] Found function tag: {rawTag}");
            functionCallAction?.Invoke(rawTag);
        }
    }
}