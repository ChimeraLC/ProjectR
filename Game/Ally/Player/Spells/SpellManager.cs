using System.Collections.Generic;
using Godot;

public enum Spell
{
    BasicHeal
}

// TODO: Prespawn based on equipped spells and then pool?
public class SpellManager
{
    Dictionary<Spell, PackedScene> spellScenes = new Dictionary<Spell, PackedScene>();

    public SpellManager()
    {
        LoadPrefabs();
    }

    // Setup prefabs
    public void LoadPrefabs()
    {
        spellScenes[Spell.BasicHeal] = GD.Load<PackedScene>("res://Game/Ally/Player/Spells/BasicHeal/BasicHealSpell.tscn");
        // TODO: Prespawn a couple to be the pool; only spawn equipped ones
    }

    public void CastSpell(Vector2 position, Spell spell)
    {
        CPUParticles2D newSpell = spellScenes[spell].Instance<CPUParticles2D>();
        newSpell.Position = position;
        GameManager.WorldRoot.AddChild(newSpell);
        newSpell.Emitting = true;
        LatentManager.RunBasicAction(2.0f, () => FinishSpell(newSpell));
    }

    public void FinishSpell(CPUParticles2D spell)
    {
        spell.QueueFree();
    }
}