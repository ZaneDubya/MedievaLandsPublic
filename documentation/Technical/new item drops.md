# Treasure Tables and Item Drops in MedievaLands


## Original Game Mechanic, as reimplemented by MedievaLands

When a monster stack is created, it is assigned two templates: a 'name template', which references a 'level template'. Each map has 40 name templates unique to that map. All maps share the collection of level templates. In Yserbius and Twinion, random encounters will only spawn monsters using the first 24 name templates from that map. The final 16 templates are only seen in static repeated encounters; static encounters can also include the first 24 name templates.

One of the data values of a name template is the 'DropChance' value, which has a range of 0 to 100. This is the chance, out of 100, that the monster stack will drop an item if it is defeated. After combat ends, the game compares a random number between 0 and 100 to the name table's DropChance. If the random number is lower than the DropChance, the server creates a loot item for the player. Many name templates have DropChance values of 0 - these monster stacks will never drop an item. By comparison, a DropChance of 100 will always drop a single item.

Each map has a 'drop table' of 32 indexes unique to that map. These indexes correspond to item template indexes that will represent the generated loot item. There are exactly 253 possible item template indexes, 1 to 254 (0 equates to 'no item' and 255 equates to 'null item').

In addition to random loot items, a map script can set explicit drops for a scripted encounter. These items will always be given to all living players after the scripted encounter concludes.

Dead players do not receive loot items.

## New Game Mechanic

As a preliminary matter, items are no longer generated only from the original game's 253 item templates. The game can now host an infinite number of types of items - all of the original item types, including cross-game item types, and any new types that I dream up.

Item drops are picked when a monster stack is generated. At this time, we know:
- The map (0-33) and map monster index (0-39)
- The monster's type (0-44)
- The monster's 'level' (1-15; note: 1-indexed)
- The monster stack count
- The monster drop %
- The monster gold amt (per monster)
- All the monster stats (ini, str, def, dex, armor, matksav)
- The player's level and stats (guild, race, str, etc).

# Proposal:

Drops are now encoded as 'treasure tables'
- A treasure table represents (1) the number of possible picks from that table, (2) the chance of 'no drop' for each pick, (3) the chance a pick will be a magic item, set item, or unique item, and (4) references to subtables or items that the pick may be chosen from.
- Some tables are defined by name in a csv file. Some are generated at runtime from 'auto table' values in the new item definition csv file.

Every map has three treasure table references: normal, extra, and unique.
- These are the treasure tables used for all monsters in the map.
- The game "makes a pick" when attempting to choose an item from the table. 
- A pick can result in no drop. Most tables have large chances (60-70%) of no pick.

When combat starts, the game will check if treasure drops have already been set for this encounter (by the map script).
- If treasure already exists for the encounter,
  - then the game will use the preset drops, 
  - and will also make a pick from the extra table for each stack beyond the first stack.
- If treasure does not already exist, and the encounter does not include a unique monster, 
  - then the game will make a pick from the normal table for the first stack,
  - and will make a pick from the extra table for all stacks beyond the first stack.
- If treasure does not already exist, and the encounter includes a unique monster, 
  - then the game will make a pick from the unique table for the first stack,
  - and will make a pick from the extra table for all stacks beyond the first stack.

## Choosing a Pick
- Choose whether the picked item will have a quality of normal, magic, set, or unique.
- Generate a random number between 0 and (noDrop + sum(pickChances) - 1).
- Use this random number to pick no drop, or a table or item based on this random number.
  - If the pick is no drop, stop and don't add an item.
  - If the pick is a table, start over picking from that table (but maintaining the initial choice of quality).
  - If the pick is an item, continue with that item.

## Pick Quality
- For each level of high quality (unique, set, magic), attempt to generate an item at that quality level...
- Chance = (((1000+QualityMF×PlayerMF)÷1000)×QualityDropFactor+QualityDropBonus*100)
  - QualityMF: 100 for magic, 80 for set, 60 for unique.
  - PlayerMF: Player's MF value 
  - QualityDropFactor: 334 for magic, 50 for set, 25 for unique
  - QualityDropBonus: as treasure table's bonus value. 1 pt = 1% increased drop chance

In summary, a player with no MF will see unique items 0.25% of the time, set items 0.5% of the time, and magic items 3.34% of the time. It takes 10 magic find to double magic item drops, 12.5 pts to double set drops, and 16.5 to double unique drops.

The game attempts to generate unique items first. If the unique roll succeeds, it checks to see if a unique item can be generated. Unique (and set) items are kept in separate tables. The important thing to note is each unique/set item has an item type and a level and a drop ratio. The game selects all unique items matching the item type and with a level less than or equal to the player's level, and chooses a drop based on the ratios of all selected unique items.

If the pick quality check fails, or if there are no unique items available, then the game attempts the next quality level, ending at normal.

