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

public partial class Lair : Arena
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

	private Godot.Collections.Dictionary<string, Marker3D> target_markers;
	private Godot.Collections.Dictionary<string, Variant> event_deck;


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
		target_markers = new Godot.Collections.Dictionary<string, Marker3D>();
		player = GetNode<Player>("Player");
		//player._set_active();
		//boss = GetNode<Boss>("ShadeBoss");
		
		
		//enemy_health = boss.GetHealth();
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


	public override void OnUpdate(double delta){
		base.OnUpdate(delta);	
		//boss_health = boss.GetHealth();
		//player_health = player.GetHealth();
		//UpdateHUD();
	}


    public override void OnVictory(){
        base.OnVictory();
		//Kill boss
		//boss.Despawn();
        //Save screenshots
        //Spawn exit
        //Door door = GetNode<Door>("VictoryDoor");
        //Marker3D door_pos = GetNode<Marker3D>("SpawnVictoryDoor");
        //door.GlobalPosition = door_pos.GlobalPosition;
        //Trigger animation and lighting as well
    }

    public override void OnDefeat(){
        base.OnDefeat();
		//Door door = GetNode<Door>("DefeatDoor");
		//label_help.Text = "You died!";
        //door.OnDoorTriggered();
    }

	public override void OnPlayerEntered(Player player){
		base.OnPlayerEntered(player);
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

	public override IVitriolic GetEnemy(){
		return boss as IVitriolic;
	}
}