using System;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Gameplay.Buildings;
using WarForFuture.Gameplay.Combat;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.Gameplay.Enemies
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Enemy Stats")]
        [SerializeField] protected int maxHp = 50;
        [SerializeField] protected int currentHp;
        [SerializeField] protected float moveSpeed = 3.0f;
        [SerializeField] protected int attackDamage = 10;
        [SerializeField] protected float attackRange = 1.2f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected float detectionRadius = 8.0f;

        protected EnemyState currentState = EnemyState.Idle;
        protected Transform targetTransform;
        protected BuildingInstance targetBuilding;
        protected float lastAttackTime;
        protected SpriteRenderer spriteRenderer;

        public event Action<int, int> OnHpChanged;
        public event Action OnEnemyDied;

        protected virtual void Awake()
        {
            currentHp = maxHp;
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Sprite loadedGoblin = UnityEngine.Resources.Load<Sprite>("Art/goblin");
                if (loadedGoblin != null)
                {
                    spriteRenderer.sprite = loadedGoblin;
                    transform.localScale = new Vector3(0.08f, 0.08f, 1f);
                }
                else if (spriteRenderer.sprite == null)
                {
                    spriteRenderer.sprite = CreatePlaceholderEnemySprite();
                }
            }
        }

        protected virtual void Start()
        {
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        protected virtual void Update()
        {
            if (currentState == EnemyState.Dead) return;

            FindTarget();

            if (targetTransform != null || targetBuilding != null)
            {
                Vector3 targetPos = targetTransform != null ? targetTransform.position : targetBuilding.transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);

                if (distance <= attackRange)
                {
                    currentState = EnemyState.Attack;
                    TryAttack();
                }
                else
                {
                    currentState = EnemyState.Detect;
                    MoveTowards(targetPos);
                }
            }
            else
            {
                currentState = EnemyState.Idle;
            }
        }

        protected virtual void FindTarget()
        {
            if (PlayerController.Instance != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
                if (distToPlayer <= detectionRadius)
                {
                    targetTransform = PlayerController.Instance.transform;
                    targetBuilding = null;
                    return;
                }
            }

            targetTransform = null;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            float minDistance = float.MaxValue;
            BuildingInstance closestBuilding = null;

            foreach (var hit in hitColliders)
            {
                var building = hit.GetComponent<BuildingInstance>();
                if (building != null && building.currentHp > 0)
                {
                    float d = Vector3.Distance(transform.position, building.transform.position);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        closestBuilding = building;
                    }
                }
            }
            targetBuilding = closestBuilding;
        }

        protected virtual void MoveTowards(Vector3 destination)
        {
            Vector3 dir = (destination - transform.position).normalized;
            transform.position += dir * (moveSpeed * Time.deltaTime);
        }

        protected virtual void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;

            lastAttackTime = Time.time;

            if (targetTransform != null)
            {
                var player = targetTransform.GetComponent<PlayerController>();
                player?.TakeDamage(attackDamage);
            }
            else if (targetBuilding != null)
            {
                targetBuilding.TakeDamage(attackDamage);
            }
        }

        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0 || currentState == EnemyState.Dead) return;

            currentHp -= damage;
            OnHpChanged?.Invoke(currentHp, maxHp);

            // Floating Combat Damage Text!
            FloatingTextManager.Instance?.SpawnDamageText(transform.position, damage, false);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        protected virtual void Die()
        {
            currentState = EnemyState.Dead;
            OnEnemyDied?.Invoke();

            // Reward Gold & Loot Items on Death!
            int droppedGold = UnityEngine.Random.Range(10, 25);
            if (LocalGameServer.Instance != null && LocalGameServer.Instance.GetSaveData() != null)
            {
                LocalGameServer.Instance.GetSaveData().gold += droppedGold;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(ItemType.Food, 1);
                InventoryManager.Instance.AddItem(ItemType.Fiber, UnityEngine.Random.Range(1, 3));
            }

            Debug.Log($"👾 Monster defeated! Dropped +{droppedGold} Gold and loot items!");
            Destroy(gameObject);
        }

        protected virtual Sprite CreatePlaceholderEnemySprite()
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.red;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
