using System;
using System.Collections;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Gameplay.Inventory;

namespace WarForFuture.Gameplay.Resources
{
    public class ResourceNode : MonoBehaviour
    {
        [SerializeField] private ItemType resourceType = ItemType.Wood;
        [SerializeField] private int maxHits = 3;
        [SerializeField] private int yieldAmount = 3;
        [SerializeField] private float respawnDuration = 15f;

        private int currentHits;
        private bool isDepleted = false;
        private SpriteRenderer spriteRenderer;
        private Collider2D nodeCollider;

        public event Action<ItemType, int> OnHarvested;

        public void SetResourceType(ItemType type)
        {
            resourceType = type;
            ApplySprite();
        }

        private void Awake()
        {
            currentHits = maxHits;
            spriteRenderer = GetComponent<SpriteRenderer>();
            nodeCollider = GetComponent<Collider2D>();

            ApplySprite();
        }

        private void ApplySprite()
        {
            if (spriteRenderer == null) return;

            string path = (resourceType == ItemType.Wood) ? "Art/tree" : "Art/stone";
            Sprite loadedSprite = UnityEngine.Resources.Load<Sprite>(path);
            if (loadedSprite != null)
            {
                spriteRenderer.sprite = loadedSprite;
                float scale = (resourceType == ItemType.Wood) ? 0.15f : 0.10f;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
            else if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreatePlaceholderSprite(resourceType);
            }
        }

        public void TakeHit(int damage = 1)
        {
            if (isDepleted) return;

            currentHits -= damage;
            Debug.Log($"Resource Node {resourceType} hit ({currentHits}/{maxHits})");

            if (currentHits <= 0)
            {
                DepleteNode();
            }
        }

        private void DepleteNode()
        {
            isDepleted = true;
            Debug.Log($"Resource Node {resourceType} depleted! Yielded {yieldAmount} items.");

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(resourceType, yieldAmount);
                if (resourceType == ItemType.Wood)
                {
                    InventoryManager.Instance.AddItem(ItemType.Fiber, 1);
                }
            }

            OnHarvested?.Invoke(resourceType, yieldAmount);
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (nodeCollider != null) nodeCollider.enabled = false;

            yield return new WaitForSeconds(respawnDuration);

            currentHits = maxHits;
            isDepleted = false;

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            if (nodeCollider != null) nodeCollider.enabled = true;
            Debug.Log($"Resource Node {resourceType} respawned!");
        }

        private Sprite CreatePlaceholderSprite(ItemType type)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color col = (type == ItemType.Wood) ? new Color(0.1f, 0.7f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
