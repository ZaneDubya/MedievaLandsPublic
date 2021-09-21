# MedievaLands Map Scripting
*In implemented for Modern maps*

'Modern' games are Yserbius-style multiplayer dungeon crawlers which run on the MedievaLands service.

This document describes how maps and map scripts are loaded, processed, and invoked for Modern games.

## File location, naming, and loading

For each game, all map files and map script files are stored in a single folder.

Each map file for a game is uniquely named.
The map index should in the filename.
The map filename must end in the extension .map.

The map script file must have the same file name as the map file, with the extension .lox.

## Map file format

    Header
    <File>
    3b      "LMP"
    1b      File format version byte = 0
    2b      Map index.
    2b      Revision.
            // In a future version:
            // For each of walls, floors, ceilings, decos:
            // 7bit    Texture count
            // *       7bitprefixedascii paths for textures x Texture count
    <Map>
    3b      "MAP"
    1b      Map data format version byte = 0
    1b      Map width in tiles
    1b      Map height in tiles
    <Tile>
    // 1 byte if not present, 24 bytes if present, potentially more if there are more than 127 textures or 127 events in the map.
    6x7bit  Texture indexes for level geometry: Floor, Walls (NESW), Ceiling
            In version 0, Ceiling index is ignored.
    6x7bit  Texture indexes for Decos: Floor, Walls (NESW), Ceiling.
            In version 0, Ceiling index is ignored.
    12x7bit Event indexes for each event type. 0 = no event.
            Event types are: TileArrive, TileLeave, FaceLook (NESW), FaceStep (NESW), UseItem, UseSpell.
    </Tile>
    </Map>
    </File>
    
## Map scripts and invocation

As an aside, tile and party flags are stored per player, and copied from the party leader when the party chanat the end of each event invocation.

Map scripts are written in the lox language and run by my LoxScript "Gears" virtual machine.

Map script functions are associated with map events with a non-zero event index.

The association is explicit in the function name: function names must be The map script function is then invoked for each player.

### Flags

There are three types of flags:
- Map flags: persistent on the map.
