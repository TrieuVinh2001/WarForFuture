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
    [Serializable]
    public struct EquipmentSlotUI
    {
        public EquipmentSlot slot;
        public Button slotButton;
        public TextMeshProUGUI slotText;
        public Image slotIcon;
    }

    public class InventoryUI : MonoBehaviour
    {
        public static InventoryUI Instance { get; private set; }

        [Header("UI Root Panels")]
        [SerializeField] private GameObject mainInventoryPanel;
        [SerializeField] private GameObject discardModalPanel;

        [Header("Left Column - Inventory Grid & Categories")]
        [SerializeField] private Transform itemGridContainer;
        [SerializeField] private Button categoryWeaponsBtn;
        [SerializeField] private Button categoryArmorBtn;
        [SerializeField] private Button categoryConsumablesBtn;
        [SerializeField] private Button sortButton;
        [SerializeField] private Button dropButton;

        [Header("Middle Column - Equipment Slots")]
        [SerializeField] private List<EquipmentSlotUI> equipmentSlots = new List<EquipmentSlotUI>();

        [Header("Right Column - Item Details & Actions")]
        [SerializeField] private TextMeshProUGUI goldGemsText;
        [SerializeField] private Image itemDetailIcon;
        [SerializeField] private TextMeshProUGUI itemTitleText;
        [SerializeField] private TextMeshProUGUI itemStatsText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button discardButton;

        [Header("Discard Modal Actions")]
        [SerializeField] private Button confirmDropButton;
        [SerializeField] private Button cancelDropButton;

        // Internal Selection State
        private int selectedCategory = 0;
        private ItemType selectedItemType = ItemType.Sword;
        private bool isSelectedEquipped = false;
        private EquipmentSlot? selectedEquippedSlot = null;
        private bool hasSelection = true;
        private bool isOpen = false;

        // Properties for Editor Builder
        public GameObject MainInventoryPanel { get => mainInventoryPanel; set => mainInventoryPanel = value; }
        public GameObject DiscardModalPanel { get => discardModalPanel; set => discardModalPanel = value; }
        public Transform ItemGridContainer { get => itemGridContainer; set => itemGridContainer = value; }
        public Button CategoryWeaponsBtn { get => categoryWeaponsBtn; set => categoryWeaponsBtn = value; }
        public Button CategoryArmorBtn { get => categoryArmorBtn; set => categoryArmorBtn = value; }
        public Button CategoryConsumablesBtn { get => categoryConsumablesBtn; set => categoryConsumablesBtn = value; }
        public Button SortButton { get => sortButton; set => sortButton = value; }
        public Button DropButton { get => dropButton; set => dropButton = value; }
        public List<EquipmentSlotUI> EquipmentSlots => equipmentSlots;
        public TextMeshProUGUI GoldGemsText { get => goldGemsText; set => goldGemsText = value; }
        public Image ItemDetailIcon { get => itemDetailIcon; set => itemDetailIcon = value; }
        public TextMeshProUGUI ItemTitleText { get => itemTitleText; set => itemTitleText = value; }
        public TextMeshProUGUI ItemStatsText { get => itemStatsText; set => itemStatsText = value; }
        public Button EquipButton { get => equipButton; set => equipButton = value; }
        public Button DiscardButton { get => discardButton; set => discardButton = value; }
        public Button ConfirmDropButton { get => confirmDropButton; set => confirmDropButton = value; }
        public Button CancelDropButton { get => cancelDropButton; set => cancelDropButton = value; }

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
            InitListenersAndSlots();
            SetOpen(false);
        }

        private void InitListenersAndSlots()
        {
            // Category Buttons
            if (categoryWeaponsBtn != null) { categoryWeaponsBtn.onClick.RemoveAllListeners(); categoryWeaponsBtn.onClick.AddListener(() => SelectCategory(0)); }
            if (categoryArmorBtn != null) { categoryArmorBtn.onClick.RemoveAllListeners(); categoryArmorBtn.onClick.AddListener(() => SelectCategory(1)); }
            if (categoryConsumablesBtn != null) { categoryConsumablesBtn.onClick.RemoveAllListeners(); categoryConsumablesBtn.onClick.AddListener(() => SelectCategory(2)); }

            // General Action Buttons
            if (sortButton != null) { sortButton.onClick.RemoveAllListeners(); sortButton.onClick.AddListener(() => RefreshInventoryGrid()); }
            if (dropButton != null) { dropButton.onClick.RemoveAllListeners(); dropButton.onClick.AddListener(() => OpenDiscardModal()); }
            if (discardButton != null) { discardButton.onClick.RemoveAllListeners(); discardButton.onClick.AddListener(() => OpenDiscardModal()); }
            if (confirmDropButton != null) { confirmDropButton.onClick.RemoveAllListeners(); confirmDropButton.onClick.AddListener(() => ConfirmDropItem()); }
            if (cancelDropButton != null) { cancelDropButton.onClick.RemoveAllListeners(); cancelDropButton.onClick.AddListener(() => { if (discardModalPanel != null) discardModalPanel.SetActive(false); }); }

            // Equipment Slot Click Events
            if (equipmentSlots != null)
            {
                foreach (var slotUI in equipmentSlots)
                {
                    EquipmentSlot slotType = slotUI.slot;
                    if (slotUI.slotButton != null)
                    {
                        slotUI.slotButton.onClick.RemoveAllListeners();
                        slotUI.slotButton.onClick.AddListener(() => OnEquipmentSlotClicked(slotType));
                    }
                }
            }
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
                    if (Keyboard.current.eKey.wasPressedThisFrame) HandlePrimaryAction();
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

        public void SelectCategory(int cat)
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
                int totalCount = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(type) : 0;
                int isEquippedOffset = (PlayerEquipmentManager.Instance != null && PlayerEquipmentManager.Instance.IsEquipped(type)) ? 1 : 0;
                int displayCount = Mathf.Max(0, totalCount - isEquippedOffset);

                if (displayCount <= 0) continue;

                int row = i / cols;
                int col = i % cols;

                Vector2 pos = new Vector2(startX + col * (slotW + 5), startY - row * (slotH + 5));
                ItemType capturedType = type;

                Button btn = CreateUIButton(itemGridContainer, $"Slot_{type}", $"x{displayCount}", pos, new Vector2(slotW, slotH), () =>
                {
                    selectedItemType = capturedType;
                    isSelectedEquipped = false;
                    selectedEquippedSlot = null;
                    hasSelection = true;
                    RefreshInventoryGrid();
                    RefreshEquippedSlots();
                    RefreshItemDetails();
                });

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

                if (!isSelectedEquipped && selectedItemType == type)
                {
                    btn.GetComponent<Image>().color = new Color(0.3f, 0.7f, 1.0f);
                }
            }
        }

        public void RefreshEquippedSlots()
        {
            if (equipmentSlots == null) return;

            foreach (var slotUI in equipmentSlots)
            {
                EquipmentSlot slot = slotUI.slot;
                var itemBonus = PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetEquippedItem(slot) : null;

                // Ensure SlotIcon image exists
                Image iconImg = slotUI.slotIcon;
                if (iconImg == null && slotUI.slotButton != null)
                {
                    Transform iconTrans = slotUI.slotButton.transform.Find("SlotIcon");
                    if (iconTrans != null) iconImg = iconTrans.GetComponent<Image>();
                }

                if (itemBonus.HasValue)
                {
                    ItemType itemType = itemBonus.Value.itemType;
                    Sprite itemSprite = ItemSpriteManager.GetItemSprite(itemType);

                    if (iconImg != null)
                    {
                        iconImg.gameObject.SetActive(true);
                        iconImg.sprite = itemSprite;
                    }

                    if (slotUI.slotText != null)
                    {
                        slotUI.slotText.text = "";
                    }

                    if (slotUI.slotButton != null)
                    {
                        bool isThisSelected = isSelectedEquipped && selectedEquippedSlot == slot;
                        slotUI.slotButton.GetComponent<Image>().color = isThisSelected ? new Color(0.3f, 0.8f, 1.0f, 0.95f) : new Color(0.18f, 0.35f, 0.45f, 0.95f);
                    }
                }
                else
                {
                    if (iconImg != null)
                    {
                        iconImg.gameObject.SetActive(false);
                    }

                    if (slotUI.slotText != null)
                    {
                        slotUI.slotText.text = $"<color=#888888>Empty {slot}</color>";
                    }

                    if (slotUI.slotButton != null)
                    {
                        slotUI.slotButton.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
                    }
                }
            }
        }

        private void OnEquipmentSlotClicked(EquipmentSlot slot)
        {
            var itemBonus = PlayerEquipmentManager.Instance != null ? PlayerEquipmentManager.Instance.GetEquippedItem(slot) : null;
            if (itemBonus.HasValue)
            {
                selectedItemType = itemBonus.Value.itemType;
                isSelectedEquipped = true;
                selectedEquippedSlot = slot;
                hasSelection = true;
                RefreshInventoryGrid();
                RefreshEquippedSlots();
                RefreshItemDetails();
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

            // Configure Equip / Unequip / Use Button
            if (equipButton != null)
            {
                TextMeshProUGUI btnText = equipButton.GetComponentInChildren<TextMeshProUGUI>();

                if (IsEquipment(selectedItemType))
                {
                    equipButton.gameObject.SetActive(true);
                    equipButton.onClick.RemoveAllListeners();

                    bool isCurrentlyEquipped = isSelectedEquipped || (PlayerEquipmentManager.Instance != null && PlayerEquipmentManager.Instance.IsEquipped(selectedItemType));
                    if (isCurrentlyEquipped)
                    {
                        if (btnText != null) btnText.text = "THÁO TRANG BỊ";
                        equipButton.GetComponent<Image>().color = new Color(0.85f, 0.4f, 0.1f, 0.95f); // Orange
                        equipButton.onClick.AddListener(() => UnequipSelectedItem());
                    }
                    else
                    {
                        if (btnText != null) btnText.text = "TRANG BỊ [E]";
                        equipButton.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 0.95f); // Green
                        equipButton.onClick.AddListener(() => EquipSelectedItem());
                    }
                }
                else if (selectedItemType == ItemType.Food)
                {
                    equipButton.gameObject.SetActive(true);
                    equipButton.onClick.RemoveAllListeners();
                    if (btnText != null) btnText.text = "SỬ DỤNG [E]";
                    equipButton.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.3f, 0.95f); // Green
                    equipButton.onClick.AddListener(() => UseSelectedItem());
                }
                else
                {
                    // Non-equippable materials (Wood, Stone, Fiber) -> Hide Equip Button
                    equipButton.gameObject.SetActive(false);
                }
            }

            // Discard Button is ALWAYS ACTIVE for all selected items
            if (discardButton != null)
            {
                discardButton.gameObject.SetActive(true);
                TextMeshProUGUI discText = discardButton.GetComponentInChildren<TextMeshProUGUI>();
                if (discText != null) discText.text = "BỎ ĐỒ [R]";
            }
        }

        private void HandlePrimaryAction()
        {
            if (!hasSelection) return;
            if (IsEquipment(selectedItemType))
            {
                bool isCurrentlyEquipped = isSelectedEquipped || (PlayerEquipmentManager.Instance != null && PlayerEquipmentManager.Instance.IsEquipped(selectedItemType));
                if (isCurrentlyEquipped) UnequipSelectedItem();
                else EquipSelectedItem();
            }
            else if (selectedItemType == ItemType.Food)
            {
                UseSelectedItem();
            }
        }

        private void EquipSelectedItem()
        {
            if (PlayerEquipmentManager.Instance != null)
            {
                PlayerEquipmentManager.Instance.EquipItem(selectedItemType);
                isSelectedEquipped = true;
                selectedEquippedSlot = PlayerEquipmentManager.Instance.GetSlotOfItem(selectedItemType);
                RefreshInventoryGrid();
                RefreshEquippedSlots();
                RefreshItemDetails();
            }
        }

        private void UnequipSelectedItem()
        {
            if (PlayerEquipmentManager.Instance != null)
            {
                EquipmentSlot? slotToUnequip = selectedEquippedSlot ?? PlayerEquipmentManager.Instance.GetSlotOfItem(selectedItemType);
                if (slotToUnequip.HasValue)
                {
                    PlayerEquipmentManager.Instance.UnequipSlot(slotToUnequip.Value);
                    isSelectedEquipped = false;
                    selectedEquippedSlot = null;
                    RefreshInventoryGrid();
                    RefreshEquippedSlots();
                    RefreshItemDetails();
                }
            }
        }

        private void UseSelectedItem()
        {
            if (!hasSelection || InventoryManager.Instance == null) return;
            int count = InventoryManager.Instance.GetItemCount(selectedItemType);
            if (count <= 0) return;

            if (selectedItemType == ItemType.Food)
            {
                InventoryManager.Instance.RemoveItem(ItemType.Food, 1);
                PlayerController.Instance?.EatFood(40, 30);
            }
            RefreshInventoryGrid();
            RefreshItemDetails();
        }

        private void OpenDiscardModal()
        {
            if (discardModalPanel != null) discardModalPanel.SetActive(true);
        }

        private void ConfirmDropItem()
        {
            if (PlayerEquipmentManager.Instance != null && (isSelectedEquipped || PlayerEquipmentManager.Instance.IsEquipped(selectedItemType)))
            {
                EquipmentSlot? slotToUnequip = selectedEquippedSlot ?? PlayerEquipmentManager.Instance.GetSlotOfItem(selectedItemType);
                if (slotToUnequip.HasValue)
                {
                    PlayerEquipmentManager.Instance.UnequipSlot(slotToUnequip.Value);
                }
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(selectedItemType, 1);
            }

            isSelectedEquipped = false;
            selectedEquippedSlot = null;

            if (discardModalPanel != null) discardModalPanel.SetActive(false);
            RefreshInventoryGrid();
            RefreshEquippedSlots();
            RefreshItemDetails();
        }

        private List<ItemType> GetFilteredItems(int cat)
        {
            List<ItemType> list = new List<ItemType>();
            var allTypes = (ItemType[])Enum.GetValues(typeof(ItemType));
            foreach (var type in allTypes)
            {
                int c = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(type) : 0;
                int isEquippedOffset = (PlayerEquipmentManager.Instance != null && PlayerEquipmentManager.Instance.IsEquipped(type)) ? 1 : 0;
                int availableInGrid = c - isEquippedOffset;

                if (availableInGrid > 0)
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
                ItemType.Sword => "Thanh Kiếm Thép Emberwrought",
                ItemType.Bow => "Cung Phức Hợp Emberwrought",
                ItemType.HelmetItem => "Mũ Da (Leather Helmet)",
                ItemType.ArmorItem => "Áo Da (Leather Armor)",
                ItemType.PantsItem => "Quần Da (Leather Pants)",
                ItemType.BootsItem => "Giày Da (Leather Boots)",
                ItemType.GlovesItem => "Găng Tay Da (Leather Gloves)",
                ItemType.NecklaceItem => "Vòng Cổ Đá (Stone Necklace)",
                ItemType.RingItem => "Nhẫn Sắt (Iron Ring)",
                ItemType.Food => "Thịt Nướng Survival",
                ItemType.Wood => "Gỗ (Crafting Material)",
                ItemType.Stone => "Đá (Crafting Material)",
                ItemType.Fiber => "Sợi (Crafting Material)",
                _ => type.ToString()
            };
        }

        private string GetItemStatsDescription(ItemType type)
        {
            return type switch
            {
                ItemType.Sword => "<color=#55FF55>+25 Sát thương vật lý</color>\n<color=#55FF55>+5% Tỷ lệ chí mạng</color>\n\nThanh kiếm thép sắc bén rèn cho thợ săn quái vật.",
                ItemType.Bow => "<color=#55FF55>+20 Sát thương tầm xa</color>\n<color=#55FF55>+10% Tốc độ bắn</color>\n\nCung trường phức hợp độ chính xác cao.",
                ItemType.HelmetItem => "<color=#55FF55>+20 Máu tối đa (HP)</color>\n<color=#55FF55>+5 Giáp phòng thủ</color>\n\nBảo vệ vùng đầu khỏi đòn chí mạng.",
                ItemType.ArmorItem => "<color=#55FF55>+50 Máu tối đa (HP)</color>\n<color=#55FF55>+12 Giáp phòng thủ</color>\n\nÁo da kiên cố bảo vệ cơ thể.",
                ItemType.PantsItem => "<color=#55FF55>+25 Máu tối đa (HP)</color>\n<color=#55FF55>+8 Giáp phòng thủ</color>\n<color=#55FF55>+0.3 Tốc độ chạy</color>",
                ItemType.BootsItem => "<color=#55FF55>+10 Máu tối đa (HP)</color>\n<color=#55FF55>+1.0 Tốc độ di chuyển</color>",
                ItemType.GlovesItem => "<color=#55FF55>+10 Máu tối đa (HP)</color>\n<color=#55FF55>+8 Tấn công</color>",
                ItemType.NecklaceItem => "<color=#55FF55>+30 Máu tối đa (HP)</color>\n<color=#55FF55>+5 Tấn công</color>\n<color=#55FF55>+5 Phòng thủ</color>",
                ItemType.RingItem => "<color=#55FF55>+15 Máu tối đa (HP)</color>\n<color=#55FF55>+12 Sức mạnh tấn công</color>",
                ItemType.Food => "<color=#55FF55>+40 Hồi Thể Lực</color>\n<color=#55FF55>+30 Hồi Máu (HP)</color>",
                ItemType.Wood => "<color=#AAAAAA>Nguyên liệu xây dựng căn cứ & chế tạo vũ khí.</color>",
                ItemType.Stone => "<color=#AAAAAA>Nguyên liệu kiên cố xây dựng tháp & tường đá.</color>",
                ItemType.Fiber => "<color=#AAAAAA>Nguyên liệu dệt vải & may trang bị da.</color>",
                _ => "Nguyên liệu thủ công sinh tồn."
            };
        }

        [ContextMenu("Auto Bind UI Elements")]
        public void AutoBindUIElements()
        {
            if (mainInventoryPanel == null)
            {
                Transform t = transform.Find("RPG_InventoryWindow");
                if (t != null) mainInventoryPanel = t.gameObject;
            }

            if (mainInventoryPanel != null)
            {
                Transform left = mainInventoryPanel.transform.Find("Left_InventoryPanel");
                if (left != null)
                {
                    Transform catW = left.Find("Cat_Weapons"); if (catW != null) categoryWeaponsBtn = catW.GetComponent<Button>();
                    Transform catA = left.Find("Cat_Armor"); if (catA != null) categoryArmorBtn = catA.GetComponent<Button>();
                    Transform catC = left.Find("Cat_Consumable"); if (catC != null) categoryConsumablesBtn = catC.GetComponent<Button>();

                    Transform gridArea = left.Find("GridArea"); if (gridArea != null) itemGridContainer = gridArea;
                    Transform sort = left.Find("Btn_Sort"); if (sort != null) sortButton = sort.GetComponent<Button>();
                    Transform drop = left.Find("Btn_Drop"); if (drop != null) dropButton = drop.GetComponent<Button>();
                }

                Transform mid = mainInventoryPanel.transform.Find("Mid_EquipmentPanel");
                if (mid != null)
                {
                    equipmentSlots.Clear();
                    string[] slotNames = new string[] { "Helmet", "Necklace", "Armor", "Gloves", "Ring", "Pants", "Boots" };
                    EquipmentSlot[] slotEnums = new EquipmentSlot[] { EquipmentSlot.Helmet, EquipmentSlot.Necklace, EquipmentSlot.Armor, EquipmentSlot.Gloves, EquipmentSlot.Ring, EquipmentSlot.Pants, EquipmentSlot.Boots };

                    for (int i = 0; i < slotNames.Length; i++)
                    {
                        Transform sTrans = mid.Find($"Slot_{slotNames[i]}");
                        if (sTrans != null)
                        {
                            Button b = sTrans.GetComponent<Button>();
                            TextMeshProUGUI txt = sTrans.GetComponentInChildren<TextMeshProUGUI>();
                            Image icon = null;
                            Transform iconTrans = sTrans.Find("SlotIcon");
                            if (iconTrans != null) icon = iconTrans.GetComponent<Image>();

                            equipmentSlots.Add(new EquipmentSlotUI
                            {
                                slot = slotEnums[i],
                                slotButton = b,
                                slotText = txt,
                                slotIcon = icon
                            });
                        }
                    }
                }

                Transform right = mainInventoryPanel.transform.Find("Right_DetailsPanel");
                if (right != null)
                {
                    Transform gg = right.Find("GoldGemsText"); if (gg != null) goldGemsText = gg.GetComponent<TextMeshProUGUI>();
                    Transform card = right.Find("ItemCardBox");
                    if (card != null)
                    {
                        Transform icon = card.Find("DetailIcon"); if (icon != null) itemDetailIcon = icon.GetComponent<Image>();
                        Transform title = card.Find("ItemTitle"); if (title != null) itemTitleText = title.GetComponent<TextMeshProUGUI>();
                        Transform stats = card.Find("ItemStats"); if (stats != null) itemStatsText = stats.GetComponent<TextMeshProUGUI>();
                    }
                    Transform eq = right.Find("Btn_Equip"); if (eq != null) equipButton = eq.GetComponent<Button>();
                    Transform disc = right.Find("Btn_Discard"); if (disc != null) discardButton = disc.GetComponent<Button>();
                }

                Transform modal = mainInventoryPanel.transform.Find("DiscardModal");
                if (modal != null)
                {
                    discardModalPanel = modal.gameObject;
                    Transform cDrop = modal.Find("Btn_ConfirmDrop"); if (cDrop != null) confirmDropButton = cDrop.GetComponent<Button>();
                    Transform cancel = modal.Find("Btn_CancelDrop"); if (cancel != null) cancelDropButton = cancel.GetComponent<Button>();
                }
            }
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

            GameObject textGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            Button btn = btnGo.GetComponent<Button>();
            if (onClickAction != null) btn.onClick.AddListener(onClickAction);
            return btn;
        }
    }
}
