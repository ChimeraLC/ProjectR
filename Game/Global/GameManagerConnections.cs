using Godot;
public partial class GameManager
{
    public static void RegisterInputManager(InputManager inInputManager)
    {
        Logger.Log("Registered Input Manager", LogLevel.debug);
        inputManager = inInputManager;
    }
}