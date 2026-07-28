using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.Gameplay;
using WarForFuture.Gameplay.DayNightCycle;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Gameplay.Player;
using WarForFuture.Network;

namespace WarForFuture.UI
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        // Top-Left 3 Status Bars
        private Slider hpSlider;
        private TextMeshProUGUI hpText;
        private Slider manaSlider;
        private TextMeshProUGUI manaText;
        private Slider foodSlider;
        private TextMeshProUGUI foodText;

        // Top-Center Info (Resources & Wave)
        private TextMeshProUGUI centerInfoText;

        // Bottom-Right Controls Guide
        private TextMeshProUGUI controlsGuideText;

        private int currentHp = 100;
        private int maxHp = 100;
        private int currentMana = 100;
        private int maxMana = 100;
        private int currentFood = 100;
        private int maxFood = 100;

        private byte currentWeaponSlot = 0;
        private WaveStateMsg currentWaveState;

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
            SetupUGUIElements();

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHpChanged += UpdateHp;
                PlayerController.Instance.OnManaChanged += UpdateMana;
                PlayerController.Instance.OnFoodChanged += UpdateFood;
                PlayerController.Instance.OnWeaponSlotChanged += (slot) => { currentWeaponSlot = slot; UpdateControlsGuide(); };

                // Immediately load initial status values on game load so HP (Red) & Mana (Blue) display bright colors!
                UpdateHp(PlayerController.Instance.CurrentHp, PlayerController.Instance.EffectiveMaxHp);
                UpdateMana(PlayerController.Instance.CurrentMana, PlayerController.Instance.MaxMana);
                UpdateFood(PlayerController.Instance.CurrentFood, PlayerController.Instance.MaxFood);
            }
            else
            {
                UpdateHp(100, 100);
                UpdateMana(100, 100);
                UpdateFood(100, 100);
            }

            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnWaveStateUpdated += (waveState) => { currentWaveState = waveState; UpdateCenterInfo(); };
            }
        }

        private void SetupUGUIElements()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // 1. TOP-LEFT: 3 LARGE STATUS BARS (HP, MANA, STAMINA)
            GameObject topLeftPanel = CreateUIPanel(canvas.transform, "TopLeft_StatusPanel", new Vector2(12, -12), new Vector2(340, 135), new Vector2(0, 1), new Vector2(0, 1));

            // Health Bar (Red)
            hpSlider = CreateUISlider(topLeftPanel.transform, "HpSlider", new Vector2(8, -8), new Vector2(324, 28), Color.red);
            hpText = CreateUIText(topLeftPanel.transform, "HpText", "HEALTH: 100/100", 15, TextAlignmentOptions.Center, new Vector2(8, -7), new Vector2(324, 28));

            // Mana Bar (Blue)
            manaSlider = CreateUISlider(topLeftPanel.transform, "ManaSlider", new Vector2(8, -48), new Vector2(324, 28), new Color(0.2f, 0.6f, 1.0f));
            manaText = CreateUIText(topLeftPanel.transform, "ManaText", "MANA: 100/100", 15, TextAlignmentOptions.Center, new Vector2(8, -47), new Vector2(324, 28));

            // Stamina/Food Bar (Orange)
            foodSlider = CreateUISlider(topLeftPanel.transform, "FoodSlider", new Vector2(8, -88), new Vector2(324, 28), new Color(1.0f, 0.7f, 0.1f));
            foodText = CreateUIText(topLeftPanel.transform, "FoodText", "STAMINA: 100/100", 15, TextAlignmentOptions.Center, new Vector2(8, -87), new Vector2(324, 28));

            // 2. TOP-CENTER: LARGE RESOURCES & WAVE STATUS
            GameObject centerPanel = CreateUIPanel(canvas.transform, "TopCenter_InfoPanel", new Vector2(0, -12), new Vector2(780, 56), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            centerInfoText = CreateUIText(centerPanel.transform, "CenterInfoText", "DAY 1 - BAN NGÀY | Gold: 0 | Gỗ: 0 | Đá: 0 | Sợi: 0 | Thức ăn: 0", 17, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(780, 56));

            // 3. BOTTOM-RIGHT: LARGE CONTROLS GUIDE PANEL
            GameObject bottomRightPanel = CreateUIPanel(canvas.transform, "BottomRight_ControlsPanel", new Vector2(-12, 12), new Vector2(340, 190), new Vector2(1, 0), new Vector2(1, 0));
            controlsGuideText = CreateUIText(bottomRightPanel.transform, "ControlsText", "", 14, TextAlignmentOptions.TopLeft, new Vector2(12, -8), new Vector2(316, 175));
            UpdateControlsGuide();
        }

        private void Update()
        {
            UpdateCenterInfo();
        }

        public void UpdateHp(int hp, int max)
        {
            currentHp = hp;
            maxHp = max;

            if (hpSlider != null) { hpSlider.minValue = 0; hpSlider.maxValue = maxHp; hpSlider.value = currentHp; }
            if (hpText != null) hpText.text = $"<b>HEALTH: {currentHp}/{maxHp}</b>";
        }

        public void UpdateMana(int mp, int max)
        {
            currentMana = mp;
            maxMana = max;

            if (manaSlider != null) { manaSlider.minValue = 0; manaSlider.maxValue = maxMana; manaSlider.value = currentMana; }
            if (manaText != null) manaText.text = $"<b>MANA: {currentMana}/{maxMana}</b>";
        }

        public void UpdateFood(int fd, int max)
        {
            currentFood = fd;
            maxFood = max;

            if (foodSlider != null) { foodSlider.minValue = 0; foodSlider.maxValue = maxFood; foodSlider.value = currentFood; }
            if (foodText != null) foodText.text = $"<b>STAMINA: {currentFood}/{maxFood}</b>";
        }

        private void UpdateControlsGuide()
        {
            if (controlsGuideText == null) return;
            string weaponName = currentWeaponSlot switch
            {
                1 => "Kiếm Sắt (Iron Sword)",
                2 => "Cung Gỗ (Wooden Bow)",
                _ => "Tay / Rìu Khai Thác"
            };

            controlsGuideText.text = $"<b>VŨ KHÍ:</b> <color=yellow>{weaponName}</color>\n\n<b>PHÍM TẮT:</b>\n[WASD]: Di chuyển | [1 2 3]: Vũ khí\n[Space/Mouse1]: Đánh/Khai thác\n[I / Tab]: Túi Đồ & Trang Bị\n[M]: MiniMap | [B]: Xây Dựng\n[Bàn Chế Tạo]: Đứng gần + [E] / Click";
        }

        private void UpdateCenterInfo()
        {
            if (centerInfoText == null) return;

            string cycleText = $"DAY {currentWaveState.dayNumber} - ";
            if (currentWaveState.phase == DayPhase.Day)
            {
                cycleText += $"BAN NGÀY ({Mathf.CeilToInt(currentWaveState.timeToNextPhase)}s)";
            }
            else
            {
                cycleText += $"BAN ĐÊM | Wave {currentWaveState.waveIndex} | Quái: {currentWaveState.enemiesRemaining}";
            }

            int wood = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(ItemType.Wood) : 0;
            int stone = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(ItemType.Stone) : 0;
            int fiber = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(ItemType.Fiber) : 0;
            int food = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(ItemType.Food) : 0;
            int gold = LocalGameServer.Instance != null && LocalGameServer.Instance.GetSaveData() != null ? LocalGameServer.Instance.GetSaveData().gold : 0;

            centerInfoText.text = $"<b><size=19>{cycleText}</size></b>\nGold: {gold} | Gỗ: {wood} | Đá: {stone} | Sợi: {fiber} | Thức ăn: {food}";
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
            img.color = new Color(0.08f, 0.1f, 0.14f, 0.94f);
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

        private Slider CreateUISlider(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
        {
            GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(parent, false);

            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Background
            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Fill Area & Fill
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = fillColor;

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = bgGo.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            return slider;
        }
    }
}
