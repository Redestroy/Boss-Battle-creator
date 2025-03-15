using Godot;
using System;
using System.Collections.Generic;

// Arena should extend IEnvironment that Overworld also extends (Overworld would not feature combat)
// 	Arena is the environment of a fight
// 	There are (currently) two types of arenas: Dojos and Lairs
// 	A Lair has a boss associated with it, as well as soundtracks and cutscenes
// 	Dojos on the other hand are simpler and let's two characters fight
//  Therefore arena handles arena side logic of:
//		Building arena from file
//		Saving and loading
//		Timing and arena manipulation
//		Spawning entities
//		Physics
//		Event passing between fighters (HUD updates)
//		Skybox
//	Dojo extends arena and adds the following functionality
//		Entry
//		Exit
//		Victory condition
//	Lair adds an additional boss related functionality
//  	Cutscene player and cutscenes
//		Event deck
//		Marker deck




public interface IArenaObject{
	public string ArenaAlias { get; set; }
	public Marker3D GetSelfPositionTargetMarket();
}

public abstract partial class Arena : Level
{

	[Signal]
	public delegate void SigVictoryEventHandler();

	[Signal]
	public delegate void SigDefeatEventHandler();

	[Signal]
	public delegate void SigLeavingSceneEventHandler();

	[Export]
	public string RewardPath{get;set;}

	private double counter;
	protected Player player;
	protected IVitriolic main_enemy;
	protected Vitriol main_enemy_vitriol;

	protected bool active; // TODO add a getter and make private

	private Control hud;
	private Label label_player_health;
	private Label label_boss_health;

	private Label label_help;

	private TextureProgressBar bar_player_health;
	private TextureProgressBar bar_boss_health;

	private int player_health;
	private double player_health_max;
	private int boss_health;
	private double boss_health_max;
	private PackedScene reward_card;

	protected Godot.Collections.Dictionary<string, Marker3D> spawn_markers;
	protected Godot.Collections.Dictionary<string, string> nodes_to_spawn;
	protected Godot.Collections.Dictionary<string, string> item_scene_paths;
	protected Godot.Collections.Dictionary<string, PackedScene> item_scenes;
	private Godot.Collections.Dictionary<string, Marker3D> target_markers;
	private Godot.Collections.Dictionary<string, Variant> event_deck;

	protected Godot.Collections.Dictionary<string, Door> door_deck;

	public override async void _Ready()
	{
		base._Ready();
		if(!CharacterInformation.InformationInitiated){
			CharacterInformation.InitializeCharacterInformation();
			CharacterInformation.active_scene = this.Name;
		}
		counter = 0;
		if(GetParent() != null){
			GD.Print(GetParent().ToString());
			//Window window = GetParent<Window>();
			//if(window.)
			//await RenderingServer.Singleton.ToSignal(GetParent(), SignalName.Ready);
		}
		target_markers = new Godot.Collections.Dictionary<string, Marker3D>();
		spawn_markers = new Godot.Collections.Dictionary<string, Marker3D>();
		nodes_to_spawn = new Godot.Collections.Dictionary<string, string>();
		reward_card = ResourceLoader.Load<PackedScene>(RewardPath);
		item_scene_paths = new Godot.Collections.Dictionary<string, string>();
		item_scene_paths.Add("reward", RewardPath);
		item_scenes = new Godot.Collections.Dictionary<string, PackedScene>();
		foreach(var item_path in item_scene_paths){
			item_scenes.Add(item_path.Key, ResourceLoader.Load<PackedScene>(item_path.Value));
		}

		//Debug
		var global_inv = (GlobalInventory) GetNode("/root/GlobalInventory");
		GD.Print("Inventories count: " + global_inv.inventories.Count);
		foreach (var inventory in global_inv.inventories)
		{
    		GD.Print($"Inventory: {inventory.Key} | Items: {inventory.Value.items.Count}");
		}
		//OnPlayerEntered(player);
	}

	public void _on_leaving_scene_door(string next_scene){
		// connect all dors to this, then emit signal
		EmitSignal(SignalName.SigLeavingScene, next_scene);
	}

	public void PostReady(){
		
		player = GetNode<Player>("Player");
		main_enemy = GetEnemy();
		main_enemy_vitriol = main_enemy.GetVitriol();
    	main_enemy_vitriol.EnemyDied += OnVictory;
		player.PlayerKilled += OnDefeat;
		player.SaveStage += OnSaveArena;
		OnPlayerEntered(player);
		active = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if esc pressed/ pause and prompt if player wishes to exit
		if(active){
			if(counter > 1){
			GD.Print("Arena is alive");
			counter = 0;
			}
			counter += delta;
			OnUpdate(delta);
		}
	}


	public virtual void OnUpdate(double delta){
		//UpdateHUD();
	}

	


    public virtual void OnVictory(){
        //Kill boss
		//main_enemy.Despawn(); // Handle this in child
        //Save screenshots
		//Spawn reward
		if(reward_card != null){
			var reward = reward_card.Instantiate() as Item;
			GD.Print($"Reward spawned {reward}");
			Marker3D rew_pos = GetNodeOrNull<Marker3D>("Skymove/SpawnRewardCard");
			if(rew_pos == null){
				rew_pos = spawn_markers["SpawnRewardCard"];
			}
			GD.Print($"Reward position {rew_pos}");
			GD.Print($"Reward {reward}");
			GD.Print($"Reward {reward_card}");
			reward.DropItem(rew_pos);
			this.AddChild(reward);
		}
        //Spawn exit
		GD.Print($"door deck {door_deck}");
		if(door_deck.ContainsKey("VictoryKeystone")){
        	Door door = door_deck["VictoryKeystone"]; //"VictoryDoor" // Change this to be less hardcoded - improve door deck
			Marker3D door_pos = GetNodeOrNull<Marker3D>("Skymove/SpawnVictoryDoor");
			if(door_pos == null){
				door_pos = spawn_markers["SpawnVictoryDoor"];
			}
        	door.TeleportTo(door_pos);
		}else{
			GD.Print("No Victory keystone found");
			foreach(var door in door_deck){
				GD.Print($"Door {door.Key}");
			}
		}
		player.GetHUD().UpdateHelpText("You Win!");
		EmitSignal(SignalName.SigVictory);
		//Trigger animation and lighting as well
    }

	public virtual void DropItem(string item_scene_key, string spawn_marker_node_path){
		var drop = item_scenes[item_scene_key].Instantiate() as Item;
		Marker3D drop_pos = GetNodeOrNull<Marker3D>(spawn_marker_node_path);
		if(drop_pos == null){
			drop_pos = spawn_markers[spawn_marker_node_path];
		}
		drop.DropItem(drop_pos);
		this.AddChild(drop);
	} 

    public virtual void OnDefeat(){
		EmitSignal(SignalName.SigVictory);
        Door door = door_deck["EnemyKeystone"];
		player.GetHUD().UpdateHelpText("You died!", false);
		active = false;
        door.OnDoorTriggered();
    }

	public void ExtractInfo(IArenaObject a_obj){
		this.target_markers[a_obj.ArenaAlias] = a_obj.GetSelfPositionTargetMarket();
	}

	public Marker3D GetTargetMarker(string key){
		if(!target_markers.ContainsKey(key)){
			GD.Print("Marker does not exist in the list");
			foreach(var marker_pair in target_markers){
				GD.Print($"{marker_pair.Key} in {target_markers[marker_pair.Key]}");
			}
			return new Marker3D();
		}
		return this.target_markers[key];
	}

	public void UpdateHUD(){
		label_boss_health.Text = $"{boss_health}";
		label_player_health.Text = $"{player_health}";

		//Update texture bars as well
		bar_boss_health.SetValueNoSignal(boss_health/boss_health_max);
		bar_player_health.SetValueNoSignal(player_health/player_health_max);
	}

	public virtual void OnPlayerEntered(Player player){
		// Constrain player
		//player.Constrain();
		// Teleport Boss to start marker
		//Marker3D start_point = GetNode<Marker3D>("SpawnBoss");
		//boss.TeleportTo(start_point);
		// On boss, play Round start animation
		//boss.PlayStartAnimation();
		// await animation end
		// Display health
		//label_help.Text = "Face your past self!";
		//UpdateHUD();
		// Set player as hostile for boss
		// boss.SetTarget(player);
		// release player
		//player.Release();
	}

	public abstract IVitriolic GetEnemy();

	public void OnSaveArena(){
		Save();
	}

	public void Save(){
		// Pause game
		// Save all arena objects in order
		//string arena_name = "Arena";
		//using var saveFile = FileAccess.Open($"user://{arena_name}.arena", FileAccess.ModeFlags.Write);
		//var saveNodes = GetTree().GetNodesInGroup("StoredGroup");
		//foreach (Node saveNode in saveNodes)
    	//{
        // Check the node is an instanced scene so it can be instanced again during load.
        //if (string.IsNullOrEmpty(saveNode.SceneFilePath))
        //{
        //    GD.Print($"persistent node '{saveNode.Name}' is not an instanced scene, skipped");
        //    continue;
        //}

        // Check the node has a save function.
        //if (!saveNode.HasMethod("Save"))
        //{
        //    GD.Print($"persistent node '{saveNode.Name}' is missing a Save() function, skipped");
        //    continue;
        //}

        // Call the node's save function.
        //var nodeData = saveNode.Call("Save");

        // Json provides a static method to serialized JSON string.
        //var jsonString = Json.Stringify(nodeData);

        // Store the save dictionary as a new line in the save file.
        //saveFile.StoreLine(jsonString);
    	//}
	}

	public static Marker3D CreateMarker3D(MarkerInfoC markerInfo){
		Marker3D marker = new Marker3D();
		marker.Position = new Vector3(markerInfo.Px, markerInfo.Py, markerInfo.Pz);
		marker.Quaternion = markerInfo.angle;
		return marker;
	}
}