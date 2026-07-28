using System;
using System.Collections.Generic;
using UnityEngine;
using WarForFuture.Gameplay.DayNightCycle;
using WarForFuture.Gameplay.Player;

namespace WarForFuture.Gameplay.Enemies
{
    public class EnemySpawnerManager : MonoBehaviour
    {
        public static EnemySpawnerManager Instance { get; private set; }

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private float spawnRadius = 12.0f;

        private readonly List<EnemyAI> activeEnemies = new List<EnemyAI>();

        public event Action<int> OnEnemyCountChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public int GetActiveEnemyCount()
        {
            activeEnemies.RemoveAll(e => e == null || e.gameObject == null);
            return activeEnemies.Count;
        }

        public void SpawnEnemyWave(int day, int waveIndex)
        {
            int countToSpawn = 2 + (day * 2) + waveIndex;
            Vector3 centerPos = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;

            for (int i = 0; i < countToSpawn; i++)
            {
                float angle = (i / (float)countToSpawn) * Mathf.PI * 2f;
                Vector3 spawnPos = centerPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;

                GameObject enemyObj;
                if (enemyPrefab != null)
                {
                    enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    enemyObj = new GameObject($"Enemy_Wave{waveIndex}_{i}");
                    enemyObj.transform.position = spawnPos;
                    var sr = enemyObj.AddComponent<SpriteRenderer>();
                    var col = enemyObj.AddComponent<CircleCollider2D>();
                    col.radius = 0.5f;
                }

                var enemyAi = enemyObj.GetComponent<EnemyAI>();
                if (enemyAi == null)
                {
                    enemyAi = enemyObj.AddComponent<EnemyAI>();
                }

                enemyAi.OnEnemyDied += () => OnEnemyDiedHandler(enemyAi);
                activeEnemies.Add(enemyAi);
            }

            OnEnemyCountChanged?.Invoke(GetActiveEnemyCount());
            Debug.Log($"Spawned {countToSpawn} enemies for Wave {waveIndex}. Active total: {GetActiveEnemyCount()}");
        }

        public void SpawnBossWave()
        {
            Vector3 centerPos = PlayerController.Instance != null ? PlayerController.Instance.transform.position : Vector3.zero;
            Vector3 spawnPos = centerPos + new Vector3(0f, spawnRadius, 0f);

            GameObject bossObj;
            if (bossPrefab != null)
            {
                bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                bossObj = new GameObject("BossEnemy");
                bossObj.transform.position = spawnPos;
                var sr = bossObj.AddComponent<SpriteRenderer>();
                var col = bossObj.AddComponent<CircleCollider2D>();
                col.radius = 1.0f;
            }

            var bossController = bossObj.GetComponent<BossController>();
            if (bossController == null)
            {
                bossController = bossObj.AddComponent<BossController>();
            }

            bossController.OnEnemyDied += () =>
            {
                OnEnemyDiedHandler(bossController);
                DayNightCycleManager.Instance?.OnBossDefeated();
            };

            activeEnemies.Add(bossController);
            OnEnemyCountChanged?.Invoke(GetActiveEnemyCount());
            Debug.Log("BOSS WAVE HAS STARTED!");
        }

        private void OnEnemyDiedHandler(EnemyAI enemy)
        {
            activeEnemies.Remove(enemy);
            OnEnemyCountChanged?.Invoke(GetActiveEnemyCount());
        }
    }
}
