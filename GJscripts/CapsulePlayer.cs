using Godot;
using System;

public partial class CapsulePlayer : Player
{
	public void _on_input_event(Camera3D camera, InputEvent ev, Vector3 event_position, Vector3 normal, int shape_idx){
		base._on_input_event();
        GD.Print("Clicked on capsule");
		if (ev.IsAction("RACTION")){
        	_set_active();
		}
		GD.Print(this.GetActivePlayer().ToString());
    }

	public override void OnPhysicsProcess(double delta)
    {
        base.OnPhysicsProcess(delta);
    }

	public override void OnPlayAction(string action_tag){
		GD.Print("Capsule action: " + action_tag);
	}
}
