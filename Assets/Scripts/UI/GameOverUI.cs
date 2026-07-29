using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using WarForFuture.Gameplay;
using WarForFuture.Gameplay.DayNightCycle;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.UI
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject endPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private Button restartButton;

        public GameObject EndPanel { get => endPanel; set => endPanel = value; }
        public TextMeshProUGUI TitleText { get => titleText; set => titleText = value; }
        public TextMeshProUGUI DetailsText { get => detailsText; set => detailsText = value; }
        public Button RestartButton { get => restartButton; set => restartButton = value; }

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
            if (endPanel != null) endPanel.SetActive(false);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartGame);
            }

            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OnPlayerDied += ShowDefeat;
            }

            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnGameWon += ShowVictory;
            }
        }

        public void ShowVictory()
        {
            if (endPanel != null) endPanel.SetActive(true);
            if (titleText != null) titleText.text = "CHIẾN THẮNG!";
            if (detailsText != null) detailsText.text = "Bạn đã sống sót qua 5 ngày và đánh bại Boss!\nThưởng: +100 Gold, Unlock Watch Tower & Chest!";

            LocalGameServer.Instance?.OnGameSessionCompleted(victory: true, daysSurvived: 5);
        }

        public void ShowDefeat()
        {
            if (endPanel != null) endPanel.SetActive(true);
            if (titleText != null) titleText.text = "THẤT BẠI!";
            if (detailsText != null) detailsText.text = "Căn cứ của bạn đã sụp đổ.";

            LocalGameServer.Instance?.OnGameSessionCompleted(victory: false, daysSurvived: 1);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        [ContextMenu("Auto Bind UI Elements")]
        public void AutoBindUIElements()
        {
            if (endPanel == null)
            {
                Transform t = transform.Find("GameOverPanel");
                if (t != null) endPanel = t.gameObject;
            }

            if (endPanel != null)
            {
                Transform title = endPanel.transform.Find("TitleText");
                if (title != null) titleText = title.GetComponent<TextMeshProUGUI>();

                Transform details = endPanel.transform.Find("DetailsText");
                if (details != null) detailsText = details.GetComponent<TextMeshProUGUI>();

                Transform btn = endPanel.transform.Find("Btn_Restart");
                if (btn != null) restartButton = btn.GetComponent<Button>();
            }
        }
    }
}
