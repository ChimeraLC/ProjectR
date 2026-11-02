using Godot;

public static partial class Utils
{
    // Temp function for y-sort
    public static int GetZIndex(Vector2 Position)
    {
        return (int) Position.y;
    }
}