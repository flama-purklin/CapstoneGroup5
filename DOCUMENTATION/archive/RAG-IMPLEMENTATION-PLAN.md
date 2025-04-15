# RAG System Implementation Plan

## Goal

Integrate a Retrieval-Augmented Generation (RAG) system to provide LLM characters with dynamic access to relevant information from the game's current state (e.g., discovered clues, world events, character knowledge updates) to enhance dialogue realism and consistency.

## Proposed Architecture

1.  **Central RAG Component:** Utilize the `LLMUnity.RAG` component as the main interface. Attach this to a dedicated GameObject (e.g., "RAGSystem") or potentially the `GameController`.
2.  **Search Method:** Use `DBSearch` (Approximate Nearest Neighbor search via Usearch) for efficient retrieval. Configure its parameters (quantization, metric, etc.) as needed.
3.  **Chunking Method:** Start with `SentenceSplitter` for breaking down text data into manageable chunks for embedding. Evaluate if other methods (`TokenSplitter`, `WordSplitter`, or custom logic) are needed later based on performance and retrieval quality.
4.  **LLM Embedder:** The `DBSearch` component requires an `LLMEmbedder` instance, which in turn needs a reference to the main `LLM` component to generate embeddings for text chunks. Ensure this is configured correctly.
5.  **Data Sources:** Identify potential sources of dynamic information to feed into the RAG system:
    *   Player's Journal / Discovered Clues
    *   Significant World Events (if tracked)
    *   Updates to Character Knowledge/Relationships (potentially from the mystery JSON or runtime changes)
    *   Environmental descriptions or interactions
6.  **Indexing:** Implement logic to:
    *   Extract relevant text from data sources.
    *   Chunk the text using the chosen `Chunking` method.
    *   Add the chunks (and their embeddings) to the `DBSearch` index using `RAG.Add()`. Assign appropriate group names (e.g., "clues", "world_state", "character_knowledge_[name]") for targeted searching.
    *   Determine *when* to update the index (e.g., when a clue is found, at the start of a scene, periodically).
7.  **Retrieval:** Implement logic within the dialogue generation process (likely modifying `CharacterPromptGenerator` or adding a pre-processing step before calling `LLMCharacter.Chat`) to:
    *   Formulate a query based on the current dialogue context (e.g., the last player utterance or topic).
    *   Search the RAG index using `RAG.Search()` or `RAG.IncrementalSearch()`/`Fetch()` for relevant chunks (potentially filtering by group).
    *   Select the top K relevant chunks.
8.  **Context Augmentation:** Inject the retrieved chunks into the prompt sent to the `LLMCharacter` for response generation. Decide on the format (e.g., a specific section in the system prompt, prepended to the user message).
9.  **Persistence:** Determine if the RAG index needs to persist between sessions. If so, implement calls to `RAG.Save()` and `RAG.Load()` using `ZipArchive` at appropriate times (e.g., game save/load points, potentially alongside character history if that were persistent). *Initial implementation might skip persistence.*

## Implementation Steps

1.  **Setup RAG GameObject:** Create a GameObject ("RAGSystem") and add the `LLMUnity.RAG` component. Configure it to use `DBSearch` and `SentenceSplitter`. Assign the main `LLM` component reference to the `LLMEmbedder` within `DBSearch`.
2.  **Identify & Extract Data:** Choose initial data sources (e.g., discovered clue descriptions). Implement functions to extract plain text from these sources.
3.  **Implement Indexing Logic:** Create a script (e.g., `RAGIndexer`) responsible for calling `RAG.Add()` with extracted text and appropriate group names. Decide on the trigger mechanism (e.g., called when a clue is added to the journal).
4.  **Implement Retrieval Logic:** Modify the dialogue generation process. Before calling `LLMCharacter.Chat()`, formulate a query, call `RAG.Search()`, retrieve top K results.
5.  **Implement Context Injection:** Modify prompt generation (`CharacterPromptGenerator` or similar) to include the retrieved RAG context in a structured way within the final prompt sent to the LLM.
6.  **Testing & Tuning:** Test dialogue interactions with RAG enabled. Tune chunking parameters, search parameters (K value), query formulation, and context injection format for relevance and quality. Evaluate different chunking/splitting methods if needed.
7.  **(Optional) Implement Persistence:** If required, add `RAG.Save()`/`Load()` calls.

## Open Questions & Considerations

*   **Indexing Frequency:** How often should the index be updated? Real-time updates vs. periodic batching?
*   **Chunking Strategy:** Is sentence splitting sufficient? Will token or word splitting yield better results for specific data types? Need experimentation.
*   **Query Formulation:** How best to generate effective search queries from dialogue context?
*   **Context Injection Format:** What's the optimal way to present retrieved information to the LLM within the prompt?
*   **Scalability:** How will performance scale with a large number of indexed documents/chunks? `DBSearch` should handle this better than `SimpleSearch`.
*   **Relevance Tuning:** How to ensure retrieved chunks are truly relevant and don't confuse the LLM? May require tuning K, search parameters, or query strategy.
*   **Group Management:** How granular should the group names be for effective filtering?

*(This plan provides a starting point. Implementation details will evolve based on testing and refinement.)*
