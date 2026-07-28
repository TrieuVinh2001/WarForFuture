using UnityEngine;

namespace WarForFuture.Gameplay.Combat
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnDamageText(Vector3 position, int damage, bool isPlayerDamage = false)
        {
            Color color = isPlayerDamage ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.9f, 0.2f);
            string text = $"-{damage}";
            SpawnText(position, text, color);
        }

        public void SpawnHealText(Vector3 position, int amount)
        {
            Color color = new Color(0.3f, 1f, 0.4f);
            string text = $"+{amount} HP";
            SpawnText(position, text, color);
        }

        public void SpawnText(Vector3 position, string text, Color color, float duration = 1.0f)
        {
            GameObject go = new GameObject("FloatingText_Instance");
            var ft = go.AddComponent<FloatingText>();
            ft.Initialize(position, text, color, duration);
        }
    }
}
