using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WarForFuture.Gameplay;
using WarForFuture.Gameplay.DayNightCycle;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject endPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailsText;

        private void Start()
        {
            if (endPanel != null) endPanel.SetActive(false);

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
    }
}
