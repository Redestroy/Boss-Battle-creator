using Godot;
using System;

public partial class Labyrinth : Node3D
{
	private double counter;
	private Player player;
	private IVitriolic main_enemy;
	private Vitriol main_enemy_vitriol;
	[Signal]
	public delegate void SigLeavingSceneEventHandler();

	public bool active = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PostReady();
	}

	public void PostReady(){
		
		player = GetNode<Player>("Player");
		this.Connect("SigLeavingScene", new Callable(player, nameof(player._on_leaving_scene)));
		//player._set_active();
		//boss = GetNode<Boss>("ShadeBoss");
		//main_enemy = GetEnemy();
		// Add boss signal manually
    	//main_enemy_vitriol.EnemyDied += OnVictory;
		// Add player signal manually
		//player.PlayerKilled += OnDefeat;
		//player.SaveStage += OnSaveArena;
		OnPlayerEntered(player);
		active = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
		player._set_active();
	}

	public void _on_leaving_scene(string next_scene){
		CharacterInformation.active_scene = next_scene;
		CharacterInformation.previous_scene = this.Name;
	}
}
