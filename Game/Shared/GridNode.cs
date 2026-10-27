using Godot;
using System;

public enum GridNodeType
{
    Invalid,    // For queries and things
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
    public bool inPlay = true;
    public float Health {get; protected set;} = 100;
    public int targetBuffer = 0; // For large units, extra buffer for finding 'closest enemy'
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

    public new virtual Vector2 GetPosition() { return GlobalPosition; }

    public virtual void Destroy(Vector2 position)
    {
        if (inPlay)
        {
            Logger.Log($"Destroying unit {Name} that is still inPlay", LogLevel.warn);
        }
        GameManager.gridManager.RemoveGridNode(this);
    }

    public virtual bool DealDamage(float damageAmount)
    {
        if (Health <= 0)
            return false; // Don't doublecount damage

        Health -= damageAmount;
        if (Health <= 0)
        {
            inPlay = false;
            return false;
        }
        return true;
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
