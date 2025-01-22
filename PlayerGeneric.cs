using Godot;
using System;

public partial class PlayerGeneric : Player
{

	public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        GD.Print(@event.AsText());
    }

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
}
