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
    private UnitState returnState;

    private Vector2 targetPosition;
    private BaseEnemy targetUnit;
    // Seeking enemies
    private const float retargetTickLength = 1.0f; // How often it'll look for a new target
    private float retargetTimer = 0.5f;

    // Character statics
    public float Health {get; private set;} = 100;


    public float MoveSpeed {get; private set;} = 40;
    public float AttackDistance {get; private set;} = 30;
    public float TargetRadius { get; private set;} = 250;
    public UnitAttackDelegate AttackTick {get; private set;} = null;

    public override void _Ready()
    {
        Initialize(GlobalPosition, GridNodeType.Ally);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 16));
    
        State = UnitState.Idle;
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
                if (AttackTick != null)
                    AttackTick(delta);
            break;
        }

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
            State = returnState;
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
            State = UnitState.Watching;
    }

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
                State = UnitState.Idle;
            }
        }
        else // Losing target
        {
            targetUnit = null;
            retargetTimer = 0; // Force a retarget immediately
            State = returnState;
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
                State = returnState;
            }
        }
    }

    public override void UpdateHitbox()
    {
        hitbox.center = GlobalPosition + new Vector2(0, -16);
    }

    public void SetMoveTarget(Vector2 location)
    {
        returnState = UnitState.Idle;
        State = UnitState.Moving;
        targetPosition = location;
    }

    public void SetAttackMoveTarget(Vector2 location)
    {
        returnState = UnitState.Seeking;
        State = UnitState.Seeking;
        targetPosition = location;
    }

    // Sets the unit's target. Should always be non-null
    public void SetUnitTarget(BaseEnemy target)
    {
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

    public override Hitbox GetHitbox()
    {   
        return hitbox;
    }

#if DEBUG
    public override void _Draw()
    {
        if (DebugManager.CheckFlag("DebugDrawUnitBehavior"))
        {
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
        }
    }
#endif // #if DEBUG
}
