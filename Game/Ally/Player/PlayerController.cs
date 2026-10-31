using Godot;

public class PlayerController : Node2D
{
    
    // Parts of sprite
    private Sprite bodySprite;

    private Vector2 velocity;
    private const float ACCELERATION = 4000;
    private const float MAXSPEED = 300;
    private const float edgePadding = 40;
    private Vector2 movementInput;
    private float movementWobble = 0;

    // Spellcasting
    private float MaxMana = 100;
    private float Mana = 100;
    private float ManaRegen = 5;

    public override void _Ready()
    {
        GameManager.RegisterPlayerController(this);

        bodySprite = GetNode<Sprite>("BodySprite");
    
        Mana = MaxMana;
    }

    public void Tick(float delta)
    {
        TickInput(delta);
        TickMovement(delta);
        TickSpells(delta);

#if DEBUG
        Update();
#endif
    }

    private void TickInput(float delta)
    {
        Vector2 movementInputNow = new Vector2(Input.GetAxis("key_left", "key_right"),
            Input.GetAxis("key_up", "key_down"));

        if (movementInputNow != Vector2.Zero)
            movementInputNow = movementInputNow.Normalized();

        movementInput = movementInputNow;
    }

    private void TickSpells(float delta)
    {
        Mana = Mathf.Min(Mana + delta * ManaRegen, MaxMana);

        if (Input.IsActionJustPressed("key_ability1") && Mana > 40)
        {
            Mana -= 40;
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

    const int wobbleRate = 12;
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

        // 'wobble' 
        if (velocity.IsEqualApprox(Vector2.Zero))
        {
            movementWobble = Utils.MoveTowardsNearestMultiple(movementWobble, Mathf.Pi, delta * wobbleRate).Item1;
        }
        else
        {
            movementWobble = (movementWobble + delta * wobbleRate) % (Mathf.Pi * 2);
            bodySprite.Frame = Utils.GetOctant(velocity);
        }
        
        bodySprite.Rotation = Mathf.Sin(movementWobble) / 3;
        bodySprite.Position = Vector2.Up * Mathf.Abs(Mathf.Sin(movementWobble)) * 10;
    }

    public override void _Draw()
    {
          // Draw health
        float manaPercent = Mana / MaxMana;
        DrawLine(new Vector2(-16 * manaPercent, -60), new Vector2(16 * manaPercent, -60), Colors.Blue, 4);
    }

}
