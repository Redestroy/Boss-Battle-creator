using Godot;
using System;
using System.Collections.Generic;


// TODOs
// Spawn an instance of player scene
// Spawn an instance of enemy scene
// Load in the dojo map with textures
// Add events for:
// 		Chicken out
// 		Victory
//		Defeat
//		Help
public partial class Dojo : Arena
{

	[Export]
	public string player_scene_path {get; set;} = "res://PlayerGeneric.tscn";

	[Export]
	public string enemy_scene_path {get; set;} = "res://SlimeMob.tscn";

	[Export]
	public string dojo_scene_path {get; set;} = "res://PlayerGeneric.tscn";

	private IEnvEnemy enemy;
	private Vitriol vitriol;
	private Godot.Collections.Dictionary<string, PackedScene> scene_deck;

	private Godot.Area3D boundary;

	bool defeated = false;
	

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		// Load scenes
		base._Ready();
		var player_scene = GD.Load<PackedScene>(player_scene_path);
		var enemy_scene = GD.Load<PackedScene>(enemy_scene_path);
		//var dojo_scene = GD.Load<PackedScene>(dojo_scene_path);
		scene_deck = new Godot.Collections.Dictionary<string, PackedScene>(); 
		scene_deck.Add("Player", player_scene);
		scene_deck.Add("Enemy", enemy_scene);
		//scene_deck.Add("Dojo", dojo_scene);
		Marker3D spawn_point_dojo = GetNodeOrNull<Marker3D>("Skymove/SpawnPlayer");
		if(spawn_point_dojo == null) spawn_point_dojo = new Marker3D();
		//spawn dojo scene
		//var dojo_instance = player_scene.Instantiate();
		//dojo = dojo_instance.FindChild("Dojo") as Dojo;
		//AddChild(dojo_instance);
		LoadDojo(dojo_scene_path);
		// In dojo, this may be a different class
		// spawn enemy scene
		LoadEnemyInstance();
		LoadPlayerInstance();
		PostReady();
		GD.Print($"Transform arena {GlobalTransform}");
		 // We ready the arena after loading. If it causes problems, we cen separate out arena post-init method
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		if(defeated){
			OnDefeat(); // Called here because otherwise it causes an error if called during a signal
		}
	}

	public void LoadDojo(string scene_name){
		// Load in an autoloader, dojo proto, or arena_info resource
		//		Autoloader - node with the necessary resource files, easier for editing
		//		Dojo proto - A full scene with all necessary environment stuff, good for godot editing
		//		Arena info - just text, used for generating arena from code alone
		// Each one unwraps in it's own way
		// Create the base arena object or load in a packed scene ( different for each, results in an arena object with some of the stuff)
		// Load in Skybox (Environment) (skip for proto)
		// Load in Arena base (Gridmap or static body or Terrain mesh) (skip for proto)
		// Load in Markers as a list (skip for proto and autoloader)
		door_deck = new Godot.Collections.Dictionary<string,Door>();
		Marker3D m_keystone_player = GetNode<Marker3D>("Skymove/ReturnKeystone");
		Marker3D m_keystone_enemy = GetNode<Marker3D>("Skymove/EnemyKeystone");
		Marker3D m_keystone_victory = GetNode<Marker3D>("Skymove/VictoryKeystone");
		// Load in Doors (And keystones)
		var keystone_scene = GD.Load<PackedScene>("res://keystone.tscn"); //all magic strings are contained in the arena info or should be skipped by proto
		var keystone_player = keystone_scene.Instantiate() as Door;
		var keystone_enemy = keystone_scene.Instantiate() as Door;
		var keystone_victory = keystone_scene.Instantiate() as Door;
		keystone_player.scene_triggered = "res://Labyrinth.tscn";			// All of these return to the previous scene
		keystone_enemy.scene_triggered = "res://Labyrinth.tscn";
		keystone_victory.scene_triggered = "res://Labyrinth.tscn"; 			// On victory, return to previous scene
		keystone_player.door_text = "Do you wish to flee?";
		keystone_enemy.door_text = "Death awaits, but whom...";
		keystone_victory.door_text = "Return to overworld?";
		keystone_player.TeleportTo(m_keystone_player);
		keystone_enemy.TeleportTo(m_keystone_enemy);
		keystone_victory.TeleportTo(m_keystone_victory);
		GetNode("Skymove").AddChild(keystone_player);
		GetNode("Skymove").AddChild(keystone_enemy);
		GetNode("Skymove").AddChild(keystone_victory);
		var door_node_player = keystone_player.GetNode<StaticBody3D>("Door");
		var door_node_enemy = keystone_enemy.GetNode<StaticBody3D>("Door");
		var door_node_victory = keystone_victory.GetNode<StaticBody3D>("Door");
		door_node_player.Connect("input_event", new Callable(keystone_player, nameof(keystone_player._on_input_event)));
		door_node_enemy.Connect("input_event", new Callable(keystone_enemy, nameof(keystone_enemy._on_input_event)));
		door_node_victory.Connect("input_event", new Callable(keystone_victory, nameof(keystone_victory._on_input_event)));
		door_deck.Add("keystone_player", keystone_player);
		door_deck.Add("keystone_enemy", keystone_enemy);
		door_deck.Add("keystone_victory", keystone_victory);
		boundary = GetNode<Godot.Area3D>("Boundary");
		GD.Print($"Outside bounds {boundary}");
		if(boundary != null){
			boundary.BodyEntered += _on_boundary_body_entered;
			boundary.BodyExited += _on_boundary_body_exited;
		}
		// Start spawning collision objects
		// Start spawning hazards
		// Spawn viewports
		// Spawn visuals 
	}

	public virtual void _on_boundary_body_entered(Node3D body){
		GD.Print(
			$"{body} entered area"
		);
	}

	public virtual void _on_boundary_body_exited(Node3D body){
		GD.Print(
			"Area exited"
		);
		if(this.active){
		if(body is Character){
			if(body is Player){
				GD.Print("Player fell");
				defeated = true;
				return;
			}
			else if(body is IVitriolic ){ //Maybe need to find a better comparison
				GD.Print("Enemy fell");
				OnVictory();
			}
		}
		}
	}

	public void LoadEnemyInstance(string scene_key="Enemy"){
		Marker3D spawn_point_enemy = GetNode<Marker3D>("Skymove/SpawnEnemy");
		var character_node = Character.Spawn(this, spawn_point_enemy, scene_deck[scene_key]);
		enemy = character_node as IEnvEnemy;
		ExtractInfo(enemy);
		vitriol = ((character_node as Node)as IVitriolic).GetVitriol();
		vitriol.EnemyDied += OnVictory;
		GD.Print(enemy);

	}

	public void LoadPlayerInstance(string scene_key="Player"){
		Marker3D spawn_point_player = GetNode<Marker3D>("Skymove/SpawnPlayer"); //Change to arena.GetSpawnMarker
		player = Character.Spawn(this, spawn_point_player, scene_deck[scene_key]) as Player;
		ExtractInfo(player);
		GD.Print(player);
		player._set_active();
		player.PlayerKilled += OnDefeat;
		foreach(var door in door_deck){
			door.Value.Connect("LookingAtDoor", new Callable(player, nameof(player._on_door_looking_at_door)));
		}
	}

	public override IVitriolic GetEnemy(){
		return enemy as IVitriolic;
	}

}
