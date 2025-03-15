using Godot;
using System;

public partial class Menu_Play : Control
{
	[Export]
	string scene_prefix = "res://Arenas/";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_online_pvp_pressed(){
		OnLaunch("Arena");
	}

	public void _on_online_pve_pressed(){
		OnLaunch("Labyrinth");
	}

	public void _on_offline_pve_pressed(){
		OnLaunch("Labyrinth1");
	}

	public void _on_offline_solo_pressed(){
		OnLaunch("Creative");
	}
	public void _on_back_pressed(){
		OnBack();
	}

	public void OnBack(){
		// return to start scene
		GD.Print("Back");
		GetTree().ChangeSceneToFile("res://Menus/Menu_Start.tscn");
	}

	public void OnLaunch(string gamemode){
		string scene_name = scene_prefix;
		switch(gamemode){
			case "Labyrinth1":
				scene_name += "Paths/";
				scene_name += gamemode;
				break;
			case "Arena":
				scene_name += "Dojos/";
				scene_name += "simple_dojo";//gamemode;
				//Add IP address
				break;
			case "Labyrinth":
				scene_name += "Paths/";
				scene_name += gamemode;
				//Join a default IP
				// for testing, it can join the labyrinth set in options or something
				break;
			case "Creative":
				scene_name += "Editor";
				//Open Editor scene
				break;
			case "Default":
				scene_name += "Menu_About";
				break;
		}
		scene_name += ".tscn";
		GD.Print("Back");
		GetTree().ChangeSceneToFile(scene_name);
	}
}
