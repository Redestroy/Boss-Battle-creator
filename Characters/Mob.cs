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
// 4) Implement the state machine           In progress
//      1) Idle state: Loop idle animation
//      2) Cutscene state: skip for now
//      3) Wander: Loop Shrug animation and add random walk
//      4) Chase: Jumps toward player
//      5) Attack: Roll and do an attack from the 4 attacks
//      6) Stance: Look Towards player and move towards mean distance((Max attack dist- min attack distance)/2 + min attack distance)
//      7) Evade: Look away from player and move forward at max speed
// 4) Add navigator perhaps?                
// 4) Maybe can use as projectile           TODO

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

// TODO
// Fix the state machine export
// Adding mob drops

public partial class Mob : Character, IEnvEnemy, IDamageable, IVitriolic{

    [Signal]
    public delegate void PlayerKilledEventHandler();

    [Signal]
    public delegate void EnemyDiedEventHandler();

    [Export]
    public Weapon Weapon{ get; set; }

    [Export]
    public Item MobItem{ get; set; }

    [Export]
    public int CollisionDamage{get; set;}

    [Export]
    public int MaxHealth{get; set;} = 10;

    [Export]
    public int InitialHealth{get; set;} = 9;

    [Export]
    public string Warcry{get; set;}

    [Export]
    public StateMachine stateMachine_{get;set;}

    public StatePlayer statePlayer;

    private Heart heart;

    public Vitriol vitriol;

    public int Health{
        get{
            return heart.Health;
        }
        set{
            heart.Health = value;
        }}
    
    //Adding a state manager - the thing that has a timer and is a node not a resource?

    public Dictionary<int, string> id_map;
    public Observer Target{get; set;}

    public int move; 
    public bool is_active = false;
    public Node3D marker_source;

    float attack_distance_max;
    float attack_distance_min;
    float sense_distance_max;
    

    public override void _Ready()
	{
        base._Ready();
        this.ArenaAlias = "Mob";
        Arena root = Character.GetRoot(this).GetNode("Arena") as Arena;
        marker_source = root as Node3D;
        TargetObserver observer = new TargetObserver(this, root);
        heart = new Heart(this, MaxHealth);
        Health = InitialHealth;
		heart.Damaged += OnDamage;
		heart.Died += OnDeath;
        vitriol = new Vitriol(this, 20);
        
        //statePlayer = new StatePlayer();
        //stateMachine_ ??= ResourceLoader.Load<StateMachine>("res://Bosses/StateMachines/state_machine_TestMob_Item.tres");
        //stateMachine_ = statePlayer.AttachStateMachine(stateMachine_, Move);
        //AddChild(statePlayer);
        
        is_active = true;
	}

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        OnPreUpdate();
        base._PhysicsProcess(delta);
        OnUpdate(delta);
    }

    public void OnPreUpdate(){
        //this.Move("Idle");
        //if(stateMachine_.HasState())stateMachine_.CurrentState.Run();
    }

    public override void OnUpdate(double delta){
        // Do not call the base function, it is already called by physics process
        base.OnUpdate(delta);
        //GD.Print("On mob update");
        if(is_active){
            //GD.Print("On active mob update");
            Target.OnUpdate();
            var observations = GetObservationDict();
            //stateMachine_.OnUpdate(observations);
        }
    }

    public Godot.Collections.Dictionary<string, Variant> GetObservationDict(){
        var obs = new Godot.Collections.Dictionary<string, Variant>(); //NIT Can simplify initialization, however it may be more readable if each obs is added separately
        // playing animation
        obs.Add("playing_animation", this.animationPlayer.IsPlaying());
        // in max attack distance
        obs.Add("in_max_attack_dist", GetDistanceToTarget() < attack_distance_max);
        // in min attack distance
        obs.Add("in_min_attack_dist", GetDistanceToTarget() < attack_distance_min);
        // Target visible
        obs.Add("target_visible", GetDistanceToTarget() < sense_distance_max);
        return obs;
    }

    public Vitriol GetVitriol(){
        return vitriol;
    }

    public float GetDistanceToTarget(){
        Vector3 position_target = GetTargetMarker().GlobalPosition;
        Vector3 position_mob = FindMarker("Self").GlobalPosition;
        float dist = position_target.DistanceTo(position_mob);
        return dist;
    }

    public override int GetCurrentMove(){
        var target = GetTargetMarker(); 
        //Return the move ID for the character
        //if(cooldown)
        {
            //return -1;
        }
        var distance = target.Position - this.Position; // convert to proper distance (pythagoras style)
        var direction = target.Position/this.Position;  // Also check if they share the same parent, or use global coords
        attack_distance_max = 5;  // Get this from Export variable or something
        attack_distance_min = 2;  // Min distance might trigger heavier attacks
        sense_distance_max = 10;  // Maximum sense distance
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
                // TODO add the arena to pick the marker
        if(node_name == "Self"){
			return this.GetNode<Marker3D>("Pivot/Character/Self");
		}else if(node_name == "Item"){
            return this.GetNode<Marker3D>("Pivot/Character/ItemMarker");
        }else if(node_name == "Weapon"){
            return this.GetNode<Marker3D>("Pivot/Character/WeaponMarker");
        }
        string marker_path_prefix = "";//"Skymove/";
		return marker_source.GetNode<Marker3D>($"{marker_path_prefix}{node_name}");
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
			this.AddAnimationsToMoveKeys(keys);
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

    public override Dictionary<string, string> LoadAliases(){ // TODO:  Refactor - modularize and generalize
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
            // This damages both (later I may change this so the one moving damages the other one, maybe)
            this.OnDamage(player_collider.collision_damage);
			player_collider.OnDamage(this.GetCollisionDamage());
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
        vitriol.OnDeath();
        DropItem();
        Despawn();
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
        //State machine handle
        OnDamage();
        if(Damage > 8 || heart.RelativeHealth < 10){ //Change magical constants OR move this to heart
            OnCritical();
            //stateMachine_.ChangeState("Evade"); //May need to call interrupt
        }
        else{
            //stateMachine_.ChangeState("Stance"); //May need to call interrupt
        }
    }

    public void OnDamage(){
        //Play OnDamage
    }

    public void OnCritical(){

    }

    public void DropItem(){
        if(MobItem!=null){
            this.RemoveChild(MobItem);
            MobItem.DropItem(FindMarker("Item"));
            marker_source.AddChild(MobItem);
        }
    }

    public override Inventory LoadInventory()
    {
        return new Inventory();
    }

}