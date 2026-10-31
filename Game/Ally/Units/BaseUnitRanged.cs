using Godot;
using System.Collections.Generic;

// Melee behavior for the ally unit
public partial class BaseUnit : GridNode
{
    private void BasicRangedAttackDelegate(float delta)
    {
        attackTimer -= delta;
        if (attackTimer <= delta)
        {
            LatentManager.RunLatentAction(0.7f, WeaponSwingDelegate, CheckAttackDistance);
            LatentManager.RunBasicAction(0.4f, ShootArrow);
        
            // Reset attack timer
            attackTimer += GetAttackSpacing();
        }
    }

    
    private void ShootArrow()
    {
        if (targetUnit != null)
        {
            ProjectileRequest arrowRequest = new ProjectileRequest()
            {
                projectileStart = GetPosition(),
                projectileEnd = targetUnit.GetPosition(),
                damage = GetDamageDealt(),
                target = targetUnit,
                duration = 0.2f,
            };

            ProjectileManager.RequestProjectile(this, arrowRequest);
        }
    }
}