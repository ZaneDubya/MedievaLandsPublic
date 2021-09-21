# A Brief Overview

In the 1992 version of Yserbius/Twinion, all map scripts were written in C or C++, which were compiled to 8086 assembly. These ran on the client. I transpiled these from 8086 to C#. The C# scripts are precompiled and run on the server. Scripts can only be changed when the server is shut down, updated, and started again.

## Notes

The 'entire' map script tile event will only fire the first time the tile is entered. It will not fire if the player teleports out and then back in. This means that the map script writer must not rely on entering the tile to set state that would be overwritten by the teleport out event (basically only block/unblock and tile-graphic-show and tile-graphic-hide events).
