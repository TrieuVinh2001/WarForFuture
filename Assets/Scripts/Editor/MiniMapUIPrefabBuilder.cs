#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class MiniMapUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate MiniMap UI Prefab")]
        public static void GenerateMiniMapUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            // 1. Root UI RectTransform
            GameObject rootGo = new GameObject("MiniMapUIPrefab", typeof(RectTransform), typeof(MiniMapUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            MiniMapUI miniMapUI = rootGo.GetComponent<MiniMapUI>();

            // 2. MiniMap Panel
            GameObject panel = new GameObject("MiniMapPanelUGUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(rootGo.transform, false);

            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1, 1);
            panelRt.anchorMax = new Vector2(1, 1);
            panelRt.pivot = new Vector2(1, 1);
            panelRt.anchoredPosition = new Vector2(-10, -10);
            panelRt.sizeDelta = new Vector2(200, 180);
            panelRt.localScale = Vector3.one;

            Image panelImg = panel.GetComponent<Image>();
            if (cachedBgSprite != null) { panelImg.sprite = cachedBgSprite; panelImg.type = Image.Type.Sliced; }
            panelImg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
            miniMapUI.MiniMapPanel = panel;

            // Title
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panel.transform, false);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 1);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -5);
            titleRt.sizeDelta = new Vector2(200, 20);
            titleRt.localScale = Vector3.one;

            TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "MINI MAP [Phím M]";
            titleTmp.fontSize = 11;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;
            miniMapUI.TitleText = titleTmp;

            // Radar Area
            GameObject radarArea = new GameObject("RadarArea", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
            radarArea.transform.SetParent(panel.transform, false);

            RectTransform radarRt = radarArea.GetComponent<RectTransform>();
            radarRt.anchorMin = new Vector2(0, 1);
            radarRt.anchorMax = new Vector2(0, 1);
            radarRt.pivot = new Vector2(0, 1);
            radarRt.anchoredPosition = new Vector2(10, -25);
            radarRt.sizeDelta = new Vector2(180, 145);
            radarRt.localScale = Vector3.one;

            Image radarImg = radarArea.GetComponent<Image>();
            if (cachedBgSprite != null) { radarImg.sprite = cachedBgSprite; radarImg.type = Image.Type.Sliced; }
            radarImg.color = new Color(0.05f, 0.12f, 0.06f, 0.95f);
            miniMapUI.RadarAreaRt = radarRt;

            // Save Prefabs
            SavePrefab(rootGo, "Assets/Prefabs/UI/MiniMapUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/MiniMapUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[MiniMapUIPrefabBuilder] MiniMap UI Prefab created successfully.");
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
            PrefabUtility.SaveAsPrefabAsset(obj, path);
        }
    }
}
#endif
