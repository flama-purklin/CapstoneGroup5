# 

# 

# 

# 

# 

# 

# 

# 

# 

# **Station No.5 — DESIGN DOCUMENT**

## **Revision 5.0**

**[OVERVIEW	3](#overview)**

[● Premise	3](#premise)

[● Production Setup	4](#production-setup)

[● Influences	4](#influences)

[● Technical Specifications	5](#technical-specifications)

[**GAMEPLAY	5**](#gameplay)

[● Gameplay Loop	5](#gameplay-loop)

[● General Mechanics	5](#general-mechanics)

[● Mystery Solving	6](#mystery-solving)

[● Machine Learning Usage:	7](#machine-learning-usage:)

[**SYSTEMS	8**](#systems)

[● Train Cart Generation	8](#train-cart-generation)

[● Generating Characters	9](#generating-characters)

[● Social Mechanics	9](#social-mechanics)

[● Mystery Generation	9](#mystery-generation)

[● Emergent Dynamism	12](#emergent-dynamism)

[**ART	13**](#art)

[● Camera Setup	13](#camera-setup)

[● Artstyle	13](#artstyle)

[● Aesthetics	13](#aesthetics)

[● Animation Containers	14](#animation-containers)

[**SCHEDULE	15**](#schedule)

[● Phase 1: Make A Game	15](#phase-1:-make-a-game)

[● Phase 2: Make It Good	16](#phase-2:-make-it-good)

[● Phase 3: Make It Better	16](#phase-3:-make-it-better)

[**IDEAS & FUTURE PLANS	17**](#ideas-&-future-plans)

[● Gameplay Loop Scaling	17](#gameplay-loop-scaling)

[● Story Elements	17](#story-elements)

[● Visuals	18](#visuals)

[● Systems Implementations	18](#systems-implementations)

# **OVERVIEW** {#overview}

* ## ***Premise*** {#premise}

  * ### Genre

    * Social Deduction  
    * Detective Mystery  
    * Social-sim

  * ### Gameplay Experience

    * The player controls a personable customer service robot given free reign to navigate between the interior spaces of a moving train, interact with NPC characters, find clues, and solve a mystery case that only the playable character possesses the ability to.

  * ### What Sets The Game Apart

    * Use of a black-box approach that allows for any mystery, as long as represented in a valid mystery json file data structure, can be fed into a mystery engine, which will instantiate it into a playable experience. This black box approach allows mysteries to come from different, non-specific sources, such as preset playlists handcrafted by the developers, procedural generation techniques, and user-generated sources.  
      * The mystery, the one the players are supposed to uncover, will be instantiated from logical and narrative templates, producing completely different sets of information to collect and evidence to gather, all coming together in an unified whole.  
        * The black-box approach allows completely different gameplay scenarios depending on the mystery fed into the engine, with a varying cast of characters, minigames, clues, logical structure, narrative, and even train layout.  
    * Enhanced social deduction elements, diverging from the traditional use of dialogue trees from preset dialogue options and giving the player full freedom over their verbal interactions with NPCs.  
      * This synergizes well with the black-box design to breed a tailored experience that should counteract the often repetitive and formulaic nature of the mystery genre while being hands-off in terms of development\*, taking full advantage of the player input element of the video game medium to lead to faster interesting and emergent results in-game.  
        * \*Development here not as the development of the game itself, but rather the mysteries that will be developed within the game. Reducing the plug-and-play aspects of making new and interesting mysteries, therefore, should also make the game and mystery engine more fun and expressive to engage with  
      * The element of novelty, as well as the technological framework used to implement this “open-typing” dialogue system, will be locally ran transformer-based language models, which will be used to role-play as characters.

* ## ***Production Setup*** {#production-setup}

  * ### Group Members and Roles

    * **Charles (Programmer)**  
    * **Dana (Artist)**  
    * **James (Programmer)**  
    * **Jorge (Programmer)**  
    * **Matt (Programmer)**  
    * **Noah (Programmer)**

  * ### Communication

    * Synchronous weekly meetings on Sundays through a dedicated Discord server’s single voice channel.  
    * Asynchronous communication through multiple classified text channels on a dedicated Discord server.  
    * In-person weekly encounters during CPI 471 sessions on Tuesdays.

  * ### Planning

    * Keep track of task completion pipeline (to-do, in progress, completed) in shared Trello project.

  * ### Code and Project Repository

    * The project is stored in a GitHub repository that was shared among all members of the group  
    * For each [project phase](#schedule), each technical team will work on separate branches. The integration of those branches will be done at another temporary branch until a stable version is finally pushed into the main branch.

* ## ***Influences*** {#influences}

  * ### Game Design

    * Detective and mystery constellation systems:   
      * Outer Wilds  
      * Deathloop  
      * Alan Wake II  
      * Return of the Obra Dinn  
      * Phasmophobia  
    * Social deduction system  
      * LA Noir  
      * Dead Meat  
    * Dialogue system  
      * NVIDIA ACE

  * ### Visuals

    * Camera Angles  
      * SIGNALIS  
      * Stardew Valley  
    * Aesthetics  
      * Film noir  
      * Mid Century futuristic illustrations

* ## ***Technical Specifications*** {#technical-specifications}

  * ### Unity Development

    * The game is to be developed on Unity 6, version 6000.0.34f1 (LTS)

  * ### Targeted Platforms

    * PC  
    * Windows 10+ OS  
    * 64-bit

  * ### Targeted System Requirement Goals

    * **System RAM**: 16GB  
    * **Physical CPU count**: 6  
    * **CPU speeds**: 2.3 Ghz to 2.69 Ghz  
    * **GPU**: NVIDIA GeForce RTX 3060 12GB

  * ### Technical Details

    * **Framerate target**: 30 fps  
    * **Primary Display Resolution**: 1080p (16:9)

# **GAMEPLAY** {#gameplay}

* ## ***Gameplay Loop*** {#gameplay-loop}

  * ### Introduction Phase

    * TBD

  * ### Inciting Incident

    * TBD

  * ### Investigatory Phase

    * TBD

  * ### Twist

    * TBD

  * ### Defusion Phase

    * TBD

  * ### Conclusion

    * TBD

* ## ***General Mechanics*** {#general-mechanics}

  * ### Navigation

    * The player can freely move between the randomly generated train cars of each run. However, the player’s view is limited to their current car, adding a feeling of spatial contention that allows for more atmospheric in-game scenarios. 

  * ### Time Is A Resource

    * Players must solve the mystery before their “energy”  runs out, inducing forward gameplay motion and creating an atmosphere of tension and suspense.  
      * The main player will have a given amount of energy at the beginning of the mystery. This energy will be depleted by running simulation through the hunch system and talking to other characters, as well as simply go down slowly over time.

  * ### Interactions

    * The player should be able to interact with their surroundings in meaningful ways.

* ## ***Mystery Solving*** {#mystery-solving}

  * ### Mystery Boards

    * The mystery board will be a pair of menus that will serve both as a cataloging tool as well as a necessary gameplay progression actualizer. As the player learns more about the mystery and its players, these boards will be filled in by the player themselves.  
    * The **detective board**, which should be dedicated to plotting the mystery through the connecting of events, locations, and relevant clues.  
    * The **suspects board** will deal with the psychological profiling and social networking of the characters within the mystery, allowing for more granular exploration of character in isolation from the rest of the mystery.

  * ### Hunches

    * **Definition and Purpose**  
      * A system that transforms the detective board from a passive tool into an interactive platform.  
      * Players formulate "hunches" by manually connecting clues to unfilled leads  
      * Creates strategic depth through limited validation opportunities  
    * **Core Mechanics**  
      * When players discover unfilled leads (e.g., "Who left these footprints?"), they can select from known elements to form a hunch  
      * Connected hunches form comprehensive "theories" about the mystery  
      * Each hunch generates new contextual leads based on the player's choice rather than predetermined solutions  
    * **Simulation System**  
      * Players receive limited simulations (e.g., 5\) at the beginning of each mystery  
      * Running a simulation tests the player's hunches against the actual solution  
      * Contiguously correct hunches stemming from correct discoveries become confirmed clues; incorrect hunches waste the simulation resource but eliminate false assumptions  
    * **Design Benefits**  
      * Encourages active engagement with the mystery rather than passive consumption and leverages the protagonist's robotic nature to explain the simulation mechanic.  
      * Rewards players for attention to detail and logical reasoning  
      * Aligns with the game's emphasis on player expression and agency

  * ### Dialogue Mechanics

    * A considerable chunk of the game will revolve around how the player chooses to interact with other characters to advance their goal of solving a given case. Therefore, it’s important that a large chunk of design considerations go into designing a dialogue system that allows not only player expression, but the control and restraint necessary to construct an engaging dynamic that simulates the riveting and strategic push-and-pull of the dialogue observed in detective and mystery media.  
      * For each mystery, all characters will have their own secrets, agendas, and undisclosed pieces of information, all of which the player should find a way of extracting through intelligent social interactions and social deduction  
      * All characters will be connected within relationship webs. The player must find out how each character relates to each other and psychologically profile them to form a more complete picture of victims, culprits, and suspects.

  * ### Collecting Evidence:

    * Some clues necessary for solving the mystery will be presented in the form of environmental evidence. Interacting with these objects will unlock new opportunities for the player both in terms of soft skills and mechanics; the player now has a new lead they can ask characters about, and new corresponding nodes will appear in the mystery map that can be used to get one step closer to solving the mystery.

  * ### Completing Minigames:

    * Solving mini puzzles like finding safe codes, keys to open locks, and other mystery related minigames. Specific pieces of evidence will not be available until these minigames are completed, restricting mystery map progress and hunches from being made.

* ## ***Machine Learning Usage:***  {#machine-learning-usage:}

  * ### Placeholder Assets

    * We plan on using transformer models to create assets that’ll be temporarily used within the game. These assets will facilitate not only visualization of the final product, but allow for more concrete testing within a faster timeline, without having to find art assets online or wait for the artist to complete particular asset requests. 

  * ### Dynamic and responsive dialogue

    * Machine learning will be used within dialogue to emphasize the social deduction aspect of the game, as breaking away from dialogue trees will not only provide players with more options, but should also create a higher sense of agency. Machine learning, in this case, is being used mostly for the sake of having NPCs respond in ways that are as varied and dynamic as the player.  
      * Light-weight language models are used, with the help of the open-source LLM for Unity package by undreamai, to lend each character the ability to engage with free-form player input during dialogue sections of the game.  
        * Through the use of system prompts and memory management, each character will not only respond to questions following the profiles generated during the setup of the mystery, but to engage with the game’s social deduction mechanics.

# **SYSTEMS** {#systems}

* ## ***Train Cart Generation*** {#train-cart-generation}

  * ### Templating

    * The layout of the train where the mysteries will play out will follow a static template that provides a basic arrangement of different train-car types and the order in which they appear, giving all mysteries a shared spatial flow.  
    * The following is the current template for the train layout:   
      * storage \<-\> second\_class \<-\> second\_class \<-\> kitchen \<-\> business\_class \<-\> bar \<-\> business\_class \<-\> lounge \<-\> first\_class \<-\> engine\_room (player spawn)  
    * In accordance to the black box design, a train’s layout is fundamentally fluid from game to game, and should be set up by the creator of the mystery.

  * ### Filling-In the Templates

    * Each car type will have multiple different visual iterations based on themes associated with them, being instantiated as a different iteration of itself on separate mysteries.

* ## ***Generating Characters*** {#generating-characters}

  * ### Character Profiles

    * Characters are instantiated in the form of a json file which will be called a character profile. The mystery template will define certain templates for each character, which is then procedurally filled as more elements are added to the mystery.  
    * Character profiles will be separated into two chambers, each containing fields that are to be generated on different phases of the mystery generation process:  
      * **CORE:** The core section of the character profile will contain the content that is directly related to the mystery and core solvability of the constellation.  
        * Relationships  
        * Whereabouts  
        * Case-related knowledge  
        * Mystery involvement  
        * Agenda  
        * Possible motive  
        * Leads  
      * **MIND ENGINE:** This section will take care of the character’s personality, voice, and the steps they take to make decisions. This section will be mostly used by the language model for role-playing purposes, but it should also define the manner in which characters dance around important pieces of information, which directly affects gameplay.  
        * Speech patterns  
        * Identity  
        * State of Mind  
        * Reasoning patterns

## 

* ## ***Social Mechanics*** {#social-mechanics}

  * ### TBD

* ## ***Mystery Generation*** {#mystery-generation}

  * ### Elements of Mystery

    * **The Problem**  
      * The murder/problem (murder, theft, missing person, terrorism, etc.)  
    * **Where?**  
      * Where did the crime take place?  
      * Where was it discovered?  
    * **When? For each of the above**  
    * **Culprit(s)**  
    * **THE MOTIVE**: The most important part of the mystery — without a compelling motive, why would the player care about solving anything?  
      * Revenge  
        * For another crime, relationship drama, getting shafted (Luigi), being insulted, etc.  
      * Greed  
        * To take money or a valuable object  
        * To take something of sentimental value  
        * To get paid (nothing personal) — they are a hitman  
      * Desperation  
        * Due to blackmail, or influence from a third party, etc.  
        * They might feel that they had to commit the crime for themselves or their loved ones  
      * Insanity  
        * Mental illness, substance abuse, behavioral disorders  
      * Politics  
        * Sending a message, racism, religion, serving their country’s military  
      * Other  
        * Gang wars, espionage, etc.  
    * **How?**  
      * How was the crime committed? Murder weapon? Timing? What was the culprit's plan for how to do it and how to get away with it?  
  * Other Possibilities To Consider  
    * **Victim(s)**  
      * Who are they? Character could be introduced before crime is committed to make players care more.  
    * **Passengers**  
      * Witnesses, suspects, and innocent bystanders  
        * What did they see/hear/feel/smell/taste?  
      * Alibis  
      * Friends  
      * Liars and red herrings  
      * Are they doing anything during the investigation or just standing around?  
      * Accomplices  
        * Optional, treated basically as an extension of the culprit  
    * **Environment**  
    * **Clues**  
    * **Evidence**  
      * Crime weapon (Knife, Gun, Blunt Object, someone's hands, poison, bomb, etc.)  
      * If the train is moving, some evidence can be thrown out the window  
    * **Alibis**  
      * “No, I wasn’t in Cart 3 when the crime happened. You can ask my buddy John, I was with him at the lounge cart.”  
        * Might be true, might be a lie.  
    * **Twist (optional)**  
    * Was the crime committed before the train ride? (poisoning, theft, etc)  
      * Culprit is not on the train, potentially  
    * **Was there even a crime?**  
      * A kid hid themselves  
      * Victim lost an item  
    * **Victim was also culprit**  
      * Suicide? Hurt themselves to put the blame on someone else and get them in trouble?  
    * **Red herrings**  
      * A passenger was guilty… of a different crime  
        * Maybe they are nervous and confess to something else, but wasn’t the actual culprit

  * ### Mystery Generation Order of Operations

1. **Choose Mystery Blueprint (CURRENTLY, ONLY MURDER IS A TEMPLATE)**  
   1. Blueprints will define the type of mystery to be generated.  
   2. Generic character setups  
      1. Victim blueprint (optional)  
      2. Culprit & accomplice setup (optional)  
   3. The blueprint should define how the mystery should be solved in broad, non-specific steps.  
2. **Pick Theme:**  
   1. Characters, motives, and the murder itself should revolve around a common theme.  
   2. The mystery’s theme is a guide used by every aspect of the mystery generation to ensure the creation of consistent narratives with dramatic throughlines.  
   3. The goal of having a mystery theme is to creatively validate the coherence of the setup, leading to a more engaging mystery-solving process.  
3. **Pick a motive**  
4. **Pick a culprit and a victim**  
5. **Generate crime**  
   1. Method  
   2. Circumstances  
   3. Culprit plan (culprit’s agenda)  
6. **Generate constellation (touchstone map/graph)**  
   1. Instantiate  generic mystery constellation with only the start and end touchstones.  
   2. Instantiate characters  
      1. Alibis  
      2. Physical location  
      3. Determine who reported the crime (if it’s not the culprits plan to “discover” it and feign innocence)  
   3. Fill in constellation with interesting paths  
      1. Add touchstones and leads by generating evidence and information with the intention of forming trails that connect the first and final touchstones. Plain information, including rumors, gossip, and any second hand data gathered from characters, will count as leads, which are the edges of the constellation graph. Evidence, including confessions, confirmed or denied alibis, or any other piece of assured knowledge, will count as touchstones, the constellation’s nodes.  
          or evidence that is connected to should be considered a touchstone  
         1. Clues  
         2. What is necessary to make the culprit confess if possible  
         3. What evidence is necessary to convince police/passengers (TBD)  
         4. Fuck-ups (by the culprit or otherwise)   
         5. Witnesses  
         6. Miscellaneous  
      2. Generate red herrings

   * ### Generating Mystery Constellation

     * Once the core mystery is formulated and accessible by json file (whether through distinct creation or procedural generation), the mystery is parsed by the system during loading. Once deserialization is complete, all created objects are stored in the persistent GameController under the class MysteryCore for easy access anywhere during runtime.   
     * The mystery constellation is then generated by accessing all MysteryNodes in the deserialized json objects. For each node present in the json, a VisualNode representation is created \- this appears to the player based on whether they have “discovered” that particular piece of evidence through discussion with NPCs, interaction with evidence objects, or completion of minigames. Similarly, Connections are created based on the deserialized objects and will only appear if both associated nodes have been discovered by the player.

* ## ***Emergent Dynamism*** {#emergent-dynamism}

  * ### Character’s Opinions

    * TBD

# **ART** {#art}

* ## ***Camera Setup*** {#camera-setup}

  * ### 2/3rd Top-Down perspective

* ## ***Artstyle*** {#artstyle}

  * ### 3D environments, 2D characters

    * Light effects from the 3D environment need to react with the 2D characters  
    * 2D characters sprites and animations need to fit within the perspective of the 3D environment

* ## ***Aesthetics*** {#aesthetics}

  * ### Mid Century futurism

    * Jetsons  
    * Space Era illustrations  
    * 60s modernism

  * ### Swinging Sixties

    * Deathloop (game by Arkane)  
    * Psychedelic art  
    * Hippie movement  
    * Late 60s western fashion  
    * Pop-art

* ## ***Animation Containers*** {#animation-containers}

  * ### NPCAnimContainer

    * ![][image1]  
    * ![][image2]  
    * All NPC Animations are stored in the NPCAnimContainer Serialized Object. Once sprite sheets are imported and sliced, they are placed into the NPC Anim Container. These objects will then be assigned programmatically to each NPC character object

# **SCHEDULE** {#schedule}

* ## ***Phase 1: Make A Game*** {#phase-1:-make-a-game}

  * ### Goals

    * The objective of the first phase of the project is to build the foundational gameplay and core systems to support the basic design structure outlined by this document.   
    * The ultimate deliverable for this phase will be to integrate these base systems into a playable alpha for later quality assurance, testing, debugging, and balancing. The playable alpha must meet the lowest baseline level of stability and functionality requirements. 

  * ### Playable Alpha

    * The playable alpha must contain a single round of the game, where the player will be able to complete a generated mystery by collecting clues and talking to characters within a generated environment.   
      * The mystery engine’s minimum requirement is that it needs to successfully generate a mystery constellation from a mystery template.  
        * At this stage, the engine doesn’t need to account for multiple templates or even optimize for the most interesting combinations of factors.  
      * The dialogue system needs to be a working prototype that allows the players to talk to characters by inputting text and reading tailored responses that align with each character’s knowledge parameters.  
        * This version doesn’t need to account for emergent dynamism like a character’s opinions towards the player or for complex social gameplay.  
        * The minimum requirement is that the player should consistently be able to collect the information necessary to solve the generated mystery constellation.  
      * The alpha shouldn’t crash when the player performs mundane actions like navigation and start/end dialogue.  
      * The alpha should contain a main menu that allows the player to exit the game (closing it), access a basic settings menu, and go into the main game scene by pressing “Play.” The settings menu must allow players to adjust the volume, switch between fullscreen and windowed, adjust the game's display resolution, and go back to the main menu. The pause menu should be implemented in the same scene as the game, and should allow the player to go back to the main menu (stop the game, not exit) and change the volume.  
        * The pause menu should be a transparent overlay over the game, not a separate scene.  
        * The pause menu doesn’t need to actually “pause” the game at this stage

  * ### Teams

    * **Foundational Systems Team**  
      * James: Procedural generation of carts and their interiors  
      * Jorge: UI, menus (including main menu and a pause menu), and scene and state management of basic game states.  
      * Noah: 3D environment and camera angle setup, 2D character integration (including player movement), and basic commands.  
    * **Mystery Team**  
      * Charles: Dialogue architecture and language model integration  
      * Matt: Generation of mystery constellation  
    * **Art Team**  
      * Dana: Concept art and initial character sprites

* ## ***Phase 2: Make It Good*** {#phase-2:-make-it-good}

  * ### Goals

    * The goal  is to improve the quality of the existing systems and scale them up, with the objective of ending up with a more engaging and well-rounded gameplay experience. Ultimately, Phase 2 will be the difference between a proof of concept and an actually fun game.  
    *  Art will become far more important in this step, since it’ll be needed to get rid of the early development-stage look of the game, giving it a stronger personality.  
    * TBD

  * ### Teams

    * TBD

* ## ***Phase 3: Make It Better*** {#phase-3:-make-it-better}

  * ### Goals

    * Polishing the game, fixing bugs, adding sound & music, and other adjustments.  
    * TBD

  * ### Teams

    * TBD

# **IDEAS & FUTURE PLANS** {#ideas-&-future-plans}

* ## ***Gameplay Loop Scaling*** {#gameplay-loop-scaling}

  * ### Cross round progression

    * Infinite game design  
      * Game goes on, giving the player mystery after mystery until some catastrophic failure  
    * Crime rings, gain or lose strength for subsequent rounds based on the results of the current one

  * ### Car Generation

    * Car templates

  * ### Dialogue

    * Alibi system? NPCs could link their behavior to specific places on the train at specific times  
    * Interrupt NPCs mid-sentence  
    * Send empty text (remain silent)  
    * Recording conversations as evidence

  * ### Additions to the Autonomous Character Controller

    * Characters each have their own goals, and they will procure them while the player tries to solve the broader mystery  
    * Characters can exchange information spontaneously as in accordance to their own goals, and change their short term actions  
    * Characters can initiate conversations with the player character by themselves if so aligns with their goals.  
      * The game wouldn’t feel like being set in a dead world where each character waits idly for the player to engage with them.

  * ### Further Machine Learning Usage

    * Use machine learning for the procedural generation of carts, if other methods like wave-function collapse, markov chains, or weighted random chance (with heuristics and constraints) methods don’t pan out.

  * ### More Mystery Templates:

    * Theft, missing person, terrorism, etc.

* ## ***Story Elements*** {#story-elements}

  * ### Setting

    * Alternate world

  * ### Player Internal Drive

    * Player is the conductor, or someone who watches over the train’s safety

  * ### Mystery Justification

    * Game occurs in a rough part of town  
    * Train operates in a particularly sketchy line  
    * Curse  
    * It’s a literal game match of an rpg or a game like clue

  * ### Scheduled Events

    * Plot segments — every couple hours, a significant event will take place that may alter the situation (an additional murder, a twist, etc)

* ## ***Visuals*** {#visuals}

  * ### 2D SFX and Particles

    * Use 2D sprites and animations for special effects and particles, so that moving foreground elements match characters’ artstyle

  * ### Technicolor Look

    * Custom shader to simulate the separated color channels and visual style of old technicolor film  
    * Post-processing effects to help recreate the old style technicolor filmic look, matching the broad aesthetics and art style of the project

* ## ***Systems Implementations*** {#systems-implementations}

  * ### Autonomous Character Controller Architecture

    * 

[image1]: <data:image/png;base64,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>

[image2]: <data:image/png;base64,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0+xyZ+h80qRJCakD8fHQuiWWO38PHjzoLl686E6cOJFT7oR//vln/4E3bdqUuC7kkvurr77qW0MHDhxwV69e9XH06Hfs2OHvf+nSJbdkyZJU/o8//tg/ww8//ODvu2vXLh/PrwudPXvWjxDwl+0E7b2EEEKITGST+5YtW9KOjx075n/JzuZrVrkDsrRiB+Jt3phY7t98841bsGCBD/ft29edO3cuo9zfffddt23btlSPe+PGjW758uWJawe58wwxpPEXIcejCl988UXqOr169XJHjhxxU6ZM8cfI/fDhwz6eYxoGc+fO9WH13IUQQpRCNrnH0EG+du1aIr5F5E6vd+LEiWli5zjf/HaQO9L85Zdf0tJWrFiRUe5I9/XXX3cjR470jBkzxh09ejRx7TDn/t1336X48ssvfRpypyUU5//111/TnnfmzJm+EUEYub/33nupNObYV61a5cOSuxBCiFIoRO5ff/21mzp1aiK+ReQOgwYNSpM7xzaPJcidvGfOnElLW7RoUUa5nz592kvXYq+db1g+bhA899xzicbF2LFj/WgCYeTONEFI++CDDyR3IYQQZZFP7lu3bnUrV65MxLeo3JHpuHHjvNj5W8hCtXhYvrGx0fXp0yeV9tlnn2WU+/79+92ECRNS+fr3759xXr8YuQPz9wMHDkwdv//++6n7S+5CCCGamlxyx3kbNmxIxFuaXe6AaJE7f21aJmK5I9A9e/a4+vp6t3DhQtfQ0JBR7uPHj3fnz5/3wxRvvPGG27dvn2/Z2GsXK/fFixf7njrD/AzJM0zP1w/Cs2WTOw2SW7du+cWD9j5CCCFENrLJPawt69KlSwqbp0XlDq+88koiLhssYou/CsdQPB/oww8/dLNmzfLz7sQj9zVr1qTy0QCgRfP555+7OXPmJK4LyJ3r23hA7pnSpk+f7q+5fv36tMWAH330UVa5w1tvveV27tzp1wLYawohhBCZyCT3zp07+29fWUaPHp3I26JyF0IIIUR+Msm9WCR3IYQQooJg05dSdoQLcG7Hjh0T1wXJXQghhGgF+AW6++67LyHtQuFcrmGvC5K7EEII0UogaXrwDK8XA+dkEzuUJXdaDXfddZe78847hRBCCNFC4N66urqElwMlyx2x33///e6FF17wv7EuhBBCiJYB9zLnjoutn6FkudNykNiFEEKI1oHNZNq1a5fw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>