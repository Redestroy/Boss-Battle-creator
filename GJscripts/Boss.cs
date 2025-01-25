using Godot;
using System;
using System.Collections.Generic;

// TODO 
// 1) Agent script injection
// 2) Observer
// 3) Extends EnvEnemy (or Hostile) for Targeting
// 4) Handle saving the boss state

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

    public abstract void Execute(string order);

	public abstract void OnUpdate(double delta);
}

public partial class BossScript: AgentScript{

	private Dictionary<string, Move> slime_moves;
	private Dictionary<string, Animation> slime_animations;
	int last_result;
	bool is_move_in_progress;
	Move current_move;



	public BossScript(Dictionary<string, Move> moves, Dictionary<string, Animation> animations){
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

    public override void Execute(string order){
        GD.Print($"Boss Executing order {order}");
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


public partial class Boss : Character, IEnvEnemy, IDamageable{

    [Signal]
    public delegate void BossVanquishedEventHandler();

    [Signal]
    public delegate void PlayerKilledEventHandler();

    public int CollisionDamage{get; set;} = 20;
    public string Warcry{get; set;} = "Fee Fii Foo";

    private Heart heart;
    public const int MaxHealth = 2000;
    public int Health{
        get{
            return heart.Health; 
        } 
        set{
            heart.Health = value;
        }}
    
    private Arena arena;
    public Character Target{get; set;}
    private List<string> move_groups;

    public override void _Ready()
	{
        base._Ready();
        arena = GetParent<Arena>();
        heart = new Heart(this,MaxHealth);
		heart.Damaged += OnDamage;
		heart.Died += OnDeath;
    }

    public override void _Process(double delta)
	{
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
	{
        base._PhysicsProcess(delta);
    }

    public int GetHealth(){
        return Health;
    }

    public void OnDamage(int dmg){
        OnDamage();
    }

    public void OnDeath(){
        EmitSignal(SignalName.BossVanquished);
        // Play Death animation
        GD.Print("You are victorious");
    }

    public override int GetCurrentMove(){
        // TODO add the random number picker code later
        bool is_idle = true;
		if(is_idle){
			Random rand = new Random();
			return rand.Next(0, move_deck_ids.Keys.Count);
		}
        return 0;
    }

    public override Marker3D FindMarker(string node_name){
        // TODO add the arena to pick the marker
        return null;
    }
    public override void Execute(string executor, string order){
        string boss_name = "boss";
        GD.Print($"{boss_name} should execute the following action: {executor} - {order}");
        switch(executor){
            case "Script":
                // Call on agent script to do a movement
                // Allows for user defined scripts
            break;
            default:
                GD.Print($"{boss_name} should execute a not supported action: {executor} - {order}");
            break;
        }
    }

    public override List<string> LoadMoveKeys(){    // Load move keys just contains two char resource specific things
                                                    // Default move keys and file path for move keys
                                                    // Therefore the specific method LoadMoveKeys
                                                    // Could be generalized to a LoadMoveKeys(path, default_list)
                                                    // and the empty one just wraps the character method
                                                    // Similarly, Save and load methods could be moved to a separate file handler file for all saving and loading  
        string path = "user://SlimeBoss_move_keys.json";
		if (!Godot.FileAccess.FileExists(path))
    	{
			var keys = new List<string>(){
				"AnimationPlayer/Idle"
			};
			// load animations from Animation player
			this.AddAnimationsToMoveKeys();
			// load shorthands from movement executor
			// Currently hardcoded for testing movement executor
			keys.Add("Script/BigJump");
			keys.Add("Script/DeltaBWD");
			keys.Add("Script/DeltaTurnLeft");
			keys.Add("Script/DeltaTurnRight");
            // But could create a similar method to AddAnimationsToMoveKeys
            // this.AddScriptActionsToMoveKeys
			FileHelper.SaveStringListAsJson(keys, path);
			return keys;
		}
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		var godot_list = FileHelper.LoadJsonAsList(path);
		return FileHelper.ConvertToList(godot_list);
    }

    public override Dictionary<string, string> LoadAliases(){
        //Todo implement
        // Unlike player, boss moves are more advanced
        // But essentially, it comes down to the following shorthands
        //      Idle around - Idle (Not attacking player)                                           Current Idle animation
        //      Cutscene triggers
        //          Enter cutscene                                                                  Player cam pans to the slime cube rising from ground and having a roar effect
        //          Defeat cutscene                                                                 Slime extends and swallows the player, leaving a skeleton
        //          Victory cutscene                                                                Slime explodes and remains melt to the ground
        //      Movement groups
        //          Wander group            Default, OnReturnFromState                              Patrol, slow walk, stroll etc.
        //          Search group            OnPlayerLeftView                                        Random walk, last spot search, area cover, line search, lighthouse search etc.
        //          Chase group             OnPlayerInView && hostile                               Move towards player until close enough for attack
        //          Stance group            OnMissThreshold                                         If player blocks, stance may prepare for a players attack
        //          Evade group             OnCritDamage                                            If taken damage, may start fleeing from player until far enough
        //      Four standard moves                                                                 Slime moves
        //          Quick attack - Forward line attack                                              Small bump
        //          Heavy attack - Wide arc attack                                                  Bludgeon
        //          Ability attack - Bullet hell wide AOE, usually forcing the player to full dodge Splash Spin
        //          Ultimate attack - Big damage attack unique to the boss,                         Big jump on player
        //                      after which the boss enters a cooldown (for player to hit or reset)
        //      Event moves
        //          Periodic moves that may be canceled that trigger an arena action                Slime example
        //          Spawn summon        - Spawn a mob                                               Spawn slime mob
        //          Heal                - Renew HP, but become vulnerable while healing              Restore a little bit of health
        //          Arena change effect - Trigger an arena transformation   (an animation)          Grow tree roots (Damage player on collision)          
        //          Arena AOE trigger   - Trigger an AOE from the arena     (A particle effect)     Raise the swamp level
		//      
        //      Unlike for player, the move groups are more of behaviors than 
        string move_group_path = "user://boss_move_groups_list.json";
        var godot_dict = FileHelper.LoadJsonAsList(move_group_path);
		if(godot_dict == null){
			// Can look into GUID to semi auto generate, but we are mostly interested in the key to action not in specific mapping names 
			// input_map[ACTION NAME IN INPUT MAP] = ALIAS FOR THE MOVE so it is human readable AND for player it is the GENERIC move name (for boss or mob it may be a script action name), NOT CHARACTER SPECIFIC;
			move_groups = new List<string>();
			move_groups.Add("Idle");
            move_groups.Add("Cutscene");
            move_groups.Add("M_Wander");
            move_groups.Add("M_Search");
            move_groups.Add("M_Chase");
            move_groups.Add("M_Stance");
            move_groups.Add("M_Evade");
            move_groups.Add("A_Quick");
            move_groups.Add("A_Heavy");
            move_groups.Add("A_Ability");
            move_groups.Add("A_Ultimate");
            move_groups.Add("E_Spawn");
            move_groups.Add("E_Heal");
            move_groups.Add("E_Reform");
            move_groups.Add("E_Zone");
			//input_map["RIGHT"] = "RotateRight";
			//input_map["FWD"] = "MoveForward";
			//input_map["BWD"] = "MoveBackward";
			//input_map["INTERACT"] = "Interact";
			//input_map["LACTION"] = "QuickAttack";
			//input_map["RACTION"] = "HeavyAttack";
			//input_map["ABILITY"] = "AbilityTrigger";
			//input_map["ULT"] = "UltimateMove";
			// Maybe separate out player only actions and do not include them in the move deck 
			//input_map["SAVE_ARENA"] = "SaveArena";
			// Add separate no input for idle reset
			//input_map["RESET"] = "Idle";
			FileHelper.SaveStringListAsJson(move_groups, move_group_path);
		}
		else{
			move_groups = FileHelper.ConvertToList(godot_dict);
		}
		// then map each move input to a key from the move deck from file, if file not present, just use default ordering from settings
		if(Godot.FileAccess.FileExists("user://BossSlime_move_aliases.json")){
			// load in file
			var godot_dict2 = FileHelper.LoadJsonAsDict("user://BossSlime_move_aliases.json");
			return FileHelper.ConvertToString(godot_dict2);
		}else{
            // Here, need to change the logic a bit as in each group there may be 0, 1 or many keys
            // Therefore, maybe the logic would need to be a tad different
            // Maybe, currently leave this as todo and return later when I've finished some more of the underlying logic
            // Especially for movement
			var move_deck_aliases = new Dictionary<string, string>();
			for(int i = 0; i<move_groups.Count; i++){
				var value_input = move_groups[i];
				if(i<GetMoveKeys().Count){
					var key_moves = GetMoveKeys()[i];
					move_deck_aliases[value_input] = key_moves;
				}
			}
			FileHelper.SaveAliasDictionaryAsJson(move_deck_aliases, "user://BossSlime_move_aliases.json");
			return move_deck_aliases;
		}
    }

    public override Marker3D GetSpawn(){
        return FindMarker("SpawnBoss");
    }

    public override string GetScenePath(){
        return this.scene_path;
    }

    //Collision handling
    // On area interactions
    public override void OnAreaEntered(Area3D area){
        GD.Print("Boss entered Area");
    }
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area){
        GD.Print("Boss exited Area");
    }
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider){
        if(!IsOnFloor()){
            GD.Print("Boss collided with Env");
        }
    }
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider){
        GD.Print("Boss collided with object");
    }
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider){
        GD.Print("Boss collided with character"); // Handled in player
    }
    // On collisions with weapons
    public override void OnCollidedWithWeapon(Weapon collider){
        this.OnDamage(collider.Damage);
    }

    public int GetCollisionDamage(){
        return CollisionDamage;
    }

    public void OnDamage(){
        //Add damage animation here
    }

    public void OnPlayerKilled(){
        GD.Print(this.Warcry);
        EmitSignal(SignalName.PlayerKilled);
    }

    public Marker3D GetTargetMarker(){
        if(Target != null){
            return Target.FindMarker("Self");
        }
        else{
            return null;
        }
    }
}