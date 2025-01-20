using Godot;
using System;

public partial class Weapon : AnimatableBody3D
{

    [Export]
    public int Damage {get; set;} = 15;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}