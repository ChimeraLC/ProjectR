using Godot;

public static partial class Utils
{
    // Returns which 8th of the circle a vector points. Starting at right = 0, counterclockwise
    // 22.5 degrees -> 0.414, 1, 1.082
    const float octTan = 0.414f;
    public static int GetOctant(Vector2 direction)
    {
        float absX = Mathf.Abs(direction.x);
        float absY = Mathf.Abs(direction.y);

        if (absX < octTan * absY) // Along vertical line
            return direction.y > 0 ? 2 : 6;
        
        if (absY < octTan * absX) // Along horizontal line
            return direction.x > 0 ? 0 : 4;

        if (direction.x > 0)
        {
            return direction.y > 0 ? 1 : 7;
        }   
        else
        {
            return direction.y > 0 ? 3 : 5;
        }     
    }
}