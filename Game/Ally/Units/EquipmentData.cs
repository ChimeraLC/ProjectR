using System.Collections.Generic;

public struct EquipmentInfo
{
    public EquipmentCategory category;
    public float defense;
    public float healthBuff;
    public float damageBuff;
    public float moveSpeedChange; // Heavy armor can slow down
    public EquipmentInfo(EquipmentCategory inCategory, float inDefense, float inHealth, float inDmg, float inMsChange)
    {
        category = inCategory;
        damageBuff = inDmg;
        healthBuff = inHealth;
        defense = inDefense;
        moveSpeedChange = inMsChange;
    }
}

public enum EquipmentCategory
{
    Any,
    Helmet,
    Body,
    Relic, // Can have a passive
}

public enum EquipmentType
{
    NoEquipment,
    WoodenHelmet
}


public static class EquipmentData
{
    public static Dictionary<EquipmentType, EquipmentInfo> Data = new Dictionary<EquipmentType, EquipmentInfo>()
    {
        // Category, defence, health, dmg buff, ms change
        [EquipmentType.NoEquipment] = new EquipmentInfo(EquipmentCategory.Any, 0, 0, 0, 0),
        [EquipmentType.WoodenHelmet] = new EquipmentInfo(EquipmentCategory.Helmet, 5, 20, 0, 0),
    };
}