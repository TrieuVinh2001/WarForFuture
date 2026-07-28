using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Data;
using WarForFuture.Gameplay.Buildings;
using WarForFuture.Gameplay.Enemies;
using WarForFuture.Gameplay.Player;
using WarForFuture.Gameplay.Resources;

namespace WarForFuture.UI
{
    public class MiniMapUI : MonoBehaviour
    {
        public static MiniMapUI Instance { get; private set; }

        private GameObject miniMapPanel;
        private RectTransform radarAreaRt;
        private TextMeshProUGUI titleText;

        private bool isFullScreenMap = false;
        private Vector2 mapWorldMin = new Vector2(-20f, -20f);
        private Vector2 mapWorldMax = new Vector2(20f, 20f);

        private readonly List<GameObject> activeBlips = new List<GameObject>();

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
            SetupUGUIMiniMapWindow();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                isFullScreenMap = !isFullScreenMap;
                UpdateMiniMapMode();
            }

            UpdateMiniMapBlips();
        }

        private void SetupUGUIMiniMapWindow()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // MiniMap Panel positioned at Top-Right
            miniMapPanel = CreateUIPanel(canvas.transform, "MiniMapPanelUGUI", new Vector2(-10, -10), new Vector2(200, 180), new Vector2(1, 1), new Vector2(1, 1));
            miniMapPanel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.95f);

            titleText = CreateUIText(miniMapPanel.transform, "TitleText", "MINI MAP [Phím M]", 11, TextAlignmentOptions.Center, new Vector2(0, -5), new Vector2(200, 20));

            // Dark Radar Area
            GameObject radarArea = CreateUIPanel(miniMapPanel.transform, "RadarArea", new Vector2(10, -25), new Vector2(180, 145), new Vector2(0, 1), new Vector2(0, 1));
            radarArea.GetComponent<Image>().color = new Color(0.05f, 0.12f, 0.06f, 0.95f);
            radarAreaRt = radarArea.GetComponent<RectTransform>();
        }

        private void UpdateMiniMapMode()
        {
            if (miniMapPanel == null) return;
            RectTransform panelRt = miniMapPanel.GetComponent<RectTransform>();

            if (isFullScreenMap)
            {
                panelRt.anchorMin = new Vector2(0.5f, 0.5f);
                panelRt.anchorMax = new Vector2(0.5f, 0.5f);
                panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = Vector2.zero;
                panelRt.sizeDelta = new Vector2(500, 500);

                if (radarAreaRt != null) radarAreaRt.sizeDelta = new Vector2(480, 465);
                if (titleText != null) titleText.text = "=== TOÀN CẢNH BẢN ĐỒ (BẤM M ĐỂ THU NHỎ) ===";
            }
            else
            {
                panelRt.anchorMin = new Vector2(1, 1);
                panelRt.anchorMax = new Vector2(1, 1);
                panelRt.pivot = new Vector2(1, 1);
                panelRt.anchoredPosition = new Vector2(-10, -10);
                panelRt.sizeDelta = new Vector2(200, 180);

                if (radarAreaRt != null) radarAreaRt.sizeDelta = new Vector2(180, 145);
                if (titleText != null) titleText.text = "MINI MAP [Phím M]";
            }
        }

        private void UpdateMiniMapBlips()
        {
            if (radarAreaRt == null) return;

            foreach (var b in activeBlips)
            {
                Destroy(b);
            }
            activeBlips.Clear();

            Vector2 radarSize = radarAreaRt.sizeDelta;

            // 1. Resources (Green)
            var resourceNodes = FindObjectsOfType<ResourceNode>();
            foreach (var node in resourceNodes)
            {
                if (node == null || !node.gameObject.activeInHierarchy) continue;
                Vector2 pos = WorldToMinimapAnchoredPos(node.transform.position, radarSize);
                CreateBlip(pos, new Vector2(4, 4), Color.green);
            }

            // 2. Buildings (Orange)
            var buildings = FindObjectsOfType<BuildingInstance>();
            foreach (var b in buildings)
            {
                if (b == null) continue;
                Vector2 pos = WorldToMinimapAnchoredPos(b.transform.position, radarSize);
                CreateBlip(pos, new Vector2(6, 6), new Color(1f, 0.7f, 0.2f));
            }

            // 3. Enemies (Red / Boss Magenta)
            var enemies = FindObjectsOfType<EnemyAI>();
            foreach (var e in enemies)
            {
                if (e == null) continue;
                Vector2 pos = WorldToMinimapAnchoredPos(e.transform.position, radarSize);
                bool isBoss = e is BossController;
                Color col = isBoss ? Color.magenta : Color.red;
                float sz = isBoss ? 8f : 4f;
                CreateBlip(pos, new Vector2(sz, sz), col);
            }

            // 4. Hero Player (Cyan)
            if (PlayerController.Instance != null)
            {
                Vector2 pos = WorldToMinimapAnchoredPos(PlayerController.Instance.transform.position, radarSize);
                CreateBlip(pos, new Vector2(8, 8), Color.cyan);
            }
        }

        private void CreateBlip(Vector2 pos, Vector2 size, Color color)
        {
            GameObject blip = new GameObject("Blip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            blip.transform.SetParent(radarAreaRt, false);

            RectTransform rt = blip.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            blip.GetComponent<Image>().color = color;
            activeBlips.Add(blip);
        }

        private Vector2 WorldToMinimapAnchoredPos(Vector3 worldPos, Vector2 radarSize)
        {
            float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, worldPos.x);
            float normY = Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, worldPos.y);

            float x = normX * radarSize.x;
            float y = normY * radarSize.y;
            return new Vector2(x, y);
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
    }
}
