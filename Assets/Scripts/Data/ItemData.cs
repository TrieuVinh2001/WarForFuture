using UnityEngine;

namespace WarForFuture.Data
{
    [CreateAssetMenu(fileName = "NewItemData", menuName = "WarForFuture/Data/ItemData")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string itemName;
        public ItemType itemType;
        public ItemCategory category;
        public Sprite icon;
        public int maxStack = 99;
        [TextArea(2, 4)]
        public string description;
    }
}
