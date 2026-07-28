using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.Gameplay;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        // UI Root Panels
        private GameObject mainInventoryPanel;
        private GameObject discardModalPanel;

        // Left Column (Grid & Categories)
        private Transform itemGridContainer;
        private List<Button> categoryButtons = new List<Button>();
        private int selectedCategory = 0;

        // Middle Column (Equipped Slots)
        private Dictionary<EquipmentSlot, TextMeshProUGUI> equippedSlotTexts = new Dictionary<EquipmentSlot, TextMeshProUGUI>();
        private Dictionary<EquipmentSlot, Image> equippedSlotIcons = new Dictionary<EquipmentSlot, Image>();

        // Right Column (Item Details & Actions)
        private TextMeshProUGUI goldGemsText;
        private Image itemDetailIcon;
        private TextMeshProUGUI itemTitleText;
        private TextMeshProUGUI itemStatsText;
        private Button equipButton;
        private Button discardButton;

        private ItemType selectedItemType = ItemType.Sword;
        private bool hasSelection = true;
        private bool isOpen = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetupUGUIInventoryWindow();
            SetOpen(false);
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    ToggleInventory();
                }

                if (isOpen && hasSelection && discardModalPanel != null && !discardModalPanel.activeSelf)
                {
                    if (Keyboard.current.eKey.wasPressedThisFrame) UseSelectedItem();
                    if (Keyboard.current.rKey.wasPressedThisFrame) OpenDiscardModal();
                }
            }
        }

        public void ToggleInventory()
        {
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            if (mainInventoryPanel != null)
            {
                mainInventoryPanel.SetActive(isOpen);
                if (isOpen)
                {
                    mainInventoryPanel.transform.SetAsLastSibling();
                }
            }
            if (discardModalPanel != null) discardModalPanel.SetActive(false);

            if (isOpen)
            {
                if (CraftingUI.Instance != null) CraftingUI.Instance.SetOpen(false);
                if (BuildingUI.Instance != null) BuildingUI.Instance.SetBuildMode(false);
                RefreshInventoryGrid();
                RefreshEquippedSlots();
                RefreshItemDetails();
            }
        }

        private void SetupUGUIInventoryWindow()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // Main Inventory Window Panel
            mainInventoryPanel = CreateUIPanel(canvas.transform, "RPG_InventoryWindow", new Vector2(0, 0), new Vector2(920, 550), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            mainInventoryPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.96f);

            // Title Header
            CreateUIText(mainInventoryPanel.transform, "TitleText", "MAIN INVENTORY  |  CHARACTER EQUIPMENT  |  STATISTICS", 18, TextAlignmentOptions.Center, new Vector2(0, -10), new Vector2(900, 35));

            // --- 1. LEFT COLUMN: INVENTORY GRID ---
            GameObject leftPanel = CreateUIPanel(mainInventoryPanel.transform, "Left_InventoryPanel", new Vector2(15, -45), new Vector2(300, 480), new Vector2(0, 1), new Vector2(0, 1));
            
            // Categories Buttons
            CreateUIButton(leftPanel.transform, "Cat_Weapons", "Weapons", new Vector2(10, -10), new Vector2(90, 30), () => SelectCategory(0));
            CreateUIButton(leftPanel.transform, "Cat_Armor", "Armor", new Vector2(105, -10), new Vector2(90, 30), () => SelectCategory(1));
            CreateUIButton(leftPanel.transform, "Cat_Consumable", "Consumable", new Vector2(200, -10), new Vector2(90, 30), () => SelectCategory(2));

            // Scrollable Grid Area
            GameObject gridArea = CreateUIPanel(leftPanel.transform, "GridArea", new Vector2(10, -45), new Vector2(280, 380), new Vector2(0, 1), new Vector2(0, 1));
            gridArea.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.8f);
            itemGridContainer = gridArea.transform;

            // Sort & Drop Buttons
            CreateUIButton(leftPanel.transform, "Btn_Sort", "Sort", new Vector2(10, -435), new Vector2(130, 35), () => Debug.Log("Sorted!"));
            CreateUIButton(leftPanel.transform, "Btn_Drop", "Drop [R]", new Vector2(150, -435), new Vector2(130, 35), () => OpenDiscardModal());

            // --- 2. MIDDLE COLUMN: CHARACTER EQUIPMENT ---
            GameObject midPanel = CreateUIPanel(mainInventoryPanel.transform, "Mid_EquipmentPanel", new Vector2(325, -45), new Vector2(290, 480), new Vector2(0, 1), new Vector2(0, 1));
            
            // Hero Badge
            CreateUIText(midPanel.transform, "HeroBadge", "<b>Lvl 38 Hero</b>\nLyra Nightshade", 15, TextAlignmentOptions.Center, new Vector2(75, -410), new Vector2(140, 45));

            // Equipment Slots Around Character
            CreateEquippedSlotUI(midPanel.transform, "Helmet", EquipmentSlot.Helmet, new Vector2(105, -10));
            CreateEquippedSlotUI(midPanel.transform, "Necklace", EquipmentSlot.Necklace, new Vector2(195, -45));
            CreateEquippedSlotUI(midPanel.transform, "Armor", EquipmentSlot.Armor, new Vector2(10, -110));
            CreateEquippedSlotUI(midPanel.transform, "Gloves", EquipmentSlot.Gloves, new Vector2(10, -180));
            CreateEquippedSlotUI(midPanel.transform, "Ring", EquipmentSlot.Ring, new Vector2(195, -180));
            CreateEquippedSlotUI(midPanel.transform, "Pants", EquipmentSlot.Pants, new Vector2(10, -250));
            CreateEquippedSlotUI(midPanel.transform, "Boots", EquipmentSlot.Boots, new Vector2(195, -320));

            // --- 3. RIGHT COLUMN: STATISTICS & ITEM CARD ---
            GameObject rightPanel = CreateUIPanel(mainInventoryPanel.transform, "Right_DetailsPanel", new Vector2(625, -45), new Vector2(280, 480), new Vector2(0, 1), new Vector2(0, 1));

            goldGemsText = CreateUIText(rightPanel.transform, "GoldGemsText", "Gold: 12,450   Gems: 35", 14, TextAlignmentOptions.Center, new Vector2(10, -10), new Vector2(260, 25));
            
            GameObject cardBox = CreateUIPanel(rightPanel.transform, "ItemCardBox", new Vector2(10, -40), new Vector2(260, 330), new Vector2(0, 1), new Vector2(0, 1));

            // Large Detail Preview Icon
            GameObject previewGo = new GameObject("DetailIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            previewGo.transform.SetParent(cardBox.transform, false);
            RectTransform prevRt = previewGo.GetComponent<RectTransform>();
            prevRt.anchorMin = new Vector2(0.5f, 1);
            prevRt.anchorMax = new Vector2(0.5f, 1);
            prevRt.pivot = new Vector2(0.5f, 1);
            prevRt.anchoredPosition = new Vector2(0, -10);
            prevRt.sizeDelta = new Vector2(55, 55);
            itemDetailIcon = previewGo.GetComponent<Image>();

            itemTitleText = CreateUIText(cardBox.transform, "ItemTitle", "Emberwrought Sword", 16, TextAlignmentOptions.Center, new Vector2(10, -70), new Vector2(240, 25));
            itemTitleText.color = Color.cyan;
            itemStatsText = CreateUIText(cardBox.transform, "ItemStats", "+25 Attack Damage\nA razor sharp steel blade.", 13, TextAlignmentOptions.TopLeft, new Vector2(10, -100), new Vector2(240, 215));

            equipButton = CreateUIButton(rightPanel.transform, "Btn_Equip", "EQUIP / USE [E]", new Vector2(10, -380), new Vector2(260, 40), () => UseSelectedItem());
            equipButton.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f);

            discardButton = CreateUIButton(rightPanel.transform, "Btn_Discard", "DISCARD / DROP [R]", new Vector2(10, -430), new Vector2(260, 35), () => OpenDiscardModal());
            discardButton.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);

            // --- DISCARD MODAL CONFIRMATION ---
            discardModalPanel = CreateUIPanel(mainInventoryPanel.transform, "DiscardModal", new Vector2(0, 0), new Vector2(380, 200), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            discardModalPanel.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.15f, 0.98f);
            CreateUIText(discardModalPanel.transform, "ModalTitle", "CONFIRM DISCARD", 15, TextAlignmentOptions.Center, new Vector2(15, -10), new Vector2(350, 30));
            CreateUIText(discardModalPanel.transform, "ModalBody", "Are you sure you want to drop this item?", 13, TextAlignmentOptions.Center, new Vector2(15, -45), new Vector2(350, 50));

            CreateUIButton(discardModalPanel.transform, "Btn_ConfirmDrop", "CONFIRM (DROP)", new Vector2(20, -130), new Vector2(160, 40), () => ConfirmDropItem());
            CreateUIButton(discardModalPanel.transform, "Btn_CancelDrop", "CANCEL", new Vector2(200, -130), new Vector2(160, 40), () => discardModalPanel.SetActive(false));
            discardModalPanel.SetActive(false);
        }

        private void SelectCategory(int cat)
        {
            selectedCategory = cat;
            RefreshInventoryGrid();
        }

        public void RefreshInventoryGrid()
        {
            if (itemGridContainer == null) return;

            foreach (Transform child in itemGridContainer)
            {
                Destroy(child.gameObject);
            }

            List<ItemType> items = GetFilteredItems(selectedCategory);
            float startX = 10f;
            float startY = -10f;
            float slotW = 60f;
            float slotH = 58f;
            int cols = 4;

            for (int i = 0; i < items.Count; i++)
            {
                ItemType type = items[i];
                int count = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(type) : 0;
                int row = i / cols;
                int col = i % cols;

                Vector2 pos = new Vector2(startX + col * (slotW + 5), startY - row * (slotH + 5));
                ItemType capturedType = type;

                Button btn = CreateUIButton(itemGridContainer, $"Slot_{type}", $"x{count}", pos, new Vector2(slotW, slotH), () =>
                {
                    selectedItemType = capturedType;
                    hasSelection = true;
                    RefreshItemDetails();
                });

                // Add Item Icon Image inside Grid Slot
                GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconGo.transform.SetParent(btn.transform, false);

                RectTransform iconRt = iconGo.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.5f, 0.6f);
                iconRt.anchorMax = new Vector2(0.5f, 0.6f);
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.sizeDelta = new Vector2(36, 36);

                Sprite sp = ItemSpriteManager.GetItemSprite(type);
                if (sp != null)
                {
                    iconGo.GetComponent<Image>().sprite = sp;
                }

                if (selectedItemType == type)
                {
                    btn.GetComponent<Image>().color = new Color(0.3f, 0.7f, 1.0f);
                }
            }
        }

        public void RefreshEquippedSlots()
        {
            if (PlayerEquipmentManager.Instance == null) return;

            foreach (var kvp in equippedSlotTexts)
            {
                var item = PlayerEquipmentManager.Instance.GetEquippedItem(kvp.Key);
                if (item.HasValue)
                {
                    kvp.Value.text = $"<b>{item.Value.name}</b>\n<color=yellow>[THÁO]</color>";
                    kvp.Value.color = Color.cyan;
                }
                else
                {
                    kvp.Value.text = $"<color=#888888>Empty {kvp.Key}</color>";
                    kvp.Value.color = Color.gray;
                }
            }
        }

        public void RefreshItemDetails()
        {
            int gold = LocalGameServer.Instance != null && LocalGameServer.Instance.GetSaveData() != null ? LocalGameServer.Instance.GetSaveData().gold : 0;
            if (goldGemsText != null) goldGemsText.text = $"<b>Gold: {gold:N0}   Gems: 35</b>";

            if (itemTitleText != null) itemTitleText.text = GetFullItemName(selectedItemType);
            if (itemStatsText != null) itemStatsText.text = GetItemStatsDescription(selectedItemType);

            if (itemDetailIcon != null)
            {
                itemDetailIcon.sprite = ItemSpriteManager.GetItemSprite(selectedItemType);
            }
        }

        private void CreateEquippedSlotUI(Transform parent, string slotName, EquipmentSlot slot, Vector2 pos)
        {
            GameObject slotGo = CreateUIPanel(parent, $"Slot_{slotName}", pos, new Vector2(85, 60), new Vector2(0, 1), new Vector2(0, 1));
            TextMeshProUGUI txt = CreateUIText(slotGo.transform, "SlotText", slotName, 11, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(85, 60));
            equippedSlotTexts[slot] = txt;

            Button btn = slotGo.AddComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                PlayerEquipmentManager.Instance?.UnequipSlot(slot);
                RefreshEquippedSlots();
            });
        }

        private void UseSelectedItem()
        {
            if (!hasSelection || InventoryManager.Instance == null) return;
            int count = InventoryManager.Instance.GetItemCount(selectedItemType);
            if (count <= 0) return;

            if (IsEquipment(selectedItemType))
            {
                PlayerEquipmentManager.Instance?.EquipItem(selectedItemType);
                RefreshEquippedSlots();
            }
            else if (selectedItemType == ItemType.Food)
            {
                InventoryManager.Instance.RemoveItem(ItemType.Food, 1);
                PlayerController.Instance?.EatFood(40, 30);
            }
            RefreshInventoryGrid();
        }

        private void OpenDiscardModal()
        {
            if (discardModalPanel != null) discardModalPanel.SetActive(true);
        }

        private void ConfirmDropItem()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(selectedItemType, 1);
            }
            if (discardModalPanel != null) discardModalPanel.SetActive(false);
            RefreshInventoryGrid();
            RefreshItemDetails();
        }

        private List<ItemType> GetFilteredItems(int cat)
        {
            List<ItemType> list = new List<ItemType>();
            var allTypes = (ItemType[])Enum.GetValues(typeof(ItemType));
            foreach (var type in allTypes)
            {
                int c = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(type) : 0;
                if (c > 0)
                {
                    if (cat == 0 && (type == ItemType.Sword || type == ItemType.Bow || type == ItemType.Arrow)) list.Add(type);
                    else if (cat == 1 && IsEquipment(type)) list.Add(type);
                    else if (cat == 2 && (type == ItemType.Food || type == ItemType.Fiber || type == ItemType.Wood || type == ItemType.Stone)) list.Add(type);
                    else if (cat == 0) list.Add(type);
                }
            }
            return list;
        }

        private bool IsEquipment(ItemType type)
        {
            return type == ItemType.HelmetItem || type == ItemType.ArmorItem || type == ItemType.PantsItem ||
                   type == ItemType.BootsItem || type == ItemType.GlovesItem || type == ItemType.NecklaceItem ||
                   type == ItemType.RingItem;
        }

        private string GetFullItemName(ItemType type)
        {
            return type switch
            {
                ItemType.Sword => "Emberwrought Steel Sword",
                ItemType.Bow => "Emberwrought Composite Bow",
                ItemType.HelmetItem => "Leather Barbute Helmet",
                ItemType.ArmorItem => "Hardened Leather Cuirass",
                ItemType.PantsItem => "Reinforced Leather Greaves",
                ItemType.BootsItem => "Windwalker Leather Boots",
                ItemType.GlovesItem => "Brawler Leather Bracers",
                ItemType.NecklaceItem => "Mystic Amulet of Agility",
                ItemType.RingItem => "Iron Signet Ring of Might",
                ItemType.Food => "Roasted Survival Ration",
                _ => type.ToString()
            };
        }

        private string GetItemStatsDescription(ItemType type)
        {
            return type switch
            {
                ItemType.Sword => "<color=#55FF55>+25 Attack Damage</color>\n<color=#55FF55>+5% Critical Hit Chance</color>\n\nA razor-sharp steel blade forged for dragon slayers.",
                ItemType.Bow => "<color=#55FF55>+20 Ranged Damage</color>\n<color=#55FF55>+10% Attack Speed</color>\n\nA finely crafted composite longbow.",
                ItemType.HelmetItem => "<color=#55FF55>+20 Max HP</color>\n<color=#55FF55>+5 Armor Defense</color>\n\nProtects against lethal head blows.",
                ItemType.ArmorItem => "<color=#55FF55>+50 Max HP</color>\n<color=#55FF55>+12 Armor Defense</color>\n\nHeavy leather chestplate for survival.",
                ItemType.PantsItem => "<color=#55FF55>+25 Max HP</color>\n<color=#55FF55>+8 Armor Defense</color>\n<color=#55FF55>+0.3 Speed</color>",
                ItemType.BootsItem => "<color=#55FF55>+10 Max HP</color>\n<color=#55FF55>+1.0 Movement Speed</color>",
                ItemType.GlovesItem => "<color=#55FF55>+10 Max HP</color>\n<color=#55FF55>+8 Attack Power</color>",
                ItemType.NecklaceItem => "<color=#55FF55>+30 Max HP</color>\n<color=#55FF55>+5 Attack</color>\n<color=#55FF55>+5 Defense</color>",
                ItemType.RingItem => "<color=#55FF55>+15 Max HP</color>\n<color=#55FF55>+12 Attack Power</color>",
                ItemType.Food => "<color=#55FF55>+40 Stamina / Food</color>\n<color=#55FF55>+30 Health Points</color>",
                _ => "Basic crafting material used in survival base building."
            };
        }

        // --- TMPro uGUI Helpers ---
        private GameObject CreateUIPanel(Transform parent, string name, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = panel.GetComponent<Image>();
            img.color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
            return panel;
        }

        private TextMeshProUGUI CreateUIText(Transform parent, string name, string content, float fontSize, TextAlignmentOptions alignment, Vector2 pos, Vector2 size)
        {
            GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);

            RectTransform rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return tmp;
        }

        private Button CreateUIButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClickAction)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btnGo.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            TextMeshProUGUI tmp = CreateUIText(btnGo.transform, "BtnText", label, 13, TextAlignmentOptions.Center, Vector2.zero, size);
            Button btn = btnGo.GetComponent<Button>();
            if (onClickAction != null) btn.onClick.AddListener(onClickAction);
            return btn;
        }
    }
}
