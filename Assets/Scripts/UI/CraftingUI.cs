using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.Gameplay;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Network;

namespace WarForFuture.UI
{
    public class CraftingUI : MonoBehaviour
    {
        public static CraftingUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject craftingWindowPanel;
        [SerializeField] private Transform recipeListContent;
        [SerializeField] private Button closeBtnX;

        private bool isOpen = false;

        public GameObject CraftingWindowPanel { get => craftingWindowPanel; set => craftingWindowPanel = value; }
        public Transform RecipeListContent { get => recipeListContent; set => recipeListContent = value; }
        public Button CloseBtnX { get => closeBtnX; set => closeBtnX = value; }
        public bool IsOpen => isOpen;

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
            if (closeBtnX != null)
            {
                closeBtnX.onClick.RemoveAllListeners();
                closeBtnX.onClick.AddListener(() => SetOpen(false));
            }

            SetOpen(false);
        }

        public void OpenCraftingWindow()
        {
            SetOpen(true);
        }

        public void ToggleCrafting()
        {
            SetOpen(!isOpen);
        }

        public void SetOpen(bool open)
        {
            isOpen = open;
            if (craftingWindowPanel != null)
            {
                craftingWindowPanel.SetActive(isOpen);
                if (isOpen)
                {
                    craftingWindowPanel.transform.SetAsLastSibling();
                    PopulateRecipes();
                }
            }

            if (isOpen && BuildingUI.Instance != null)
            {
                BuildingUI.Instance.SetBuildMode(false);
            }
        }

        public void PopulateRecipes()
        {
            if (recipeListContent == null) return;

            foreach (Transform child in recipeListContent)
            {
                Destroy(child.gameObject);
            }

            var recipes = CraftingManager.Instance != null ? CraftingManager.Instance.GetAllRecipes() : null;
            if (recipes != null)
            {
                float rowHeight = 75f;
                float spacing = 10f;

                RectTransform contentRt = recipeListContent.GetComponent<RectTransform>();
                if (contentRt != null)
                {
                    contentRt.sizeDelta = new Vector2(0, recipes.Count * (rowHeight + spacing) + 30f);
                }

                float yPos = -15f;
                foreach (var r in recipes)
                {
                    string reqStr = "";
                    foreach (var ing in r.ingredients)
                    {
                        reqStr += $"{ing.amount} {ing.itemType}, ";
                    }
                    if (reqStr.EndsWith(", ")) reqStr = reqStr.Substring(0, reqStr.Length - 2);

                    CreateRecipeRowUI(recipeListContent, r.recipeId, r.recipeName, reqStr, r.resultItemType, yPos, rowHeight);
                    yPos -= (rowHeight + spacing);
                }
            }
        }

        private void CreateRecipeRowUI(Transform parent, int recipeId, string name, string cost, ItemType itemType, float yPos, float rowHeight)
        {
            GameObject rowBox = CreateUIPanel(parent, $"Row_{recipeId}", new Vector2(15, yPos), new Vector2(0, rowHeight), new Vector2(0, 1), new Vector2(1, 1));
            rowBox.GetComponent<RectTransform>().offsetMin = new Vector2(15, yPos - rowHeight);
            rowBox.GetComponent<RectTransform>().offsetMax = new Vector2(-15, yPos);
            rowBox.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.95f);

            // Item Icon Image
            GameObject iconGo = new GameObject("ItemIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(rowBox.transform, false);

            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(12, 0);
            iconRt.sizeDelta = new Vector2(52, 52);

            Sprite itemSprite = ItemSpriteManager.GetItemSprite(itemType);
            if (itemSprite != null)
            {
                iconGo.GetComponent<Image>().sprite = itemSprite;
            }

            CreateUIText(rowBox.transform, "RecipeLabel", $"<b><size=18>{name}</size></b>\n<color=#CCCCCC><size=15>Cần: {cost}</size></color>", 15, TextAlignmentOptions.MidlineLeft, new Vector2(75, 0), new Vector2(550, rowHeight));

            int capturedId = recipeId;
            Button btn = CreateUIButton(rowBox.transform, "Btn_Craft", "CHẾ TẠO", new Vector2(-20, -(rowHeight * 0.15f)), new Vector2(160, rowHeight * 0.7f), () => RequestCraft(capturedId));
            RectTransform btnRt = btn.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 1);
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.pivot = new Vector2(1, 1);
            btn.GetComponent<Image>().color = new Color(0.2f, 0.68f, 0.32f);
        }

        public void RequestCraft(int recipeId)
        {
            Debug.Log($"Sending Craft Request for Recipe {recipeId} to Server...");
            CraftRequestMsg msg = new CraftRequestMsg(recipeId);
            LocalGameServer.Instance?.ProcessCraftRequest(msg);
        }

        [ContextMenu("Auto Bind UI Elements")]
        public void AutoBindUIElements()
        {
            if (craftingWindowPanel == null)
            {
                Transform t = transform.Find("CraftingWindowUGUI");
                if (t != null) craftingWindowPanel = t.gameObject;
            }

            if (craftingWindowPanel != null)
            {
                Transform close = craftingWindowPanel.transform.Find("Btn_CloseX");
                if (close != null) closeBtnX = close.GetComponent<Button>();

                Transform sv = craftingWindowPanel.transform.Find("ScrollViewport");
                if (sv != null)
                {
                    Transform content = sv.Find("Content");
                    if (content != null) recipeListContent = content;
                }
            }
        }

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

            GameObject textGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            Button btn = btnGo.GetComponent<Button>();
            if (onClickAction != null) btn.onClick.AddListener(onClickAction);
            return btn;
        }
    }
}
