#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace WarForFuture.EditorTools
{
    public static class AllUIPrefabGenerator
    {
        [MenuItem("Tools/WarForFuture/Generate ALL UI Prefabs", false, 0)]
        public static void GenerateAllUIPrefabs()
        {
            Debug.Log("[AllUIPrefabGenerator] Starting generation of all 7 UI Prefabs...");

            InventoryUIPrefabBuilder.GenerateInventoryUIPrefab();
            CraftingUIPrefabBuilder.GenerateCraftingUIPrefab();
            BuildingUIPrefabBuilder.GenerateBuildingUIPrefab();
            CampfireUIPrefabBuilder.GenerateCampfireUIPrefab();
            HUDUIPrefabBuilder.GenerateHUDUIPrefab();
            MiniMapUIPrefabBuilder.GenerateMiniMapUIPrefab();
            GameOverUIPrefabBuilder.GenerateGameOverUIPrefab();

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "UI Prefabs Generation Complete",
                "Successfully generated all 7 UI Prefabs under 'Assets/Prefabs/UI/':\n\n" +
                "1. InventoryUIPrefab.prefab\n" +
                "2. CraftingUIPrefab.prefab\n" +
                "3. BuildingUIPrefab.prefab\n" +
                "4. CampfireUIPrefab.prefab\n" +
                "5. HUDUIPrefab.prefab\n" +
                "6. MiniMapUIPrefab.prefab\n" +
                "7. GameOverUIPrefab.prefab",
                "OK"
            );
        }
    }
}
#endif
