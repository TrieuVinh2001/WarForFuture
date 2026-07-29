#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class InventoryUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate Inventory UI Prefab")]
        public static void GenerateInventoryUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite cachedBtnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Create Root UI RectTransform GameObject
            GameObject rootGo = new GameObject("InventoryUIPrefab", typeof(RectTransform), typeof(InventoryUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            InventoryUI inventoryUI = rootGo.GetComponent<InventoryUI>();

            // 2. Main Window Panel
            GameObject mainPanel = CreateUIPanel(rootGo.transform, "RPG_InventoryWindow", Vector2.zero, new Vector2(920, 550), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), cachedBgSprite);
            mainPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.96f);
            inventoryUI.MainInventoryPanel = mainPanel;

            // Title Header
            CreateUIText(mainPanel.transform, "TitleText", "MAIN INVENTORY  |  CHARACTER EQUIPMENT  |  STATISTICS", 18, TextAlignmentOptions.Center, new Vector2(0, -10), new Vector2(900, 35));

            // --- 3. LEFT COLUMN: INVENTORY GRID ---
            GameObject leftPanel = CreateUIPanel(mainPanel.transform, "Left_InventoryPanel", new Vector2(15, -45), new Vector2(300, 480), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            
            inventoryUI.CategoryWeaponsBtn = CreateUIButton(leftPanel.transform, "Cat_Weapons", "Weapons", new Vector2(10, -10), new Vector2(90, 30), cachedBtnSprite);
            inventoryUI.CategoryArmorBtn = CreateUIButton(leftPanel.transform, "Cat_Armor", "Armor", new Vector2(105, -10), new Vector2(90, 30), cachedBtnSprite);
            inventoryUI.CategoryConsumablesBtn = CreateUIButton(leftPanel.transform, "Cat_Consumable", "Consumable", new Vector2(200, -10), new Vector2(90, 30), cachedBtnSprite);

            GameObject gridArea = CreateUIPanel(leftPanel.transform, "GridArea", new Vector2(10, -45), new Vector2(280, 380), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            gridArea.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.85f);
            inventoryUI.ItemGridContainer = gridArea.transform;

            inventoryUI.SortButton = CreateUIButton(leftPanel.transform, "Btn_Sort", "Sort", new Vector2(10, -435), new Vector2(130, 35), cachedBtnSprite);
            inventoryUI.DropButton = CreateUIButton(leftPanel.transform, "Btn_Drop", "Drop [R]", new Vector2(150, -435), new Vector2(130, 35), cachedBtnSprite);

            // --- 4. MIDDLE COLUMN: CHARACTER EQUIPMENT ---
            GameObject midPanel = CreateUIPanel(mainPanel.transform, "Mid_EquipmentPanel", new Vector2(325, -45), new Vector2(290, 480), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            CreateUIText(midPanel.transform, "HeroBadge", "<b>Lvl 38 Hero</b>\nLyra Nightshade", 15, TextAlignmentOptions.Center, new Vector2(75, -410), new Vector2(140, 45));

            inventoryUI.EquipmentSlots.Clear();
            AddEquippedSlot(inventoryUI, midPanel.transform, "Helmet", EquipmentSlot.Helmet, new Vector2(105, -10), cachedBgSprite);
            AddEquippedSlot(inventoryUI, midPanel.transform, "Necklace", EquipmentSlot.Necklace, new Vector2(195, -45), cachedBgSprite);
            AddEquippedSlot(inventoryUI, midPanel.transform, "Armor", EquipmentSlot.Armor, new Vector2(10, -110), cachedBgSprite);
            AddEquippedSlot(inventoryUI, midPanel.transform, "Gloves", EquipmentSlot.Gloves, new Vector2(10, -180), cachedBgSprite);
            AddEquippedSlot(inventoryUI, midPanel.transform, "Ring", EquipmentSlot.Ring, new Vector2(195, -180), cachedBgSprite);
            AddEquippedSlot(inventoryUI, midPanel.transform, "Pants", EquipmentSlot.Pants, new Vector2(10, -250), cachedBgSprite);
            AddEquippedSlot(inventoryUI, midPanel.transform, "Boots", EquipmentSlot.Boots, new Vector2(195, -320), cachedBgSprite);

            // --- 5. RIGHT COLUMN: DETAILS PANEL ---
            GameObject rightPanel = CreateUIPanel(mainPanel.transform, "Right_DetailsPanel", new Vector2(625, -45), new Vector2(280, 480), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            inventoryUI.GoldGemsText = CreateUIText(rightPanel.transform, "GoldGemsText", "Gold: 12,450   Gems: 35", 14, TextAlignmentOptions.Center, new Vector2(10, -10), new Vector2(260, 25));

            GameObject cardBox = CreateUIPanel(rightPanel.transform, "ItemCardBox", new Vector2(10, -40), new Vector2(260, 330), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            
            GameObject previewGo = new GameObject("DetailIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewGo.transform.SetParent(cardBox.transform, false);
            RectTransform prevRt = previewGo.GetComponent<RectTransform>();
            prevRt.anchorMin = new Vector2(0.5f, 1);
            prevRt.anchorMax = new Vector2(0.5f, 1);
            prevRt.pivot = new Vector2(0.5f, 1);
            prevRt.anchoredPosition = new Vector2(0, -10);
            prevRt.sizeDelta = new Vector2(55, 55);
            prevRt.localScale = Vector3.one;

            Image previewImg = previewGo.GetComponent<Image>();
            if (cachedBtnSprite != null) { previewImg.sprite = cachedBtnSprite; previewImg.type = Image.Type.Sliced; }
            inventoryUI.ItemDetailIcon = previewImg;

            inventoryUI.ItemTitleText = CreateUIText(cardBox.transform, "ItemTitle", "Emberwrought Sword", 16, TextAlignmentOptions.Center, new Vector2(10, -70), new Vector2(240, 25));
            inventoryUI.ItemTitleText.color = Color.cyan;
            inventoryUI.ItemStatsText = CreateUIText(cardBox.transform, "ItemStats", "+25 Attack Damage\nA razor sharp steel blade.", 13, TextAlignmentOptions.TopLeft, new Vector2(10, -100), new Vector2(240, 215));

            inventoryUI.EquipButton = CreateUIButton(rightPanel.transform, "Btn_Equip", "TRANG BỊ [E]", new Vector2(10, -380), new Vector2(260, 40), cachedBtnSprite);
            inventoryUI.EquipButton.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);

            inventoryUI.DiscardButton = CreateUIButton(rightPanel.transform, "Btn_Discard", "BỎ ĐỒ [R]", new Vector2(10, -430), new Vector2(260, 35), cachedBtnSprite);
            inventoryUI.DiscardButton.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);

            // --- 6. DISCARD MODAL ---
            GameObject modal = CreateUIPanel(mainPanel.transform, "DiscardModal", new Vector2(0, 0), new Vector2(380, 200), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), cachedBgSprite);
            modal.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 0.98f);
            inventoryUI.DiscardModalPanel = modal;

            CreateUIText(modal.transform, "ModalTitle", "CONFIRM DISCARD", 15, TextAlignmentOptions.Center, new Vector2(15, -10), new Vector2(350, 30));
            CreateUIText(modal.transform, "ModalBody", "Are you sure you want to drop this item?", 13, TextAlignmentOptions.Center, new Vector2(15, -45), new Vector2(350, 50));

            inventoryUI.ConfirmDropButton = CreateUIButton(modal.transform, "Btn_ConfirmDrop", "CONFIRM (DROP)", new Vector2(20, -130), new Vector2(160, 40), cachedBtnSprite);
            inventoryUI.CancelDropButton = CreateUIButton(modal.transform, "Btn_CancelDrop", "CANCEL", new Vector2(200, -130), new Vector2(160, 40), cachedBtnSprite);

            modal.SetActive(false);

            // 7. Save Prefab to Assets/Prefabs/UI & Assets/Resources/Prefabs/UI
            SavePrefab(rootGo, "Assets/Prefabs/UI/InventoryUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/InventoryUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[InventoryUIPrefabBuilder] Inventory UI Prefab created successfully.");
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Sprite bgSprite)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            Image img = panel.GetComponent<Image>();
            if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }
            img.color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
            return panel;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string name, string content, float fontSize, TextAlignmentOptions alignment, Vector2 pos, Vector2 size)
        {
            GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);

            RectTransform rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Sprite btnSprite)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            Image img = btnGo.GetComponent<Image>();
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; }
            img.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            GameObject textGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.localScale = Vector3.one;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            Button btn = btnGo.GetComponent<Button>();
            return btn;
        }

        private static void AddEquippedSlot(InventoryUI ui, Transform parent, string slotName, EquipmentSlot slot, Vector2 pos, Sprite bgSprite)
        {
            GameObject slotGo = CreateUIPanel(parent, $"Slot_{slotName}", pos, new Vector2(85, 60), new Vector2(0, 1), new Vector2(0, 1), bgSprite);
            
            // Add Item Icon Image inside Equipped Slot
            GameObject iconGo = new GameObject("SlotIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(slotGo.transform, false);
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.6f);
            iconRt.anchorMax = new Vector2(0.5f, 0.6f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(36, 36);
            iconRt.localScale = Vector3.one;

            Image iconImg = iconGo.GetComponent<Image>();
            iconGo.SetActive(false);

            TextMeshProUGUI txt = CreateUIText(slotGo.transform, "SlotText", slotName, 10, TextAlignmentOptions.Center, new Vector2(0, -18), new Vector2(85, 20));
            Button btn = slotGo.AddComponent<Button>();

            ui.EquipmentSlots.Add(new EquipmentSlotUI
            {
                slot = slot,
                slotButton = btn,
                slotText = txt,
                slotIcon = iconImg
            });
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
            PrefabUtility.SaveAsPrefabAsset(obj, path);
        }
    }
}
#endif
