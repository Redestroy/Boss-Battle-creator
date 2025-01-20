

using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class Shade : Player
{

    Area3D area;
    private AnimationPlayer animationPlayer;
	Dictionary<string, Animation> animation_deck;
    Dictionary<string, string> event_alliases;

    public override void _Ready(){
        base._Ready();

        //animation_deck = new Dictionary<string, Animation>();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		//animation_deck["LAction"] = animationPlayer.GetAnimation();
		//animation_deck["RAction"] = animationPlayer.GetAnimation("RActionSpin");
		//animation_deck["Ability"] = animationPlayer.GetAnimation("AbilityExpand");
        //animation_deck["Ultimate"] = animationPlayer.GetAnimation("AbilityExpand");
        event_alliases = new Dictionary<string, string>();
        event_alliases["LACTION"] = "LActionExtend";
        event_alliases["RACTION"] = "RActionSpin";
        event_alliases["ABILITY"] = "AbilityExpand";
        event_alliases["ULT"] = "Bloodcleave";

        //Forbidden area collider
        //area = GetNode<Area3D>("Area3D");
        //area.BodyExited += _on_area_exited;
    }

    public void _on_input_event(Camera3D camera, InputEvent ev, Vector3 event_position, Vector3 normal, int shape_idx){
        GD.Print("Clicked on shade");
        if (ev.IsAction("RACTION")){
        	_set_active();
		}
		GD.Print(this.GetActivePlayer().ToString());
    }

    public void _on_area_entered(Node area){
        GD.Print("Entered Area");
    }


    public void _on_area_exited(Node area){
        OnFled();
    }

    public override void OnPhysicsProcess(double delta)
    {
        base.OnPhysicsProcess(delta);
    }

	public override void OnPlayAction(string action_tag){
		GD.Print("Shade action: " + action_tag);
        // make a dictionary for input map, string - string, action tag - animation key
        if(event_alliases.TryGetValue(action_tag, out String animation)){
            animationPlayer.Play(animation);
        }
	}

    public void _on_door_looking_at_door(InputEvent ev, string door_text){
        OnLookingAtDoor(ev, door_text);
    }

    public void OnLookingAtDoor(InputEvent ev, string door_text){
		GetHUD().GetNode<Label>("Help").Text = door_text;
	}

    public override void _ExitTree()
    {
        // Disconnect the signal when the node is about to exit the scene tree to avoid memory leaks
        //this.BodyEntered -= OnBodyEntered;
        base._ExitTree();
    }
}
