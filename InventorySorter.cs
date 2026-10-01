using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySort
{
    internal static class InventorySorter
    {
        private const int HotbarRow = 0;

        /// <summary>True when the inventory screen is open and the player isn't typing or dragging.</summary>
        public static bool CanSortNow()
        {
            if (Player.m_localPlayer == null) return false;
            if (InventoryGui.instance == null || !InventoryGui.IsVisible()) return false;
            if (Chat.instance != null && Chat.instance.HasFocus()) return false;
            if (global::Console.IsVisible() || TextInput.IsVisible() || Menu.IsVisible()) return false;
            if (InventoryGui.instance.m_dragGo != null) return false; // an item is being dragged
            return true;
        }

        public static void SortContainer(Container container)
        {
            if (container == null) return;

            // The player who has a container open owns its ZDO; if not, our changes would be overwritten.
            if (container.m_nview == null || !container.m_nview.IsValid() || !container.m_nview.IsOwner())
            {
                InventorySortPlugin.Log.LogDebug("Container not owned by local player, skipping sort");
                return;
            }

            Sort(container.GetInventory(), InventorySortPlugin.ContainerFill.Value, _ => false);
        }

        public static void SortPlayer()
        {
            Player player = Player.m_localPlayer;
            if (player == null) return;

            bool skipHotbar = InventorySortPlugin.SkipHotbar.Value;
            HashSet<(int, int)> locked = InventorySortPlugin.RespectQuickStackLocks.Value
                ? QuickStackLocks.LoadFor(player)
                : new HashSet<(int, int)>();

            // Equipped items stay in their current slot; everything else sorts around them.
            Inventory inventory = player.GetInventory();
            if (inventory != null && InventorySortPlugin.IgnoreEquipped.Value)
            {
                foreach (ItemDrop.ItemData item in inventory.m_inventory)
                    if (item.m_equipped) locked.Add((item.m_gridPos.x, item.m_gridPos.y));
            }

            Sort(inventory, InventorySortPlugin.PlayerFill.Value,
                pos => (skipHotbar && pos.y == HotbarRow) || locked.Contains((pos.x, pos.y)));
        }

        /// <param name="isReserved">Slots that are neither emptied nor filled by the sort (hotbar, locked slots).</param>
        private static void Sort(Inventory inventory, FillStart fill, Func<Vector2i, bool> isReserved)
        {
            if (inventory == null) return;

            List<ItemDrop.ItemData> all = inventory.m_inventory;
            List<ItemDrop.ItemData> movable = all.Where(i => !isReserved(i.m_gridPos)).ToList();
            if (movable.Count == 0) return;

            if (InventorySortPlugin.MergeStacks.Value)
                Merge(movable, all);

            List<Vector2i> slots = BuildSlotOrder(inventory.GetWidth(), inventory.GetHeight(), fill, isReserved);
            List<ItemDrop.ItemData> ordered = Order(movable);

            if (ordered.Count > slots.Count)
            {
                // Only possible if items sit outside the grid (e.g. a grid-size mod was removed).
                InventorySortPlugin.Log.LogWarning(
                    $"Not enough slots to sort ({ordered.Count} items, {slots.Count} slots); leaving layout as is");
                inventory.Changed();
                return;
            }

            for (int i = 0; i < ordered.Count; i++)
                ordered[i].m_gridPos = slots[i];

            inventory.Changed(); // refreshes UI, weight, and saves container contents to its ZDO
        }

        /// <summary>Slots in fill order: row by row from the chosen edge, left to right within a row.</summary>
        private static List<Vector2i> BuildSlotOrder(int width, int height, FillStart fill, Func<Vector2i, bool> isReserved)
        {
            var slots = new List<Vector2i>(width * height);
            for (int r = 0; r < height; r++)
            {
                int y = fill == FillStart.Top ? r : height - 1 - r;
                for (int x = 0; x < width; x++)
                {
                    var pos = new Vector2i(x, y);
                    if (!isReserved(pos)) slots.Add(pos);
                }
            }
            return slots;
        }

        private static void Merge(List<ItemDrop.ItemData> movable, List<ItemDrop.ItemData> all)
        {
            var groups = movable.Where(IsMergeable).GroupBy(StackKey).ToList();

            foreach (var group in groups)
            {
                List<ItemDrop.ItemData> items = group.OrderByDescending(i => i.m_stack).ToList();
                if (items.Count < 2) continue;

                int remaining = items.Sum(i => i.m_stack);
                int max = items[0].m_shared.m_maxStackSize;

                foreach (ItemDrop.ItemData item in items)
                {
                    int take = Mathf.Min(max, remaining);
                    remaining -= take;

                    if (take > 0)
                    {
                        item.m_stack = take;
                    }
                    else
                    {
                        movable.Remove(item);
                        all.Remove(item);
                    }
                }
            }
        }

        private static bool IsMergeable(ItemDrop.ItemData item) =>
            item.m_shared.m_maxStackSize > 1 &&
            (item.m_customData == null || item.m_customData.Count == 0);

        private static string StackKey(ItemDrop.ItemData item) =>
            $"{item.m_shared.m_name}|{item.m_quality}|{item.m_variant}|{item.m_worldLevel}";

        private static List<ItemDrop.ItemData> Order(List<ItemDrop.ItemData> items)
        {
            StringComparer nameCmp = StringComparer.CurrentCultureIgnoreCase;

            // Group by item type so stacks of the same item always sit together.
            var types = items
                .GroupBy(i => i.m_shared.m_name)
                .Select(g => new
                {
                    Name = LocalizedName(g.Key),
                    Total = g.Sum(i => i.m_stack),
                    Items = g.OrderByDescending(i => i.m_quality)
                             .ThenByDescending(i => i.m_stack)
                             .ThenByDescending(i => i.m_durability)
                             .ToList()
                });

            var sorted = InventorySortPlugin.Mode.Value == SortMode.Quantity
                ? types.OrderByDescending(t => t.Total).ThenBy(t => t.Name, nameCmp).ToList()
                : types.OrderBy(t => t.Name, nameCmp).ToList();

            if (InventorySortPlugin.Reverse.Value)
                sorted.Reverse();

            return sorted.SelectMany(t => t.Items).ToList();
        }

        private static string LocalizedName(string token) =>
            Localization.instance != null ? Localization.instance.Localize(token) : token;
    }
}
