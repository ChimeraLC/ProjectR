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
            LatentManager.RunLatentAction(0.7f, WeaponSwingDelegate);
            LatentManager.RunBasicAction(0.4f, ShootArrow);
        
            attackTimer += GetAttackSpacing();

            // Return to chasing in case enemy left range during attack
            State = UnitState.Chasing;
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
                damage = 10,
                target = targetUnit,
                duration = 0.2f,
            };

            ProjectileManager.RequestProjectile(this, arrowRequest);
        }
    }
}