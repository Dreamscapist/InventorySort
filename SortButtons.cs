using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventorySort
{
    [HarmonyPatch(typeof(InventoryGui), "Awake")]
    internal static class InventoryGui_Awake_Patch
    {
        private static void Postfix(InventoryGui __instance) => SortButtons.Create(__instance);
    }

    internal static class SortButtons
    {
        private static Button _containerButton;
        private static Button _playerButton;

        public static void Create(InventoryGui gui)
        {
            Button template = gui.m_takeAllButton;
            if (template == null)
            {
                InventorySortPlugin.Log.LogWarning("Take All button not found; Sort buttons not created");
                return;
            }

            Vector2 size = ((RectTransform)template.transform).rect.size;

            // Parent each button to its panel so it shows/hides with that panel.
            _containerButton = Clone(template, gui.m_container, "InventorySort_StorageSortButton", size);
            _containerButton.onClick.AddListener(() =>
            {
                Container c = InventoryGui.instance != null ? InventoryGui.instance.m_currentContainer : null;
                if (c != null) InventorySorter.SortContainer(c);
            });

            _playerButton = Clone(template, gui.m_player, "InventorySort_InventorySortButton", size);
            _playerButton.onClick.AddListener(InventorySorter.SortPlayer);

            ApplyLayout();
        }

        public static void ApplyLayout()
        {
            Place(_containerButton, InventorySortPlugin.ContainerButtonX.Value, InventorySortPlugin.ContainerButtonY.Value);
            Place(_playerButton, InventorySortPlugin.PlayerButtonX.Value, InventorySortPlugin.PlayerButtonY.Value);
        }

        private static Button Clone(Button template, Transform parent, string name, Vector2 size)
        {
            GameObject go = Object.Instantiate(template.gameObject, parent, false);
            go.name = name;

            Button button = go.GetComponent<Button>();
            // Replace the event entirely: RemoveAllListeners() doesn't clear persistent (inspector) listeners,
            // which would make our button also trigger "Take All".
            button.onClick = new Button.ButtonClickedEvent();

            // Drop the controller hint/binding copied from Take All so a gamepad press doesn't fire both.
            foreach (UIGamePad pad in go.GetComponentsInChildren<UIGamePad>(true))
            {
                if (pad.m_hint != null) Object.Destroy(pad.m_hint);
                Object.Destroy(pad);
            }

            TMP_Text label = go.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = "Sort";

            // Anchor to the panel's top-right corner, sitting just outside it.
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = size;

            return button;
        }

        private static void Place(Button button, float x, float y)
        {
            if (button == null) return;
            ((RectTransform)button.transform).anchoredPosition = new Vector2(x, y);
            button.gameObject.SetActive(InventorySortPlugin.ShowButtons.Value);
        }
    }
}
