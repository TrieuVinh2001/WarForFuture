using System;
using UnityEngine;

namespace WarForFuture.Data
{
    [Serializable]
    public struct IngredientRequirement
    {
        public ItemType itemType;
        public int amount;

        public IngredientRequirement(ItemType itemType, int amount)
        {
            this.itemType = itemType;
            this.amount = amount;
        }
    }

    [CreateAssetMenu(fileName = "NewRecipeData", menuName = "WarForFuture/Data/RecipeData")]
    public class RecipeData : ScriptableObject
    {
        public int recipeId;
        public string recipeName;
        public ItemType resultItemType;
        public int resultAmount = 1;
        public IngredientRequirement[] ingredients;
        public bool requiresWorkbench;
        public Sprite icon;
    }
}
