using Godot;

public enum UnitState
{
    Idle,   // Not taking any action
    Watching, // Idle, but will start targeting enemies that enter range 
    Moving, // Moving, will not attack unless attacked
    Seeking, // Moving, will attack targets it finds
    Chasing,    // Actively following a target
    Attacking,  // Actively attacking a target
}

public delegate void UnitAttackDelegate(float delta);

public class BaseUnit : GridNode
{   
    private RectHitbox hitbox;
    public UnitState State {get; private set; }
    private bool seekingState; // If it still has a seek target

    private Vector2 targetPosition;
    private BaseEnemy targetUnit;
    // Seeking enemies
    private const float retargetTickLength = 0.5f; // How often it'll look for a new target
    private float retargetTimer = 0.5f;

    // Character statics
    private const float maxHealth = 100;


    public float MoveSpeed {get; private set;} = 40;
    public float AttackDistance {get; private set;} = 30;
    public float TargetRadius { get; private set;} = 250;
    public UnitAttackDelegate AttackTick {get; private set;} = null;
    public UnitAttackDelegate PassiveTick {get; private set;} = null;

    // Attack timer
    private float attackTimer = 0;
    private float attackSpacing = 1;

    public override void _Ready()
    {
        Initialize(GlobalPosition, GridNodeType.Ally);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 16));
    
        State = UnitState.Idle;

        AttackTick = BasicAttackDelegate;
        PassiveTick = BasicPassiveDelegate;

        Health = maxHealth; // TODO: Move this into a "GetMaxHealth()" function
    }

    public override void Tick(float delta)
    {
        switch (State)
        {
            case UnitState.Idle:
            break;
            case UnitState.Moving:
                TickMoving(delta);
            break;
            case UnitState.Watching:
                TickWatching(delta);
            break;
            case UnitState.Seeking:
                TickSeeking(delta);
            break;
            case UnitState.Chasing: 
                TickChasing(delta);
            break;
            case UnitState.Attacking:
                TickAttacking(delta);
            break;
        }

        if (PassiveTick != null)
            PassiveTick(delta);

#if DEBUG
        Update();
#endif // #if DEBUG
    }

    private void TickMoving(float delta)
    {
        (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
            targetPosition, delta * MoveSpeed);
        SetPosition(newPosition);
        if (reached)
            State = UnitState.Idle;
    }

    private void TickWatching(float delta)
    {
        // Look for new enemies
        TickRetarget(delta);
    }

    // Move, but also go on any new targets
    private void TickSeeking(float delta)
    {
        (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
            targetPosition, delta * MoveSpeed);
        SetPosition(newPosition);
        TickRetarget(delta);
        if (reached)
        {
            State = UnitState.Watching;
            seekingState = false;
        }
    }

    // Has an active target it's following
    private void TickChasing(float delta)
    {
        // Retargeting
        // TickRetarget(delta); // TODO: Shouldn't lose target

        if (targetUnit != null && targetUnit.inPlay)
        {
            (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
                targetUnit.GetPosition(), delta * MoveSpeed, AttackDistance);
            SetPosition(newPosition);
            if (reached)
            {
                // TODO: Do at least one attack
                State = UnitState.Attacking; // TODO: Differentiate between an idle attack, and a seeking attack
            }
        }
        else // Losing target
        {
            targetUnit = null;
            retargetTimer = 0; // Force a retarget immediately
            State = seekingState ? UnitState.Seeking : UnitState.Watching;
        }
    }

    // TODO: Have it be slightly harder to lose target than it is to get them
    private void TickRetarget(float delta)
    {
        retargetTimer -= delta;
        if (retargetTimer <= 0)
        {
            GridNode potentialUnit = GameManager.gridManager.GetClosestNode(GlobalPosition, TargetRadius, GridNodeType.Enemy);
            if (potentialUnit is BaseEnemy potentialEnemy) // TODO: Check if it's an 'enemy type' once that exists
            {
                targetUnit = potentialEnemy;
                retargetTimer += retargetTickLength * 2;
                State = UnitState.Chasing;
            }
            else
            {
                retargetTimer += retargetTickLength;
            }
        }
    }

    private void TickAttacking(float delta)
    {
            
        if (targetUnit != null && targetUnit.inPlay)
        {
            // Run attack tick
            if (AttackTick != null)
                AttackTick(delta);
        }
        else
        {
            // Retarget
            retargetTimer = 0;
            State = seekingState ? UnitState.Seeking : UnitState.Watching;
        }
    }

    public override void UpdateHitbox()
    {
        hitbox.center = GlobalPosition + new Vector2(0, -16);
    }

    public void SetMoveTarget(Vector2 location)
    {
        State = UnitState.Moving;
        targetPosition = location;
    }

    public void SetAttackMoveTarget(Vector2 location)
    {
        seekingState = true;
        State = UnitState.Seeking;
        targetPosition = location;

        // Immediately look for target
        retargetTimer = 0;
    }

    // Sets the unit's target. Should always be non-null
    public void SetUnitTarget(BaseEnemy target)
    {
        seekingState = false;
        if (target != null)
        {
            targetUnit = target;
            State = UnitState.Chasing;
        }
        else
        {
            Logger.Log("SetUnitTarget() recieved a null target", LogLevel.error);
        }
    }

    public bool Heal(float healAmount)
    {
        Health = Mathf.Min(Health + healAmount, maxHealth);
        return Health >= maxHealth;
    }

    public override Hitbox GetHitbox()
    {   
        return hitbox;
    }

    private void BasicAttackDelegate(float delta)
    {
        attackTimer -= delta;
        if (attackTimer <= delta)
        {
            targetUnit.DealDamage(10); // TODO: Get "Damage amount" or whatever
        
            attackTimer += attackSpacing;

            // Return to chasing in case enemy left range during attack
            State = UnitState.Chasing;
        }
    }

    private void BasicPassiveDelegate(float delta)
    {
        attackTimer = Mathf.Max(0, attackTimer - delta);
    }

#if DEBUG
    public override void _Draw()
    {
        if (DebugManager.CheckFlag("DebugDrawUnitBehavior"))
        {
            // Draw behavior
            switch (State)
            {
                case UnitState.Moving:
                    DrawLine(Vector2.Zero, targetPosition - GlobalPosition, Colors.Green);
                break;
                case UnitState.Seeking:
                    DrawLine(Vector2.Zero, targetPosition - GlobalPosition, Colors.Yellow);
                break;
                case UnitState.Attacking:
                case UnitState.Chasing:
                    DrawLine(Vector2.Zero, targetUnit.GetPosition() - GlobalPosition, Colors.Red);
                break;
            }     
            
            // Draw health
            float healthPercentage = Health / maxHealth;
            DrawLine(new Vector2(-16 * healthPercentage, -40), new Vector2(16 * healthPercentage, -40), Colors.Red, 4);
        }
    }
#endif // #if DEBUG
}
