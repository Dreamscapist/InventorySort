using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace InventorySort
{
    public enum FillStart { Bottom, Top }
    public enum SortMode { Alphabetical, Quantity }

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class InventorySortPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "dreamscapist.valheim.inventorysort";
        public const string PluginName = "InventorySort";
        public const string PluginVersion = "1.2.0";

        internal static ManualLogSource Log;

        // General
        internal static ConfigEntry<KeyboardShortcut> SortKey;
        internal static ConfigEntry<bool> KeySortsPlayerWhenNoContainer;

        // Sorting
        internal static ConfigEntry<SortMode> Mode;
        internal static ConfigEntry<bool> Reverse;
        internal static ConfigEntry<bool> MergeStacks;
        internal static ConfigEntry<FillStart> ContainerFill;
        internal static ConfigEntry<FillStart> PlayerFill;
        internal static ConfigEntry<bool> SkipHotbar;
        internal static ConfigEntry<bool> RespectQuickStackLocks;
        internal static ConfigEntry<bool> IgnoreEquipped;

        // UI
        internal static ConfigEntry<bool> ShowButtons;
        internal static ConfigEntry<float> ContainerButtonX;
        internal static ConfigEntry<float> ContainerButtonY;
        internal static ConfigEntry<float> PlayerButtonX;
        internal static ConfigEntry<float> PlayerButtonY;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            SortKey = Config.Bind("1 - General", "SortKey", new KeyboardShortcut(KeyCode.T),
                "Sorts the open storage while the inventory is open.");
            KeySortsPlayerWhenNoContainer = Config.Bind("1 - General", "KeySortsPlayerWhenNoContainer", true,
                "If no storage is open, the sort key sorts your own inventory instead.");

            Mode = Config.Bind("2 - Sorting", "SortMode", SortMode.Alphabetical,
                "Alphabetical = by item name. Quantity = item types you have the most of come first.");
            Reverse = Config.Bind("2 - Sorting", "ReverseOrder", false,
                "Reverses the order (Z→A for alphabetical, fewest first for quantity).");
            MergeStacks = Config.Bind("2 - Sorting", "MergeStacks", true,
                "Combine partial stacks of the same item while sorting.");
            ContainerFill = Config.Bind("2 - Sorting", "StorageFillStart", FillStart.Bottom,
                "Where sorted items start filling in storages. Bottom = bottom row first; Top = top row first.");
            PlayerFill = Config.Bind("2 - Sorting", "PlayerFillStart", FillStart.Top,
                "Where sorted items start filling in your own inventory.");
            SkipHotbar = Config.Bind("2 - Sorting", "SkipHotbar", true,
                "Leave the hotbar row (top row, slots 1–8) of your own inventory untouched.");
            RespectQuickStackLocks = Config.Bind("2 - Sorting", "RespectQuickStackDepositLocks", true,
                "Leave inventory slots locked by QuickStackDeposit untouched. Does nothing if QuickStackDeposit isn't installed.");
            IgnoreEquipped = Config.Bind("2 - Sorting", "IgnoreEquippedItems", true,
                "Leave equipped items (weapons, armor, etc.) in their current slots when sorting your inventory.");

            ShowButtons = Config.Bind("3 - UI", "ShowSortButtons", true, "Show the Sort buttons.");
            ContainerButtonX = Config.Bind("3 - UI", "StorageButtonOffsetX", 8f,
                "Horizontal offset of the storage Sort button from the storage panel's top-right corner.");
            ContainerButtonY = Config.Bind("3 - UI", "StorageButtonOffsetY", 0f,
                "Vertical offset of the storage Sort button from the storage panel's top-right corner.");
            PlayerButtonX = Config.Bind("3 - UI", "InventoryButtonOffsetX", 8f,
                "Horizontal offset of the inventory Sort button from the inventory panel's top-right corner.");
            PlayerButtonY = Config.Bind("3 - UI", "InventoryButtonOffsetY", 0f,
                "Vertical offset of the inventory Sort button from the inventory panel's top-right corner.");

            // Let button position/visibility be tweaked live (e.g. via ConfigurationManager)
            ShowButtons.SettingChanged += (_, __) => SortButtons.ApplyLayout();
            ContainerButtonX.SettingChanged += (_, __) => SortButtons.ApplyLayout();
            ContainerButtonY.SettingChanged += (_, __) => SortButtons.ApplyLayout();
            PlayerButtonX.SettingChanged += (_, __) => SortButtons.ApplyLayout();
            PlayerButtonY.SettingChanged += (_, __) => SortButtons.ApplyLayout();

            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll();

            Log.LogInfo($"{PluginName} {PluginVersion} loaded");
        }

        private void Update()
        {
            if (!SortKey.Value.IsDown()) return;
            if (!InventorySorter.CanSortNow()) return;

            Container container = InventoryGui.instance.m_currentContainer;
            if (container != null)
                InventorySorter.SortContainer(container);
            else if (KeySortsPlayerWhenNoContainer.Value)
                InventorySorter.SortPlayer();
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
