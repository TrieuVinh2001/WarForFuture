#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class CraftingUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate Crafting UI Prefab")]
        public static void GenerateCraftingUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite cachedBtnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Root UI RectTransform
            GameObject rootGo = new GameObject("CraftingUIPrefab", typeof(RectTransform), typeof(CraftingUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            CraftingUI craftingUI = rootGo.GetComponent<CraftingUI>();

            // 2. Panel Widescreen
            GameObject panel = new GameObject("CraftingWindowUGUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(rootGo.transform, false);

            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.04f, 0.04f);
            panelRt.anchorMax = new Vector2(0.96f, 0.96f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            panelRt.localScale = Vector3.one;

            Image panelImg = panel.GetComponent<Image>();
            if (cachedBgSprite != null) { panelImg.sprite = cachedBgSprite; panelImg.type = Image.Type.Sliced; }
            panelImg.color = new Color(0.08f, 0.1f, 0.14f, 0.97f);
            craftingUI.CraftingWindowPanel = panel;

            // 3. Header Title & Close Button
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panel.transform, false);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1);
            titleRt.anchorMax = new Vector2(0.5f, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -15);
            titleRt.sizeDelta = new Vector2(800, 45);
            titleRt.localScale = Vector3.one;

            TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "=== BÀN CHẾ TẠO VŨ KHÍ & TRANG BỊ ===";
            titleTmp.fontSize = 22;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;

            GameObject closeBtnGo = new GameObject("Btn_CloseX", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(panel.transform, false);
            RectTransform closeRt = closeBtnGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1, 1);
            closeRt.anchorMax = new Vector2(1, 1);
            closeRt.pivot = new Vector2(1, 1);
            closeRt.anchoredPosition = new Vector2(-15, -15);
            closeRt.sizeDelta = new Vector2(45, 45);
            closeRt.localScale = Vector3.one;

            Image closeImg = closeBtnGo.GetComponent<Image>();
            if (cachedBtnSprite != null) { closeImg.sprite = cachedBtnSprite; closeImg.type = Image.Type.Sliced; }
            closeImg.color = new Color(0.85f, 0.2f, 0.2f);
            craftingUI.CloseBtnX = closeBtnGo.GetComponent<Button>();

            GameObject closeTxtGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            closeTxtGo.transform.SetParent(closeBtnGo.transform, false);
            RectTransform closeTxtRt = closeTxtGo.GetComponent<RectTransform>();
            closeTxtRt.anchorMin = Vector2.zero;
            closeTxtRt.anchorMax = Vector2.one;
            closeTxtRt.sizeDelta = Vector2.zero;
            closeTxtRt.localScale = Vector3.one;
            TextMeshProUGUI closeTmp = closeTxtGo.GetComponent<TextMeshProUGUI>();
            closeTmp.text = "X";
            closeTmp.fontSize = 18;
            closeTmp.alignment = TextAlignmentOptions.Center;

            // 4. Scroll Viewport
            GameObject scrollViewport = new GameObject("ScrollViewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D), typeof(ScrollRect));
            scrollViewport.transform.SetParent(panel.transform, false);

            RectTransform svRt = scrollViewport.GetComponent<RectTransform>();
            svRt.anchorMin = new Vector2(0, 0);
            svRt.anchorMax = new Vector2(1, 1);
            svRt.pivot = new Vector2(0.5f, 0.5f);
            svRt.offsetMin = new Vector2(25, 25);
            svRt.offsetMax = new Vector2(-25, -70);
            svRt.localScale = Vector3.one;

            Image svImg = scrollViewport.GetComponent<Image>();
            if (cachedBgSprite != null) { svImg.sprite = cachedBgSprite; svImg.type = Image.Type.Sliced; }
            svImg.color = new Color(0.05f, 0.06f, 0.08f, 0.85f);

            GameObject contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(scrollViewport.transform, false);
            RectTransform contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0, 1);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.localScale = Vector3.one;
            craftingUI.RecipeListContent = contentGo.transform;

            ScrollRect sr = scrollViewport.GetComponent<ScrollRect>();
            sr.content = contentRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;

            // 5. Save Prefabs
            SavePrefab(rootGo, "Assets/Prefabs/UI/CraftingUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/CraftingUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[CraftingUIPrefabBuilder] Crafting UI Prefab created successfully.");
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
