using System.Collections.Generic;

public struct WeaponInfo
{
    public float attackSpacing;
    public float attackDistance;
    public float moveSpeed;
    public WeaponCategory category;
    public float damage;
    public WeaponInfo(WeaponCategory inWeaponType, float inDamage, float inAttackSpacing, float inAttackDistance, float inMoveSpeed)
    {
        attackSpacing = inAttackSpacing;
        damage = inDamage;
        attackDistance = inAttackDistance;
        moveSpeed = inMoveSpeed;
        category = inWeaponType;
    }
}

// Weapon categories. Each has their own run function
public enum WeaponCategory
{
    Melee,
    Projectile
}

// Individual weapon types, each with their own data
public enum WeaponType
{
    BasicSword,
    BasicBow
}

public static class WeaponData
{
    public static Dictionary<WeaponType, WeaponInfo> Data = new Dictionary<WeaponType, WeaponInfo>()
    {
        // Type, damage, spacing, range, ms
        [WeaponType.BasicSword] = new WeaponInfo(WeaponCategory.Melee, 15, 2, 30, 60),
        [WeaponType.BasicBow] = new WeaponInfo(WeaponCategory.Projectile, 10, 3, 150, 55),
    };
}