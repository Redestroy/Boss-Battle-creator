using Godot;
using System;

public partial class CutsceneScript : Control
{
	private double timer;

	[Export]
	public double DisplayLength {get;set;} = 20;

	[Export]
	public string next_scene {get; set;} = "res://StartScreen.tscn";

	[Export]
	public string cutscene_file {get; set;} = "res://icon.svg";
	public string cutscene_text {get; set;} = "Defeat";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Cutscene started");
		GD.Print("Cutscene text: " + cutscene_text);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(timer > DisplayLength){
			GD.Print("Cutscene ended");
			GetTree().ChangeSceneToFile(next_scene);
			timer = 0;
		}
		timer += delta;
	}
}
