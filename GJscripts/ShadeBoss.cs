using Godot;
using System;
using System.Collections.Generic;

public partial class ShadeBoss : Boss
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	    private AnimationPlayer animationPlayer;
	Dictionary<string, Animation> animation_deck;
    Dictionary<string, string> event_alliases;
	Dictionary<int, string> event_ids;

    public override void _Ready(){
        base._Ready();

        //animation_deck = new Dictionary<string, Animation>();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		//animation_deck["LAction"] = animationPlayer.GetAnimation();
		//animation_deck["RAction"] = animationPlayer.GetAnimation("RActionSpin");
		//animation_deck["Ability"] = animationPlayer.GetAnimation("AbilityExpand");
        //animation_deck["Ultimate"] = animationPlayer.GetAnimation("AbilityExpand");
        event_alliases = new Dictionary<string, string>();
        event_alliases["LACTION"] = "LActionSideSwipe";
        event_alliases["RACTION"] = "RActionSmashingStrike";
        event_alliases["ABILITY"] = "AbilitySpinAttack";
        event_alliases["ULT"] = "HeavenlySword";

		event_ids = new Dictionary<int, string>();
        event_ids[0] = "LACTION";
        event_ids[1] = "RACTION";
        event_ids[2] = "ABILITY";
        event_ids[3] = "ULT";

        //Forbidden area collider
        //area = GetNode<Area3D>("Area3D");
        //area.BodyExited += _on_area_exited;
    }

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		OnUpdate(delta);
	}

	public void OnUpdate(double delta){

		// Random generation logic
		// 0 to 2 normal moves are random
		// After count of n moves, an ultimate is triggered
		// After Ult, play Idle animation 3 times
		string current_animation = animationPlayer.CurrentAnimation;
		if(!event_alliases.ContainsValue(current_animation)){
			Random rand = new Random();
			animationPlayer.Play(event_alliases[event_ids[rand.Next(0,event_ids.Keys.Count)]]);
		}
		//observation = get_observation(world_signal); //??
		//UpdateState(observation);
		//action = get_action();
		//DoAction(action);
	}
}
