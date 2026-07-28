using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using WarForFuture.Data;
using WarForFuture.Gameplay.Buildings;
using WarForFuture.Gameplay.Combat;
using WarForFuture.Gameplay.DayNightCycle;
using WarForFuture.Gameplay.Enemies;
using WarForFuture.Gameplay.Inventory;
using WarForFuture.Gameplay.Player;
using WarForFuture.Gameplay.Resources;
using WarForFuture.UI;

namespace WarForFuture.Gameplay
{
    public class GameBootstrapManager : MonoBehaviour
    {
        private void Awake()
        {
            EnsureSingletonsAndSceneSetup();
        }

        private void EnsureSingletonsAndSceneSetup()
        {
            // 1. Camera & CameraController Setup
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                mainCam.orthographic = true;
                mainCam.orthographicSize = 7f;
                camObj.tag = "MainCamera";
                camObj.transform.position = new Vector3(0, 0, -10);
            }

            if (mainCam.GetComponent<CameraController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController>();
            }

            // 2. Core Managers
            EnsureComponent<LocalGameServer>("LocalGameServer");
            EnsureComponent<InventoryManager>("InventoryManager");
            EnsureComponent<CraftingManager>("CraftingManager");
            EnsureComponent<BuildingGridManager>("BuildingGridManager");
            EnsureComponent<CombatSystem>("CombatSystem");
            EnsureComponent<DayNightCycleManager>("DayNightCycleManager");
            EnsureComponent<EnemySpawnerManager>("EnemySpawnerManager");
            EnsureComponent<PlayerEquipmentManager>("PlayerEquipmentManager");
            EnsureComponent<FloatingTextManager>("FloatingTextManager");

            // 3. Player Setup
            if (PlayerController.Instance == null)
            {
                var playerObj = new GameObject("Player");
                playerObj.transform.position = Vector3.zero;
                playerObj.AddComponent<SpriteRenderer>();
                playerObj.AddComponent<CircleCollider2D>();
                playerObj.AddComponent<PlayerController>();
            }

            // 4. Resource Nodes Setup (Trees & Stones around map)
            SetupResourceNodes();

            // 5. Canvas, EventSystem & uGUI Setup
            SetupCanvasAndUI();
        }

        private T EnsureComponent<T>(string name) where T : Component
        {
            T comp = FindObjectOfType<T>();
            if (comp == null)
            {
                var go = new GameObject(name);
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        private void SetupResourceNodes()
        {
            if (FindObjectsOfType<ResourceNode>().Length == 0)
            {
                Vector3[] treePositions = new Vector3[]
                {
                    new Vector3(-4, 3, 0),
                    new Vector3(-5, -2, 0),
                    new Vector3(4, 4, 0),
                    new Vector3(6, -3, 0),
                    new Vector3(-7, 2, 0),
                    new Vector3(7, 5, 0)
                };

                foreach (var pos in treePositions)
                {
                    var tree = new GameObject("Tree_ResourceNode");
                    tree.transform.position = pos;
                    tree.AddComponent<SpriteRenderer>();
                    tree.AddComponent<CircleCollider2D>();
                    var node = tree.AddComponent<ResourceNode>();
                    node.SetResourceType(ItemType.Wood);
                }

                Vector3[] stonePositions = new Vector3[]
                {
                    new Vector3(-2, 5, 0),
                    new Vector3(5, 2, 0),
                    new Vector3(-6, -4, 0),
                    new Vector3(3, -5, 0),
                    new Vector3(-3, -6, 0),
                    new Vector3(6, 1, 0)
                };

                foreach (var pos in stonePositions)
                {
                    var stone = new GameObject("Stone_ResourceNode");
                    stone.transform.position = pos;
                    stone.AddComponent<SpriteRenderer>();
                    stone.AddComponent<CircleCollider2D>();
                    var node = stone.AddComponent<ResourceNode>();
                    node.SetResourceType(ItemType.Stone);
                }
            }
        }

        private void SetupCanvasAndUI()
        {
            // Ensure EventSystem with InputSystemUIInputModule for New Input System compatibility
            EventSystem es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }
            else
            {
                var legacyModule = es.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    Destroy(legacyModule);
                }
                if (es.GetComponent<InputSystemUIInputModule>() == null)
                {
                    es.gameObject.AddComponent<InputSystemUIInputModule>();
                }
            }

            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("HUDCanvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();

                var uiManagerGo = new GameObject("UIManager");
                uiManagerGo.transform.SetParent(canvasGo.transform);
                uiManagerGo.AddComponent<HUDManager>();
                uiManagerGo.AddComponent<CraftingUI>();
                uiManagerGo.AddComponent<BuildingUI>();
                uiManagerGo.AddComponent<CampfireUI>();
                uiManagerGo.AddComponent<InventoryUI>();
                uiManagerGo.AddComponent<MiniMapUI>();
                uiManagerGo.AddComponent<GameOverUI>();
            }
        }
    }
}
