using Godot;
using System;

public partial class Menu_Options : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	
	public void _on_apply_pressed(){
		OnApplyAndExit();
	}

	public void _on_save_pressed(){
		OnSave();
	}
	public void _on_cancel_pressed(){
		OnCancel();
	}
	public void _on_back_pressed(){
		OnBack();
	}
	public void _on_default_pressed(){
		OnDefault();
	}

	public void OnApplyAndExit(){
		// return to start scene
		GD.Print("Apply");
		OnSave();
		GD.Print("Exit");
		OnBack();
	}

	public void OnBack(){
		// return to start scene
		GD.Print("Back");
		GetTree().ChangeSceneToFile("res://Menus/Menu_Start.tscn");
	}

	public void OnSave(){
		// Save settings to structure
		GD.Print("Save");
	}

	public void OnCancel(){
		// Change values back to struct values and update fields
		GD.Print("Cancel");
	}
	public void OnDefault(){
		//load settings from default file
		
		string default_file = "default.json";
		GD.Print("default " + default_file);
		load_settings_from_file(default_file);

	}

	private void load_settings_from_file(string filename){
		// load text from file as a dictionary
		// save settings in the settings structure
		GD.Print("Loading");
	}
}
