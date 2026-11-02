using Godot;

// Enemy that has infinite hp and doesn't do anything
public class TestEnemy : BaseEnemy
{
    private Sprite bodySprite;
    
    public override void _Ready()
    {
        Initialize(GlobalPosition, GridNodeType.Enemy);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 24));
        bodySprite = GetNode<Sprite>("BodySprite");
    
        State = UnitState.Idle;
    
        Health = maxHealth;
    }

    protected override void TickAttack(float delta)
    {
        if (targetUnit != null && targetUnit.Alive)
        {
            (Vector2 newPosition, bool reached) = Utils.MoveTowards(GetPosition(),
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

            // Basic Animation
            bodySprite.Frame = Utils.GetOctant(targetUnit.GetPosition() - GetPosition());
        }
        else
        {
            // TODO: Some kind of wandering behavior
            TickRetarget(delta);
        }
    }

    public override void UpdateHitbox()
    {   
        hitbox.center = GlobalPosition + new Vector2(0, -24);
    }

#if DEBUG
    public override void _Draw()
    {  
        if (DebugManager.CheckFlag("DebugDrawUnitBehavior"))
        {
            float healthPercentage = Mathf.Max(0, Health / maxHealth);
            DrawLine(new Vector2(-16 * healthPercentage, -60), new Vector2(16 * healthPercentage, -60), Colors.Red, 4);
        }
    }
#endif
}