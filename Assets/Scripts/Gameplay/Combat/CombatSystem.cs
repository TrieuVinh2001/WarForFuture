using System;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Gameplay.Enemies;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Gameplay.Resources;

namespace WarForFuture.Gameplay.Combat
{
    public class CombatSystem : MonoBehaviour
    {
        public static CombatSystem Instance { get; private set; }

        [SerializeField] private float meleeRange = 1.5f;
        [SerializeField] private int swordDamage = 25;
        [SerializeField] private int bowDamage = 20;

        public event Action<Vector2, float> OnMeleeSwing;
        public event Action<Vector2> OnRangedShoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool PerformMeleeAttack(Vector2 origin, Vector2 direction)
        {
            Vector2 hitPoint = origin + direction.normalized * (meleeRange * 0.5f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitPoint, meleeRange * 0.7f);

            bool hitAnything = false;
            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(swordDamage);
                    hitAnything = true;
                }

                var resourceNode = hit.GetComponent<ResourceNode>();
                if (resourceNode != null)
                {
                    resourceNode.TakeHit(1);
                    hitAnything = true;
                }
            }

            OnMeleeSwing?.Invoke(hitPoint, meleeRange);
            return hitAnything;
        }

        public bool PerformRangedAttack(Vector2 origin, Vector2 direction, GameObject projectilePrefab = null)
        {
            GameObject arrowObj;
            if (projectilePrefab != null)
            {
                arrowObj = Instantiate(projectilePrefab, origin, Quaternion.identity);
            }
            else
            {
                arrowObj = new GameObject("ArrowProjectile");
                arrowObj.transform.position = origin;
                var sr = arrowObj.AddComponent<SpriteRenderer>();

                Sprite loadedArrow = UnityEngine.Resources.Load<Sprite>("Art/arrow");
                if (loadedArrow != null)
                {
                    sr.sprite = loadedArrow;
                    arrowObj.transform.localScale = new Vector3(0.06f, 0.06f, 1f);
                }
                else
                {
                    sr.sprite = CreateArrowSprite();
                }
                sr.sortingOrder = 10;

                var col = arrowObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.4f, 0.2f);
            }

            var proj = arrowObj.GetComponent<Projectile>();
            if (proj == null)
            {
                proj = arrowObj.AddComponent<Projectile>();
            }

            proj.Initialize(direction, bowDamage, true);
            OnRangedShoot?.Invoke(origin);
            return true;
        }

        private Sprite CreateArrowSprite()
        {
            Texture2D tex = new Texture2D(16, 4);
            Color[] pixels = new Color[16 * 4];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.yellow;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 4), new Vector2(0.5f, 0.5f), 16);
        }
    }
}
