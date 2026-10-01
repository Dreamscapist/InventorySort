# InventorySort

Sort your inventory and storages with one click or one key.

## Features

- **Sort buttons** next to the storage panel and your inventory panel.
- **Sort key** (default **T**) sorts the open storage while the inventory screen is open. With no storage open, it sorts your own inventory (can be turned off).
- **Storages fill from the bottom** by default — sorted items start at the bottom-left slot and work upward. Switch to top-first in the config.
- **Two sort modes:** alphabetical, or by quantity (item types you have the most of come first).
- **Reverse order** option.
- **Stack merging** — partial stacks of the same item are combined.
- **Hotbar is left alone** when sorting your own inventory (configurable).
- **Equipped items stay put** when sorting your inventory (configurable).
- **QuickStackDeposit locked slots are respected** — items in locked slots stay put, and everything else is sorted around them. https://valheim.hexium.gg/mods/Freelancers_Union/QuickStackDeposit

Works with carts, ships, and any other container.

## Configuration

`BepInEx/config/dreamscapist.valheim.inventorysort.cfg` — editable in r2modman's config editor or live with ConfigurationManager.

| Section | Setting | Default | Description |
|---|---|---|---|
| General | SortKey | T | Key that sorts the open storage |
| General | KeySortsPlayerWhenNoContainer | true | Sort key sorts your inventory when no storage is open |
| Sorting | SortMode | Alphabetical | `Alphabetical` or `Quantity` |
| Sorting | ReverseOrder | false | Z→A, or fewest-first for quantity |
| Sorting | MergeStacks | true | Combine partial stacks |
| Sorting | StorageFillStart | Bottom | `Bottom` or `Top` |
| Sorting | PlayerFillStart | Top | `Bottom` or `Top` for your own inventory |
| Sorting | SkipHotbar | true | Don't move items in the hotbar row |
| Sorting | IgnoreEquippedItems | true | Don't move equipped items |
| Sorting | RespectQuickStackDepositLocks | true | Don't touch slots locked by QuickStackDeposit |
| UI | ShowSortButtons | true | Show the Sort buttons |
| UI | Storage/InventoryButtonOffsetX/Y | 8 / 0 | Button position relative to each panel's top-right corner |

## Notes

- Items with custom data (e.g. from other mods) are sorted but never merged.
- In multiplayer, only the player who has the storage open can sort it — same as moving items by hand.
