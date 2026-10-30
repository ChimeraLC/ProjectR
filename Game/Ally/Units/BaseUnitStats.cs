// Handles equipment and stat calculations of a unit
public partial class BaseUnit : GridNode
{

    public WeaponInfo currentWeapon {get; private set;} = WeaponData.BaseRangedData;

    // TODO: Add in calculations based on equipment
    private float GetMaxHealth()
    {
        return 100;
    }

    private float GetAttackSpacing()
    {
        return currentWeapon.attackSpacing;
    }

    private float GetAttackDistance()
    {
        return currentWeapon.attackDistance;
    }

    private float GetTargetRadius()
    {
        return 250;
    }

    private float GetMoveSpeed()
    {
        return currentWeapon.moveSpeed;
    }
}