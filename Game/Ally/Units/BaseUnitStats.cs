using System.Collections.Generic;
using Godot;
// Handles equipment and stat calculations of a unit
public partial class BaseUnit : GridNode
{

    [Export]
    public WeaponType currentWeaponType;
    public WeaponInfo currentWeapon {get; private set;} = WeaponData.Data[WeaponType.BasicBow];

    public EquipmentInfo currentHelmet;
    public EquipmentType currentHelmetType = EquipmentType.NoEquipment;
    public EquipmentInfo currentBody;
    public EquipmentType currentBodyType = EquipmentType.NoEquipment;
    public EquipmentInfo currentRelic;
    public EquipmentType currentRelicType = EquipmentType.NoEquipment;

    private bool SetWeaponDelegates()
    {
        switch (currentWeapon.category)
        {
            case WeaponCategory.Melee:
                AttackTick = BasicAttackDelegate;
                PassiveTick = BasicPassiveDelegate;
            break;
            case WeaponCategory.Projectile:
                AttackTick = BasicRangedAttackDelegate;
                PassiveTick = BasicPassiveDelegate;
            break;
            default:
                Logger.Log($"Weapon category {currentWeapon.category} has no delegates");
            return false;
        }

        return true;
    }

    private void SetCurrentWeapon()
    {
        if (!WeaponData.Data.ContainsKey(currentWeaponType))
        {
            Logger.Log($"Weapon type {currentWeaponType} has no associated data");
            return;
        }
        
        currentWeapon = WeaponData.Data[currentWeaponType];
        
        if (!SetWeaponDelegates())
            return;
        
        // TODO: Update sprites and stuff
    }

    private void SetCurrentEquipment()
    {
        // TODO: Update sprites and stuff
    }

    const int defaultHealth = 100;
    // TODO: Add in calculations based on equipment
    private float GetMaxHealth()
    {
        float retHealth = defaultHealth;
        retHealth += currentHelmet.healthBuff;
        retHealth += currentBody.healthBuff;
        retHealth += currentRelic.healthBuff;
        
        return retHealth;
    }

    private float GetDamageDealt()
    {
        float retDamage = currentWeapon.damage;
        retDamage += currentHelmet.damageBuff;
        retDamage += currentBody.damageBuff;
        retDamage += currentRelic.damageBuff;

        return retDamage;
    }

    private float GetDefense()
    {
        return currentHelmet.defense + currentBody.defense + currentRelic.defense;
    }

    // Time between each attack
    private float GetAttackSpacing()
    {
        return currentWeapon.attackSpacing;
    }

    // Distance required for attack
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