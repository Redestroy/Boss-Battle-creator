using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// TODO Extend Boss
// 1) Customize decks
// 2) Add custom controller
// 3) Organize move decks
// 4) Add the randomization stuff

public class Action{
	int Id;
	int arg_count;
	List<string> arguments;
	Func<List<string>, int> act;

	public int do_action(){
		return act(arguments);
	} 
}

public class Observation{
	Dictionary<string, Tuple<string,string>> data;
}

//Todo change to interface
public abstract class AgentScript{
	public abstract Action get_action(Observation observation);
	public abstract Observation get_observation(World3D world);
	public abstract void DoAction(Action action);

	public abstract void OnUpdate(double delta);
}

public partial class SlimeScript: AgentScript{

	private Dictionary<string, Move> slime_moves;
	private Dictionary<string, Animation> slime_animations;
	int last_result;
	bool is_move_in_progress;
	Move current_move;
	public SlimeScript(Dictionary<string, Move> moves, Dictionary<string, Animation> animations){
		slime_moves = moves;
		slime_animations = animations;
		last_result = -1;
		is_move_in_progress = false;
		if(moves.TryGetValue("Idle", out Move move)){
			current_move = move;
		}
	}

	public override Action get_action(Observation observation){
		// if not move in progress
		// if human roll six sided die
		// 6 jump attack
		// 3 or 5 jump towards human
		// 2 or 4 jump in random dir
		// 1 Shrug   
		// else if damaged
		// recoil
		// rotate 60 degrees
		// else 
		// Flip coin 
		// if heads 
		// Idle, shrug, jiggle
		// if tails
		// random jump
		// random rotate
		return new Action();
	}
	public override Observation get_observation(World3D world){
		// Raycast in front
		// Return following dictionary within observation
		// <"is_human": <"bool", "true">>
		// <"position": <"Vector3D", "{x,y,z}">>
		// <"is_wall": <"bool", "true">>
		// <"wall_distance": <"double", "dist">>
		return new Observation();
	}
	public override void DoAction(Action action){
		
		action.do_action();
	}

	public override void OnUpdate(double delta){
		is_move_in_progress = last_result != -1;
		if(!is_move_in_progress){
			// move logic
		}else{
			// idle logic
		}
	}
}

public partial class SlimeBoss : CharacterBody3D//Boss
{

	[Export]
	private int health;
	[Export]
	private int bump_damage;
	[Export]
	private int attack_damage;

	private bool is_moving;

	private AgentScript script;
	private AnimationPlayer animationPlayer;
	Dictionary<string, Animation> animation_deck;
	Dictionary<string, Move> move_deck;

	public override void _Ready()
	{
		GD.Print("Spawned slime boss");
		//variables = _load_variables_from_file() as Dictionary;
		health = 200;
		bump_damage = 15;
		attack_damage = 30;
		is_moving = false;
		// Need to make some moves
		Move pounce = new Move();
		Move directed_jump = new Move();
		Move idle = new Move();
		// Need to make some animations
		move_deck = new Dictionary<string, Move>();
		move_deck["Pounce"] = pounce;
		move_deck["jump"] = directed_jump;
		move_deck["Idle"] = idle;
		animation_deck = new Dictionary<string, Animation>();
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animation_deck["Shrug"] = animationPlayer.GetAnimation("Shrug");
		animation_deck["Recoil"] = animationPlayer.GetAnimation("Recoil");
		animation_deck["Jiggle"] = animationPlayer.GetAnimation("Jiggle");
		//move_deck = _load_move_deck_from_file("SlimeDeck.move") as Deck<Move>;
		//animation_deck = _load_animation_deck_from_file("SlimeDeck.anim") as Deck<Move>;
		script = new SlimeScript(move_deck, animation_deck);

		
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
		OnUpdate(delta);
    }

    public void OnUpdate(double delta){
		string current_animation = animationPlayer.CurrentAnimation;
		if(!animation_deck.ContainsKey(current_animation)){
			Random rand = new Random();
			string[] animation_keys = animation_deck.Keys.ToArray<string>();
			animationPlayer.Play(animation_keys[rand.Next(0,animation_keys.Count())]);
		}
		//observation = get_observation(world_signal); //??
		//UpdateState(observation);
		//action = get_action();
		//DoAction(action);
	}

	public void Jump(List<string> arguments){
		//every argument is a key value pair in form of key: value
		//type is inferred, if needed, it can be added after underscore
		// make functions to handle parsing, but should be as simple as possible
		//Jump calculation
		//double gravity = 0.0; //Used for ballistic calculation
		Transform3D transform = Transform;
		// integrate the jump
		if(!is_moving){
			is_moving = true;
			//target_velocity.y += jump_impulse;
		}
		Transform = transform;
	}

	//public Action get_action(Observation observation){

	//}

	//public Observation Observe(){
		// Observation depends
	//}


	//public AddOverridingAgentScript(AgentScript script){}

	public void Despawn(){
		//Delete instance
	}


}
