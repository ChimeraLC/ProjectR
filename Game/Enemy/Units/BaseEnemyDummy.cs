using Godot;

// Enemy that has infinite hp and doesn't do anything
public class BaseEnemyDummy : BaseEnemy
{
    
    public override void _Ready()
    {
        Initialize(GlobalPosition, GridNodeType.Enemy);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 16));
    
        State = UnitState.Idle;
    
        Health = Mathf.Inf;
    }

    public override void Tick(float delta)
    {
    }

    protected override void TickAttack(float delta)
    {
    }

    public override void UpdateHitbox()
    {   
        hitbox.center = GlobalPosition + new Vector2(0, -16);
    }
    
    protected override void TickDeath(float delta)
    {
    }
}