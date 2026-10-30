using Godot;
using System;
using System.Collections.Generic;

// Manages tracking all the active projectiles
public partial class ProjectileManager
{
    public const int PoolSize = 16;
    private int poolIndex = 0;
    public static ProjectileManager Instance { get; private set;}

    private PackedScene projectileScene;
    private HashSet<Projectile> activeProjectiles;
    private HashSet<Projectile> GCProjectiles;
    private List<Projectile> projectilePool;

    public ProjectileManager()
    {
        activeProjectiles = new HashSet<Projectile>();
        GCProjectiles = new HashSet<Projectile>();
        projectilePool = new List<Projectile>();

        projectileScene = GD.Load<PackedScene>("res://Game/Shared/Projectiles/Projectile.tscn");

        // for (int i = 0; i < PoolSize; i++)
        // {
        //     AddProjectileToPool(true);
        // }

        Instance = this;
    }

    private Projectile AddProjectileToPool(bool atStart)
    {
        Projectile newProjectile = projectileScene.Instance<Projectile>();
        newProjectile.Position = new Vector2(-2000, -2000);
        GameManager.WorldRoot.AddChild(newProjectile);
        projectilePool.Add(newProjectile);

        if (!atStart)
        {
            Logger.Log($"Adding new projectile to pool. Current size {projectilePool.Count}",
                LogLevel.info);
        }

        return newProjectile;
    }

    public static void RegisterProjectile(Projectile newProjectile)
    {
        if (Instance != null)
        {
            Instance.activeProjectiles.Add(newProjectile);
        }
        else
        {
            Logger.Log($"Attempted to register projectile {newProjectile.Name} before manager was created",
                LogLevel.error); 
        }
    }

    public static void RequestProjectile(GridNode owner, ProjectileRequest requestData)
    {
        if (Instance != null)
        {
            Instance.RequestProjectileInternal(owner, requestData);
        }
        else
        {
            Logger.Log("Requested projectile before manager created", LogLevel.error);
        }
    }

    public void RequestProjectileInternal(GridNode owner, ProjectileRequest requestData)
    {
        // Find active pool
        Projectile foundProjectile = null;
        for (int i = 0; i < projectilePool.Count; i++)
        {
            poolIndex = (poolIndex + 1) % projectilePool.Count;
            if (!projectilePool[poolIndex].Active)
            {
                foundProjectile = projectilePool[poolIndex];
                break;
            }   
        }

        if (foundProjectile == null)
            foundProjectile = AddProjectileToPool(false);

        // Activate projectile
        foundProjectile.Initialize(owner, requestData);
        activeProjectiles.Add(foundProjectile);
    }

    public void Tick(float delta)
    {
        GCProjectiles.Clear();

        foreach (Projectile projectile in activeProjectiles)
        {
            if (!projectile.Active)
            {
                GCProjectiles.Add(projectile);
            }
            else
            {
                projectile.Tick(delta);
            }
        }

        foreach (Projectile finishedProjectile in GCProjectiles)
        {
            activeProjectiles.Remove(finishedProjectile);

            // Remove from visibility
            finishedProjectile.Position = new Vector2(-2000, -2000);
        }
    }
}