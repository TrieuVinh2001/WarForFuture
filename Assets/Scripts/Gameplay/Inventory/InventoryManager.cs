using System;
using System.Collections.Generic;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Network;

namespace WarForFuture.Gameplay.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        private readonly Dictionary<ItemType, int> items = new Dictionary<ItemType, int>();

        public event Action<ItemType, int, int> OnInventoryChanged; // itemType, currentCount, delta

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize default resources for MVP start testing
            AddItem(ItemType.Wood, 10);
            AddItem(ItemType.Stone, 10);
            AddItem(ItemType.Fiber, 5);
        }

        public int GetItemCount(ItemType itemType)
        {
            items.TryGetValue(itemType, out int count);
            return count;
        }

        public bool HasIngredients(IngredientRequirement[] ingredients)
        {
            if (ingredients == null || ingredients.Length == 0) return true;

            foreach (var req in ingredients)
            {
                if (GetItemCount(req.itemType) < req.amount)
                {
                    return false;
                }
            }
            return true;
        }

        public bool AddItem(ItemType itemType, int amount)
        {
            if (amount <= 0) return false;

            if (!items.ContainsKey(itemType))
            {
                items[itemType] = 0;
            }

            items[itemType] += amount;
            OnInventoryChanged?.Invoke(itemType, items[itemType], amount);
            return true;
        }

        public bool RemoveItem(ItemType itemType, int amount)
        {
            if (amount <= 0) return false;

            int current = GetItemCount(itemType);
            if (current < amount) return false;

            items[itemType] = current - amount;
            OnInventoryChanged?.Invoke(itemType, items[itemType], -amount);
            return true;
        }

        public bool DeductIngredients(IngredientRequirement[] ingredients)
        {
            if (!HasIngredients(ingredients)) return false;

            foreach (var req in ingredients)
            {
                RemoveItem(req.itemType, req.amount);
            }
            return true;
        }
    }
}
