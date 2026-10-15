using System.Collections.Generic;
using Godot;

public enum InputMode
{
    UnitSelection
}

public partial class InputManager : Node2D
{
    /// <summary> Minimum mouse distance to start drag select </summary>
    const float DragMinimum = 10;
    private Vector2 mouseDown; // Where mouse was first pressed down
    private Vector2 mouseCurrent;
    private bool dragging = false;

    // Tracks all currently selected nodes; can have both ally and enemy units (but not simultaneously)
    private HashSet<GridNode> selectedUnits;

    public override void _Ready()
    {
        GameManager.RegisterInputManager(this);

        selectedUnits = new HashSet<GridNode>();
    }

    public void Tick(float delta)
    {
        if (Input.IsActionJustPressed("key_select"))
        {
            mouseDown = GameManager.GetMousePosition();
            dragging = false;
        }

        if (Input.IsActionPressed("key_select"))
        {
            mouseCurrent = GameManager.GetMousePosition();
            if (Mathf.Abs(mouseCurrent.x - mouseDown.x) >= DragMinimum ||
                Mathf.Abs(mouseCurrent.y - mouseDown.y) >= DragMinimum)
            {
                dragging = true;
            }   
        }

        if (Input.IsActionJustReleased("key_select"))
        {
            if (!Input.IsActionPressed("key_shift"))
                selectedUnits.Clear(); // Holding shift to add more units
    
            // Getting all units within rectangle, or just at the given point
            if (dragging)
            {
                GameManager.gridManager.PushContainedNodes(mouseDown, mouseCurrent, selectedUnits);
            }
            else
            {
                GridNode potentialSelection = GameManager.gridManager.GetNodeAtPosition(mouseCurrent);
                if (potentialSelection != null)
                    selectedUnits.Add(potentialSelection);
            }

            dragging = false;
        }

        Update();
    }

    public override void _Draw()
    {
        // TODO: Move this into a ui element instead of draw
        if (dragging)
        {
            DrawRect(new Rect2(mouseDown, mouseCurrent - mouseDown).Abs(), Colors.Red, false);
        }

        foreach (GridNode unit in selectedUnits)
        {
            DrawRect(new Rect2(unit.Position - new Vector2(15, 10), new Vector2(30, 20)),
                Colors.Red, false);
        }
    }
    
}