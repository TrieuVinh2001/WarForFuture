using UnityEngine;

namespace WarForFuture.Gameplay.Combat
{
    public class FloatingText : MonoBehaviour
    {
        private string textToDisplay = "";
        private Color textColor = Color.yellow;
        private float lifetime = 1.0f;
        private float floatSpeed = 1.8f;
        private float spawnTime;
        private Vector3 worldPos;

        public void Initialize(Vector3 startPos, string text, Color color, float duration = 1.0f)
        {
            worldPos = startPos + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.2f, 0.5f), 0f);
            textToDisplay = text;
            textColor = color;
            lifetime = duration;
            spawnTime = Time.time;
            transform.position = worldPos;
        }

        private void Update()
        {
            float elapsed = Time.time - spawnTime;
            if (elapsed >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            worldPos += Vector3.up * (floatSpeed * Time.deltaTime);
            transform.position = worldPos;
        }

        private void OnGUI()
        {
            if (Camera.main == null) return;

            float elapsed = Time.time - spawnTime;
            float alpha = Mathf.Clamp01(1.0f - (elapsed / lifetime));

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            if (screenPos.z < 0) return; // Behind camera

            // Flip Y for GUI
            float guiY = Screen.height - screenPos.y;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            Color c = textColor;
            c.a = alpha;
            style.normal.textColor = c;

            // Draw shadow for readability
            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = new Color(0, 0, 0, alpha * 0.8f);

            Rect rect = new Rect(screenPos.x - 50, guiY - 15, 100, 30);
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), textToDisplay, shadowStyle);
            GUI.Label(rect, textToDisplay, style);
        }
    }
}
