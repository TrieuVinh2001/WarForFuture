#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class HUDUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate HUD UI Prefab")]
        public static void GenerateHUDUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite cachedBtnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Root UI RectTransform
            GameObject rootGo = new GameObject("HUDUIPrefab", typeof(RectTransform), typeof(HUDManager));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            HUDManager hudManager = rootGo.GetComponent<HUDManager>();

            // 2. Top-Left Status Panel
            GameObject topLeftPanel = CreateUIPanel(rootGo.transform, "TopLeft_StatusPanel", new Vector2(12, -12), new Vector2(340, 135), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);

            hudManager.HpSlider = CreateUISlider(topLeftPanel.transform, "HpSlider", new Vector2(8, -8), new Vector2(324, 28), Color.red, cachedBgSprite);
            hudManager.HpText = CreateUIText(topLeftPanel.transform, "HpText", "HEALTH: 100/100", 15, TextAlignmentOptions.Center, new Vector2(8, -7), new Vector2(324, 28));

            hudManager.ManaSlider = CreateUISlider(topLeftPanel.transform, "ManaSlider", new Vector2(8, -48), new Vector2(324, 28), new Color(0.2f, 0.6f, 1.0f), cachedBgSprite);
            hudManager.ManaText = CreateUIText(topLeftPanel.transform, "ManaText", "MANA: 100/100", 15, TextAlignmentOptions.Center, new Vector2(8, -47), new Vector2(324, 28));

            hudManager.FoodSlider = CreateUISlider(topLeftPanel.transform, "FoodSlider", new Vector2(8, -88), new Vector2(324, 28), new Color(1.0f, 0.7f, 0.1f), cachedBgSprite);
            hudManager.FoodText = CreateUIText(topLeftPanel.transform, "FoodText", "STAMINA: 100/100", 15, TextAlignmentOptions.Center, new Vector2(8, -87), new Vector2(324, 28));

            // 3. Top-Center Info Panel
            GameObject centerPanel = CreateUIPanel(rootGo.transform, "TopCenter_InfoPanel", new Vector2(0, -12), new Vector2(780, 56), new Vector2(0.5f, 1), new Vector2(0.5f, 1), cachedBgSprite);
            hudManager.CenterInfoText = CreateUIText(centerPanel.transform, "CenterInfoText", "DAY 1 - BAN NGÀY | Gold: 0 | Gỗ: 0 | Đá: 0 | Sợi: 0 | Thức ăn: 0", 17, TextAlignmentOptions.Center, new Vector2(0, 0), new Vector2(780, 56));

            // 4. Bottom-Right Controls Panel
            GameObject bottomRightPanel = CreateUIPanel(rootGo.transform, "BottomRight_ControlsPanel", new Vector2(-12, 12), new Vector2(340, 190), new Vector2(1, 0), new Vector2(1, 0), cachedBgSprite);
            hudManager.ControlsGuideText = CreateUIText(bottomRightPanel.transform, "ControlsText", "", 14, TextAlignmentOptions.TopLeft, new Vector2(12, -8), new Vector2(316, 175));

            // Save Prefabs
            SavePrefab(rootGo, "Assets/Prefabs/UI/HUDUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/HUDUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[HUDUIPrefabBuilder] HUD UI Prefab created successfully.");
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Sprite bgSprite)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            Image img = panel.GetComponent<Image>();
            if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }
            img.color = new Color(0.08f, 0.1f, 0.14f, 0.94f);
            return panel;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string name, string content, float fontSize, TextAlignmentOptions alignment, Vector2 pos, Vector2 size)
        {
            GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);

            RectTransform rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            return tmp;
        }

        private static Slider CreateUISlider(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor, Sprite bgSprite)
        {
            GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(parent, false);

            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgRt.localScale = Vector3.one;
            
            Image bgImg = bgGo.GetComponent<Image>();
            if (bgSprite != null) { bgImg.sprite = bgSprite; bgImg.type = Image.Type.Sliced; }
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = Vector2.zero;
            fillAreaRt.localScale = Vector3.one;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            fillRt.localScale = Vector3.one;
            
            Image fillImg = fill.GetComponent<Image>();
            if (bgSprite != null) { fillImg.sprite = bgSprite; fillImg.type = Image.Type.Sliced; }
            fillImg.color = fillColor;

            Slider slider = sliderGo.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = bgImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            return slider;
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
