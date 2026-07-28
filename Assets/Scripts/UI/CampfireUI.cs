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

        private GameObject campfireWindowPanel;
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
            SetupUGUICampfireWindow();
            SetOpen(false);
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

        private void SetupUGUICampfireWindow()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            campfireWindowPanel = CreateUIPanel(canvas.transform, "CampfireWindowUGUI", new Vector2(0, 0), new Vector2(440, 280), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            campfireWindowPanel.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.05f, 0.95f);

            CreateUIText(campfireWindowPanel.transform, "Title", "[LỬA TRẠI] NẤU THỨC ĂN (COOKING)", 15, TextAlignmentOptions.Center, new Vector2(0, -12), new Vector2(440, 30));

            // Option 1
            GameObject opt1 = CreateUIPanel(campfireWindowPanel.transform, "Opt1", new Vector2(15, -55), new Vector2(410, 65), new Vector2(0, 1), new Vector2(0, 1));
            CreateUIText(opt1.transform, "Lbl1", "<b>Nướng Bánh Sợi</b> (+50 HP)\n<color=#AAAAAA>Cần: 1 Sợi, 1 Gỗ</color>", 13, TextAlignmentOptions.MidlineLeft, new Vector2(10, 0), new Vector2(280, 65));
            Button btn1 = CreateUIButton(opt1.transform, "Btn1", "NẤU", new Vector2(300, -14), new Vector2(100, 38), () => TryCookMeal(ItemType.Fiber, 1, ItemType.Wood, 1, 50));
            btn1.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.1f);

            // Option 2
            GameObject opt2 = CreateUIPanel(campfireWindowPanel.transform, "Opt2", new Vector2(15, -135), new Vector2(410, 65), new Vector2(0, 1), new Vector2(0, 1));
            CreateUIText(opt2.transform, "Lbl2", "<b>Nướng Thịt Nóng</b> (+80 HP)\n<color=#AAAAAA>Cần: 1 Thức ăn, 1 Gỗ</color>", 13, TextAlignmentOptions.MidlineLeft, new Vector2(10, 0), new Vector2(280, 65));
            Button btn2 = CreateUIButton(opt2.transform, "Btn2", "NẤU", new Vector2(300, -14), new Vector2(100, 38), () => TryCookMeal(ItemType.Food, 1, ItemType.Wood, 1, 80));
            btn2.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.1f);

            // Close
            CreateUIButton(campfireWindowPanel.transform, "Btn_Close", "ĐÓNG LỬA TRẠI", new Vector2(20, -220), new Vector2(400, 40), () => SetOpen(false));
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
