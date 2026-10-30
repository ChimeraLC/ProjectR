using Godot;

public static partial class Utils
{
    public static (Vector2, bool) MoveTowards(Vector2 start, Vector2 goal, float distance, 
        float padding = 0, bool isWithin = false)
    {
        float curDistance = (goal-start).Length();

        // Easy out
        if (padding <= 0 && curDistance <= distance)
            return (goal, true);
        if (isWithin && curDistance <= padding)
            return (start, true);

        (float goalDistance, bool reached) = MoveTowards(curDistance, padding, distance);

        Vector2 direction = curDistance == 0 ? Vector2.Left : (start - goal) / curDistance;
        
        return (goal + direction * goalDistance, reached);
    }

    
    public static (Vector2, bool) MoveTowardsWithin(Vector2 start, Vector2 goal, float distance, 
        float padding = 0)
    {
        float curDistance = (goal-start).Length();

        // Easy out
        if (curDistance <= distance)
            return (goal, true);

        (float goalDistance, bool reached) = MoveTowards(curDistance, padding, distance);

        Vector2 direction = curDistance == 0 ? Vector2.Left : (start - goal) / curDistance;
        
        return (goal + direction * goalDistance, reached);
    }

    public static (float, bool) MoveTowards(float start, float end, float distance, float padding = 0)
    {
        float curDistance = Mathf.Abs(start - end);
        
        // Close enough to goal position
        if (Mathf.Abs(curDistance - padding) < distance)
            return (end + padding * Mathf.Sign(start - end), true);

        if (curDistance < padding) 
        {
            return (start + distance * Mathf.Sign(start - end), false);
        }
        else
        {
            return (start + distance * Mathf.Sign(end - start), false);   
        }
    }

    /// <summary> Returns smallest positive solution if it exists; returns -1 in all other cases </summary>
    public static float PositiveQuadraticIfPossible(float a, float b, float c)
    {
        if (a == 0)
            return -c / b;

        float det = b * b - 4 * a * c;
        if (det < 0)
            return -1;
        det = Mathf.Sqrt(det);
        
        float sol1 = (-b + det) / (2 * a);
        float sol2 = (-b - det) / (2 * a);

        if (sol1 < 0)
            return sol2;
        
        if (sol2 < 0)
            return sol1;
        
        return sol1 < sol2 ? sol1 : sol2;
    }
}