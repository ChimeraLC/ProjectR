using Godot;
using System;
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
            LatentManager.RunLatentAction(0.7f, WeaponSwingDelegate, CheckAttackDistance);
            LatentManager.RunBasicAction(0.4f, DamageEnemyMelee);
        
            // Reset attack timer
            attackTimer += GetAttackSpacing();
        }
    }

    private void BasicPassiveDelegate(float delta)
    {
        attackTimer = Mathf.Max(0, attackTimer - delta);
    }

    private void WeaponSwingDelegate(float delta, float currentTick, float proportion)
    {
        weaponSprite.Rotation = Keyframes.GetValue(meleeSwingFrames, currentTick) *
        (Math.Abs(facingOct - 4) <= 1 ? -1 : 1);
    }

    private void DamageEnemyMelee()
    {
        if (targetUnit != null)
            targetUnit.TakeDamage(this, GetDamageDealt());
    }

    private void CheckAttackDistance()
    {
        // If enemy has left range
        if (State == UnitState.Attacking && (targetUnit.GetPosition() - GetPosition()).Length() < GetAttackDistance() + 20) // TODO: Padding should be determined elsewhere?
           State = UnitState.Chasing;
    }
}