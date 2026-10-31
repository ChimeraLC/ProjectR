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

public partial class BaseUnit : GridNode
{   
    // Parts of sprite
    private Sprite bodySprite;
    private Node2D weaponSprite;

    // Generic stuff
    private float movementWobble = 0;
    private RectHitbox hitbox;
    public UnitState State {get; private set; }
    private bool seekingState; // If it still has a seek target

    private Vector2 targetPosition;
    private BaseEnemy targetUnit;
    private int facingOct;
    // Seeking enemies
    private const float retargetTickLength = 0.5f; // How often it'll look for a new target
    private float retargetTimer = 0.5f;

    public UnitAttackDelegate AttackTick {get; private set;} = null;
    public UnitAttackDelegate PassiveTick {get; private set;} = null;

    // Attack timer
    private float attackTimer = 0;

    public override void _Ready()
    {
        Initialize(GlobalPosition, GridNodeType.Ally);
    
        // Get sprites
        bodySprite = GetNode<Sprite>("BodySprite");
        weaponSprite = bodySprite.GetNode<Node2D>("WeaponSprite");

        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 20));
    
        State = UnitState.Idle;

        SetCurrentWeapon();

        Health = GetMaxHealth();
    }

    public override void Tick(float delta)
    {
        if (Alive)
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

            TickAnimation(delta);
        }
        else
        {
            // When dead, don't leave play, since can be 'revived'    
        }

#if DEBUG
        Update();
#endif // #if DEBUG
    }

    const int wobbleRate = 8;
    private void TickAnimation(float delta)
    {
        // Basic 'wobble' when moving
        if (IsMoving())
        {
            movementWobble = (movementWobble + delta * wobbleRate) % (Mathf.Pi * 2);
        }
        else
        {
            movementWobble = Utils.MoveTowardsNearestMultiple(movementWobble, Mathf.Pi, delta * wobbleRate).Item1;
        }

        bodySprite.Rotation = Mathf.Sin(movementWobble) / 3;
        bodySprite.Position = Vector2.Up * Mathf.Abs(Mathf.Sin(movementWobble)) * 10;

        // Look direction
        Vector2 lookDirection = (State == UnitState.Chasing || State == UnitState.Attacking) ? 
            (targetUnit.GetPosition() - GetPosition()) : // Currently focused on a target
            targetPosition - GetPosition(); // Focused on a position
        
        facingOct = Utils.GetOctant(lookDirection);
        bodySprite.Frame = facingOct;
    }

    private bool IsMoving()
    {
        return State == UnitState.Moving || State == UnitState.Chasing || State == UnitState.Seeking;
    }

    private void TickMoving(float delta)
    {
        (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
            targetPosition, delta * GetMoveSpeed());
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
            targetPosition, delta * GetMoveSpeed());
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
        if (targetUnit != null && targetUnit.Alive)
        {
            (Vector2 newPosition, bool reached) = Utils.MoveTowards(GlobalPosition,
                targetUnit.GetPosition(), delta * GetMoveSpeed(), GetAttackDistance(), true);
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
            Retarget();
        }
    }

    private void Retarget()
    {
        GridNode potentialUnit = GridManager.Instance.GetClosestNode(GlobalPosition, GetTargetRadius(), GridNodeType.Enemy);
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

    private void TickAttacking(float delta)
    {
            
        if (targetUnit != null && targetUnit.Alive)
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
        hitbox.center = GlobalPosition + new Vector2(0, -20);
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
        Retarget();
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

    public override bool TakeDamage(GridNode source, float damageAmount)
    {
        // Do defense damage reductions
        damageAmount = Mathf.Max(0, damageAmount - GetDefense());
        return base.TakeDamage(source, damageAmount);
    }


    public bool Heal(float healAmount)
    {
        Health = Mathf.Min(Health + healAmount, GetMaxHealth());
        return Health >= GetMaxHealth();
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
            float healthPercentage = Health / GetMaxHealth();
            DrawLine(new Vector2(-16 * healthPercentage, -60), new Vector2(16 * healthPercentage, -60), Colors.Red, 4);
        }
    }
#endif // #if DEBUG
}
