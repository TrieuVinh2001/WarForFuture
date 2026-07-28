using System;
using System.Collections.Generic;
using UnityEngine;
using WarForFuture.Data;

namespace WarForFuture.Gameplay.Player
{
    public struct EquipmentStatBonus
    {
        public string name;
        public EquipmentSlot slot;
        public int hpBonus;
        public int attackBonus;
        public int defenseBonus;
        public float speedBonus;

        public EquipmentStatBonus(string name, EquipmentSlot slot, int hp, int atk, int def, float spd)
        {
            this.name = name;
            this.slot = slot;
            this.hpBonus = hp;
            this.attackBonus = atk;
            this.defenseBonus = def;
            this.speedBonus = spd;
        }
    }

    public class PlayerEquipmentManager : MonoBehaviour
    {
        public static PlayerEquipmentManager Instance { get; private set; }

        private readonly Dictionary<EquipmentSlot, EquipmentStatBonus> equippedItems = new Dictionary<EquipmentSlot, EquipmentStatBonus>();

        public event Action OnEquipmentChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public EquipmentStatBonus? GetEquippedItem(EquipmentSlot slot)
        {
            if (equippedItems.TryGetValue(slot, out var bonus))
            {
                return bonus;
            }
            return null;
        }

        public static EquipmentStatBonus GetItemStats(ItemType type)
        {
            switch (type)
            {
                case ItemType.HelmetItem:
                    return new EquipmentStatBonus("Mũ Da (Leather Helmet)", EquipmentSlot.Helmet, hp: 20, atk: 0, def: 5, spd: 0f);
                case ItemType.ArmorItem:
                    return new EquipmentStatBonus("Áo Da (Leather Armor)", EquipmentSlot.Armor, hp: 50, atk: 0, def: 12, spd: 0f);
                case ItemType.PantsItem:
                    return new EquipmentStatBonus("Quần Da (Leather Pants)", EquipmentSlot.Pants, hp: 25, atk: 0, def: 8, spd: 0.3f);
                case ItemType.BootsItem:
                    return new EquipmentStatBonus("Giày Da (Leather Boots)", EquipmentSlot.Boots, hp: 10, atk: 0, def: 3, spd: 1.0f);
                case ItemType.GlovesItem:
                    return new EquipmentStatBonus("Găng Tay Da (Leather Gloves)", EquipmentSlot.Gloves, hp: 10, atk: 8, def: 3, spd: 0f);
                case ItemType.NecklaceItem:
                    return new EquipmentStatBonus("Vòng Cổ Đá (Stone Necklace)", EquipmentSlot.Necklace, hp: 30, atk: 5, def: 5, spd: 0f);
                case ItemType.RingItem:
                    return new EquipmentStatBonus("Nhẫn Sắt (Iron Ring)", EquipmentSlot.Ring, hp: 15, atk: 12, def: 2, spd: 0.2f);
                default:
                    return new EquipmentStatBonus("None", EquipmentSlot.Helmet, 0, 0, 0, 0f);
            }
        }

        public bool EquipItem(ItemType itemType)
        {
            EquipmentStatBonus stats = GetItemStats(itemType);
            equippedItems[stats.slot] = stats;
            Debug.Log($"Equipped {stats.name} into {stats.slot} slot!");

            OnEquipmentChanged?.Invoke();
            return true;
        }

        public void UnequipSlot(EquipmentSlot slot)
        {
            if (equippedItems.ContainsKey(slot))
            {
                equippedItems.Remove(slot);
                Debug.Log($"Unequipped {slot} slot.");
                OnEquipmentChanged?.Invoke();
            }
        }

        public int GetTotalHpBonus()
        {
            int total = 0;
            foreach (var item in equippedItems.Values) total += item.hpBonus;
            return total;
        }

        public int GetTotalAttackBonus()
        {
            int total = 0;
            foreach (var item in equippedItems.Values) total += item.attackBonus;
            return total;
        }

        public int GetTotalDefenseBonus()
        {
            int total = 0;
            foreach (var item in equippedItems.Values) total += item.defenseBonus;
            return total;
        }

        public float GetTotalSpeedBonus()
        {
            float total = 0f;
            foreach (var item in equippedItems.Values) total += item.speedBonus;
            return total;
        }
    }
}
