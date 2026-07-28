using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WarForFuture.Save
{
    [Serializable]
    public class PlayerSaveData
    {
        public int gold = 0;
        public List<string> unlockedBuildings = new List<string>();
        public int highestDayReached = 1;
        public int gamesWon = 0;
    }

    public static class SaveSystem
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "PlayerSave.json");

        public static void SaveData(PlayerSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"Data saved successfully to {SaveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save player data: {ex.Message}");
            }
        }

        public static PlayerSaveData LoadData()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    Debug.Log("Save file not found. Creating new save data.");
                    var newData = new PlayerSaveData();
                    newData.unlockedBuildings.Add("Wood Wall");
                    newData.unlockedBuildings.Add("Door");
                    newData.unlockedBuildings.Add("Workbench");
                    SaveData(newData);
                    return newData;
                }

                string json = File.ReadAllText(SaveFilePath);
                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
                Debug.Log("Data loaded successfully.");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load player data: {ex.Message}");
                return new PlayerSaveData();
            }
        }
    }
}
