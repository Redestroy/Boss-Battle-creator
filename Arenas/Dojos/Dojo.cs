using Godot;
using Godot.Collections;
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
	public string enemy_scene_path {get; set;} = "res://Characters/TestMob/TestMob_Item.tscn";

	[Export]
	public string dojo_scene_path {get; set;} = "res://PlayerGeneric.tscn";

	[Export]
	public ArenaInfo arenaInfo {get; set;}

	private IEnvEnemy enemy;
	private Vitriol vitriol;
	private Godot.Collections.Dictionary<string, PackedScene> scene_deck;

	private Godot.Area3D boundary;

	bool defeated = false;

	private Godot.Collections.Dictionary<string, string> magics = new Godot.Collections.Dictionary<string,string>(){
		{"keystone", "res://Environment/Doors/keystone.tscn"},
		{"labyrinth", "res://Arenas/Paths/Labyrinth.tscn"},
		{"1", ""},
		{"2", ""}
	};
	

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		// Load scenes
		//GD.Print("Is GlobalInventory still available? " + (GlobalInventory.Instance != null));
		base._Ready();
		var player_scene = GD.Load<PackedScene>(player_scene_path);
		var enemy_scene = GD.Load<PackedScene>(enemy_scene_path);
		var dojo_scene = GD.Load<PackedScene>(dojo_scene_path);
		scene_deck = new Godot.Collections.Dictionary<string, PackedScene>(); 
		scene_deck.Add("Player", player_scene);
		scene_deck.Add("Enemy", enemy_scene);
		scene_deck.Add("Dojo", dojo_scene);
		LoadDojo(dojo_scene_path);
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
		string arena_build_mode = GetBuildMode();
		switch (arena_build_mode)
		{
			case "FROM_INFO":
				// Load Skymove
				Marker3D skymove = MarkerInfoC.CreateMarker3D(arenaInfo.SkymoveInfo);
				AddChild(skymove);
				// Load WorldEnvironment
				var WorldEnvironment = new WorldEnvironment();
				WorldEnvironment.Environment = arenaInfo.WorldEnvironment;
				AddChild(WorldEnvironment);
				// Load terrain
				var dojo_node = scene_deck["Dojo"].Instantiate<Node3D>();
				dojo_node.Transform = skymove.GlobalTransform;
				skymove.AddChild(dojo_node);
				//Create type decks
				var type_deck = new Godot.Collections.Dictionary<string,string>();
				door_deck = new Godot.Collections.Dictionary<string,Door>();
				var door_info_deck = new Godot.Collections.Dictionary<string,DoorInfo>();
				// Load in spawn markers to the arena
				foreach(var marker_info in arenaInfo.SpawnMarkers){
					spawn_markers.Add(marker_info.Name, MarkerInfoC.CreateMarker3D(marker_info));
					nodes_to_spawn.Add(marker_info.Name, marker_info.SpawnedNode);
					skymove.AddChild(spawn_markers[marker_info.Name]);
					type_deck.Add(marker_info.Name, marker_info.NodeType);
				}
				// Load in packed scenes
				foreach(var scene_pair in arenaInfo.NodeScenePaths){
					scene_deck.Add( scene_pair.Key ,GD.Load<PackedScene>(scene_pair.Value));
				}
				// Instantiate scenes in order
				Godot.Collections.Array<string> skipped_scenes = new Godot.Collections.Array<string>(){"Dojo", "Player", "Enemy"};
				foreach(var spawn_marker in spawn_markers){
					if(!nodes_to_spawn.ContainsKey(spawn_marker.Key)){
						GD.Print($"Nodes to spawn does not contain element {spawn_marker.Key}");
						continue;
					}
					if(!scene_deck.ContainsKey(nodes_to_spawn[spawn_marker.Key])){
						GD.Print($"Scene deck does not contain element {nodes_to_spawn[spawn_marker.Key]}");
						continue;
					}
					if(skipped_scenes.Contains(nodes_to_spawn[spawn_marker.Key])){
						GD.Print($"Skipping node: {nodes_to_spawn[spawn_marker.Key]}");
						continue;
					}
					var spawned_node = scene_deck[nodes_to_spawn[spawn_marker.Key]].Instantiate<Node3D>(); // Instantiate node
					spawned_node.Transform = spawn_marker.Value.GlobalTransform; // Move node to the marker location
					skymove.AddChild(spawned_node); // TODO: Determine parent somehow
					if(type_deck[spawn_marker.Key] == "Door"){ //For each special type, separate initialization is needed
						var door_node = spawned_node as Door;
						door_deck.Add(spawn_marker.Key, door_node);
					}
 				}
				
				foreach(var door in door_deck){
					//TODO: Instantiate door from specific door info
					door.Value.InitializeDoor(DoorInfo.FindDoorInfoByName(door.Key, arenaInfo.DoorInfoDeck));
					// Connect signals to door nodes
					var door_node = door.Value.GetNode<StaticBody3D>("Door");
					door_node.Connect("input_event", new Callable(door.Value, nameof(door.Value._on_input_event)));
					door.Value.Connect("LeavingScene", new Callable(this, nameof(this._on_leaving_scene_door)));
				}
				// TODO: Fix collision handling
				// Add boundary area
				boundary = CreateBoundary(69, 50); // TODO: determine height and radius either from arena dimensions or arena info
			break;
			case "FROM_SCENE":
				door_deck = new Godot.Collections.Dictionary<string,Door>();
				Marker3D m_keystone_player = GetNode<Marker3D>("Skymove/ReturnKeystone");
				Marker3D m_keystone_enemy = GetNode<Marker3D>("Skymove/EnemyKeystone");
				Marker3D m_keystone_victory = GetNode<Marker3D>("Skymove/VictoryKeystone");
				// Load in Doors (And keystones)
				var keystone_scene = GD.Load<PackedScene>(magics["keystone"]); //all magic strings are contained in the arena info or should be skipped by proto
				var keystone_player = keystone_scene.Instantiate() as Door;
				var keystone_enemy = keystone_scene.Instantiate() as Door;
				var keystone_victory = keystone_scene.Instantiate() as Door;
				keystone_player.scene_triggered = magics["labyrinth"];			// All of these return to the previous scene
				keystone_enemy.scene_triggered = magics["labyrinth"];
				keystone_victory.scene_triggered = magics["labyrinth"]; 			// On victory, return to previous scene
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
				keystone_player.Connect("LeavingScene", new Callable(this, nameof(this._on_leaving_scene_door)));
				keystone_enemy.Connect("LeavingScene", new Callable(this, nameof(this._on_leaving_scene_door)));
				keystone_victory.Connect("LeavingScene", new Callable(this, nameof(this._on_leaving_scene_door)));
				door_deck.Add("PlayerKeystone", keystone_player);
				door_deck.Add("EnemyKeystone", keystone_enemy);
				door_deck.Add("VictoryKeystone", keystone_victory);
				boundary = GetNode<Godot.Area3D>("Boundary");
				GD.Print($"Outside bounds {boundary}");
				if(boundary != null){
					boundary.BodyEntered += _on_boundary_body_entered;
					boundary.BodyExited += _on_boundary_body_exited;
				}
			break;
			default:
				GD.Print("Arena could not be built");
			break;
		}
		// Load in Skybox (Environment) (skip for proto)
		// Load in Arena base (Gridmap or static body or Terrain mesh) (skip for proto)
		// Load in Markers as a list (skip for proto and autoloader)
		
		// Start spawning collision objects
		// Start spawning hazards
		// Spawn viewports
		// Spawn visuals 
	}

	public string GetBuildMode(){
		if(arenaInfo != null){
			return "FROM_INFO";
		}
		// Check if this is an arena auto builder
		//if(this is ArenaAutoBuild){

		//}
		return "FROM_SCENE";
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
		Marker3D spawn_point_enemy = GetNodeOrNull<Marker3D>("Skymove/SpawnEnemy");
		if(spawn_point_enemy == null && arenaInfo != null){
			spawn_point_enemy = spawn_markers[$"Spawn{scene_key}"];
		}
		var character_node = Character.Spawn(this, spawn_point_enemy, scene_deck[scene_key]);
		enemy = character_node as IEnvEnemy;
		ExtractInfo(enemy);
		vitriol = ((character_node as Node)as IVitriolic).GetVitriol();
		GD.Print(enemy);

	}

	public void LoadPlayerInstance(string scene_key="Player"){
		Marker3D spawn_point_player = GetNodeOrNull<Marker3D>("Skymove/SpawnPlayer"); //Change to arena.GetSpawnMarker
		if(spawn_point_player == null && arenaInfo != null){
			spawn_point_player = spawn_markers[$"Spawn{scene_key}"];
		}
		player = Character.Spawn(this, spawn_point_player, scene_deck[scene_key]) as Player;
		ExtractInfo(player);
		GD.Print(player);
		player._set_active();
		this.Connect("SigLeavingScene", new Callable(player, nameof(player._on_leaving_scene)));
		foreach(var door in door_deck){
			door.Value.Connect("LookingAtDoor", new Callable(player, nameof(player._on_door_looking_at_door)));
		}
	}

	public Godot.Area3D CreateBoundary(float radius, float height){
		var area_boundary = new Godot.Area3D();
		area_boundary.Name = "Boundary";
		var boundary_cylinder = new CollisionShape3D();
		var cylinder = new CylinderShape3D();
		cylinder.Radius = radius;
		cylinder.Height = height;
		boundary_cylinder.Shape = cylinder;
		area_boundary.AddChild(boundary_cylinder);
		AddChild(area_boundary);
		area_boundary.BodyEntered += _on_boundary_body_entered;
		area_boundary.BodyExited += _on_boundary_body_exited;
		return area_boundary;
	}

	public override IVitriolic GetEnemy(){
		return enemy as IVitriolic;
	}

}
