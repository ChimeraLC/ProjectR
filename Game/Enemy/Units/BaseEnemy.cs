using Godot;
using System;

public class BaseEnemy : GridNode
{ 
    protected const float maxHealth = 100;
    protected RectHitbox hitbox;
    public UnitState State {get; protected set;}

    protected BaseUnit targetUnit;
    protected const float retargetTickLength = 0.5f;
    protected float retargetTimer = 0.5f;
    
    public float MoveSpeed {get; protected set;} = 40;
    public float AttackDistance {get; protected set;} = 30;
    public float TargetRadius { get; protected set;} = 400;
    protected float attackTimer = 0;
    protected float attackSpacing = 2;
    protected float deathTimer = 1;

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
        if (Alive)
        {
            TickAttack(delta);
        }
        else
        {
            // Do death behavior
            deathTimer -= delta;
            Modulate = new Color(1, 1, 1, deathTimer);
            if (deathTimer <= 0)
                EndPlay();
        }

#if DEBUG
        Update();
#endif // #if DEBUG
    }

    protected virtual void TickAttack(float delta)
    {
        if (targetUnit != null && targetUnit.Alive)
        {
            (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
                targetUnit.GetPosition(), delta * MoveSpeed, AttackDistance);
            SetPosition(newPosition);
            if (reached)
            {
                attackTimer -= delta;
                if (attackTimer <= delta)
                {
                    targetUnit.TakeDamage(this, 10); // TODO: Get "Damage amount" or whatever
                
                    attackTimer += attackSpacing;
                }
            }
        }
        else
        {
            // TODO: Some kind of wandering behavior
            TickRetarget(delta);
        }
    }

    protected void TickRetarget(float delta)
    {
        
        retargetTimer -= delta;
        if (retargetTimer <= 0)
        {
            GridNode potentialUnit = GridManager.Instance.GetClosestNode(GlobalPosition, TargetRadius, GridNodeType.Ally);
            if (potentialUnit is BaseUnit potentialAlly)
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
