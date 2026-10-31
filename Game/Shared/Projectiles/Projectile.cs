using System;
using System.Collections.Generic;
using Godot;

public enum ProjectileType
{
    Arrow,
}

// Struct for all possible data in a projectile request. Not everything is used for every type
public struct ProjectileRequest
{
    public ProjectileType projectileType;
    public GridNode target;
    public Vector2 projectileStart;
    public Vector2 projectileEnd;
    public float damage;
    public float speed;
    public float duration;
}

public class Projectile : Node2D
{
    public bool Active = false;
    private Node2D Sprite;

    public ProjectileType activeType;

    private Dictionary<ProjectileType, (Action<float> tickAction, Action initAction)> projectileBehaviors;

    private ProjectileRequest curReq;

    // Internal data for different projectile types
    private GridNode owner;
    private Vector2 startPos;
    private Vector2 endPos;
    private float duration;
    private float distance;
    private Vector2 direction;
    private float age = 0;

    public override void _Ready()
    {
        Sprite = GetNode<Node2D>("ProjectileSprite");

        // Has to be in _Ready() because of compiler stuff?
        projectileBehaviors = new Dictionary<ProjectileType, (Action<float> tickAction, Action initAction)>()
        {
            [ProjectileType.Arrow] = (ArrowTick, ArrowInit),
        };

    }

    public void Initialize(GridNode inOwner, ProjectileRequest projectileRequest)
    {
        owner = inOwner;
        activeType = projectileRequest.projectileType;
        curReq = projectileRequest;
        Active = true;
        age = 0;

        if (projectileBehaviors.ContainsKey(activeType))
        {
            projectileBehaviors[activeType].initAction();
        }
        else
        {
            Logger.Log($"Projectile with type {activeType} has no start action", LogLevel.warn);
            Active = false;
        }
    }

    public virtual void Destroy()
    {
        QueueFree();
    }

    public virtual void Tick(float delta)
    {
        if (Active)
        {
            age += delta;
            if (projectileBehaviors.ContainsKey(activeType))
            {
                projectileBehaviors[activeType].tickAction(delta);
            }
            else
            {
                Logger.Log($"Projectile with type {activeType} has no assigned actions", LogLevel.warn);
            }
        }
        else
        {
            Logger.Log("Projectile ticking while not active", LogLevel.error);
        }
    }

// MARK: Projectile Types

    private void ArrowInit()
    {
        startPos = curReq.projectileStart;
        endPos = curReq.projectileEnd;
        Position = startPos;
        direction = endPos - startPos;
        duration = curReq.duration;
    }

    private void ArrowTick(float delta)
    {
        float travelProportion = age / duration;

        if (curReq.target.InPlay == false)
            curReq.target = null;

        if (travelProportion >= 1)
        {
            // Finish projectile action
            if (curReq.target != null)
                curReq.target.TakeDamage(owner, curReq.damage);
    
            Active = false;
        }
        else
        {
            Position = startPos + travelProportion * direction;
        }
    }
}