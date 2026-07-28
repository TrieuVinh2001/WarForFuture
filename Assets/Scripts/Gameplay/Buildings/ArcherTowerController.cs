using UnityEngine;
using WarForFuture.Gameplay.Combat;
using WarForFuture.Gameplay.Enemies;

namespace WarForFuture.Gameplay.Buildings
{
    public class ArcherTowerController : MonoBehaviour
    {
        [SerializeField] private float attackRange = 7.0f;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private int arrowDamage = 20;

        private float lastAttackTime;

        private void Update()
        {
            if (Time.time - lastAttackTime < attackInterval) return;

            EnemyAI target = FindNearestEnemy();
            if (target != null)
            {
                lastAttackTime = Time.time;
                ShootArrowAt(target);
            }
        }

        private EnemyAI FindNearestEnemy()
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            EnemyAI nearest = null;
            float minDistance = float.MaxValue;

            foreach (var hit in hitColliders)
            {
                var enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        nearest = enemy;
                    }
                }
            }

            return nearest;
        }

        private void ShootArrowAt(EnemyAI enemy)
        {
            Vector2 dir = (enemy.transform.position - transform.position).normalized;
            CombatSystem.Instance?.PerformRangedAttack(transform.position, dir);
            Debug.Log($"Archer Tower shot an arrow at enemy {enemy.name}!");
        }
    }
}
