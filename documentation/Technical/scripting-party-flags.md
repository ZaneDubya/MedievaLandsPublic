# MedievaLands flags

What are the types of flags in the game? How are they referenced and interpreted? What values can they hold?

- As necessary preface, MedievaLands runs two kinds of games:
    - Legacy games have capabilities similar to the Yserbius era.
      - Legacy game flags are referenced by tuple type and index. e.g. Map 0 and Dungeon 100.
      - They are integers and can have any value between 0 and 255.
      - They are interpreted as a byte or boolean value; boolean 0 is false, boolean non-zero is true.
    - Modern games have expanded capabilities.
      - Modern game flags are referenced by tuple type and name. e.g. Map KILLED_GHOST and Dungeon ACTIVATED_PORTAL.
      - They are integers and can have any value between 0 and 255.
      - They are interpreted as a byte or boolean value; 0 is boolean false, non-zero is boolean true.
- There are two kinds of flags: session flags and progression flags.
  - Session flags are temporary. They are cleared when a predefined 'end of session' event occurs.
    - Session flags are always the same for all party members.
    - When I join a party, the party leader's session flags are copied to my session flags. My session flags are overwritten.
    - Entering the dungeon (moving to the dungeon entrance, as a new adventure or after death) clear all session flags.
    - Returning to a dungeon through the lower entrance is distinct from entering the dungeon, and does not clear session flags.
  - Progression flags are permanent. They are never cleared.
    - Progression flags can be different for party members.
    - Progression flags are not coped on party join.
- *Tile Flags* are session flags. Tile flags are cleared every time the party moves into a new tile.
- *Map Flags* are session flags. Map flags are cleared every time the party moves into a new map.
- *Progression Flags* (in Legacy, these are currently called 'Dungeon flags') are progression flags.
