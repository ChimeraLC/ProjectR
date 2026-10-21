using System;
using System.Collections.Generic;
using System.Linq;
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

    public int GetSelectionCount() { return selectedUnits.Count; }

    public void Tick(float delta)
    {
        if (Input.IsActionJustPressed("key_select"))
        {
            mouseDown = GameManager.GetMousePosition();
            dragging = false;

            GameManager.gridManager.GetClosestNode(mouseDown, 200);
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

        if (Input.IsActionJustPressed("key_move"))
        {
            // Check if it's a move, or a target
            mouseCurrent = GameManager.GetMousePosition();
            GridNode potentialTarget = GameManager.gridManager.GetNodeAtPosition(mouseCurrent);
            if (potentialTarget is BaseEnemy potentialEnemy)
            {
                foreach (GridNode node in selectedUnits)
                {
                    if (node is BaseUnit unit)
                    {
                        unit.SetUnitTarget(potentialEnemy);
                    }
                }
            }
            else // Moving towards a location
            {
                SetUnitTargeLocation(false);
            }
        }

        if (Input.IsActionJustPressed("key_attackMove"))
        {
            SetUnitTargeLocation(true);
        }

        Update();
    }

    private void SetUnitTargeLocation(bool attacking)
    {
        // Basic 'don't overlap' code
        if (selectedUnits.Count == 1)
        {
            if (selectedUnits.First() is BaseUnit unit)
            {
                if (attacking)
                {
                    unit.SetAttackMoveTarget(GameManager.GetMousePosition());
                }
                else
                {
                    unit.SetMoveTarget(GameManager.GetMousePosition());
                }
            }
        }
        else if (selectedUnits.Count >= 2)
        {
            int index = 0;
            float angle = 2 * Mathf.Pi / selectedUnits.Count;
            foreach (GridNode node in selectedUnits)
            {
                index += 1;
                if (node is BaseUnit unit)
                {
                    Vector2 targetPosition = GameManager.GetMousePosition()
                        + 16 * new Vector2(Mathf.Cos(index * angle), Mathf.Sin(index * angle));
                    if (attacking)
                    {
                        unit.SetAttackMoveTarget(targetPosition);
                    }
                    else
                    {
                        unit.SetMoveTarget(targetPosition);
                    }
                }
            }
        }
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