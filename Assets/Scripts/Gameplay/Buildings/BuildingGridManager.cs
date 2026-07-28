using System;
using System.Collections.Generic;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Network;

namespace WarForFuture.Gameplay.Buildings
{
    public class BuildingGridManager : MonoBehaviour
    {
        public static BuildingGridManager Instance { get; private set; }

        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private Transform buildingParent;

        private readonly Dictionary<Vector2Int, BuildingInstance> gridOccupancy = new Dictionary<Vector2Int, BuildingInstance>();

        public event Action<BuildingInstance> OnBuildingPlaced;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (buildingParent == null)
            {
                var parentGo = new GameObject("BuildingsParent");
                buildingParent = parentGo.transform;
            }
        }

        public Vector2Int WorldToGridPos(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / cellSize);
            int y = Mathf.FloorToInt(worldPos.y / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 GridToWorldPos(Vector2Int gridPos, Vector2Int size)
        {
            float x = (gridPos.x + size.x * 0.5f) * cellSize;
            float y = (gridPos.y + size.y * 0.5f) * cellSize;
            return new Vector3(x, y, 0f);
        }

        public bool CanPlaceBuilding(Vector2Int gridPos, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int checkCell = new Vector2Int(gridPos.x + x, gridPos.y + y);
                    if (gridOccupancy.ContainsKey(checkCell))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool TryPlaceBuilding(BuildRequestMsg buildMsg, ItemType requiredItem, BuildingType buildingType, GameObject prefab = null, int maxHp = 100, Vector2Int size = default)
        {
            if (size == default) size = new Vector2Int(1, 1);

            // Check if player has the pre-crafted item OR sufficient raw resources directly
            bool hasCraftedItem = InventoryManager.Instance != null && InventoryManager.Instance.GetItemCount(requiredItem) > 0;
            bool hasRawMaterials = false;

            int reqWood = 0;
            int reqStone = 0;

            switch (buildingType)
            {
                case BuildingType.Wall: reqWood = 4; break;
                case BuildingType.Door: reqWood = 6; break;
                case BuildingType.Workbench: reqWood = 10; reqStone = 5; break;
                case BuildingType.Campfire: reqWood = 5; reqStone = 2; break;
                case BuildingType.Chest: reqWood = 8; break;
                case BuildingType.WatchTower: reqWood = 15; reqStone = 10; break;
            }

            if (InventoryManager.Instance != null)
            {
                int currentWood = InventoryManager.Instance.GetItemCount(ItemType.Wood);
                int currentStone = InventoryManager.Instance.GetItemCount(ItemType.Stone);
                if (currentWood >= reqWood && currentStone >= reqStone)
                {
                    hasRawMaterials = true;
                }
            }

            if (!hasCraftedItem && !hasRawMaterials)
            {
                string reqText = reqStone > 0 ? $"{reqWood} Gỗ và {reqStone} Đá" : $"{reqWood} Gỗ";
                Debug.LogWarning($"Build failed: Không đủ nguyên liệu! Cần {reqText} để xây {buildingType}.");
                return false;
            }

            // Check grid occupancy
            if (!CanPlaceBuilding(buildMsg.gridPos, size))
            {
                Debug.LogWarning($"Build failed: Vị trí {buildMsg.gridPos} đã có công trình khác.");
                return false;
            }

            // Deduct item or raw materials
            if (hasCraftedItem)
            {
                InventoryManager.Instance.RemoveItem(requiredItem, 1);
            }
            else if (hasRawMaterials)
            {
                if (reqWood > 0) InventoryManager.Instance.RemoveItem(ItemType.Wood, reqWood);
                if (reqStone > 0) InventoryManager.Instance.RemoveItem(ItemType.Stone, reqStone);
            }

            // Create building object
            Vector3 worldPos = GridToWorldPos(buildMsg.gridPos, size);
            GameObject buildingObj;

            if (prefab != null)
            {
                buildingObj = Instantiate(prefab, worldPos, Quaternion.identity, buildingParent);
            }
            else
            {
                buildingObj = new GameObject($"Building_{buildingType}");
                buildingObj.transform.position = worldPos;
                buildingObj.transform.SetParent(buildingParent);

                var sr = buildingObj.AddComponent<SpriteRenderer>();
                Sprite spriteToUse = null;
                float scale = 0.10f;

                if (buildingType == BuildingType.Wall) spriteToUse = UnityEngine.Resources.Load<Sprite>("Art/wall");
                else if (buildingType == BuildingType.Door) spriteToUse = UnityEngine.Resources.Load<Sprite>("Art/door");
                else if (buildingType == BuildingType.Workbench) spriteToUse = UnityEngine.Resources.Load<Sprite>("Art/workbench");
                else if (buildingType == BuildingType.WatchTower) { spriteToUse = UnityEngine.Resources.Load<Sprite>("Art/tower"); scale = 0.15f; }
                else if (buildingType == BuildingType.Campfire) { spriteToUse = UnityEngine.Resources.Load<Sprite>("Art/campfire"); scale = 0.10f; }
                else if (buildingType == BuildingType.Chest) { spriteToUse = UnityEngine.Resources.Load<Sprite>("Art/chest"); scale = 0.10f; }

                if (spriteToUse != null)
                {
                    sr.sprite = spriteToUse;
                    buildingObj.transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    sr.sprite = CreatePlaceholderBuildingSprite(buildingType);
                }
                sr.sortingOrder = 5;

                var boxCol = buildingObj.AddComponent<BoxCollider2D>();
                boxCol.size = new Vector2(size.x * cellSize, size.y * cellSize);
            }

            var instance = buildingObj.GetComponent<BuildingInstance>();
            if (instance == null)
            {
                instance = buildingObj.AddComponent<BuildingInstance>();
            }

            instance.Initialize(buildingType, buildMsg.gridPos, size, maxHp);

            // Attach specific building functionality scripts
            if (buildingType == BuildingType.WatchTower)
            {
                if (buildingObj.GetComponent<ArcherTowerController>() == null)
                    buildingObj.AddComponent<ArcherTowerController>();
            }
            else if (buildingType == BuildingType.Campfire)
            {
                if (buildingObj.GetComponent<CampfireController>() == null)
                    buildingObj.AddComponent<CampfireController>();
            }
            else if (buildingType == BuildingType.Chest)
            {
                if (buildingObj.GetComponent<ChestController>() == null)
                    buildingObj.AddComponent<ChestController>();
            }
            else if (buildingType == BuildingType.Workbench)
            {
                if (buildingObj.GetComponent<WorkbenchController>() == null)
                    buildingObj.AddComponent<WorkbenchController>();
            }

            // Register in grid occupancy map
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int cell = new Vector2Int(buildMsg.gridPos.x + x, buildMsg.gridPos.y + y);
                    gridOccupancy[cell] = instance;
                }
            }

            Debug.Log($"Successfully placed {buildingType} at grid {buildMsg.gridPos}!");
            OnBuildingPlaced?.Invoke(instance);
            return true;
        }

        public void RemoveBuilding(Vector2Int gridPos, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + y);
                    if (gridOccupancy.ContainsKey(cell))
                    {
                        gridOccupancy.Remove(cell);
                    }
                }
            }
        }

        private Sprite CreatePlaceholderBuildingSprite(BuildingType type)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color fillColor = Color.gray;

            switch (type)
            {
                case BuildingType.Wall: fillColor = new Color(0.6f, 0.4f, 0.2f); break; // Brown
                case BuildingType.Door: fillColor = new Color(0.8f, 0.5f, 0.2f); break; // Light brown
                case BuildingType.Workbench: fillColor = new Color(0.3f, 0.6f, 0.9f); break; // Blue
                case BuildingType.Campfire: fillColor = new Color(1.0f, 0.4f, 0.1f); break; // Orange
                case BuildingType.Chest: fillColor = new Color(0.9f, 0.8f, 0.2f); break; // Yellow
                case BuildingType.WatchTower: fillColor = new Color(0.4f, 0.4f, 0.4f); break; // Dark gray
            }

            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fillColor;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
