# MedievaLands Map Scripting
*In implemented for Modern maps*

'Modern' games are Yserbius-style multiplayer dungeon crawlers which run on the MedievaLands service.

This document describes how maps and map scripts are loaded and invoked for Modern games.

Each map file for a game must be uniquely named.
The map index should in the filename.
The map filename must end in the extension .map.

The map script file must have the same name as the map file.
The map script file must endi n the extension .lox.

## Map file format

    Header
    <File>
    3b      "LMP"
    1b      File format version byte = 0
            // In a future version:
            // 7bit    Texture count for walls, floors, ceilings, decos.
            // *       7bitprefixedascii paths for textures x Texture count
    1b      Map index.
    <Map>
    3b      "MAP"
    1b      Map format version byte = 0
    1b      Map width in tiles
    1b      Map height in tiles
    <Tile>
    // 24 bytes at minimum, can be more if there are more than 127 textures or 127 events in the map.
    6x7bit  Texture indexes for Borders: Walls (NESW), Floor, Ceiling
            In version 0, Ceiling index is ignored.
    6x7bit  Texture indexes for Decos: Walls (NESW), Floor, Ceiling.
            In version 0, Ceiling index is ignored.
    12x7bit Event indexes for each event type. 0 = no event.
            Event types are: TileArrive, TileLeave, FaceLook (NESW), FaceStep (NESW), UseItem, UseSpell.
    </Tile>
    </Map>
    </File>
    
## Map scripts and invocation
