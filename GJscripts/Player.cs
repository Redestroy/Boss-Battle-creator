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
// 2) HUD In Progress 
// 3) Camera viewport
// 4) Inventory In progress // ISSUE: Problems on loading inventory for mob, player
// 5) Ext. Targetable/ish, Character
// 6) 

// NOTE minor limit - cannot create files in non existing directories.

public partial class Player : Character, IDamageable
{

	[Signal]
	public delegate void PlayerKilledEventHandler();

	[Signal]
	public delegate void SaveStageEventHandler();

	[Export]
	public bool is_active = false;

	[Export]
	private int initial_health { get; set; } = 100;

	[Export]
	private int initial_damage { get; set; } = 20;

	[Export]
	public int collision_damage { get; set; } = 5;

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

	public Heart heart; // add methods to privatize variable
	private Arena arena; //// Note: for spawning use a separate interface as overworld should work a tad differently (as well as multiplayer) 
	private HUD hud;

	public Dictionary<string, string> input_map;

	public override void _Ready()
	{
		base._Ready();
		if (!player_selected)
		{
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
		if(collision_damage < 5) collision_damage = 5;
		hud = GetHUD();
		//if(hud!= null){
			hud.SigEquipItem += SlotItem;
			hud.SigDeEquipItem += DeSlotItem;
			hud.Connect("SigEquipMove", new Callable(this, nameof(SlotMove)));
			hud.Connect("SigDeEquipMove", new Callable(this, nameof(DeSlotMove)));
			//hud.SigEquipMove += SlotMove;
			//hud.SigDeEquipMove += DeSlotMove;
		//}
		hud.move_deck_display.AttachPlayer(this);
		//this.SigSlotItem += hud.move_deck_display._on_slot_item;
		//this.SigDeSlotItem += hud.move_deck_display._on_deslot_item;
		OnDamage(initial_damage);
		hud.UpdateLifeBar();

		camera = GetNode<Camera3D>("Marker3D/Camera3D");
		weapon = GetNode<Weapon>("Pivot/WeaponPivot/Weapon");
		this.ArenaAlias = "Player"; //export
		arena = Character.GetRoot(this).GetNodeOrNull("Arena") as Arena;
		GD.Print($"Arena {arena}"); // Add a case for overworld
		if(arena != null){
			arena.SigVictory += HUD_VictoryUpdate;
			arena.SigDefeat += HUD_DefeatUpdate;
		}
		if(CharacterInformation.previous_scene != ""){
			OnSceneEntered();
		}
		//arena = GetParent<Node3D>();//GetParent<Arena>();
		//TeleportTo(Spawn);
		//raycast = GetNode<RayCast3D>("RayCast3D");
		hud.Refresh();
	}

	public override void _PhysicsProcess(double delta)
	{
		if(arena == null){
			//arena = Character.GetRoot(this) as Arena;
		}
		base._PhysicsProcess(delta);
		Vector3 velocity = Velocity;
		_space_state = GetWorld3D().DirectSpaceState;

		if (is_active && !constrained && heart.Alive)
		{  //

			foreach (var key in input_map.Keys)
			{
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

	public void _on_door_looking_at_door(InputEvent ev, string door_text){
        OnLookingAtDoor(ev, door_text);
    }

    public void OnLookingAtDoor(InputEvent ev, string door_text){
		hud.UpdateHelpText(door_text);
	}

	public void OnSceneEntered(){
		RestoreState();
	}


	public Player GetActivePlayer()
	{
		return active_player;
	}

	public void play_action(string action_tag)
	{ // action tag is actually move already, leaving for testing but may change later
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
		//GD.Print(action_tag);
		OnPlayAction(action_tag);
	}

	public string get_player_tag_from_cast()
	{
		return "MISS";
	}

	public void change_player(string player_tag)
	{
		//Get node from player tag or pass it as arg, then set it active
		//GetTree().UniqueName(player_tag).set_active;
		is_active = false;
	}

	public void _set_active()
	{
		active_player.change_player("PLACEHOLDER");
		active_player = this;
		//this.GetHUD() // TODO reInit HUD on new player
		camera.MakeCurrent();
		is_active = true;
	}

	public virtual void _on_input_event()
	{
		GD.Print("Clicked on player");
	}

	public virtual void _on_leaving_scene(string next_scene){
		//save inventory
		CharacterInformation.previous_scene = arena.Name;
		StoreState();
	}

	public virtual void OnPlayAction(string tag)
	{
		// Send the action to characters move deck
	}

	public virtual void OnPhysicsProcess(double delta)
	{

		base._PhysicsProcess(delta);
	}

	public void StoreState()
	{
		GD.Print($"Save state{Name}");
		var global_inv = (GlobalInventory) GetNode("/root/GlobalInventory");
		global_inv.SaveInventory(this.Name, inventory);
		//load health, weapon and modifiers to file/singleton to store info between scenes
	}

	public void RestoreState()
	{
		// Read info (for variables) from file
		GD.Print($"Restore state{Name}");
		var global_inv = (GlobalInventory) GetNode("/root/GlobalInventory");
		inventory = global_inv.LoadInventory(this.Name);
		inventory.PrintContents();
		ReinstanceEquipment();
		//hud.Refresh();
	}

	public void SaveArena()
	{
		EmitSignal(SignalName.SaveStage);
		//load health, weapon and modifiers to file/singleton to store info between scenes
	}

	public void Constrain()
	{
		constrained = true;
	}

	public void Release()
	{
		constrained = false;
	}

	public int GetHealth()
	{
		return heart.Health;
	}

	public void OnDamage(int dmg)
	{
		heart.OnDamage(dmg);
		GD.Print("Ouch! That hurt");
		UpdateLifeBar();
	}

	public void UpdateLifeBar()
	{
		hud.UpdateLifeBar();
	}

	public void OnFled()
	{
		GD.Print("Player ran like a chicken");
		OnDeath();
	}

	public void OnDeath()
	{
		// Play Death animation
		EmitSignal(SignalName.PlayerKilled);
	}

	public override int GetCurrentMove()
	{
		return current_move; //current move is set on input
	}
	public override Marker3D FindMarker(string node_name)
	{
		//Find child in arena object
		if(node_name == "Self"){
			return this.GetNode<Marker3D>("Pivot/Character/Self"); //TODO: Change to finding a uniquely named node
		}else if(node_name == "Item"){
            return this.GetNode<Marker3D>("Pivot/Character/ItemMarker");
        }else if(node_name == "Weapon"){
            return this.GetNode<Marker3D>("Pivot/WeaponPivot/WeaponMarker");
        }
		string marker_path_prefix = "";//"Skymove/";
		return arena.GetNode<Marker3D>($"{marker_path_prefix}{node_name}");
	}
	public override void Execute(string executor, string order)
	{
		GD.Print($"Player should execute the following action: {executor} - {order}");
	}

	public override List<string> LoadMoveKeys()
	{
		string path = "user://PlayerGeneric_move_keys.json";
		if (!Godot.FileAccess.FileExists(path))
		{
			var keys = new List<string>(){
				"Idle"
			};
			// load animations from Animation player
			this.AddAnimationsToMoveKeys(keys);
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

	public override Dictionary<string, string> LoadAliases()
	{
		// Load file "PlayerInputMap" from player settings
		// load file as json
		//JSON load
		// create dict from json for input map
		var godot_dict = FileHelper.LoadJsonAsDict("user://input_map.json");
		if (godot_dict == null)
		{
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
		else
		{
			input_map = FileHelper.ConvertToString(godot_dict);
		}
		// then map each move input to a key from the move deck from file, if file not present, just use default ordering from settings
		if (Godot.FileAccess.FileExists("user://PlayerGeneric_move_aliases.json"))
		{
			// load in file
			var godot_dict2 = FileHelper.LoadJsonAsDict("user://PlayerGeneric_move_aliases.json");
			return FileHelper.ConvertToString(godot_dict2);
		}
		else
		{
			var move_deck_aliases = new Dictionary<string, string>();
			for (int i = 0; i < input_map.Keys.Count; i++)
			{
				var value_input = input_map.ElementAt(i).Value;
				if (i < GetMoveKeys().Count)
				{
					var key_moves = GetMoveKeys()[i];
					move_deck_aliases[value_input] = key_moves;
				}
			}
			FileHelper.SaveAliasDictionaryAsJson(move_deck_aliases, "user://PlayerGeneric_move_aliases.json");
			return move_deck_aliases;
		}
	}

	public override Marker3D GetSpawn()
	{
		return arena.GetNode<Marker3D>("SpawnPlayer");
		//return FindMarker("SpawnPlayer");
	}

	public override string GetScenePath()
	{
		return this.scene_path;
	}

	//Collision handling
	public override void OnAreaEntered(Area3D area)
	{
		GD.Print("Entered area");
	}
	//public abstract void OnTerrainAreaEntered(Terrain area);
	//public abstract void OnTerrainAreaExited(Terrain area);
	//public abstract void OnHazardAreaEntered(Hazard area); //Hazards
	//public abstract void OnHazardAreaExited(Hazard area);
	public override void OnAreaExited(Area3D area)
	{
		GD.Print("Exited area");
	}
	public override void OnCollidedWithEnvironment(StaticBody3D collider)
	{
		if (!IsOnFloor())
		{
			GD.Print("Hit environment wall");
		}
	}
	public override void OnCollidedWithObject(RigidBody3D collider)
	{
		GD.Print("Hit object");
	}
	public override void OnCollidedWithCharacter(CharacterBody3D collider)
	{
		GD.Print("Ran into another character");
		if (collider is IEnvEnemy)
		{
			GD.Print("ITs ENEMY!!!");
			var enemy = collider as IEnvEnemy;
			this.OnDamage(enemy.GetCollisionDamage());
			enemy.OnDamage(this.collision_damage);
		}
	}
	public override void OnCollidedWithWeapon(Weapon collider)
	{
		GD.Print($"Collider layer {collider.CollisionLayer}");
		if(weapon.IsNodeReady() && weapon.IsEquipped){
			GD.Print("Hit a weapon");
			this.OnDamage(collider.Damage);
		}
	}

	public void AddItemInInventory(string node_path, string type){
		// Currently, just have a list in the hud that just has all the strings
		// Later attach a node to player
		// 1st) Load PackedScene
		GD.Print($"Type {type}");
		var packedScene = LoadItemScene(node_path);
		// 2nd) Add PackedScene item name to the item list
		string display_name = packedScene.GetState().GetNodeName(0);
		inventory.OnPickup(display_name, node_path, type);
		if(type == "Move"){
			int index = GetHUD().AddToMoveList(display_name);
			move_inventory_ref_.Add(index, new Tuple<string, PackedScene>(display_name, packedScene));
			GetHUD().move_inventory_ref_ui.Add(index);
		}
		else{
			int index = GetHUD().AddToItemList(display_name);
			inventory_ref_.Add(index, new Tuple<string, PackedScene>(display_name, packedScene));
			GetHUD().inventory_ref_ui.Add(index);
		}
	}

	public HUD GetHUD()
	{
		//Problem when using spawning for players
		return GetNode<HUD>("HUD");
	}

	// HUD methods
	public void HUD_VictoryUpdate(){ // Move to HUD scripts
		GetHUD().UpdateHelpText("You win!");
	}

	public void HUD_DefeatUpdate(){ // Move to HUD scripts
		GetHUD().UpdateHelpText("You lose!");
	}
}