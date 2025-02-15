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

	private Player player;
	private IEnvEnemy enemy; // later change to enemy or IEnvEnemy
	private Godot.Collections.Dictionary<string, PackedScene> scene_deck;

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
		 // In dojo, this may be a different class
		// spawn enemy scene
		LoadEnemyInstance();
		LoadPlayerInstance();
		GD.Print($"Transform arena {GlobalTransform}");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	public void LoadDojo(string scene_name){

	}

	public void LoadEnemyInstance(string scene_key="Enemy"){
		Marker3D spawn_point_enemy = GetNode<Marker3D>("Skymove/SpawnEnemy");
		enemy = Character.Spawn(this, spawn_point_enemy, scene_deck[scene_key]) as IEnvEnemy;
		ExtractInfo(enemy);
		GD.Print(enemy);
	}

	public void LoadPlayerInstance(string scene_key="Player"){
		Marker3D spawn_point_player = GetNode<Marker3D>("Skymove/SpawnPlayer"); //Change to arena.GetSpawnMarker
		player = Character.Spawn(this, spawn_point_player, scene_deck[scene_key]) as Player;
		ExtractInfo(player);
		GD.Print(player);
	}

}
