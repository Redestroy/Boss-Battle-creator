using Godot;
using System;

public partial class Arena : Node3D
{
	private double counter;
	private Player player;
	private Boss boss;

	private bool active;

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


	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		counter = 0;
		if(GetParent() != null){
			GD.Print(GetParent().ToString());
			//Window window = GetParent<Window>();
			//if(window.)
			//await RenderingServer.Singleton.ToSignal(GetParent(), SignalName.Ready);
		}
		//player = GetNode<Player>("Shade");
		//player._set_active();
		//boss = GetNode<Boss>("ShadeBoss");
		
		
		//boss_health = boss.GetHealth();
		//player_health = player.GetHealth();
		//hud = player.GetHUD();
		//label_player_health = hud.GetNode<Label>("Health");
		//label_boss_health = hud.GetNode<Label>("BossHealth");
		//bar_player_health = hud.GetNode<TextureProgressBar>("HealthBar");
		//bar_boss_health = hud.GetNode<TextureProgressBar>("BossHealthBar");
		//player_health_max = player_health;
		//boss_health_max = boss_health;
		//label_help = hud.GetNode<Label>("Help");
		
		// Add boss signal manually
    	//boss.BossVanquished += OnVictory;
		// Add player signal manually
		//player.PlayerKilled += OnDefeat;
		//player.SaveStage += OnSaveArena;

		active = true;

		//OnPlayerEntered(player);
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


	public void OnUpdate(double delta){
		//boss_health = boss.GetHealth();
		//player_health = player.GetHealth();
		//UpdateHUD();
	}


    public void OnVictory(){
        //Kill boss
		//boss.Despawn();
        //Save screenshots
        //Spawn exit
        //Door door = GetNode<Door>("VictoryDoor");
        //Marker3D door_pos = GetNode<Marker3D>("SpawnVictoryDoor");
        //door.GlobalPosition = door_pos.GlobalPosition;
        //Trigger animation and lighting as well
    }

    public void OnDefeat(){
        //Door door = GetNode<Door>("DefeatDoor");
		//label_help.Text = "You died!";
        //door.OnDoorTriggered();
    }

	public void UpdateHUD(){
		label_boss_health.Text = $"{boss_health}";
		label_player_health.Text = $"{player_health}";

		//Update texture bars as well
		bar_boss_health.SetValueNoSignal(boss_health/boss_health_max);
		bar_player_health.SetValueNoSignal(player_health/player_health_max);
	}

	public void OnPlayerEntered(Player player){
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
}