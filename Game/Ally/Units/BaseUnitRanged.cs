using Godot;
using System;
using System.Collections.Generic;

// Melee behavior for the ally unit
public partial class BaseUnit : GridNode
{
    static KeyframesFloat rangedSwingFrames = new KeyframesFloat(
        new List<(float time, float value)>{
            (0, 0),
            (0.75f, Mathf.Pi / 2),
            (0.95f, -0.4f),
            (1f, 0)});
    private void BasicRangedAttackDelegate(float delta)
    {
        attackTimer -= delta;
        if (attackTimer <= delta)
        {
            LatentManager.RunLatentAction(1f, BowSwingDelegate, CheckAttackDistance);
            LatentManager.RunBasicAction(0.75f, ShootArrow);
        
            // Reset attack timer
            attackTimer += GetAttackSpacing();
        }
    }

    private void BowSwingDelegate(float delta, float currentTick, float proportion)
    {
        weaponSprite.Rotation = Keyframes.GetValue(rangedSwingFrames, currentTick) *
        (Math.Abs(facingOct - 4) <= 1 ? -1 : 1);
    }
    
    private void ShootArrow()
    {
        if (targetUnit != null)
        {
            ProjectileRequest arrowRequest = new ProjectileRequest()
            {
                projectileStart = GetPosition() + 16 * Vector2.Up,
                projectileEnd = targetUnit.GetPosition(),
                damage = GetDamageDealt(),
                target = targetUnit,
                duration = 0.5f,
            };

            ProjectileManager.RequestProjectile(this, arrowRequest);
        }
    }
}