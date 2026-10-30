public struct WeaponInfo
{
    public float attackSpacing;
    public float attackDistance;
    public float moveSpeed;
    public WeaponType weaponType;
    public WeaponInfo(WeaponType inWeaponType, float inAttackSpacing, float inAttackDistance, float inMoveSpeed)
    {
        attackSpacing = inAttackSpacing;
        attackDistance = inAttackDistance;
        moveSpeed = inMoveSpeed;
        weaponType = inWeaponType;
    }
}

public enum WeaponType
{
    Melee,
    Projectile
}

public static class WeaponData
{
    public static WeaponInfo BaseMeleeData = new WeaponInfo(WeaponType.Melee, 2, 30, 40);
    public static WeaponInfo BaseRangedData = new WeaponInfo(WeaponType.Projectile, 3, 150, 35);
}