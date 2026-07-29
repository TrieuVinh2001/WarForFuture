#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WarForFuture.UI;

namespace WarForFuture.EditorTools
{
    public static class GameOverUIPrefabBuilder
    {
        [MenuItem("Tools/WarForFuture/Generate GameOver UI Prefab")]
        public static void GenerateGameOverUIPrefab()
        {
            Sprite cachedBgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            Sprite cachedBtnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // 1. Root UI RectTransform
            GameObject rootGo = new GameObject("GameOverUIPrefab", typeof(RectTransform), typeof(GameOverUI));
            RectTransform rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            rootRt.localScale = Vector3.one;

            GameOverUI gameOverUI = rootGo.GetComponent<GameOverUI>();

            // 2. GameOver Panel
            GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(rootGo.transform, false);

            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = Vector2.zero;
            panelRt.sizeDelta = new Vector2(500, 300);
            panelRt.localScale = Vector3.one;

            Image panelImg = panel.GetComponent<Image>();
            if (cachedBgSprite != null) { panelImg.sprite = cachedBgSprite; panelImg.type = Image.Type.Sliced; }
            panelImg.color = new Color(0.1f, 0.05f, 0.05f, 0.96f);
            gameOverUI.EndPanel = panel;

            // Title Text
            GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panel.transform, false);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1);
            titleRt.anchorMax = new Vector2(0.5f, 1);
            titleRt.pivot = new Vector2(0.5f, 1);
            titleRt.anchoredPosition = new Vector2(0, -20);
            titleRt.sizeDelta = new Vector2(460, 45);
            titleRt.localScale = Vector3.one;

            TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
            titleTmp.text = "KẾT QUẢ TRẬN ĐẤU";
            titleTmp.fontSize = 24;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.yellow;
            gameOverUI.TitleText = titleTmp;

            // Details Text
            GameObject detailsGo = new GameObject("DetailsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            detailsGo.transform.SetParent(panel.transform, false);
            RectTransform detailsRt = detailsGo.GetComponent<RectTransform>();
            detailsRt.anchorMin = new Vector2(0.5f, 0.5f);
            detailsRt.anchorMax = new Vector2(0.5f, 0.5f);
            detailsRt.pivot = new Vector2(0.5f, 0.5f);
            detailsRt.anchoredPosition = new Vector2(0, 10);
            detailsRt.sizeDelta = new Vector2(440, 120);
            detailsRt.localScale = Vector3.one;

            TextMeshProUGUI detailsTmp = detailsGo.GetComponent<TextMeshProUGUI>();
            detailsTmp.text = "Bạn đã hoàn thành phiên chơi!";
            detailsTmp.fontSize = 16;
            detailsTmp.alignment = TextAlignmentOptions.Center;
            detailsTmp.color = Color.white;
            gameOverUI.DetailsText = detailsTmp;

            // Restart Button
            GameObject btnGo = new GameObject("Btn_Restart", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel.transform, false);

            RectTransform btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0);
            btnRt.anchorMax = new Vector2(0.5f, 0);
            btnRt.pivot = new Vector2(0.5f, 0);
            btnRt.anchoredPosition = new Vector2(0, 20);
            btnRt.sizeDelta = new Vector2(200, 45);
            btnRt.localScale = Vector3.one;

            Image btnImg = btnGo.GetComponent<Image>();
            if (cachedBtnSprite != null) { btnImg.sprite = cachedBtnSprite; btnImg.type = Image.Type.Sliced; }
            btnImg.color = new Color(0.2f, 0.65f, 0.3f);
            gameOverUI.RestartButton = btnGo.GetComponent<Button>();

            GameObject textGo = new GameObject("BtnText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.localScale = Vector3.one;

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "CHƠI LẠI";
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            panel.SetActive(false);

            // Save Prefabs
            SavePrefab(rootGo, "Assets/Prefabs/UI/GameOverUIPrefab.prefab");
            SavePrefab(rootGo, "Assets/Resources/Prefabs/UI/GameOverUIPrefab.prefab");
            Object.DestroyImmediate(rootGo);

            Debug.Log("[GameOverUIPrefabBuilder] GameOver UI Prefab created successfully.");
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
