using Godot;
using System;

public class BasicSlimeEnemy : BaseEnemy
{
    private Sprite bodySprite;
    private Node2D eyeSprite;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
        Initialize(GlobalPosition, GridNodeType.Enemy);
    
        hitbox = new RectHitbox(this, GlobalPosition, new Vector2(20, 16));

        bodySprite = GetNode<Sprite>("BodySprite");
        eyeSprite = GetNode<Node2D>("EyeSprite");
    
        State = UnitState.Idle;
    
        Health = maxHealth;
    }

    protected override void TickAttack(float delta)
    {
    }

    public override void UpdateHitbox()
    {   
        hitbox.center = GlobalPosition + new Vector2(0, -16);
    }
}
