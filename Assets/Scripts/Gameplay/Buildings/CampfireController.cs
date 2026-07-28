using UnityEngine;
using UnityEngine.InputSystem;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.Gameplay.Buildings
{
    public class CampfireController : MonoBehaviour
    {
        [SerializeField] private float interactRange = 3.0f;
        private bool isPlayerNear = false;

        private void Update()
        {
            if (PlayerController.Instance == null) return;

            float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
            isPlayerNear = (dist <= interactRange);

            if (isPlayerNear)
            {
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    UI.CampfireUI.Instance?.ToggleCampfireWindow();
                }
            }
        }

        private void OnMouseDown()
        {
            if (isPlayerNear)
            {
                UI.CampfireUI.Instance?.OpenCampfireWindow();
            }
            else
            {
                Debug.Log("Đến gần Lửa Trại để nấu thức ăn!");
            }
        }
    }
}
