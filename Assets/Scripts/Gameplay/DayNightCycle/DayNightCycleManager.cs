using System;
using System.Collections;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Gameplay.Enemies;
using WarForFuture.Network;

namespace WarForFuture.Gameplay.DayNightCycle
{
    public class DayNightCycleManager : MonoBehaviour
    {
        public static DayNightCycleManager Instance { get; private set; }

        [Header("Cycle Timings")]
        [SerializeField] private float dayDuration = 45.0f; // Day length in seconds
        [SerializeField] private int maxDays = 5;

        private int currentDay = 1;
        private DayPhase currentPhase = DayPhase.Day;
        private float phaseTimer = 0f;
        private int currentWaveIndex = 0;
        private bool isWaveInProgress = false;

        public event Action<WaveStateMsg> OnWaveStateUpdated;
        public event Action<int> OnDayStarted;
        public event Action<int> OnNightStarted;
        public event Action OnGameWon;

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
            StartDay();
        }

        private void Update()
        {
            if (currentPhase == DayPhase.Day)
            {
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0f)
                {
                    StartNight();
                }
            }

            BroadcastState();
        }

        public void StartDay()
        {
            currentPhase = DayPhase.Day;
            phaseTimer = dayDuration;
            isWaveInProgress = false;
            Debug.Log($"--- DAY {currentDay} STARTED ---");
            OnDayStarted?.Invoke(currentDay);
            BroadcastState();
        }

        public void StartNight()
        {
            currentPhase = DayPhase.Night;
            phaseTimer = 0f;
            currentWaveIndex = 0;
            isWaveInProgress = true;
            Debug.Log($"--- NIGHT {currentDay} STARTED ---");
            OnNightStarted?.Invoke(currentDay);

            if (currentDay >= maxDays)
            {
                // Day 5 Night -> Spawn Boss!
                EnemySpawnerManager.Instance?.SpawnBossWave();
            }
            else
            {
                // Normal Night -> Spawn 3 Waves
                StartCoroutine(RunNightWaves());
            }
        }

        private IEnumerator RunNightWaves()
        {
            for (int w = 1; w <= 3; w++)
            {
                currentWaveIndex = w;
                Debug.Log($"Spawning Wave {w} of Night {currentDay}...");
                EnemySpawnerManager.Instance?.SpawnEnemyWave(currentDay, w);

                // Wait until enemies of this wave are cleared or max wait time
                float waitTimeout = 40f;
                while (EnemySpawnerManager.Instance != null && EnemySpawnerManager.Instance.GetActiveEnemyCount() > 0 && waitTimeout > 0)
                {
                    waitTimeout -= Time.deltaTime;
                    yield return null;
                }

                yield return new WaitForSeconds(3f); // Small break between waves
            }

            isWaveInProgress = false;
            AdvanceDay();
        }

        public void OnBossDefeated()
        {
            Debug.Log("Game Won! 5 Days and Boss survived!");
            OnGameWon?.Invoke();
        }

        private void AdvanceDay()
        {
            if (currentDay < maxDays)
            {
                currentDay++;
                StartDay();
            }
            else
            {
                OnBossDefeated();
            }
        }

        private void BroadcastState()
        {
            int remainingEnemies = EnemySpawnerManager.Instance != null ? EnemySpawnerManager.Instance.GetActiveEnemyCount() : 0;
            WaveStateMsg msg = new WaveStateMsg(currentDay, currentPhase, currentWaveIndex, remainingEnemies, phaseTimer);
            OnWaveStateUpdated?.Invoke(msg);
        }
    }
}
