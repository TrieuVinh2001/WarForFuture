namespace WarForFuture.Data
{
    public enum ItemType
    {
        Wood,
        Stone,
        Fiber,
        Food,
        Sword,
        Bow,
        Arrow,
        WallItem,
        DoorItem,
        WorkbenchItem,
        CampfireItem,
        ChestItem,
        WatchTowerItem,

        // Equipment Items
        HelmetItem,
        ArmorItem,
        PantsItem,
        BootsItem,
        GlovesItem,
        NecklaceItem,
        RingItem
    }

    public enum EquipmentSlot
    {
        Helmet,
        Armor,
        Pants,
        Boots,
        Gloves,
        Necklace,
        Ring
    }

    public enum ItemCategory
    {
        Resource,
        Weapon,
        Building,
        Consumable,
        Ammo,
        Equipment
    }

    public enum BuildingType
    {
        Wall,
        Door,
        Workbench,
        Campfire,
        Chest,
        WatchTower
    }

    public enum DayPhase
    {
        Day,
        Night
    }

    public enum EnemyState
    {
        Idle,
        Patrol,
        Detect,
        Attack,
        Dead
    }
}
