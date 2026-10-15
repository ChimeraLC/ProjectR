using Godot;
using System;

public class BaseUnit : GridNode
{   
    RectHitbox hitbox;
    
    public override void _Ready()
    {
        Initialize(GlobalPosition, GridNodeType.Ally);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(16, 16));
    }

    public override void Tick(float delta)
    {
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
