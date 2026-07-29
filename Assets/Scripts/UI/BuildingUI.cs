using System.Collections.Generic;
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

        [Header("UI References")]
        [SerializeField] private GameObject buildingHotbarPanel;
        [SerializeField] private Button[] buildingButtons;

        [Header("Preview Preview")]
        [SerializeField] private GameObject ghostPreviewObject;
        private SpriteRenderer ghostRenderer;

        private bool isBuildMode = false;
        private BuildingType selectedBuildingType = BuildingType.Wall;
        private ItemType selectedItemType = ItemType.WallItem;
        private Vector2Int currentGridPos;

        public GameObject BuildingHotbarPanel { get => buildingHotbarPanel; set => buildingHotbarPanel = value; }
        public Button[] BuildingButtons { get => buildingButtons; set => buildingButtons = value; }

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
            InitButtonListeners();
            SetBuildMode(false);
        }

        private void InitButtonListeners()
        {
            if (buildingButtons != null)
            {
                for (int i = 0; i < buildingButtons.Length; i++)
                {
                    int index = i;
                    if (buildingButtons[i] != null)
                    {
                        buildingButtons[i].onClick.RemoveAllListeners();
                        buildingButtons[i].onClick.AddListener(() => SelectBuilding(index));
                    }
                }
            }
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

        private Sprite CreateGhostSprite()
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        [ContextMenu("Auto Bind UI Elements")]
        public void AutoBindUIElements()
        {
            if (buildingHotbarPanel == null)
            {
                Transform t = transform.Find("BuildingHotbarUGUI");
                if (t != null) buildingHotbarPanel = t.gameObject;
            }

            if (buildingHotbarPanel != null)
            {
                List<Button> list = new List<Button>();
                for (int i = 0; i < 6; i++)
                {
                    Transform b = buildingHotbarPanel.transform.Find($"Btn_Building_{i}");
                    if (b != null) list.Add(b.GetComponent<Button>());
                }
                buildingButtons = list.ToArray();
            }
        }
    }
}
