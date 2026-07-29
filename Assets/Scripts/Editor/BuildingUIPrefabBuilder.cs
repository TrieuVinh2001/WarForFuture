#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class BuildingUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate Building UI Prefab")]
        public static void GenerateBuildingUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite cachedBtnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Root UI RectTransform
            GameObject rootGo = new GameObject("BuildingUIPrefab", typeof(RectTransform), typeof(BuildingUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            BuildingUI buildingUI = rootGo.GetComponent<BuildingUI>();

            // 2. Hotbar Panel
            GameObject panel = new GameObject("BuildingHotbarUGUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(rootGo.transform, false);

            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0);
            panelRt.anchorMax = new Vector2(0.5f, 0);
            panelRt.pivot = new Vector2(0.5f, 0);
            panelRt.anchoredPosition = new Vector2(0, 60);
            panelRt.sizeDelta = new Vector2(680, 60);
            panelRt.localScale = Vector3.one;

            Image panelImg = panel.GetComponent<Image>();
            if (cachedBgSprite != null) { panelImg.sprite = cachedBgSprite; panelImg.type = Image.Type.Sliced; }
            panelImg.color = new Color(0.08f, 0.1f, 0.14f, 0.95f);
            buildingUI.BuildingHotbarPanel = panel;

            // Header Text
            GameObject headerGo = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            headerGo.transform.SetParent(panel.transform, false);
            RectTransform headerRt = headerGo.GetComponent<RectTransform>();
            headerRt.anchorMin = new Vector2(0, 1);
            headerRt.anchorMax = new Vector2(1, 1);
            headerRt.pivot = new Vector2(0.5f, 1);
            headerRt.anchoredPosition = new Vector2(0, -5);
            headerRt.sizeDelta = new Vector2(680, 20);
            headerRt.localScale = Vector3.one;

            TextMeshProUGUI headerTmp = headerGo.GetComponent<TextMeshProUGUI>();
            headerTmp.text = "=== CHỌN CÔNG TRÌNH CẦN ĐẶT (CLICK VÀO VỊ TRÍ BẢN ĐỒ ĐỂ XÂY) ===";
            headerTmp.fontSize = 11;
            headerTmp.alignment = TextAlignmentOptions.Center;
            headerTmp.color = Color.white;

            // 6 Hotbar Buttons
            string[] names = new string[] { "1. Tường Gỗ", "2. Cửa Gỗ", "3. Bàn Chế Tạo", "4. Lửa Trại", "5. Rương", "6. Tháp Cung" };
            float startX = 10f;
            float btnW = 105f;
            List<Button> buttons = new List<Button>();

            for (int i = 0; i < names.Length; i++)
            {
                GameObject btnGo = new GameObject($"Btn_Building_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                btnGo.transform.SetParent(panel.transform, false);

                RectTransform btnRt = btnGo.GetComponent<RectTransform>();
                btnRt.anchorMin = new Vector2(0, 1);
                btnRt.anchorMax = new Vector2(0, 1);
                btnRt.pivot = new Vector2(0, 1);
                btnRt.anchoredPosition = new Vector2(startX + i * (btnW + 5), -25);
                btnRt.sizeDelta = new Vector2(btnW, 30);
                btnRt.localScale = Vector3.one;

                Image btnImg = btnGo.GetComponent<Image>();
                if (cachedBtnSprite != null) { btnImg.sprite = cachedBtnSprite; btnImg.type = Image.Type.Sliced; }
                btnImg.color = new Color(0.2f, 0.25f, 0.35f, 0.9f);

                GameObject textGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textGo.transform.SetParent(btnGo.transform, false);
                RectTransform textRt = textGo.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;
                textRt.localScale = Vector3.one;

                TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
                tmp.text = names[i];
                tmp.fontSize = 11;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                buttons.Add(btnGo.GetComponent<Button>());
            }

            buildingUI.BuildingButtons = buttons.ToArray();

            // 3. Save Prefabs
            SavePrefab(rootGo, "Assets/Prefabs/UI/BuildingUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/BuildingUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[BuildingUIPrefabBuilder] Building UI Prefab created successfully.");
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
