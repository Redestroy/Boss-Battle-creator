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
	public string dojo_scene {get; set;} = "res://PlayerGeneric.tscn";

	private Player player;
	//private Enemy enemy;
	private Dictionary<string, PackedScene> scene_deck;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		// Load scenes
		var player_scene = GD.Load<PackedScene>(player_scene_path);
		var enemy_scene = GD.Load<PackedScene>(player_scene_path);
		var dojo_scene = GD.Load<PackedScene>(player_scene_path);
		scene_deck = new Dictionary<string, PackedScene>(); 
		scene_deck.Add("Player", player_scene);
		scene_deck.Add("Enemy", enemy_scene);
		scene_deck.Add("Dojo", dojo_scene);
		//spawn dojo scene
		//var dojo_instance = player_scene.Instantiate();
		//dojo = dojo_instance.FindChild("Dojo") as Dojo;
		//AddChild(dojo_instance);
		//spawn player scene
		var player_instance = player_scene.Instantiate();
		player = player_instance as Player;
		GD.Print(player);
		AddChild(player_instance);
		// spawn enemy scene
		// var enemy_instance = enemy_scene.Instantiate();
		// enemy = enemy_instance.FindChild("Enemy") as Enemy;
		// AddChild(player_instance);

		player._set_active();
		Marker3D spawn_point = GetNode<Marker3D>("Skymove/SpawnPlayer");
		player.TeleportTo(spawn_point);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void LoadPlayerInstance(string scene_name){
		var player_instance = scene_deck[scene_name].Instantiate();
		player = player_instance.FindChild("Player") as Player;
		AddChild(player_instance);
	}

	public void LoadEnemyInstance(string scene_name){

	}

	public void LoadDojo(string scene_name){

	}

}
