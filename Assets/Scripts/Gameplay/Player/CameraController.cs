using UnityEngine;

namespace WarForFuture.Gameplay.Player
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Target & Smooth Follow")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5.0f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        [Header("Map Boundaries (Clamping)")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-15f, -15f);
        [SerializeField] private Vector2 maxBounds = new Vector2(15f, 15f);

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
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null)
                {
                    target = PlayerController.Instance.transform;
                }
                return;
            }

            Vector3 desiredPosition = target.position + offset;

            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            }

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }
    }
}
