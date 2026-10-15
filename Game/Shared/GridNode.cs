using Godot;
using System;

public enum GridNodeType
{
    Ally,
    Enemy,
    Generic
}

/// <summary>
/// Generic class for node that exists on the grid
/// </summary>
public abstract class GridNode : Node2D, HitboxOwner
{
    public int gridIndex = 0;
    public GridNodeType nodeType;
    public virtual void Initialize(Vector2 position, GridNodeType nodeType)
    {
        this.nodeType = nodeType;
        GameManager.gridManager.AddGridNode(this, position);
    }

    public virtual void Tick(float delta) {}

    // TODO: Does this actually override
    public new virtual void SetPosition(Vector2 newPosition)
    {
        GlobalPosition = newPosition;
        GameManager.gridManager.MoveGridNode(this, newPosition);
    }

    public virtual void Destroy(Vector2 position)
    {
        GameManager.gridManager.RemoveGridNode(this);
    }

    // Hitbox interface
    public  abstract void UpdateHitbox();
    public abstract Hitbox GetHitbox();
    public virtual void DrawHitbox(Node2D drawNode)
    {
        UpdateHitbox();
        GetHitbox().DrawHitbox(drawNode);
    }
}
