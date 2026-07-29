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

        [Header("Top-Left Status Bars")]
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Slider manaSlider;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private Slider foodSlider;
        [SerializeField] private TextMeshProUGUI foodText;

        [Header("Top-Center Info")]
        [SerializeField] private TextMeshProUGUI centerInfoText;

        [Header("Bottom-Right Guide")]
        [SerializeField] private TextMeshProUGUI controlsGuideText;

        public Slider HpSlider { get => hpSlider; set => hpSlider = value; }
        public TextMeshProUGUI HpText { get => hpText; set => hpText = value; }
        public Slider ManaSlider { get => manaSlider; set => manaSlider = value; }
        public TextMeshProUGUI ManaText { get => manaText; set => manaText = value; }
        public Slider FoodSlider { get => foodSlider; set => foodSlider = value; }
        public TextMeshProUGUI FoodText { get => foodText; set => foodText = value; }
        public TextMeshProUGUI CenterInfoText { get => centerInfoText; set => centerInfoText = value; }
        public TextMeshProUGUI ControlsGuideText { get => controlsGuideText; set => controlsGuideText = value; }

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
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnHpChanged += UpdateHp;
                PlayerController.Instance.OnManaChanged += UpdateMana;
                PlayerController.Instance.OnFoodChanged += UpdateFood;
                PlayerController.Instance.OnWeaponSlotChanged += (slot) => { currentWeaponSlot = slot; UpdateControlsGuide(); };

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

        [ContextMenu("Auto Bind UI Elements")]
        public void AutoBindUIElements()
        {
            Transform topLeft = transform.Find("TopLeft_StatusPanel");
            if (topLeft != null)
            {
                Transform hpS = topLeft.Find("HpSlider"); if (hpS != null) hpSlider = hpS.GetComponent<Slider>();
                Transform hpT = topLeft.Find("HpText"); if (hpT != null) hpText = hpT.GetComponent<TextMeshProUGUI>();

                Transform mpS = topLeft.Find("ManaSlider"); if (mpS != null) manaSlider = mpS.GetComponent<Slider>();
                Transform mpT = topLeft.Find("ManaText"); if (mpT != null) manaText = mpT.GetComponent<TextMeshProUGUI>();

                Transform fdS = topLeft.Find("FoodSlider"); if (fdS != null) foodSlider = fdS.GetComponent<Slider>();
                Transform fdT = topLeft.Find("FoodText"); if (fdT != null) foodText = fdT.GetComponent<TextMeshProUGUI>();
            }

            Transform center = transform.Find("TopCenter_InfoPanel");
            if (center != null)
            {
                Transform info = center.Find("CenterInfoText"); if (info != null) centerInfoText = info.GetComponent<TextMeshProUGUI>();
            }

            Transform bottomRight = transform.Find("BottomRight_ControlsPanel");
            if (bottomRight != null)
            {
                Transform ctrl = bottomRight.Find("ControlsText"); if (ctrl != null) controlsGuideText = ctrl.GetComponent<TextMeshProUGUI>();
            }
        }
    }
}
