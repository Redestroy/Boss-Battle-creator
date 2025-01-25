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
	public string enemy_scene_path {get; set;} = "res://NonPlayerGeneric.tscn";

	[Export]
	public string dojo_scene_path {get; set;} = "res://PlayerGeneric.tscn";

	private Player player;
	//private Enemy enemy;
	private Dictionary<string, PackedScene> scene_deck;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		// Load scenes
		base._Ready();
		var player_scene = GD.Load<PackedScene>(player_scene_path);
		//var enemy_scene = GD.Load<PackedScene>(enemy_scene_path);
		//var dojo_scene = GD.Load<PackedScene>(dojo_scene_path);
		//scene_deck = new Dictionary<string, PackedScene>(); 
		//scene_deck.Add("Player", player_scene);
		//scene_deck.Add("Enemy", enemy_scene);
		//scene_deck.Add("Dojo", dojo_scene);
		Marker3D spawn_point_dojo = GetNodeOrNull<Marker3D>("Skymove/SpawnPlayer");
		if(spawn_point_dojo == null) spawn_point_dojo = new Marker3D();
		//spawn dojo scene
		//var dojo_instance = player_scene.Instantiate();
		//dojo = dojo_instance.FindChild("Dojo") as Dojo;
		//AddChild(dojo_instance);
		Marker3D spawn_point_boss = GetNode<Marker3D>("Skymove/SpawnEnemy"); // In dojo, this may be a different class
		// spawn enemy scene
		// var enemy_instance = enemy_scene.Instantiate();
		// enemy = enemy_instance.FindChild("Enemy") as Enemy;
		// AddChild(player_instance);
		Marker3D spawn_point_player = GetNode<Marker3D>("Skymove/SpawnPlayer");
		//Character.Spawn(this, spawn_point_player, player_scene_path); // Maybe add an override for a packaged scene?
		player = Character.Spawn(this, spawn_point_player, player_scene) as Player;
		GD.Print(player);
		player._set_active();
		player.TeleportTo(spawn_point_boss);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	//public void LoadPlayerInstance(string scene_name){
	//	var player_instance = scene_deck[scene_name].Instantiate();
	//	player = player_instance.FindChild("Player") as Player;
	//	AddChild(player_instance);
	//}

	//public void LoadEnemyInstance(string scene_name){
//
//	}

	public void LoadDojo(string scene_name){

	}

}
