using Godot;
using System;

public partial class Door : Node3D
{

	[Signal]
    public delegate void LookingAtDoorEventHandler(InputEvent ev, string door_text);

	[Signal]
    public delegate void LeavingSceneEventHandler(string next_scene);

	[Export]
	public string scene_triggered {get; set;} = "res://Arena_shade.tscn";

	[Export]
	public string door_text{get; set;} = "Enter to proceed";

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Connect("input_event", new Callable(this, nameof(_on_input_event)));
	}

	public void InitializeDoor(DoorInfo info){
		this.scene_triggered = info.scene_triggered;
		this.door_text = info.door_text;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_input_event(Camera3D camera, InputEvent ev, Vector3 event_position, Vector3 normal, int shape_idx){
		//GD.Print("Door event");
		OnDoorEvent(ev);
	}

	public void OnDoorEvent(InputEvent ev){
		// If mouse cursor entered
		// light up door
		//If Raction
		//Change scene to boss fight
		if (ev.IsAction("INTERACT")){
			OnDoorTriggered();
		}else{
			EmitSignal(SignalName.LookingAtDoor, ev, door_text);
		}
	}

	public void OnDoorTriggered(){
		EmitSignal(SignalName.LeavingScene, scene_triggered);
		GetTree().ChangeSceneToFile(scene_triggered);
	}

	public void TeleportTo(Marker3D spot){
        if(!this.IsInsideTree()){
			//this.GlobalTransform = coordinates;
            this.Position = spot.Position;
        }else{
            this.GlobalPosition = spot.GlobalPosition;
        }
	}

	public static DoorInfo GetDoorInfo(Door door){
		return new DoorInfo(door);
	}

}
