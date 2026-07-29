using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.UI
{
    public class CampfireUI : MonoBehaviour
    {
        public static CampfireUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject campfireWindowPanel;
        [SerializeField] private Button cookBtn1;
        [SerializeField] private Button cookBtn2;
        [SerializeField] private Button closeBtn;

        private bool isOpen = false;

        public GameObject CampfireWindowPanel { get => campfireWindowPanel; set => campfireWindowPanel = value; }
        public Button CookBtn1 { get => cookBtn1; set => cookBtn1 = value; }
        public Button CookBtn2 { get => cookBtn2; set => cookBtn2 = value; }
        public Button CloseBtn { get => closeBtn; set => closeBtn = value; }

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
            InitListeners();
            SetOpen(false);
        }

        private void InitListeners()
        {
            if (cookBtn1 != null)
            {
                cookBtn1.onClick.RemoveAllListeners();
                cookBtn1.onClick.AddListener(() => TryCookMeal(ItemType.Fiber, 1, ItemType.Wood, 1, 50));
            }
            if (cookBtn2 != null)
            {
                cookBtn2.onClick.RemoveAllListeners();
                cookBtn2.onClick.AddListener(() => TryCookMeal(ItemType.Food, 1, ItemType.Wood, 1, 80));
            }
            if (closeBtn != null)
            {
                closeBtn.onClick.RemoveAllListeners();
                closeBtn.onClick.AddListener(() => SetOpen(false));
            }
        }

        public void ToggleCampfireWindow()
        {
            SetOpen(!isOpen);
        }

        public void OpenCampfireWindow()
        {
            SetOpen(true);
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            if (campfireWindowPanel != null) campfireWindowPanel.SetActive(isOpen);
        }

        private void TryCookMeal(ItemType ing1, int count1, ItemType ing2, int count2, int healAmount)
        {
            if (InventoryManager.Instance == null) return;

            int c1 = InventoryManager.Instance.GetItemCount(ing1);
            int c2 = InventoryManager.Instance.GetItemCount(ing2);

            if (c1 >= count1 && c2 >= count2)
            {
                InventoryManager.Instance.RemoveItem(ing1, count1);
                InventoryManager.Instance.RemoveItem(ing2, count2);

                PlayerController.Instance?.Heal(healAmount);
                Debug.Log($"Đã nấu thành công món ăn! Hồi +{healAmount} HP cho Hero!");
            }
            else
            {
                Debug.LogWarning($"Không đủ nguyên liệu để nấu! Cần {count1} {ing1} và {count2} {ing2}.");
            }
        }

        [ContextMenu("Auto Bind UI Elements")]
        public void AutoBindUIElements()
        {
            if (campfireWindowPanel == null)
            {
                Transform t = transform.Find("CampfireWindowUGUI");
                if (t != null) campfireWindowPanel = t.gameObject;
            }

            if (campfireWindowPanel != null)
            {
                Transform opt1 = campfireWindowPanel.transform.Find("Opt1");
                if (opt1 != null)
                {
                    Transform b1 = opt1.Find("Btn1");
                    if (b1 != null) cookBtn1 = b1.GetComponent<Button>();
                }

                Transform opt2 = campfireWindowPanel.transform.Find("Opt2");
                if (opt2 != null)
                {
                    Transform b2 = opt2.Find("Btn2");
                    if (b2 != null) cookBtn2 = b2.GetComponent<Button>();
                }

                Transform close = campfireWindowPanel.transform.Find("Btn_Close");
                if (close != null) closeBtn = close.GetComponent<Button>();
            }
        }
    }
}
