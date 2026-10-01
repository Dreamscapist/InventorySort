# Building InventorySort

## Requirements
- .NET SDK (any version that builds `netstandard2.1`)
- Valheim at `D:\Steam\steamapps\common\Valheim`
- r2modman profile `DreamHeim2` with BepInExPack installed

Paths are set as properties at the top of `InventorySort.csproj` (`ValheimDir`, `ProfileDir`) — change them there if they move.

## Build
```
dotnet build -c Release
```
The post-build step copies `InventorySort.dll` to
`DreamHeim2\BepInEx\plugins\Dreamscapist-InventorySort\`.

`System.Net.Http` / `System.IO.Compression` version-conflict warnings are expected noise.

## Project layout
| File | Purpose |
|---|---|
| `Plugin.cs` | Plugin entry, config bindings, sort-key handling |
| `InventorySorter.cs` | Merge + order + slot placement logic |
| `QuickStackLocks.cs` | Reads QuickStackDeposit's `…quickstackdeposit.locks.txt` (soft dependency) |
| `SortButtons.cs` | Harmony postfix on `InventoryGui.Awake`; clones the Take All button into two Sort buttons |

## Implementation notes
- `assembly_valheim` is publicized for `Inventory.m_inventory`, `Inventory.Changed()`, `InventoryGui.m_currentContainer`, `m_dragGo`, `m_takeAllButton`, `m_player`, `m_container`, `Container.m_nview`.
- Sorting rewrites each item's `m_gridPos` in place and calls `Inventory.Changed()`, which refreshes the grid and saves container contents to the ZDO (there is no public `Container.Save()` in 1.0).
- Container sorting is skipped unless the local player owns the container's ZDO (opening a container grants ownership).
- The cloned button gets a fresh `ButtonClickedEvent` (persistent Take All listeners would otherwise survive `RemoveAllListeners()`), and its `UIGamePad` hint is removed.

- QuickStackDeposit locks are read from the locks file on every sort (format `playerId|x,y;x,y`, keyed by `Player.GetPlayerID()` exactly as QuickStackDeposit writes it), so there is no hard dependency on its DLL.

## Releasing
Bump the version in **both** `[BepInPlugin]` and `PluginVersion` (they share the const) to avoid BepInEx loading a stale DLL.
