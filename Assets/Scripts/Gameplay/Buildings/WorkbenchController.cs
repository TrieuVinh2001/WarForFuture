using UnityEngine;
using UnityEngine.InputSystem;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.Gameplay.Buildings
{
    public class WorkbenchController : MonoBehaviour
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
                    UI.CraftingUI.Instance?.OpenCraftingWindow();
                }
            }
        }

        private void OnMouseDown()
        {
            if (isPlayerNear)
            {
                UI.CraftingUI.Instance?.OpenCraftingWindow();
            }
            else
            {
                Debug.Log("Đến gần Bàn Chế Tạo để chế tạo vũ khí & trang bị!");
            }
        }
    }
}
