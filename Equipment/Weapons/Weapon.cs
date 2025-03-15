using Godot;
using System;

public partial class Weapon : Equippable
{

    [Export]
    public int Damage {get; set;} = 15;

	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		MarkerTag = "Weapon";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		MarkerTag = "Weapon";
	}

	public override void Slot(){

	}

    public override void DeSlot(){

	}

	public override string GetParentNodeTag(){
		return "Pivot/WeaponPivot";
	}

	public override string GetMarkerTag(){
		return "Weapon";
	}

}