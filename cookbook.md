*   Persistence database for storing state
    *   Player persistence
    *   World state (if needed)
    *   Analysis tools
    *   Reversion/correction tools
    *   DB cleanup tools/game design
*   Template database for storing source data
    *   Objects
    *   Creatures
    *   Other forms of content
    *   Version control
    *   Statistical analysis tools
*   Networking
    *   Network traffic culling system
        *   Vis/invis
        *   Distance
        *   Per user
        *   Notability/importance
    *   Connection manager
        *   Link loss, reconnection
    *   Encryption
    *   Verification and security, including flood control etc
    *   User multiplexing (“user server”)
    *   Server cluster management
*   Commerce system/shop
    *   Credit card and prepay card support
    *   Refunds
    *   Fraud handling
    *   Shop backend
    *   Metrics
*   Community support
    *   Forums
    *   Community tools
    *   Event tools
*   Metrics system
    *   Dashboards
        *   Pre-made
        *   User-definable
    *   Logs
        *   Account activity
        *   Network activity
        *   Commerce activity
        *   User activity
        *   Gameplay events/balance
    *   Alert/pager system
*   Server simulation
    *   Event handler, manager, dispatcher
        *   Recursion limits/loop trapping/performance monitoring
    *   Spatial simulation
        *   Terrain system
            *   Tile/heightfield/mesh
            *   Pathing map
            *   Region system
        *   Room system
            *   Exits
            *   Mirroring
            *   Instancing
        *   Coordinate system
        *   Cross-server handoffs
        *   Player load balancing
    *   Physics
        *   Collision
        *   Forces
    *   AI support
        *   Range-based triggers
        *   Event-based triggers
        *   Instantiation/destruction triggers
    *   Alternate simulation layers
        *   Time-based
            *   Weather
            *   Day/night
        *   Influence maps for AI
        *   Cellular automata
        *   Resource maps for harvesting/crafting
*   Players
    *   Identity profile
        *   Account
            *   Name/pw/etc
            *   Source/geolocation
            *   Join date
            *   Commerce info (credit card, etc)
            *   RMT currency
            *   Content access/service tiers (e.g. expansions active, etc)
            *   CS history
        *   Characters
            *   Basics
                *   Name(s)
                *   Race/species
                *   Class
            *   Group affiliations
                *   Guild, towns, etc
                *   PvP status/flags
            *   Statistics
                *   Game stats
                *   UI preferences
                    *   Keymappings
                    *   Screen layout
                    *   Macros/bindings/custom UI
                *   Temporary affects
                    *   Blind, dizzy, stunned, etc
                    *   Position
                    *   Lost link/connected
                *   Skills/powers
                    *   Skills
                    *   Levels
                *   Currencies
                    *   XP
                    *   Game money
                *   Character cosmetic customization
                    *   Hair
                    *   Skin
                    *   Morph targets/faces/etc
            *   Inventories
                *   Physical
                    *   System (grid, capacity, weight)
                    *   Limits
                    *   Nesting/abuse caps
                    *   Binding
                    *   Paperdoll
                *   Friends lists
                *   Achievements/badges
                *   Skills/powers
                *   Macros
                *   In-world ownerships
                    *   Houses
                    *   Vehicles
                    *   Shops
                    *   Pets
            *   Metrics
                *   *   Play frequency
                    *   Session length
                    *   Logs
            *   Intangibles/non-game affecting
                *   Description
                *   Matchmaking
                *   etc
*   Social structures
    *   Chat
        *   Abuse prevention
            *   Word filter
            *   Circular buffer and reporting
            *   DDOS/spamming
            *   Muting
            *   Blocking
        *   Chat window
            *   Logging
            *   Filtering/tabs
            *   Color coding
        *   In-world
            *   Chat bubbles
            *   Visible signals
        *   Translation service
        *   Channel system
            *   Join
            *   Leave
            *   Kick
            *   Create
            *   Destroy
            *   Q&A system
                *   Mod queue
                *   Submit
                *   Approve/deny
                *   Mod status
    *   Guilds
        *   Identity
            *   Name
            *   Abbreviation
            *   Heraldry/visuals
        *   Membership
            *   Join
                *   Leave
                *   Kick
            *   Powers structure
                *   Delegation
                *   Tiers
                *   Change of leader
            *   Shared inventories
                *   Banks
                *   Enemy guilds/PvP flagging
            *   Shared ownership of structures
    *   Friends
        *   Location notification
        *   Alerts
    *   Grouping
        *   HUD management
        *   Join/leave/kick
        *   Disconnection and distance handling
        *   Group chat
        *   Matchmaking systems
        *   XP/loot management
    *   Emotes and socials
        *   Textual pre-written
            *   No target
            *   Self target
            *   Other target
        *   Animated
            *   Solo
            *   Tandem
                *   Consent system
            *   Chat parsing for play
            *   Eye/head tracking for recent chats
        *   Moods
    *   Text communication
        *   Say
            *   Alternates
            *   Tell
            *   Whisper
            *   Emote
        *   Voice communication
*   Customer service support
    *   Live paging
    *   CRM database
    *   Ticket system
    *   Power tiers
    *   Escalation and management process
    *   QC subsampling
    *   Logging
    *   Metrics
*   Objects
    *   Object types
        *   Flags (stream/not, pick up, etc)
        *   Inheritance structure
        *   Affect system
    *   Structures
        *   Ownership
        *   Access
        *   Instancing
    *   Vehicles
        *   Physics
        *   Controls
        *   Ownership
        *   Storage/retrieval
        *   Multi-user
    *   Interactables
        *   Event triggers
        *   Dynamic light attachment
        *   Dynamic sound attachment
        *   Animation
        *   Serverside UI definition
    *   Stackables/fungibles
    *   Usables
        *   Weapons
            *   Projectile
            *   Melee
        *   Wearables
            *   Clothing
            *   Armor
        *   Consumables
            *   Food/drink
            *   Potions/wands/etc
*   Creatures
    *   Statistics
        *   Affects
        *   Stats
        *   Hatreds/friendships
        *   Ranges
        *   Balancing tools/metrics
    *   Spawning
        *   Point
        *   Region
        *   Timer-based
        *   Population-based
    *   AI
        *   Grouping
        *   Factions, hatreds, etc
        *   Powers
        *   Conversation system
        *   Wandering, patrolling, etc
        *   Movement speeds
        *   Pathing parameters
        *   Simulation map usage (influence, resource, etc)
    *   Shopkeepers
        *   Buying price estimator routines
        *   Stock maintenance/spawning
        *   Interface
        *   Metrics
*   Scripting
    *   Quest system
        *   Templated data
        *   Conversations/flagging
        *   Maps/NPC tagging
    *   Arbitrary
        *   Queuing, deque, priority, fallthrough
        *   Event handlers
            *   Collision and range
            *   Timer
            *   Player interaction
            *   Physics
            *   Spawn/destroy
            *   Messages
        *   API
            *   Statistics
            *   UI
            *   Sound
            *   Effects
        *   Messaging
            *   Combat
            *   Spawning
            *   Etc (basically, every system)
*   Game systems
    *   Combat system
        *   PvP handling
            *   Event ownership
            *   Object ownership
            *   Guilds
            *   Duels
            *   Region-based
            *   Flagging systems
                *   UI
        *   Core system
            *   States
            *   Affects
            *   Ranges
            *   Line of sight
            *   Balance logging
    *   Advancement system
        *   Skills
            *   Data
            *   Prerequisite tables
        *   Levels
            *   Data
            *   Points accrual system(s)
        *   Reputation systems
    *   Crafting system
        *   Locating resources
        *   Harvesting
            *   Real time
            *   Asynchronous farming
        *   Refining
        *   Combining
        *   Templates
            *   Recipe book and interface
            *   Unlock mechanism
        *   Experimenting and customization
        *   Mass production
        *   Maker’s marks
        *   Repair and damage system
    *   Housing system
        *   Access permissions
        *   Kick/ban/etc controls
        *   Lockdown and decoration
        *   Transfer
        *   Rent or expiration or other sprawl control
    *   Travel system
        *   Teleport or transit points
        *   Bookmarking
    *   Other possible systems
        *   Pets/hirelings
        *   Magic
        *   Politics
        *   Territory
        *   Exploration
        *   Player shops
        *   Events
        *   Async behaviors (mining, manufacturing, farming, etc)
        *   Feeds in and out of game
    *   Tutorial
