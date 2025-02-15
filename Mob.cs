// Quick rundown of what follows
// Function 1: Dojo fight vs the slime mob
// Add the basic MOVES (PlayAnimations + Standard movement)
// Add the cooldown system to prevent premature move switching
// Add Slime stuff to SlimeMobScene
// Spawn Mob slime in Dojo
// Test collisions and damage
// Add floating health bar to the Slime mob and HUD bindings 
// Add Door events to the Dojo
//      OnEnter
//      OnFled
//      OnDefeat
//      OnVictory
// Add Coins of victory to the player information
// Add dojo linkage to the overworld (Loading dojo OnEnter/Going back to Overworld OnDefeat/OnVictory/OnFled)
// Add proper weapon to the playerGeneric
// Finished when: playerGeneric can Flee the fight, can be killed by the slime mob, and can 

// TODO
// 1) Extends Env Enemy                     Kinda done
// 2) Adds simple observer                  Kinda done
// 3) Add simple movement controller        In progress
// 4) Add navigator perhaps?                
// 4) Maybe can use as projectile           TODO

using BossBattleClash;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MobMovementExecutor : MovementExecutor{
    public MobMovementExecutor() : base(){
        shorthands.Add("LookTowardsTarget", $"Target");
    }

    // ()
    // Init - may use super
    // GetTransform3D - Can use super
    // GetVelocity - Can use super
    // OnUpdate - Can use default (probably)
    // ExecuteOrder - Maybe should override as most movement for mobs will be slightly longer and will not follow the input processing pattern of the player one

    public override void OnExecuteFailed(string order)
    {
        base.OnExecuteFailed(order);
        string[] parts = order.Split(new[] { "/" }, StringSplitOptions.None);
        switch(parts[0]){
            case "LookAt":
                //LookAtTarget
            break;
            default:

            break;
        }
    }
}

public partial class Mob : Character, IEnvEnemy, IDamageable{

    [Signal]
    public delegate void PlayerKilledEventHandler();

    [Signal]
    public delegate void EnemyDiedEventHandler();

    private Heart heart;

    [Export]
    public Weapon Weapon{ get; set; }

    [Export]
    public int CollisionDamage{get; set;}

    [Export]
    public int MaxHealth{get; set;} = 10;

    [Export]
    public int Health{
        get{
            return heart.Health;
        }
        set{
            heart.Health = value;
        }}
    [Export]
    public string Warcry{get; set;}

    [Export]
    public StateMachine_Slime stateMachine_{get;set;} 

    public Dictionary<int, string> id_map;
    public Observer Target{get; set;}

    public int move; 
    public bool is_active = false;
    

    public override void _Ready()
	{
        base._Ready();
        this.ArenaAlias = "Mob";
        Arena root = Character.GetRoot(this).GetNode("Arena") as Arena;
        TargetObserver observer = new TargetObserver(this, root);
        heart = new Heart(this, MaxHealth);
		heart.Damaged += OnDamage;
		heart.Died += OnDeath;
        is_active = true;
        stateMachine_ ??= new StateMachine_Slime();
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
        
    }

    public override void _PhysicsProcess(double delta)
    {
        OnPreUpdate();
        base._PhysicsProcess(delta);
        stateMachine_.OnUpdate();
        OnUpdate(delta);
    }

    public void OnPreUpdate(){
        //var target = Target as TargetObserver;
        stateMachine_.States[stateMachine_.ActiveState].Run();
        //if(target.WorldArena == null){
        //    GD.Print($" Character.GetRoot(this) { Character.GetRoot(this)}");
		//	target.WorldArena = Character.GetRoot(this) as Arena;
		//}
    }

    public override void OnUpdate(double delta){
        // mobs have a simple action script
        //
        base.OnUpdate(delta);
        if(is_active){
            Target.OnUpdate();
            stateMachine_.OnUpdate();
        }
    }

    public override int GetCurrentMove(){
        // TODO Rethink cooldown system
        // TODO Calculate distance and direction properly.
        var target = GetTargetMarker(); 
        //Return the move ID for the character
        //if(cooldown)
        {
            //return -1;
        }
        var distance = target.Position - this.Position; // convert to proper distance (pythagoras style)
        var direction = target.Position/this.Position;  // Also check if they share the same parent, or use global coords
        float attack_distance_max = 5;  // Get this from Export variable or something
        float attack_distance_min = 2;  // Min distance might trigger heavier attacks
        float sense_distance_max = 10;  // Maximum sense distance
        float sense_sector = 1;         // Sector offset to check if target is in vision
        //if(distance<sense_distance_max && InsideSector(direction, sense_sector))
        {
            //if(distance < attack_distance_min)
            {
            // Heavy attack group
            // or ability
            return HeavyAttack();
            }
            //if(distance < attack_distance_max)
            {
            // Light attack group
            //return LightAttack();
            }
            //if(distance )
            {
            //return MoveTowardsTarget(target); 
            }
        }
        //else
        {
            //return WanderAbout();
        }
    }

    public int LightAttack(){
        // Play animation LAction
        return 0;
    }

    public int HeavyAttack(){
        // Play animation RAction
        return 0;
    }

    public int MoveTowardsTarget(Marker3D target){
        // Look at target
        
        // Add a bit of random noise to rotation
        
        // Move forward && Jump
        return 0;
    }

    public int WanderAbout(){
        return 0;
    }



    public override Marker3D FindMarker(string node_name){
        //Have a collision area and get marker from there;
        return null;
    }

    public override void Execute(string executor, string order){
        string mob_name = "mob";
        GD.Print($"{mob_name} should execute the following action: {executor} - {order}");
    }

    public void SetObserver(Observer observer){
        this.Target = observer;
    }

    public override List<string> LoadMoveKeys(){
        string path = "user://Mob_move_keys.json";
		if (!Godot.FileAccess.FileExists(path))
		{
			var keys = new List<string>(){
				"AnimationPlayer/Idle"
			};
			// load animations from Animation player
			this.AddAnimationsToMoveKeys();
			// load shorthands from movement executor
			// Currently hardcoded for testing movement executor
			keys.Add("Movement/DeltaFWD");
			keys.Add("Movement/DeltaBWD");
			keys.Add("Movement/DeltaTurnLeft");
			keys.Add("Movement/DeltaTurnRight");
			//FileHelper.SaveStringListAsJson(keys, path);
            GD.Print("Move keys created");
            foreach(var key in keys){
                GD.Print($"\t {key}");
            }
			return keys;
		}
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		var godot_list = FileHelper.LoadJsonAsList(path);
        GD.Print("Move keys loaded");
        foreach(var key in godot_list){
            GD.Print($"\t {key}");
        }
		return FileHelper.ConvertToList(godot_list);
    }

    //TODO add load id's override

    public override Dictionary<string, string> LoadAliases(){ // TODO:  Refactor - modularise and generalize
        var godot_dict = FileHelper.LoadJsonAsDict("user://id_map.json");
		if (godot_dict == null)
		{
			// Can look into GUID to semi auto generate, but we are mostly interested in the key to action not in specific mapping names 
			// input_map[ACTION NAME IN INPUT MAP] = ALIAS FOR THE MOVE so it is human readable AND for player it is the GENERIC move name (for boss or mob it may be a script action name), NOT CHARACTER SPECIFIC;
			id_map = new Dictionary<int, string>();
			//id_map[10] = "RotateLeft";
			//id_map[1] = "RotateRight";
			//id_map[2] = "MoveForward";
			//id_map[3] = "MoveBackward";
			id_map[1] = "Interact";
			id_map[2] = "QuickAttack";
			id_map[3] = "HeavyAttack";
			id_map[4] = "AbilityTrigger";
			id_map[5] = "UltimateMove";
			// Maybe separate out player only actions and do not include them in the move deck 
			//input_map["SAVE_ARENA"] = "SaveArena";
			// Add separate no input for idle reset
            
			id_map[0] = "Idle";
			FileHelper.SaveIdsAsJson(id_map, "user://id_map.json");
		}
		else
		{
			id_map = FileHelper.ConvertToIdMap(godot_dict);
		}
		// then map each move input to a key from the move deck from file, if file not present, just use default ordering from settings

        var aliases_path = "user://Mob_move_aliases.json";
		if (Godot.FileAccess.FileExists(aliases_path))
		{
			// load in file
			var godot_dict2 = FileHelper.LoadJsonAsDict(aliases_path);
			return FileHelper.ConvertToString(godot_dict2);
		}
		else
		{
			var move_deck_aliases = new Dictionary<string, string>();
			for (int i = 0; i < id_map.Keys.Count; i++)
			{
				var value_input = id_map.ElementAt(i).Value;
				if (i < GetMoveKeys().Count)
				{
					var key_moves = GetMoveKeys()[i];
					move_deck_aliases[value_input] = key_moves;
				}
			}
			//FileHelper.SaveAliasDictionaryAsJson(move_deck_aliases, aliases_path);
            foreach(var pair in move_deck_aliases){
                GD.Print($"{pair.Key} -> {pair.Value}");
            }
			return move_deck_aliases;
		}
    }

    public override Marker3D GetSpawn(){
        return FindMarker("MobSpawn");
    }
    public override string GetScenePath(){
        return this.scene_path;
    }

    //Collision handling
    // On area interactions
    public override void OnAreaEntered(Area3D area){
        GD.Print($"Mob entered area");
    }
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area){
        GD.Print($"Mob exited area");
    }
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider){
        if(!IsOnFloor()){
            GD.Print($"Mob hit a wall");
        }
    }
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider){
        GD.Print($"Mob hit an object");
        //TODO add specific collider damage
    }
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider){
        GD.Print($"Mob hit another character");
        if(collider is Player){
            var player_collider = collider as Player;
            player_collider.OnDamage(CollisionDamage);
        }else if (collider is Boss){
            var boss_collider = collider as Boss;
            this.OnDamage(boss_collider.CollisionDamage);
        }
    }
    // On collisions with weapons
    public override void OnCollidedWithWeapon(Weapon collider){
        GD.Print($"Mob hit weapon");
        this.OnDamage(collider.Damage);
    }

    public int GetCollisionDamage(){
        return CollisionDamage;
    }

    public void OnDeath(){ 
        EmitSignal(SignalName.EnemyDied);
    }

    public void OnPlayerKilled(){
        GD.Print(this.Warcry);
        EmitSignal(SignalName.PlayerKilled);
    }

    public Marker3D GetTargetMarker(){
        if(Target is TargetObserver){
            TargetObserver targetObserver = this.Target as TargetObserver;
            return targetObserver.GetTargetMarker();
        }
        return null;
    }

    public void OnDamage(int Damage){
        //Play OnDamage
        heart.OnDamage(Damage);
        OnDamage();
    }

    public void OnDamage(){
        //Play OnDamage
    }

}