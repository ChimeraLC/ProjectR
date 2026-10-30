using Godot;
using System.Collections.Generic;

// Melee behavior for the ally unit
public partial class BaseUnit : GridNode
{
    static KeyframesFloat meleeSwingFrames = new KeyframesFloat(
        new List<(float time, float value)>{
            (0, 0),
            (0.3f, -0.5f),
            (0.45f, 0.5f),
            (0.6f, 0)});
    private void BasicAttackDelegate(float delta)
    {
        attackTimer -= delta;
        if (attackTimer <= delta)
        {
            LatentManager.RunLatentAction(0.7f, WeaponSwingDelegate);
            LatentManager.RunBasicAction(0.4f, DamageEnemyMelee);
        
            attackTimer += GetAttackSpacing();

            // Return to chasing in case enemy left range during attack
            State = UnitState.Chasing;
        }
    }

    private void BasicPassiveDelegate(float delta)
    {
        attackTimer = Mathf.Max(0, attackTimer - delta);
    }

    private void WeaponSwingDelegate(float delta, float currentTick, float proportion)
    {
        weaponSprite.Rotation = Keyframes.GetValue(meleeSwingFrames, currentTick);
    }

    private void DamageEnemyMelee()
    {
        if (targetUnit != null)
            targetUnit.DealDamage(this, 10);
    }
}