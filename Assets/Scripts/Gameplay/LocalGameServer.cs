using System;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Gameplay.Buildings;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Network;
using WarForFuture.Save;

namespace WarForFuture.Gameplay
{
    public class LocalGameServer : MonoBehaviour
    {
        public static LocalGameServer Instance { get; private set; }

        private PlayerSaveData currentSaveData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentSaveData = SaveSystem.LoadData();
        }

        public PlayerSaveData GetSaveData() => currentSaveData;

        // Authoritative Handlers for Client Requests
        public void ProcessCraftRequest(CraftRequestMsg msg)
        {
            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.TryCraft(msg, isNearWorkbench: true);
            }
        }

        public void ProcessBuildRequest(BuildRequestMsg msg, ItemType itemType, BuildingType buildingType)
        {
            if (BuildingGridManager.Instance != null)
            {
                BuildingGridManager.Instance.TryPlaceBuilding(msg, itemType, buildingType);
            }
        }

        public void OnGameSessionCompleted(bool victory, int daysSurvived)
        {
            if (currentSaveData == null) currentSaveData = new PlayerSaveData();

            if (victory)
            {
                currentSaveData.gold += 100;
                currentSaveData.gamesWon++;
                if (!currentSaveData.unlockedBuildings.Contains("Watch Tower"))
                {
                    currentSaveData.unlockedBuildings.Add("Watch Tower");
                }
                if (!currentSaveData.unlockedBuildings.Contains("Chest"))
                {
                    currentSaveData.unlockedBuildings.Add("Chest");
                }
            }
            else
            {
                currentSaveData.gold += daysSurvived * 10;
            }

            if (daysSurvived > currentSaveData.highestDayReached)
            {
                currentSaveData.highestDayReached = daysSurvived;
            }

            SaveSystem.SaveData(currentSaveData);
        }
    }
}
