using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;

// Refactor note:
// Move static methods to a file helper later

// TODO player functionality
// 1) Input mapping
// 2) HUD
// 3) Camera viewport
// 4) Inventory
// 5) Ext. Targetable/ish, Character
// 6) 

// NOTE minor limit - cannot create files in non existing directories.

public partial class Player : Character, IDamageable{

	[Signal]
    public delegate void PlayerKilledEventHandler();

	[Signal]
    public delegate void SaveStageEventHandler();

	[Export] 
	public bool is_active = false;

	[Export]
    private int initial_health {get;set;} = 100; 

	[Export]
    private int initial_damage {get;set;} = 20; 

	public static bool player_selected = false;

	private int current_move = 0;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private Camera3D camera;
	private PhysicsDirectSpaceState3D _space_state;
	private static Player active_player;
	private bool constrained; 

	private Weapon weapon;
	private RayCast3D raycast;
	private float rayLength = 10.0f; // Length of the ray

	private int max_health;

	private Heart heart;
	private Arena arena; //// Note: for spawning use a separate interface as overworld should work a tad differently (as well as multiplayer) 

	public Dictionary<string, string> input_map;

	public override void _Ready()
	{
		base._Ready();
		if(!player_selected){
			// Select first player as active
			GD.Print("Player selected as active");
			is_active = true;
			player_selected = true;
			active_player = this;
		}
		constrained = false;
		heart = new Heart(this, initial_health);
		heart.Damaged += OnDamage;
		heart.Died += OnDeath;
		OnDamage(initial_damage);

		var hud = GetHUD();
		var label_player_health = hud.GetNode<Label>("Health");
		label_player_health.Text = $"{heart.Health}";
		var HUD = GetHUD();
		var bar_player_health = HUD.GetNode<TextureProgressBar>("HealthBar");
		bar_player_health.SetValueNoSignal(100*heart.Health/heart.MaxHealth);

 		camera = GetNode<Camera3D>("Marker3D/Camera3D");
		weapon = GetNode<Weapon>("Pivot/WeaponPivot/Weapon");
		arena = GetParent<Arena>();
		
		//raycast = GetNode<RayCast3D>("RayCast3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		Vector3 velocity = Velocity;
		_space_state = GetWorld3D().DirectSpaceState;

		if (is_active && !constrained && heart.Alive ){  //
		
		foreach (var key in input_map.Keys){
			if (Input.IsActionPressed(key))
    		{
				play_action(input_map[key]);
    		}
		}

		if (Input.IsActionPressed("Grow"))
    	{
			//this.SetScale(3.0f);
			OnDamage(-1);
    	}

		if (Input.IsActionPressed("Shrink"))
    	{
			//this.SetScale(0.3f);
			OnDamage(1);
    	}

		if (Input.IsActionPressed("Normalize"))
    	{
			//this.SetScale(1f);
			heart.Health = 100;
			OnDamage(0);
    	}
		//raycast. .CastTo = new Vector3(0, -rayLength, 0); // Pointing downwards
        //raycast.Enabled = true;

        // Check if the ray is colliding
        //if (raycast.IsColliding())
        //{
            //var collider = (CollisionObject3D)raycast.GetCollider();
            // Optionally, check if it's terrain (if your terrain is in a specific group)
            //if (collider.HasGroup("terrain"))
            //{
            //    GD.Print("Colliding with terrain!");
            //}
        //}

		}
        OnPhysicsProcess(delta);
	}


	public Player GetActivePlayer(){
		return active_player;
	}

	public void play_action(string action_tag){ // action tag is actually move already, leaving for testing but may change later
		Move(action_tag);
		/*
		if (action_tag == "LEFT"){
			rotate(d_rot);
		}
		else if (action_tag == "RIGHT"){
			rotate(-d_rot);
		}
		else if (action_tag == "FWD"){
			move(d_mov);
		}
		else if (action_tag == "BWD"){
			move(-d_mov);
		}
		else if (action_tag == "RACTION"){
			//raycast to get player
			string player_tag = get_player_tag_from_cast();
			//call change player
			if (player_tag != "MISS"){
				change_player(player_tag);
			}else{
				GD.Print("No player: ");
			}
		}
		else if (action_tag == "SaveArena"){
			SaveArena();
		}
		*/
		GD.Print(action_tag);
        OnPlayAction(action_tag);
	}

//	public void rotate(float delta){
//		Transform3D transform = Transform;
//		Vector3 axis = new Vector3(0, 1, 0);
//		transform.Basis = transform.Basis.Rotated(axis, delta);
//		Transform = transform;
//	}

//	public void move(float delta){
//		Transform3D transform = Transform;
//		transform = transform.TranslatedLocal(new Vector3(0,0,delta));
//		Transform = transform;
//	}

	public string get_player_tag_from_cast(){
		return "MISS";
	}

	public void change_player(string player_tag){
		//Get node from player tag or pass it as arg, then set it active
		//GetTree().UniqueName(player_tag).set_active;
		is_active = false;
	}

	public void _set_active(){
		active_player.change_player("PLACEHOLDER");
		active_player = this;
		camera.MakeCurrent();
		is_active = true;
	}

	public virtual void _on_input_event(){
        GD.Print("Clicked on player");
    }

    public virtual void OnPlayAction(string tag){
		// Send the action to characters move deck
    }

     public virtual void OnPhysicsProcess(double delta){

        base._PhysicsProcess(delta);
    }

	public void StoreState(){
		//load health, weapon and modifiers to file/singleton to store info between scenes
	}

	public void RestoreState(){
		// Read info (for variables) from file
	}

	public void SaveArena(){
		EmitSignal(SignalName.SaveStage);
		//load health, weapon and modifiers to file/singleton to store info between scenes
	}

	public void Constrain(){
		constrained = true;
	}

	public void Release(){
		constrained = false;
	}
	
    public int GetHealth(){
        return heart.Health;
    }

	public void OnDamage(int dmg){
		GD.Print("Ouch! That hurt");
        UpdateLifeBar();
    }

	public void UpdateLifeBar(){ //NIT Maybe change getting a reference each time (or not)
		var HUD = GetHUD();
		var bar_player_health = HUD.GetNode<TextureProgressBar>("HealthBar");
		var label_player_health = HUD.GetNode<Label>("Health");
		label_player_health.Text = $"{heart.Health}";
		bar_player_health.SetValueNoSignal(100*heart.Health/heart.MaxHealth);
	}

	public void OnFled(){
		GD.Print("Player ran like a chicken");
		OnDeath();
	}

    public void OnDeath(){
		// Play Death animation
		EmitSignal(SignalName.PlayerKilled);
    }

	public override int GetCurrentMove(){
		return current_move; //current move is set on input
	}
    public override Marker3D FindMarker(string node_name){
		//Find child in arena object
		string marker_path_prefix = "";
		return arena.GetNode<Marker3D>($"{marker_path_prefix}/{node_name}");
	}
    public override void Execute(string executor, string order){
		GD.Print($"Player should execute the following action: {executor} - {order}");
	}

	public override List<string> LoadMoveKeys(){
		string path = "user://PlayerGeneric_move_keys.json";
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
			FileHelper.SaveStringListAsJson(keys, path);
			return keys;
		}
		using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		var godot_list = FileHelper.LoadJsonAsList(path);
		return FileHelper.ConvertToList(godot_list);// Error! We don't have a save to load.
		//var godot_dict2 = LoadJsonAsDict("user://PlayerGeneric/move_aliases.json");
		//return ConvertToString(godot_dict2);// Error! We don't have a save to load.
	}

	public override Dictionary<string, string> LoadAliases(){
		// Load file "PlayerInputMap" from player settings
		// load file as json
		//JSON load
		// create dict from json for input map
		var godot_dict = FileHelper.LoadJsonAsDict("user://input_map.json");
		if(godot_dict == null){
			// Can look into GUID to semi auto generate, but we are mostly interested in the key to action not in specific mapping names 
			// input_map[ACTION NAME IN INPUT MAP] = ALIAS FOR THE MOVE so it is human readable AND for player it is the GENERIC move name (for boss or mob it may be a script action name), NOT CHARACTER SPECIFIC;
			input_map = new Dictionary<string, string>();
			input_map["LEFT"] = "RotateLeft";
			input_map["RIGHT"] = "RotateRight";
			input_map["FWD"] = "MoveForward";
			input_map["BWD"] = "MoveBackward";
			input_map["INTERACT"] = "Interact";
			input_map["LACTION"] = "QuickAttack";
			input_map["RACTION"] = "HeavyAttack";
			input_map["ABILITY"] = "AbilityTrigger";
			input_map["ULT"] = "UltimateMove";
			// Maybe separate out player only actions and do not include them in the move deck 
			//input_map["SAVE_ARENA"] = "SaveArena";
			// Add separate no input for idle reset
			input_map["RESET"] = "Idle";
			FileHelper.SaveAliasDictionaryAsJson(input_map, "user://input_map.json");
		}
		else{
			input_map = FileHelper.ConvertToString(godot_dict);
		}
		// then map each move input to a key from the move deck from file, if file not present, just use default ordering from settings
		if(Godot.FileAccess.FileExists("user://PlayerGeneric_move_aliases.json")){
			// load in file
			var godot_dict2 = FileHelper.LoadJsonAsDict("user://PlayerGeneric_move_aliases.json");
			return FileHelper.ConvertToString(godot_dict2);
		}else{
			var move_deck_aliases = new Dictionary<string, string>();
			for(int i = 0; i<input_map.Keys.Count; i++){
				var value_input = input_map.ElementAt(i).Value;
				if(i<GetMoveKeys().Count){
					var key_moves = GetMoveKeys()[i];
					move_deck_aliases[value_input] = key_moves;
				}
			}
			FileHelper.SaveAliasDictionaryAsJson(move_deck_aliases, "user://PlayerGeneric_move_aliases.json");
			return move_deck_aliases;
		}
	}

    public override Marker3D GetSpawn(){
		return arena.GetNode<Marker3D>("SpawnPlayer");
		//return FindMarker("SpawnPlayer");
	}

    public override string GetScenePath(){
		return this.scene_path;
	}

    //Collision handling
    // On area interactions
    public override void OnAreaEntered(Area3D area){
		GD.Print("Entered area");
	}
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area){
		GD.Print("Exited area");
	}
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider){
		if(!IsOnFloor()){
			GD.Print("Hit environment wall");
		}
	}
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider){
		GD.Print("Hit object");
	}
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider){
		GD.Print("Ran into another character");
		if(collider is EnvEnemy){
			GD.Print("ITs ENEMY!!!");
			var enemy = collider as EnvEnemy;
			this.OnDamage(enemy.GetCollisionDamage());
		}
	}
    // On collisions with weapons
    public override void OnCollidedWithWeapon(Weapon collider){
		GD.Print("Hit a weapon");
		this.OnDamage(collider.Damage);
	}

	public Control GetHUD(){
		//Problem when using spawning for players
		return GetNode<Control>("HUD");
	}
}