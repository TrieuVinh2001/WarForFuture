#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class CampfireUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate Campfire UI Prefab")]
        public static void GenerateCampfireUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite cachedBtnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Root UI RectTransform
            GameObject rootGo = new GameObject("CampfireUIPrefab", typeof(RectTransform), typeof(CampfireUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            CampfireUI campfireUI = rootGo.GetComponent<CampfireUI>();

            // 2. Main Window Panel
            GameObject panel = new GameObject("CampfireWindowUGUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(rootGo.transform, false);

            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = new Vector2(440, 280);
            panelRt.localScale = Vector3.one;

            Image panelImg = panel.GetComponent<Image>();
            if (cachedBgSprite != null) { panelImg.sprite = cachedBgSprite; panelImg.type = Image.Type.Sliced; }
            panelImg.color = new Color(0.12f, 0.08f, 0.05f, 0.95f);
            campfireUI.CampfireWindowPanel = panel;

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panel.transform, false);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1);
            titleRt.anchorMax = new Vector2(0.5f, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -12);
            titleRt.sizeDelta = new Vector2(440, 30);
            titleRt.localScale = Vector3.one;

            TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "[LỬA TRẠI] NẤU THỨC ĂN (COOKING)";
            titleTmp.fontSize = 15;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;

            // Option 1
            GameObject opt1 = CreateUIPanel(panel.transform, "Opt1", new Vector2(15, -55), new Vector2(410, 65), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            CreateUIText(opt1.transform, "Lbl1", "<b>Nướng Bánh Sợi</b> (+50 HP)\n<color=#AAAAAA>Cần: 1 Sợi, 1 Gỗ</color>", 13, TextAlignmentOptions.MidlineLeft, new Vector2(10, 0), new Vector2(280, 65));
            Button b1 = CreateUIButton(opt1.transform, "Btn1", "NẤU", new Vector2(300, -14), new Vector2(100, 38), cachedBtnSprite);
            b1.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.1f);
            campfireUI.CookBtn1 = b1;

            // Option 2
            GameObject opt2 = CreateUIPanel(panel.transform, "Opt2", new Vector2(15, -135), new Vector2(410, 65), new Vector2(0, 1), new Vector2(0, 1), cachedBgSprite);
            CreateUIText(opt2.transform, "Lbl2", "<b>Nướng Thịt Nóng</b> (+80 HP)\n<color=#AAAAAA>Cần: 1 Thức ăn, 1 Gỗ</color>", 13, TextAlignmentOptions.MidlineLeft, new Vector2(10, 0), new Vector2(280, 65));
            Button b2 = CreateUIButton(opt2.transform, "Btn2", "NẤU", new Vector2(300, -14), new Vector2(100, 38), cachedBtnSprite);
            b2.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.1f);
            campfireUI.CookBtn2 = b2;

            // Close
            Button closeB = CreateUIButton(panel.transform, "Btn_Close", "ĐÓNG LỬA TRẠI", new Vector2(20, -220), new Vector2(400, 40), cachedBtnSprite);
            campfireUI.CloseBtn = closeB;

            // Save Prefabs
            SavePrefab(rootGo, "Assets/Prefabs/UI/CampfireUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/CampfireUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[CampfireUIPrefabBuilder] Campfire UI Prefab created successfully.");
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
            img.color = new Color(0.1f, 0.12f, 0.15f, 0.9f);
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

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Sprite btnSprite)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent, false);

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            Image img = btnGo.GetComponent<Image>();
            if (btnSprite != null) { img.sprite = btnSprite; img.type = Image.Type.Sliced; }
            img.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

            GameObject textGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.localScale = Vector3.one;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnGo.GetComponent<Button>();
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
