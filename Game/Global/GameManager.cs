using System;
using Godot;
public partial class GameManager : Node2D
{   
    public static GameManager Instance { get; private set; }
    public static GridManager gridManager { get; private set; }
    public static LatentManager latentManager { get; private set; }
    public static ProjectileManager projectileManager { get; private set; }

    public static InputManager inputManager { get; private set; }
    public static PlayerController playerController { get; private set; }

    // Only calculate mouse position once per frame
    private Vector2 mousePosition;
    public static Node2D WorldRoot;
    public static bool IsPaused { get; private set; }

    public static Font DefaultFont;
    public static long currentFrame;

    public override void _Ready()
    {
        Instance = this;
        gridManager = new GridManager();
        latentManager = new LatentManager();
        projectileManager = new ProjectileManager();

        IsPaused = false;

        SetupDebugConfigs();

        // Draw above everything else
        ZIndex = 1000;

        WorldRoot = GetTree().Root.GetNode<Node2D>("World");
        DefaultFont = new Control().GetFont("font");
        currentFrame = 0;
    }

    public static float GetTimeDilation()
    {
        return IsPaused ? 0 : 1;
    }


    public override void _Process(float delta)
    {
        float trueDelta = delta * GetTimeDilation();
        Tick(trueDelta);

#if DEBUG
        Update();
#endif
    }

    public void Tick(float delta)
    {
        currentFrame++;

        // Cached information
        mousePosition = GetViewport().GetMousePosition();

        // Ending the game (TEMP)
        if (Input.IsActionJustPressed("key_escape"))
            EndGame();

        // Running ticks
        if (inputManager != null)
            inputManager.Tick(delta);   

        if (playerController != null)
            playerController.Tick(delta);

        gridManager.Tick(delta);
        latentManager.Tick(delta);
        projectileManager.Tick(delta);
    }

    /// <summary> Force ends the game from anywhere </summary>
    public static void EndGame()
    {
        Instance.GetTree().Quit();
    }

    public static Vector2 GetMousePosition()
    {
        return Instance.mousePosition;
    }

    /// <summary> Initializes all debug flags </summary>
    private static void SetupDebugConfigs()
    {
        File fileReader = new File();
        if (fileReader.Open("res://configs.txt", File.ModeFlags.Read) == Error.Ok)
        {
            string content = fileReader.GetLine();
            while (content != "")
            {
                // Set debugs
                string[] splits = content.Split(" ");
                bool valid = false;
                if (splits.Length == 2 && splits[0].Length > 0)
                {
                    if (int.TryParse(splits[1], out int value))
                    {
                        valid = true;
                        DebugManager.SetFlag(splits[0], value);
                    }                    
                }

                if (!valid)
                    Logger.Log($"Configs has invalid line: \"{content}\"", LogLevel.warn);

                content = fileReader.GetLine();
            }
        }
    }

    public override void _Draw()
    {
        if (DebugManager.GetFlag("DebugDrawGrid") > 0)
        {
            gridManager.DrawGrid(this);
        }

        if (DebugManager.GetFlag("DebugDrawUnitHitboxes") > 0)
        {
            gridManager.DrawUnitHitboxes(this);
        }

        if (DebugManager.GetFlag("DebugDisplayStats") > 0)
        {
            displayStats();
        }
    }

    private void displayStats()
    {
        // Selection count
        string statString = "";

        // Selection count
        if (inputManager != null)
            statString += $"Selected: {inputManager.GetSelectionCount()}\n";

        DrawString(DefaultFont, new Vector2(40, 40), statString);
    }

}
