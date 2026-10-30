using Godot;

public class PlayerController : Node2D
{
    
    private Vector2 velocity;
    private const float ACCELERATION = 5000;
    private const float MAXSPEED = 400;
    private const float edgePadding = 40;
    Vector2 movementInput;
    public override void _Ready()
    {
        GameManager.RegisterPlayerController(this);
    }

    public void Tick(float delta)
    {
        TickInput(delta);
        TickMovement(delta);
    }

    private void TickInput(float delta)
    {
        Vector2 movementInputNow = new Vector2(Input.GetAxis("key_left", "key_right"),
            Input.GetAxis("key_up", "key_down"));

        if (movementInputNow != Vector2.Zero)
            movementInputNow = movementInputNow.Normalized();

        movementInput = movementInputNow;

        if (Input.IsActionJustPressed("key_slot1"))
        {
            GridManager.Instance.ForeachDelegateInRadius(Position,
                100, HealSpell, GridNodeType.Ally);
        }
    }

    private void HealSpell(GridNode node)
    {
        if (node is BaseUnit unit)
        {
            unit.Heal(20);
        }
    }

    private void TickMovement(float delta)
    {
        
        velocity = Utils.MoveTowards(velocity, movementInput * MAXSPEED, ACCELERATION * delta).Item1;
        Vector2 newPosition = Position + velocity * delta;
        if (newPosition.x > Consts.ScreenWidth - edgePadding)
            newPosition.x = Consts.ScreenWidth - edgePadding;
        if (newPosition.x < edgePadding)
            newPosition.x = edgePadding;
        if (newPosition.y > Consts.ScreenHeight - edgePadding)
            newPosition.y = Consts.ScreenHeight - edgePadding;
        if (newPosition.y < edgePadding)
            newPosition.y = edgePadding;

        Position = newPosition;
    }
}
