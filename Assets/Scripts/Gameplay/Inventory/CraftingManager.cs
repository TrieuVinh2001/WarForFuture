using System;
using System.Collections.Generic;
using UnityEngine;
using WarForFuture.Data;
using WarForFuture.Network;

namespace WarForFuture.Gameplay.Inventory
{
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }

        [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();

        public event Action<RecipeData, bool> OnCraftResult; // recipe, success

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDefaultEquipmentRecipes();
        }

        private void InitializeDefaultEquipmentRecipes()
        {
            if (recipes.Count > 0) recipes.Clear();

            // 1. Sword (5 Wood, 3 Stone)
            AddRecipe(1, "Kiếm Sắt (Iron Sword)", ItemType.Sword, 1, new IngredientRequirement(ItemType.Wood, 5), new IngredientRequirement(ItemType.Stone, 3));

            // 2. Bow (5 Wood, 5 Fiber)
            AddRecipe(2, "Cung Gỗ (Wooden Bow)", ItemType.Bow, 1, new IngredientRequirement(ItemType.Wood, 5), new IngredientRequirement(ItemType.Fiber, 5));

            // 3. Arrow Pack x5 (2 Wood, 2 Fiber)
            AddRecipe(3, "Mũi Tên x5 (Arrows)", ItemType.Arrow, 5, new IngredientRequirement(ItemType.Wood, 2), new IngredientRequirement(ItemType.Fiber, 2));

            // 4. Food Ration (2 Fiber)
            AddRecipe(4, "Lương Khô Hồi HP (Food)", ItemType.Food, 1, new IngredientRequirement(ItemType.Fiber, 2));

            // 5. Mũ Da (Leather Helmet) - 4 Fiber, 2 Wood
            AddRecipe(5, "Mũ Da (Leather Helmet)", ItemType.HelmetItem, 1, new IngredientRequirement(ItemType.Fiber, 4), new IngredientRequirement(ItemType.Wood, 2));

            // 6. Áo Da (Leather Armor) - 8 Fiber, 4 Wood
            AddRecipe(6, "Áo Da (Leather Armor)", ItemType.ArmorItem, 1, new IngredientRequirement(ItemType.Fiber, 8), new IngredientRequirement(ItemType.Wood, 4));

            // 7. Quần Da (Leather Pants) - 6 Fiber, 3 Wood
            AddRecipe(7, "Quần Da (Leather Pants)", ItemType.PantsItem, 1, new IngredientRequirement(ItemType.Fiber, 6), new IngredientRequirement(ItemType.Wood, 3));

            // 8. Giày Da (Leather Boots) - 4 Fiber, 2 Wood
            AddRecipe(8, "Giày Da (Leather Boots)", ItemType.BootsItem, 1, new IngredientRequirement(ItemType.Fiber, 4), new IngredientRequirement(ItemType.Wood, 2));

            // 9. Găng Tay Da (Leather Gloves) - 3 Fiber, 2 Wood
            AddRecipe(9, "Găng Tay Da (Leather Gloves)", ItemType.GlovesItem, 1, new IngredientRequirement(ItemType.Fiber, 3), new IngredientRequirement(ItemType.Wood, 2));

            // 10. Vòng Cổ Đá (Stone Necklace) - 5 Stone, 3 Fiber
            AddRecipe(10, "Vòng Cổ Đá (Stone Necklace)", ItemType.NecklaceItem, 1, new IngredientRequirement(ItemType.Stone, 5), new IngredientRequirement(ItemType.Fiber, 3));

            // 11. Nhẫn Sắt (Iron Ring) - 3 Stone, 2 Fiber
            AddRecipe(11, "Nhẫn Sắt (Iron Ring)", ItemType.RingItem, 1, new IngredientRequirement(ItemType.Stone, 3), new IngredientRequirement(ItemType.Fiber, 2));
        }

        private void AddRecipe(int id, string name, ItemType resultType, int amount, params IngredientRequirement[] ingredients)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeData>();
            recipe.recipeId = id;
            recipe.recipeName = name;
            recipe.resultItemType = resultType;
            recipe.resultAmount = amount;
            recipe.ingredients = ingredients;
            recipes.Add(recipe);
        }

        public List<RecipeData> GetAllRecipes()
        {
            return recipes;
        }

        public RecipeData GetRecipeById(int id)
        {
            return recipes.Find(r => r.recipeId == id);
        }

        public bool TryCraft(CraftRequestMsg craftMsg, bool isNearWorkbench = true)
        {
            RecipeData recipe = GetRecipeById(craftMsg.recipeId);
            if (recipe == null)
            {
                Debug.LogWarning($"Crafting failed: Recipe ID {craftMsg.recipeId} not found.");
                OnCraftResult?.Invoke(null, false);
                return false;
            }

            if (!InventoryManager.Instance.HasIngredients(recipe.ingredients))
            {
                Debug.LogWarning($"Crafting failed: Insufficient ingredients for {recipe.recipeName}.");
                OnCraftResult?.Invoke(recipe, false);
                return false;
            }

            // Deduct ingredients and award product
            InventoryManager.Instance.DeductIngredients(recipe.ingredients);
            InventoryManager.Instance.AddItem(recipe.resultItemType, recipe.resultAmount);

            Debug.Log($"Successfully crafted {recipe.recipeName} x{recipe.resultAmount}!");
            OnCraftResult?.Invoke(recipe, true);
            return true;
        }
    }
}
