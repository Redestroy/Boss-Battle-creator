using Godot;
using System;

public partial class Menu_Start : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_play_pressed(){
		OnPlay();
	}

	public void _on_library_pressed(){
		OnLibrary();
	}

	public void _on_options_pressed(){
		OnOptions();
	}

	public void _on_about_pressed(){
		OnAbout();
	}

	public void OnPlay(){
		GD.Print("Play");
		//string level_file = "res://Testworld.tscn";
		//string level_file = "res://OpenWorld.tscn";
		string level_file = "res://Menu_Play.tscn";
		GetTree().ChangeSceneToFile(level_file);
	}

	public void OnExit(){
		GD.Print("Exited...");
	}

	public void OnLibrary(){
		GD.Print("Library");
		GetTree().ChangeSceneToFile("res://Menu_Library.tscn");
	}

	public void OnOptions(){
		GD.Print("Options");
		GetTree().ChangeSceneToFile("res://Menu_Options.tscn");
	}

	public void OnAbout(){
		GD.Print("Options");
		GetTree().ChangeSceneToFile("res://Menu_About.tscn");
	}
}
