NOTE: Review treasure drop system to make sure it uses all fields as currently implemented: quality system, new affixes, limited affix selection, mapLevelReq (and map levels!), dropRatio, dropTable, CLvlReq, Templating, ItemType, CostMul, random icon selection.


# Item Picks in MedievaLands

This is a description of the item generation mechanic in MedievaLands. It is a replacement of the vast majority of the item mechanics in Yserbius.

## Item Data Tables

In the original game, there were only 253 item templates. This is now expanded: items are defined by their attributes, which are generated from item data tables. Some of the original item templates from the original game can still be spawned - a few unique items (e.g. Barbarian Axe) and items that are referenced by their template index in map scripts (quest items, lockpicks, a few others).

There are four item data tables:
* medievalands-itemdata - new-tt.csv - the new treasure tables, which determine which items are picked when a monster is killed.
* medievalands-itemdata - new-items.csv - the new base item data, created in large part from Reginald's work on the MedievaLands forums.
* medievalands-itemdata - new-mods.csv - the new modifier data, which describe the new item bonuses, assembled with Reginald's guidance.
* medievalands-itemdata - new-unique.csv - the new unique items, which are picked very rarely and are somewhat more powerful than normal items.

In addition to this new data, each map in Yserbius and Twinion has been appended with the following data:
* One "Map Level" variable, from 1-30. This represents the difficulty of monsters on the map. Higher map levels correspond to better modifier affixes and some better items.
* Two "Treasure Table" references. These are the treasure tables that are used to pick which items will pick. The first table is the "standard" pick table - it is used to pick an item for the first pick in a battle. The second table is the "extra" pick table - it is used to pick subsequent picks in battle.
* Note: If I ever add "unique" encounters to the game (these would be extra-tough monster encounters that a player could find on each map), then I would add an additional "hard" treasure table that would be similar to standard, but would have a better chance of picking more powerful weapons.

All other map variables, including all monster information, are the same, except that the original map treasure table is not used, and the "chance of no pick" is now a constant controlled by the treasure tables, and the original no pick chance value from the monster template is ignored.

## The Treasure Tables and Picks

So how does the game pick which items to pick? The process starts with the treasure tables mentioned above.

A treasure table represents (1) the number of possible picks from a table, (2) the chance a pick will be a "magic quality" pick (with one or two bonus modifiers) or a "unique quality" pick (with three bonus modifiers, one of which is more powerful than any magic item), and (3) a list of either items or sub-tables that the pick will be chosen from, including a chance that no pick will be chosen. Most tables have large chances (60-70%) of no pick, which helps the player not be overwhelmed with items after each combat.

Most of the treasure tables are defined in the "new-tt.csv" file noted above. Some are generated at runtime. These are 10 tables filled with armor types (Armo1 through Armo10) and 10 tables filled with weapon types (Weap1 through Weap10), which are generated at runtime based on the "DropTable" data field in the "new-items.csv" file.

Items are picked during combat as monsters die (or a player uses "Pickpocket" to generate an item drop for that player).
* The first item drop is always picked from Standard (or Unique if this a unique monster encounter), unless one or more items were set by the map script.
* Otherwise all drops (up to a total of 5 picks) are picked from Extra.

## Choosing a Pick in several easy steps.

When a pick is needed, the game is passed a treasure table, the player's magic find value, and a set of bonuses to the chance of better quality drops.

The first thing that happens is that the game picks drops from the table.
- Generate a random number between 0 and (noDrop + sum(pickChances) - 1). Use this random number to pick no drop, or a table or item choice based on this random number.
- If the pick is no drop, stop and don't add an item.
- If the pick is a table, start over picking from that table (but maintaining the initial choice of quality).
- If the pick is an item, continue with that item.
  
Second, the game chooses the quality for the pick:
- For each level of "higher than normal" quality (unique, magic), attempt to generate an item at that quality level...
- Chance = (((1000+QualityMF×PlayerMF)÷1000)×QualityDropFactor+QualityDropBonus*100)
  - QualityMF: 100 for magic, 80 for set, 60 for unique.
  - PlayerMF: Player's MF value 
  - QualityDropFactor: 334 for magic, 50 for set, 25 for unique
  - QualityDropBonus: as treasure table's bonus value. 1 pt = 1% increased drop chance

In summary, a player with no MF will see unique items 0.25% of the time, set items 0.5% of the time, and magic items 3.34% of the time. It takes 10 magic find to double magic item drops, 12.5 pts to double set drops, and 16.5 to double unique drops.

The game attempts to generate unique items first. If the unique roll succeeds, it checks to see if a unique item can be generated. Unique (and set) items are kept in separate tables. The important thing to note is each unique/set item has an item type and a level and a drop ratio. The game picks all unique items matching the item type and with a level less than or equal to the player's level, and chooses a drop based on the ratios of all selected unique items.

If the pick quality check fails, or if there are no unique items available, then the game attempts the next quality level, ending at normal.

