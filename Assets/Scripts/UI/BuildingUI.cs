using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.Gameplay;
using WarForFuture.Gameplay.Buildings;
using WarForFuture.Network;

namespace WarForFuture.UI
{
    public class BuildingUI : MonoBehaviour
    {
        public static BuildingUI Instance { get; private set; }

        private GameObject buildingHotbarPanel;

        [SerializeField] private GameObject ghostPreviewObject;
        private SpriteRenderer ghostRenderer;

        private bool isBuildMode = false;
        private BuildingType selectedBuildingType = BuildingType.Wall;
        private ItemType selectedItemType = ItemType.WallItem;
        private Vector2Int currentGridPos;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (ghostPreviewObject == null)
            {
                ghostPreviewObject = new GameObject("GhostBuildingPreview");
                ghostRenderer = ghostPreviewObject.AddComponent<SpriteRenderer>();
                ghostRenderer.sortingOrder = 20;
            }
            else
            {
                ghostRenderer = ghostPreviewObject.GetComponent<SpriteRenderer>();
            }

            ghostPreviewObject.SetActive(false);
        }

        private void Start()
        {
            SetupUGUIBuildingHotbar();
            SetBuildMode(false);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                SetBuildMode(!isBuildMode);
            }

            if (!isBuildMode) return;

            Vector2 mouseScreenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
            Vector3 mouseWorldPos = Camera.main != null ? Camera.main.ScreenToWorldPoint(mouseScreenPos) : Vector3.zero;
            mouseWorldPos.z = 0f;

            if (BuildingGridManager.Instance != null)
            {
                currentGridPos = BuildingGridManager.Instance.WorldToGridPos(mouseWorldPos);
                Vector3 snappedWorldPos = BuildingGridManager.Instance.GridToWorldPos(currentGridPos, new Vector2Int(1, 1));
                ghostPreviewObject.transform.position = snappedWorldPos;

                bool canPlace = BuildingGridManager.Instance.CanPlaceBuilding(currentGridPos, new Vector2Int(1, 1));
                if (ghostRenderer != null)
                {
                    ghostRenderer.color = canPlace ? new Color(0.2f, 1f, 0.2f, 0.6f) : new Color(1f, 0.2f, 0.2f, 0.6f);
                }

                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (mouseScreenPos.y > 100 && mouseScreenPos.y < Screen.height - 100)
                    {
                        BuildRequestMsg msg = new BuildRequestMsg((int)selectedBuildingType, currentGridPos);
                        LocalGameServer.Instance?.ProcessBuildRequest(msg, selectedItemType, selectedBuildingType);
                    }
                }
            }

            bool cancelTriggered = false;
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) cancelTriggered = true;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) cancelTriggered = true;

            if (cancelTriggered)
            {
                SetBuildMode(false);
            }
        }

        public void SetBuildMode(bool active)
        {
            isBuildMode = active;
            if (buildingHotbarPanel != null) buildingHotbarPanel.SetActive(isBuildMode);

            if (isBuildMode && CraftingUI.Instance != null)
            {
                CraftingUI.Instance.SetOpen(false);
            }

            if (ghostPreviewObject != null)
            {
                ghostPreviewObject.SetActive(active);
                if (active && ghostRenderer != null)
                {
                    Sprite wallSprite = UnityEngine.Resources.Load<Sprite>("Art/wall");
                    if (wallSprite != null && selectedBuildingType == BuildingType.Wall)
                    {
                        ghostRenderer.sprite = wallSprite;
                        ghostPreviewObject.transform.localScale = new Vector3(0.10f, 0.10f, 1f);
                    }
                    else
                    {
                        ghostRenderer.sprite = CreateGhostSprite();
                        ghostPreviewObject.transform.localScale = new Vector3(1f, 1f, 1f);
                    }
                }
            }
        }

        public void SelectBuilding(int typeIndex)
        {
            selectedBuildingType = (BuildingType)typeIndex;

            switch (selectedBuildingType)
            {
                case BuildingType.Wall: selectedItemType = ItemType.WallItem; break;
                case BuildingType.Door: selectedItemType = ItemType.DoorItem; break;
                case BuildingType.Workbench: selectedItemType = ItemType.WorkbenchItem; break;
                case BuildingType.Campfire: selectedItemType = ItemType.CampfireItem; break;
                case BuildingType.Chest: selectedItemType = ItemType.ChestItem; break;
                case BuildingType.WatchTower: selectedItemType = ItemType.WatchTowerItem; break;
            }

            SetBuildMode(true);
        }

        private void SetupUGUIBuildingHotbar()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            buildingHotbarPanel = CreateUIPanel(canvas.transform, "BuildingHotbarUGUI", new Vector2(0, 60), new Vector2(680, 60), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
            buildingHotbarPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            CreateUIText(buildingHotbarPanel.transform, "Header", "=== CHỌN CÔNG TRÌNH CẦN ĐẶT (CLICK VÀO VỊ TRÍ BẢN ĐỒ ĐỂ XÂY) ===", 11, TextAlignmentOptions.Center, new Vector2(0, -5), new Vector2(680, 20));

            string[] names = new string[] { "1. Tường Gỗ", "2. Cửa Gỗ", "3. Bàn Chế Tạo", "4. Lửa Trại", "5. Rương", "6. Tháp Cung" };
            float startX = 10f;
            float btnW = 105f;

            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                CreateUIButton(buildingHotbarPanel.transform, $"Btn_Building_{i}", names[i], new Vector2(startX + i * (btnW + 5), -25), new Vector2(btnW, 30), () => SelectBuilding(index));
            }
        }

        private Sprite CreateGhostSprite()
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
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

            TextMeshProUGUI tmp = CreateUIText(btnGo.transform, "BtnText", label, 11, TextAlignmentOptions.Center, Vector2.zero, size);
            Button btn = btnGo.GetComponent<Button>();
            if (onClickAction != null) btn.onClick.AddListener(onClickAction);
            return btn;
        }
    }
}
