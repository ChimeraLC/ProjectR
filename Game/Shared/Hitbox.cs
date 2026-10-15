using Godot;
using System.Collections.Generic;


public interface HitboxOwner
{
    // Called before any hitbox calculations, so they aren't updated every frame
    void UpdateHitbox();
    Hitbox GetHitbox();
}

public abstract class Hitbox
{
    public int lastUpdatedHitboxFrame = -1;

    protected HitboxOwner hitboxOwner;
    public abstract (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction);
    /// <summary> Returns if given position overlaps hitbox </summary>
    public abstract (bool, HitboxOwner) GetContainsHit(Vector2 testPosition);
    public abstract Vector2 GetNormal(Vector2 testPoint);
    // TODO: Have this work for any type of hitbox
    public abstract (bool, HitboxOwner) OverlapsHitbox(RectHitbox hitbox);
    public void UpdateHitbox() { hitboxOwner.UpdateHitbox(); }
    public abstract void DrawHitbox( Node2D drawOwner );
}

// TODO: Checks for if ray starts already within hitbox?

// Rectable hitbox defined by its center and width/height
public class RectHitbox : Hitbox
{
    public Vector2 center;
    public Vector2 halfDimensions;
    public RectHitbox(HitboxOwner inOwner, Vector2 inCenter, Vector2 inHalfDimensions)
    {
        hitboxOwner = inOwner;
        center = inCenter;
        halfDimensions = inHalfDimensions;
    }

    public override (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        // Set center of ray to be at 0, 0
        Vector2 offsetCenter = center - start;

        // TODO: Could this ever 'slide' through a corner

        // Only need to check closer 2 sides
        if (!Mathf.IsZeroApprox(direction.x))
        {
            float xHit = offsetCenter.x + (direction.x > 0 ? -1 : 1) * halfDimensions.x;
            float hitTime = xHit / direction.x;
            // Valid hit that's within the bounds
            if (hitTime > 0 && Mathf.Abs(direction.y * hitTime - offsetCenter.y) <= halfDimensions.y)
                return (hitTime, hitboxOwner);
        }

        if (!Mathf.IsZeroApprox(direction.y))
        {
            float yHit = offsetCenter.y + (direction.y > 0 ? -1 : 1) * halfDimensions.y;
            float hitTime = yHit / direction.y;

            if (hitTime > 0 && Mathf.Abs(direction.x * hitTime - offsetCenter.x) <= halfDimensions.x)
                return (hitTime, hitboxOwner);
        }
        
        return (-1, hitboxOwner);
    }

    public override (bool, HitboxOwner) GetContainsHit(Vector2 testPosition)
    {
        Vector2 offsetCenter = testPosition - center;
        return (Mathf.Abs(offsetCenter.x) < halfDimensions.x && Mathf.Abs(offsetCenter.y) < halfDimensions.y,
            hitboxOwner);
    }

    public override Vector2 GetNormal(Vector2 testPoint)
    {
        Vector2 offset = testPoint - center;
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            return Mathf.Sign(offset.x) * Vector2.Right;
        }
        else
        {
            return Mathf.Sign(offset.y) * Vector2.Down;
        }
    }

    public override (bool, HitboxOwner) OverlapsHitbox(RectHitbox hitbox)
    {
        if (Mathf.Abs(center.x - hitbox.center.x) <= halfDimensions.x + hitbox.halfDimensions.x
        && Mathf.Abs(center.y - hitbox.center.y) <= halfDimensions.y + hitbox.halfDimensions.y)
            return (true, hitboxOwner);

        return (false, null);
    }

    public override void DrawHitbox(Node2D drawOwner)
    {
        drawOwner.DrawRect(new Rect2(center - halfDimensions, 2 * halfDimensions), Colors.Purple, false);
    }
}

// Circle hitbox defined by its center and radius
public class CircleHitbox : Hitbox
{
    public Vector2 center;
    public float radius;
    public CircleHitbox(HitboxOwner inOwner, Vector2 inCenter, float inRadius)
    {
        hitboxOwner = inOwner;
        center = inCenter;
        radius = inRadius;
    }

    public override (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        // Set center of ray to be at 0, 0
        Vector2 offsetCenter = center - start;

        // (tx - a)^2 + (ty - b)^2 = r^2
        // (x^2+y^2)t^2 - (2xa + 2yb)t + (a^2 + b^2 - r^2) = 0
        // This only works since direction is normalized
        return (Utils.PositiveQuadraticIfPossible(direction.x * direction.x + direction.y * direction.y,
            -2 * direction.x * offsetCenter.x - 2 * direction.y * offsetCenter.y,
            offsetCenter.x * offsetCenter.x + offsetCenter.y * offsetCenter.y - radius * radius), hitboxOwner);
    }

    public override (bool, HitboxOwner) GetContainsHit(Vector2 testPosition)
    {
        
        Vector2 offsetCenter = testPosition - center;
        // Initial check to avoid length queries
        if (Mathf.Abs(offsetCenter.x) < radius && Mathf.Abs(offsetCenter.y) < radius)
        {
            return (offsetCenter.LengthSquared() < radius * radius, hitboxOwner);
        }
        return (false, null);
    }

    public override (bool, HitboxOwner) OverlapsHitbox(RectHitbox hitbox)
    {
        // Closest point on the rectangle
        Vector2 closest = new Vector2(
            Mathf.Min(hitbox.center.x + hitbox.halfDimensions.x, Mathf.Max(hitbox.center.x - hitbox.halfDimensions.x, center.x)),
            Mathf.Min(hitbox.center.y + hitbox.halfDimensions.y, Mathf.Max(hitbox.center.y - hitbox.halfDimensions.y, center.y))
        );

        if ((closest - center).LengthSquared() <= radius * radius)
        {
            return (true, hitboxOwner);
        }
        return (false, null);
    }
    public override Vector2 GetNormal(Vector2 testPoint)
    {
        return (testPoint - center).Normalized();
    }

    
    public override void DrawHitbox(Node2D drawOwner)
    {
        drawOwner.DrawArc(center, radius, 0, Mathf.Pi * 2, 32, Colors.Purple);
    }
}

// Hitbox consisting of multiple hitboxes
public class CompoundHitbox : Hitbox
{
    private HashSet<Hitbox> hitboxes;

    public CompoundHitbox( HitboxOwner owner, ref HashSet<Hitbox> hitboxes )
    {
        this.hitboxes = hitboxes;
        hitboxOwner = owner;
    }

    public override (float, HitboxOwner) GetFirstRayHit(Vector2 start, Vector2 direction)
    {
        float firstHit = -1;
        HitboxOwner outOwner = hitboxOwner;

        foreach (Hitbox hitbox in hitboxes)
        {
            (float potentialHit, HitboxOwner potentialOwner) = hitbox.GetFirstRayHit(start, direction);
            if (potentialHit > 0)
            {
                if (firstHit < 0 || potentialHit < firstHit)
                {
                    firstHit = potentialHit;
                    outOwner = potentialOwner;
                }
            }
        }

        return (firstHit, outOwner);
    }

    public override (bool, HitboxOwner) GetContainsHit(Vector2 testPosition)
    {
        foreach (Hitbox hitbox in hitboxes)
        {
            (bool hit, HitboxOwner potentialOwner) = hitbox.GetContainsHit(testPosition);
            if (hit)
            {
                return (true, potentialOwner);
            }
        }
        return (false, hitboxOwner);
    }

    public override Vector2 GetNormal(Vector2 testPoint)
    {
        // TODO: This kind of sucks, assumes GetNormal() is right after any GetHits()
        Logger.Log("Compound hitbox was directly queried for normal", LogLevel.error);
        return Vector2.Up;
    }

    public override (bool, HitboxOwner) OverlapsHitbox(RectHitbox hitbox)
    {
        return (false, null);
    }

    public override void DrawHitbox(Node2D drawOwner)
    {
        foreach (Hitbox hitbox in hitboxes)
        {
            hitbox.UpdateHitbox();
            hitbox.DrawHitbox(drawOwner);
        }
    }
}