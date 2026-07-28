using UnityEngine;

namespace WarForFuture.Data
{
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "WarForFuture/Data/BuildingData")]
    public class BuildingData : ScriptableObject
    {
        public int buildingId;
        public string buildingName;
        public BuildingType buildingType;
        public Vector2Int gridSize = new Vector2Int(1, 1);
        public int maxHp = 100;
        public Sprite icon;
        public GameObject prefab;
    }
}
