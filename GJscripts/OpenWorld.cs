using Godot;
using System;

public partial class OpenWorld : Node3D
{
	private double timer;
	private Player player;
	private bool defeated;
	private string scene_defeat = "res://CutsceneDefeat.tscn";
	// Called when the node enters the scene tree for the first time.

	private Control hud;
	private Label label_player_health;
	private Label label_boss_health;

	private Label label_help;

	private TextureProgressBar bar_player_health;
	private TextureProgressBar bar_boss_health;

	private int player_health;
	private double player_health_max;

	public override async void _Ready()
	{
		timer = 0;
		//await RenderingServer.Singleton.ToSignal();
		player = GetNode<Player>("Shade");
		player._set_active();

		//Hud
		player_health = player.GetHealth();
		hud = player.GetHUD();
		label_player_health = hud.GetNode<Label>("Health");
		label_boss_health = hud.GetNode<Label>("BossHealth");
		label_boss_health.Visible = false;
		bar_player_health = hud.GetNode<TextureProgressBar>("HealthBar");
		bar_boss_health = hud.GetNode<TextureProgressBar>("BossHealthBar");
		bar_boss_health.Visible = false;
		player_health_max = player_health;
		label_help = hud.GetNode<Label>("Help");
		label_help.Text = "Get back what you lost, do not turn back";

		Marker3D start_point = GetNode<Marker3D>("StartMarker");
		player.TeleportTo(start_point);
		defeated = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if esc pressed/ pause and prompt if player wishes to exit
		if(!defeated){
			if(timer > 1){
				GD.Print("World is alive");
				timer = 0;
			}
			timer += delta;
		}else{
			GetTree().ChangeSceneToFile(scene_defeat);
		}
	}

	public void _on_shade_player_killed(){
		OnDefeat();
	}

	public void OnDefeat(){
        //Door door = GetNode<Door>("DefeatDoor");
		//label_help.Text = "You died!";
		//
		defeated = true;
    }
}
