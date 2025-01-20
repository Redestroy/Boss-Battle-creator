using Godot;
using System;

public partial class Door : Node3D
{

	[Signal]
    public delegate void LookingAtDoorEventHandler(InputEvent ev, string door_text);

	[Export]
	public string scene_triggered {get; set;} = "res://Arena_shade.tscn";

	[Export]
	public string door_text{get; set;} = "Enter to proceed";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_input_event(Camera3D camera, InputEvent ev, Vector3 event_position, Vector3 normal, int shape_idx){
		GD.Print("Door event");
		OnDoorEvent(ev);
	}

	public void OnDoorEvent(InputEvent ev){
		// If mouse cursor entered
		// light up door
		//If Raction
		//Change scene to boss fight
		if (ev.IsAction("Interact")){
			OnDoorTriggered();
		}else{
			EmitSignal(SignalName.LookingAtDoor, ev, door_text);
		}
	}

	public void OnDoorTriggered(){
		GetTree().ChangeSceneToFile(scene_triggered);
	}

}
