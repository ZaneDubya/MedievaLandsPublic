# CommunityKit Items
Now that the new item framework is in, we can add any item we can imagine. Of course, items are now much larger in memory and in network traffic... but they're not much larger in serialization. This was planned, and it allows me to adjust item stats with just a server reboot (can I do it on the fly?)

## Item Hierarchy and Serialization
The base item type is GenericItem.
- GenericItem serializes its globally unique id (aka the 'serial') and its type reference index. These two values are used to initialize most of the item's properties.
- GenericItem also serializes its current charges and current stack. Most items do not use these values, and they will be 0 and 1 respectively.
- GenericItem has a GameID of NotDefined, and cannot exist in any game.

Items that exist in Yserbius and Twinion are type YsItem. YsItem introduces all the stat effect values that are used in Yserbius.
- Inherit from GenericItem.
- Does not serialize anything at present, but could in the future host suffixes/prefixes, buffs, etc.
- YsItem has a GameID of Yserbius, and can only exist in Yserbius. TwItem, which inherits from YsItem, overrides its GameID to Twinion.

Items from the original Yserbius and Twinion games are 'Templated', and get most of their information from the Yserbius/Twinion item template data.
- YsItemTemplated inherits from YsItem, and serializes its template index.
- TwItemTemplated inherits from TwItem : YsItem, and serializes its template index.

That's it for now...