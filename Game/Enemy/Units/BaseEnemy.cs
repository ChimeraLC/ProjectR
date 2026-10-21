using Godot;
using System;

public class BaseEnemy : GridNode
{ 
    private RectHitbox hitbox;
    public UnitState State {get; private set;}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        Initialize(GlobalPosition, GridNodeType.Enemy);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 16));
    
        State = UnitState.Idle;
    }

    public override void UpdateHitbox()
    {   
        hitbox.center = GlobalPosition + new Vector2(0, -16);
    }

    public override Hitbox GetHitbox()
    {   
        return hitbox;
    }


}
