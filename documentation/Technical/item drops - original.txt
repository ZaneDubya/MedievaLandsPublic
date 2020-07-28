# Item Drops in Yserbius

*As reimplemented in MedievaLands.*

When a monster stack is created, it is assigned two templates: a 'name template', which references a 'level template'. Each map has 40 name templates unique to that map. All maps share the collection of level templates. In Yserbius and Twinion, random encounters will only spawn monsters using the first 24 name templates from that map. The final 16 templates are only seen in static repeated encounters; static encounters can also include the first 24 name templates.

One of the data values of a name template is the 'DropChance' value, which has a range of 0 to 100. This is the chance, out of 100, that the monster stack will drop an item if it is defeated. After combat ends, the game compares a random number between 0 and 100 to the name table's DropChance. If the random number is lower than the DropChance, the server creates a loot item for the player. Many name templates have DropChance values of 0 - these monster stacks will never drop an item. By comparison, a DropChance of 100 will always drop a single item.

Each map has a 'drop table' of 32 indexes unique to that map. These indexes correspond to item template indexes that will represent the generated loot item. There are exactly 253 possible item template indexes, represented by integer values 1 to 254 (0 equates to 'no item' and 255 equates to 'null item').

In addition to random loot items, a map script can set explicit drops for a scripted encounter. These items will always be given to all living players after the scripted encounter concludes.

Dead players do not receive loot items.
