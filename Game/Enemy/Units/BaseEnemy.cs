using Godot;
using System;

public class BaseEnemy : GridNode
{ 
    private const float maxHealth = 100;
    private RectHitbox hitbox;
    public UnitState State {get; private set;}

    private BaseUnit targetUnit;
    private const float retargetTickLength = 0.5f;
    private float retargetTimer = 0.5f;
    
    public float MoveSpeed {get; private set;} = 40;
    public float AttackDistance {get; private set;} = 30;
    public float TargetRadius { get; private set;} = 400;
    private float attackTimer = 0;
    private float attackSpacing = 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        Initialize(GlobalPosition, GridNodeType.Enemy);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 16));
    
        State = UnitState.Idle;
    
        Health = maxHealth;
    }

    public override void Tick(float delta)
    {
        if (targetUnit != null && targetUnit.inPlay)
        {
            (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
                targetUnit.GetPosition(), delta * MoveSpeed, AttackDistance);
            SetPosition(newPosition);
            if (reached)
            {
                attackTimer -= delta;
                if (attackTimer <= delta)
                {
                    targetUnit.DealDamage(10); // TODO: Get "Damage amount" or whatever
                
                    attackTimer += attackSpacing;
                }
            }
        }
        else
        {
            // TODO: Some kind of wandering behavior
            TickRetarget(delta);
        }

#if DEBUG
        Update();
#endif // #if DEBUG
    }

    private void TickRetarget(float delta)
    {
        
        retargetTimer -= delta;
        if (retargetTimer <= 0)
        {
            GridNode potentialUnit = GameManager.gridManager.GetClosestNode(GlobalPosition, TargetRadius, GridNodeType.Ally);
            if (potentialUnit is BaseUnit potentialAlly) // TODO: Check if it's an 'enemy type' once that exists
            {
                targetUnit = potentialAlly;
                retargetTimer += retargetTickLength * 2;
                State = UnitState.Chasing;
            }
            else
            {
                retargetTimer += retargetTickLength;
            }
        }
    }

    public override void UpdateHitbox()
    {   
        hitbox.center = GlobalPosition + new Vector2(0, -16);
    }

    public override Hitbox GetHitbox()
    {   
        return hitbox;
    }

#if DEBUG
    public override void _Draw()
    {  
        if (DebugManager.CheckFlag("DebugDrawUnitBehavior"))
        {
            float healthPercentage = Health / maxHealth;
            DrawLine(new Vector2(-16 * healthPercentage, -40), new Vector2(16 * healthPercentage, -40), Colors.Red, 4);
        }
    }
#endif
}
