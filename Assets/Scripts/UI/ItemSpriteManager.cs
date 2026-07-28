using System.Collections.Generic;
using UnityEngine;
using WarForFuture.Data;

namespace WarForFuture.UI
{
    public static class ItemSpriteManager
    {
        private static readonly Dictionary<ItemType, Sprite> cachedSprites = new Dictionary<ItemType, Sprite>();

        public static Sprite GetItemSprite(ItemType itemType)
        {
            if (cachedSprites.TryGetValue(itemType, out var sprite) && sprite != null)
            {
                return sprite;
            }

            // Try loading from Resources/Art
            string resourceName = GetResourceName(itemType);
            if (!string.IsNullOrEmpty(resourceName))
            {
                Sprite resSprite = Resources.Load<Sprite>($"Art/{resourceName}");
                if (resSprite != null)
                {
                    cachedSprites[itemType] = resSprite;
                    return resSprite;
                }
            }

            // Generate high quality procedural icon sprite
            Sprite generatedSprite = CreateProceduralItemSprite(itemType);
            cachedSprites[itemType] = generatedSprite;
            return generatedSprite;
        }

        private static string GetResourceName(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Sword => "sword",
                ItemType.Bow => "bow",
                ItemType.HelmetItem => "helmet",
                ItemType.WallItem => "wall",
                ItemType.DoorItem => "door",
                ItemType.WorkbenchItem => "workbench",
                ItemType.CampfireItem => "campfire",
                ItemType.ChestItem => "chest",
                ItemType.WatchTowerItem => "tower",
                _ => null
            };
        }

        private static Sprite CreateProceduralItemSprite(ItemType itemType)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            // Default transparent background
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color primaryColor = GetItemPrimaryColor(itemType);
            Color accentColor = GetItemAccentColor(itemType);

            // Draw iconic shape on 64x64 texture
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = (x - 32f) / 32f;
                    float cy = (y - 32f) / 32f;
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);

                    bool isInsideShape = CheckInsideShape(itemType, cx, cy, dist);
                    if (isInsideShape)
                    {
                        // Inner fill + darker border
                        if (dist > 0.75f)
                        {
                            pixels[y * size + x] = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark border
                        }
                        else
                        {
                            float gradient = 1.0f - (dist * 0.4f);
                            pixels[y * size + x] = Color.Lerp(accentColor, primaryColor, gradient);
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
        }

        private static bool CheckInsideShape(ItemType type, float x, float y, float dist)
        {
            return type switch
            {
                ItemType.HelmetItem => (dist < 0.75f && y > -0.3f), // Dome helmet shape
                ItemType.ArmorItem => (dist < 0.8f && Mathf.Abs(x) < 0.7f), // Chestplate body shape
                ItemType.PantsItem => (Mathf.Abs(x) < 0.6f && y < 0.4f && y > -0.8f && (y > -0.1f || Mathf.Abs(x) > 0.15f)), // Leg pants shape
                ItemType.BootsItem => (Mathf.Abs(x) < 0.5f && y < 0.2f && y > -0.7f && (y < -0.3f || Mathf.Abs(x) < 0.35f)), // Pair of boots shape
                ItemType.GlovesItem => (dist < 0.65f), // Mittens / gloves shape
                ItemType.NecklaceItem => (dist > 0.4f && dist < 0.75f && y < 0.5f), // Amulet ring shape
                ItemType.RingItem => (dist > 0.35f && dist < 0.7f), // Ring circle shape
                ItemType.Food => (dist < 0.65f && (y > -0.4f)), // Meat / ration shape
                ItemType.Wood => (Mathf.Abs(x) < 0.35f && Mathf.Abs(y) < 0.75f), // Log shape
                ItemType.Stone => (dist < 0.7f && Mathf.Abs(x + y) < 0.9f), // Rock shape
                ItemType.Fiber => (dist < 0.65f && Mathf.Abs(x - y) < 0.6f), // Plant bundle shape
                ItemType.Arrow => (Mathf.Abs(x + y) < 0.15f && dist < 0.8f), // Arrow shaft shape
                _ => (dist < 0.75f)
            };
        }

        private static Color GetItemPrimaryColor(ItemType type)
        {
            return type switch
            {
                ItemType.HelmetItem => new Color(0.65f, 0.45f, 0.25f), // Brown leather
                ItemType.ArmorItem => new Color(0.55f, 0.35f, 0.2f),   // Dark brown leather
                ItemType.PantsItem => new Color(0.45f, 0.3f, 0.18f),  // Leather pants
                ItemType.BootsItem => new Color(0.35f, 0.22f, 0.12f),  // Leather boots
                ItemType.GlovesItem => new Color(0.7f, 0.5f, 0.3f),    // Soft leather
                ItemType.NecklaceItem => new Color(0.2f, 0.7f, 0.9f), // Cyan magic gem
                ItemType.RingItem => new Color(0.95f, 0.8f, 0.2f),    // Gold ring
                ItemType.Food => new Color(0.85f, 0.35f, 0.2f),       // Roasted meat
                ItemType.Wood => new Color(0.6f, 0.4f, 0.2f),         // Wood brown
                ItemType.Stone => new Color(0.55f, 0.55f, 0.6f),      // Stone gray
                ItemType.Fiber => new Color(0.2f, 0.8f, 0.3f),        // Plant green
                ItemType.Arrow => new Color(0.8f, 0.8f, 0.8f),        // Steel tip
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
        }

        private static Color GetItemAccentColor(ItemType type)
        {
            return type switch
            {
                ItemType.NecklaceItem => new Color(0.9f, 0.9f, 1.0f),
                ItemType.RingItem => new Color(1.0f, 0.95f, 0.6f),
                ItemType.Food => new Color(1.0f, 0.6f, 0.3f),
                ItemType.ArmorItem => new Color(0.8f, 0.5f, 0.3f),
                ItemType.HelmetItem => new Color(0.85f, 0.65f, 0.4f),
                _ => Color.white
            };
        }
    }
}
