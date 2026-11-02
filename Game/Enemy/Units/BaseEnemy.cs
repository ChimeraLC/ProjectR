using Godot;
using System;

public abstract class BaseEnemy : GridNode
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
            TickDeath(delta);
        }

        // Place z-index
        ZIndex = Utils.GetZIndex(GetPosition());

#if DEBUG
        Update();
#endif // #if DEBUG
    }

    protected abstract void TickAttack(float delta);
    protected virtual void TickDeath(float delta)
    {
        // Basic death tick
        deathTimer -= delta;
        Modulate = new Color(1, 1, 1, deathTimer);
        if (deathTimer <= 0)
            EndPlay();
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

    public override Hitbox GetHitbox()
    {   
        return hitbox;
    }
}
