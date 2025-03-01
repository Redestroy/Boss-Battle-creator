using Godot;
using System;

public partial class Item : Node3D
{

	[Export]
	public string node_path{get; set;} = "";

	private bool delete_this = false;
	[Export]
	private bool on_ground = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Godot.Area3D pickup_area = GetNode<Godot.Area3D>("Area3D");
		pickup_area.BodyEntered += _on_area_body_entered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(delete_this){
			Despawn();
		}
	}

	public void _on_area_body_entered(Node3D body){
		GD.Print($"{body} entered item pickup area");
		if(body is Player && on_ground){
			GD.Print($"{body} is player");
			var pl = body as Player;
			OnPickup(pl);
		}
	}

	public void Despawn(){
		QueueFree();
	}

	public void DropItem(Marker3D marker){
		this.Position = marker.GlobalPosition;
		on_ground = true;
	}

	public void OnPickup(Player player){
		player.AddItemInInventory(node_path);
		delete_this = true;
	}
}
