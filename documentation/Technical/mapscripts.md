Map scripts are currently written in (precompiled) C#. These cannot be changed until the server is updated and rebooted.

Future iterations of map scripts will be written in a dialect of lox, which will be editable at run time.

An important note is that the 'entire' map script tile event will only fire the first time the tile is entered. It will not fire if the player teleports out and then back in. This means that the map script writer must not rely on entering the tile to set state that would be overwritten by the teleport out event (basically only block/unblock and tile-graphic-show and tile-graphic-hide events).
