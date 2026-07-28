using System;
using UnityEngine;
using WarForFuture.Data;

namespace WarForFuture.Gameplay.Buildings
{
    public class BuildingInstance : MonoBehaviour
    {
        public BuildingType buildingType;
        public Vector2Int gridPos;
        public Vector2Int gridSize = new Vector2Int(1, 1);
        public int maxHp = 100;
        public int currentHp;

        public event Action<int, int> OnHpChanged; // currentHp, maxHp
        public event Action OnBuildingDestroyed;

        private void Awake()
        {
            currentHp = maxHp;
        }

        public void Initialize(BuildingType type, Vector2Int pos, Vector2Int size, int hp)
        {
            buildingType = type;
            gridPos = pos;
            gridSize = size;
            maxHp = hp;
            currentHp = hp;
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || currentHp <= 0) return;

            currentHp -= damage;
            OnHpChanged?.Invoke(currentHp, maxHp);

            if (currentHp <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            OnBuildingDestroyed?.Invoke();
            if (BuildingGridManager.Instance != null)
            {
                BuildingGridManager.Instance.RemoveBuilding(gridPos, gridSize);
            }
            Destroy(gameObject);
        }
    }
}
