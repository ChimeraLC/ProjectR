using Godot;

public static class Consts
{
    public static int ScreenWidth = 1280;
    public static int ScreenHeight = 720;

    public static Color ColorDark = Color.Color8(36, 32, 36);

    // Used for points that should never be hit
    public static Vector2 VectorOffscreen = new Vector2(-10000, -10000);
}