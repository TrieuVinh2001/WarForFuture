using UnityEngine;
using WarForFuture.Gameplay.Enemies;

namespace WarForFuture.Gameplay.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 14f;
        [SerializeField] private int damage = 25;
        [SerializeField] private float lifetime = 3f;

        private Vector2 direction;
        private bool isPlayerOwned = true;
        private float spawnTime;

        public void Initialize(Vector2 dir, int damageAmount, bool fromPlayer)
        {
            direction = dir.normalized;
            damage = damageAmount;
            isPlayerOwned = fromPlayer;
            spawnTime = Time.time;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void Update()
        {
            Vector3 prevPos = transform.position;
            transform.Translate(Vector3.right * (speed * Time.deltaTime), Space.Self);
            Vector3 nextPos = transform.position;

            // Perform continuous hit check along flight path
            if (isPlayerOwned)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(nextPos, 0.4f);
                foreach (var hit in hits)
                {
                    var enemy = hit.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                        Debug.Log($"Arrow hit enemy {enemy.name} dealing {damage} damage!");
                        Destroy(gameObject);
                        return;
                    }
                }
            }

            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isPlayerOwned)
            {
                var enemy = collision.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Debug.Log($"Arrow trigger hit enemy {enemy.name} dealing {damage} damage!");
                    Destroy(gameObject);
                }
            }
            else
            {
                var player = collision.GetComponent<Player.PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}
