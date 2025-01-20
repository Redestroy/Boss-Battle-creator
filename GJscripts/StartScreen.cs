using Godot;
using System;

public partial class StartScreen : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Hello from C# to Godot :)");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_play_pressed(){
		OnPlay();
		
	}

	public void _on_options_pressed(){
		OnOptions();
	}

	public void OnPlay(){
		GD.Print("Play");
		//string level_file = "res://Testworld.tscn";
		//string level_file = "res://OpenWorld.tscn";
		string level_file = "res://CutsceneEntered.tscn";
		GetTree().ChangeSceneToFile(level_file);
	}

	public void OnExit(){
		GD.Print("Exited...");
	}

	public void OnOptions(){
		GD.Print("Options");
		GetTree().ChangeSceneToFile("res://OptionsScreen.tscn");
	}


}
