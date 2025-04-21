import React, { useState, useEffect, useRef, useCallback } from 'react';

// --- Configuration ---
const config = {
    maxDialogueWidth: 'max-w-3xl', // Tailwind class for max pixel width (768px)
    // --- NEW: Typewriter Speed (m illiseconds per character) ---
    typingSpeed: 30, // Lower is faster
};

// --- IMPORTANT DEMO NOTE ---
// This React component is a VISUAL MOCKUP for UI/UX demonstration ONLY.
// The NPC responses (`npcResponses` array) and the simulated delay (`setTimeout`)
// are placeholders. In the actual game, the dialogue text will be streamed
// from the live LLM connection, and player input submission will interact
// with existing game systems. This demo focuses SOLELY on the front-end
// appearance, layout, adaptive behavior, input ghost, and typewriter effect simulation.
// --- END DEMO NOTE ---

// --- Demo Content (Placeholder) ---
const npcResponses = [ /* ... responses remain the same ... */
    "Welcome, Detective. What brings you here on this dreary evening? Ask me anything, but choose your words carefully.",
    "I suppose you have a point there. The watch was indeed in the evidence locker, but how did you know which one to look for?",
    "A hunch? Come now, we both know that's not entirely true. The specific timepiece was mentioned nowhere in the police reports. You would have had to read Eleanor's private diary, and that was supposedly locked away with her personal effects after her death. Unless you're implying someone gave you access?",
    "Eleanor's sister? That's... interesting. I wasn't aware Victoria was speaking to anyone about the case. Especially not after her public statement asking for privacy. And certainly not over drinks. You see, Victoria has been a teetotaler for over fifteen years. Not a drop has passed her lips since that unfortunate incident at her college graduation. It was quite the scandal at the time. So I'm afraid you've just caught yourself in a rather obvious lie. Which makes me wonder what else you've been dishonest about. Perhaps you'd care to start again? This time with the truth? The timepieces are more than collector's items. Each one represents a specific moment in time. Moments that, when pieced together, tell a story that certain people would prefer remained untold. You are delving into dangerous waters, Detective. Be sure you know how to swim before you dive deeper.",
    "Intriguing... You seem to know more than you let on. Very well, ask your next question.",
    "That's classified information. I can't possibly comment on that.",
    "Why the sudden interest in the old clock tower? It hasn't chimed in years.",
];

// --- Custom Hook for Typewriter Effect ---
function useTypewriter(text, speed = 50) {
    const [displayedText, setDisplayedText] = useState('');
    const [isTypingComplete, setIsTypingComplete] = useState(false);

    useEffect(() => {
        setDisplayedText(''); // Reset displayed text when target text changes
        setIsTypingComplete(false); // Reset completion flag
        if (!text) return; // Don't type if text is empty

        let i = 0;
        const intervalId = setInterval(() => {
            setDisplayedText(prev => prev + text.charAt(i));
            i++;
            if (i >= text.length) {
                clearInterval(intervalId);
                setIsTypingComplete(true); // Mark as complete
            }
        }, speed);

        // Cleanup function to clear interval if component unmounts or text changes
        return () => clearInterval(intervalId);
    }, [text, speed]); // Rerun effect if text or speed changes

    return { displayedText, isTypingComplete };
}

// --- React Components ---

/**
 * CharacterInfo Component: Displays portrait and name. (Unchanged)
 */
function CharacterInfo({ name }) { /* ... component code remains the same ... */
    return (
        <div
            className="absolute text-center z-10"
            style={{ left: '1em', top: '-2.5vh', width: '7vh' }}
        >
            <div
                className="h-[7vh] w-[7vh] bg-[#354048] border-2 border-[#444] rounded-[5px] shadow-md flex items-center justify-center text-[1.5em] overflow-hidden font-mono text-[#8898a8] font-bold"
                aria-label={`${name} portrait`}
            >
                {':)'}
            </div>
            <div className="bg-[rgba(0,0,0,0.9)] text-white text-[0.65em] font-bold py-[0.3em] rounded-b-[4px] w-full mt-[-2px] relative">
                {name}
            </div>
        </div>
    );
}

/**
 * InputGhost Component: Displays the player's last input.
 * @param {string} text - The last input text from the player.
 */
function InputGhost({ text }) {
    // Render only if text exists
    if (!text) {
        return null;
    }
    return (
        <div className="px-[1em] pt-[0.5em] pb-[0.5em] flex-shrink-0"> {/* Padding matches DialogueBox, pt/pb added */}
            <p className="font-sans text-[0.8em] text-gray-400 opacity-80 italic leading-normal m-0 break-words"> {/* Smaller, italic, less prominent */}
                You said: "{text}"
            </p>
        </div>
    );
}


/**
 * NpcResponseArea Component: Displays scrollable NPC text with typewriter effect.
 * @param {string} fullResponseText - The complete target response text.
 */
function NpcResponseArea({ fullResponseText }) {
    const responseAreaRef = useRef(null);
    const [isScrollable, setIsScrollable] = useState(false);
    // Use the typewriter hook
    const { displayedText, isTypingComplete } = useTypewriter(fullResponseText, config.typingSpeed);

    // Effect to check scrollability (runs when displayedText changes)
    useEffect(() => {
        const checkScrollable = () => {
            const element = responseAreaRef.current;
            if (element) {
                // Scroll to bottom as text types out if needed
                element.scrollTop = element.scrollHeight;
                // Check if scrollbar should be active
                setIsScrollable(element.scrollHeight > element.clientHeight + 1);
            }
        };
        let rafId;
        const runCheck = () => { cancelAnimationFrame(rafId); rafId = requestAnimationFrame(checkScrollable); };
        runCheck();
        return () => cancelAnimationFrame(rafId);
    }, [displayedText]); // Check scroll on text update

    return (
        <div
            ref={responseAreaRef}
            className={`flex-grow flex-shrink min-h-0 overflow-y-auto mb-[0.7em] pr-[0.6em] relative scrollbar-thin scrollbar-thumb-[rgba(255,255,255,0.4)] scrollbar-track-[rgba(255,255,255,0.1)] ${isScrollable ? 'is-scrollable' : ''}`}
        >
            {/* Display the incrementally revealed text */}
            <p className="font-serif text-[1em] leading-normal text-[#f0f0f0] p-[0.6em] pt-[0.3em] m-0 min-h-[1.5em] break-words">
                {displayedText}
                {/* Blinking cursor effect while typing */}
                {!isTypingComplete && (
                    <span className="inline-block w-[2px] h-[1em] bg-white ml-1 animate-blink"></span>
                )}
            </p>
            {/* Fade out effect */}
            {isScrollable && (
                 <div className="sticky bottom-0 left-0 right-[0.6em] h-[2em] bg-gradient-to-b from-transparent to-[rgba(10,10,10,0.95)] pointer-events-none opacity-100"></div>
            )}
        </div>
    );
}

/**
 * PlayerInputArea Component: Renders the text input field. (Unchanged)
 */
function PlayerInputArea({ inputValue, onInputChange, onSendMessage }) { /* ... component code remains the same ... */
    const textareaRef = useRef(null);
    const handleKeyDown = (event) => { if (event.key === 'Enter' && !event.shiftKey) { event.preventDefault(); onSendMessage(); } };
    useEffect(() => {
        const textarea = textareaRef.current;
        if (textarea) { textarea.style.height = 'auto'; const scrollHeight = textarea.scrollHeight; textarea.style.height = `${scrollHeight}px`; }
    }, [inputValue]);
    return (
        <div className="flex-shrink-0 relative">
            <textarea
                ref={textareaRef} id="playerInput" value={inputValue} onChange={onInputChange} onKeyDown={handleKeyDown}
                placeholder="Type your question or statement... (Press Enter to send)" rows="1"
                className="block w-full bg-[rgba(30,30,30,0.9)] border border-[#444] rounded-[4px] text-[#ddd] font-sans text-[0.9em] leading-normal py-[0.6em] px-[0.7em] box-border resize-none overflow-y-auto focus:outline-none focus:border-[#777] focus:bg-[rgba(40,40,40,0.95)] transition-height duration-100 ease-out"
                style={{ minHeight: '2.5em', maxHeight: '6em' }}
            />
        </div>
    );
}

/**
 * DialogueBox Component: Main container. Manages input ghost state.
 */
function DialogueBox({ npcName, currentResponse, onPlayerSend }) {
    const [playerInput, setPlayerInput] = useState('');
    // *** ADDED: State for the last player input (input ghost) ***
    const [lastPlayerInput, setLastPlayerInput] = useState('');
    // *** REMOVED: displayedText state (now handled by typewriter hook) ***
    // const [displayedText, setDisplayedText] = useState(currentResponse);

    const dialogueBoxRef = useRef(null);

    // *** REMOVED: useEffect to update displayedText (now handled by typewriter) ***
    // useEffect(() => { setDisplayedText(currentResponse); }, [currentResponse]);

    const handleInputChange = (event) => { setPlayerInput(event.target.value); };

    const handleSendMessage = useCallback(() => {
        const trimmedInput = playerInput.trim();
        if (trimmedInput) {
            console.log("Player sent:", trimmedInput);

            // *** ADDED: Update lastPlayerInput state BEFORE clearing input ***
            setLastPlayerInput(trimmedInput);

            // *** REMOVED: setDisplayedText(""); *** // Typewriter hook handles reset

            setPlayerInput(''); // Clear input field visually

            // Simulate delay before triggering parent to get next response
            setTimeout(() => {
                onPlayerSend(trimmedInput); // Notify parent component
            }, 500 + Math.random() * 500); // Demo delay ONLY
        }
    }, [playerInput, onPlayerSend]);

    return (
        <div
            ref={dialogueBoxRef}
            className={`relative bg-[rgba(10,10,10,0.85)] border border-[rgba(85,85,85,0.5)] rounded-[0.5em] w-[80%] ${config.maxDialogueWidth} mb-[4vh] flex p-[1em] shadow-lg`}
            style={{ paddingLeft: `calc(7vh + 1.5em)`, minHeight: `calc(7vh - 2.5vh + 2.5em + 2em)` }}
        >
            <CharacterInfo name={npcName} />
            {/* Interaction Area */}
            {/* Apply max-height here */}
            <div className="flex-grow flex flex-col min-w-0 max-h-[35vh] overflow-hidden">
                {/* *** ADDED: Render Input Ghost above response area *** */}
                <InputGhost text={lastPlayerInput} />
                {/* Pass the full response text to the area with the typewriter */}
                <NpcResponseArea fullResponseText={currentResponse} />
                <PlayerInputArea
                    inputValue={playerInput}
                    onInputChange={handleInputChange}
                    onSendMessage={handleSendMessage}
                />
            </div>
        </div>
    );
}

/**
 * App Component: Root component. Sets base font size and blink animation.
 */
function App() {
    const [isVisible, setIsVisible] = useState(true);
    const [currentNpcResponseIndex, setCurrentNpcResponseIndex] = useState(0);
    const npcName = "MORGAN";

    const handlePlayerSend = (inputText) => {
        // NOTE: This is DEMO logic to cycle responses. Real game uses LLM.
        console.log("LLM Input:", inputText);
        setCurrentNpcResponseIndex(prevIndex => (prevIndex + 1) % npcResponses.length);
    };

    useEffect(() => {
        const handleKeyDown = (event) => {
            if (event.key === 'Escape') { setIsVisible(false); console.log("Dialogue closed"); }
            if (event.key === 'e' && !isVisible) { setIsVisible(true); console.log("Dialogue opened"); }
        };
        window.addEventListener('keydown', handleKeyDown);
        return () => { window.removeEventListener('keydown', handleKeyDown); };
    }, [isVisible]);

    const currentResponse = npcResponses[currentNpcResponseIndex];

    // Add global styles for base font and blink animation
    const globalStyle = `
        body { font-size: clamp(14px, calc(1vw + 0.6em), 17px); line-height: 1.5; }
        @keyframes blink { 50% { opacity: 0; } }
        .animate-blink { animation: blink 1s step-end infinite; }
    `;

    return (
        <div className="font-sans m-0 bg-[#1a1a1a] text-[#eee] overflow-hidden h-screen">
            <style>{globalStyle}</style> {/* Inject global styles */}
            {/* Overlay */}
            <div className={`fixed inset-0 bg-[rgba(0,0,0,0.65)] z-10 flex justify-center items-end transition-opacity duration-300 ease-in-out ${isVisible ? 'opacity-100' : 'opacity-0 pointer-events-none'}`}>
                {isVisible && ( <DialogueBox npcName={npcName} currentResponse={currentResponse} onPlayerSend={handlePlayerSend} /> )}
            </div>
            {/* Instructions */}
            <div className="fixed bottom-[1vh] left-1/2 transform -translate-x-1/2 text-[0.7em] text-[#aaa] bg-[rgba(0,0,0,0.5)] py-[0.2em] px-[0.6em] rounded-[3px] z-20">
                Press Enter to send | Press Esc to close dialogue
            </div>
        </div>
    );
}
export default App;
