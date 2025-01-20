using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

// TODO move HUD to it's own class

public partial class CreatorPlayer : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	Control active_ui;
	Node3D editor;
	// godot list<string> tutorial
	Godot.Collections.Dictionary<string, Control> UI_elements;
	Godot.Collections.Dictionary<string, PackedScene> UI_views;
	public override void _Ready()
	{
		UI_elements = new Godot.Collections.Dictionary<string,Control>();
		UI_views = new Godot.Collections.Dictionary<string,PackedScene>();
		AddUIFromScene("Nothing");
		AddUIFromScene("Something");
		AddUIFromScene("Debug");
		Activate("Something");
		Init();
		// Get arena for saving and loading
		editor = GetParent() as Node3D;
		// Attach Signals to Methods
	}


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// TODO fix inputs

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Init(){
		// Open UI file
		// According to file
		//	Load UI nodes
		//LoadUI("user://");
		LoadUI("res://");
		PopulateItemList();
		// 	Set hidden
		//  Set Active
	}

	public void LoadUI(string res_path){
		// Load a scene
		// Todo: replace with reading of a JSON or yaml file
		Dictionary<string,string> ui_view_dict = new Dictionary<string, string> {
			{"Arena" , "UI_arena.tscn"},
			{"World" , "UI_world.tscn"},
			{"Decks" , "UI_decks.tscn"},
			{"Markers" , "UI_markers.tscn"},
			{"Objects" , "UI_objects.tscn"},
			{"Debug" , "UI_debug.tscn"}
		};
		// Convert it to UI
		var hud = GetNode("Marker3D/HUD/") as Control;
		var debug_node = GetNode("Marker3D/HUD/"+"Debug") as Control;
		var item_list = debug_node.GetNode("TabContainer/ActiveViews") as ItemList;
		foreach(var item in ui_view_dict){
			UI_views[item.Key] = ResourceLoader.Load<PackedScene>(res_path + item.Value);
			hud.AddChild(UI_views[item.Key].Instantiate());
			item_list.AddItem(item.Key);
		}
		// Attach signals
	}

	public void PopulateItemList(){
		var debug_node = GetNode("Marker3D/HUD/"+"Debug") as Control;
		var item_list = debug_node.GetNode("TabContainer/UI Dictionary") as ItemList;
		foreach(var pair in UI_elements){
			item_list.AddItem(pair.Key);
		}
	}

	public void _on_option_button_item_selected(int idx){
		var debug_node = GetNode("Marker3D/HUD/"+"Debug") as Control;
		var tabs = debug_node.GetNode("TabContainer") as Control;
		var opts = debug_node.GetNode("OptionButton") as OptionButton;
		if(opts.GetItemText(idx) == "Show"){
			tabs.Show();
		}else{
			tabs.Hide();
		}
	}

	public void _on_ui_dictionary_item_activated(int idx){
		var debug_node = GetNode("Marker3D/HUD/"+"Debug") as Control;
		var item_list = debug_node.GetNode("TabContainer/UI Dictionary") as ItemList;
		Activate(item_list.GetItemText(idx));
		//ChangeUI(item_list.GetItemText(idx));
	}

	public void on_change_ui_requested(string ui){
		ChangeUI(ui);
	}

	public void on_continue_pressed(){

	}

	public void ChangeUI(string ui){
		Deactivate();
		Activate(ui);
	}

	public void Deactivate(){
		active_ui.Hide();
	}

	public void Activate(string ui){
		var new_ui = GetUI(ui);
		active_ui = new_ui;
		active_ui.Show();
	}

	public Control GetUI(string ui){
		if(UI_elements.TryGetValue(ui, out Control element)){
			return element;
		}
		else{
			//TODO: Throw exception or fail silently
			return null;
		}
	}

	public void AddUIFromScene(string ui_node_name){
		//TODO: May be problems with duplicate node names, fix later
		var ui_node = GetNode("Marker3D/HUD/"+ui_node_name) as Control;
		UI_elements.Add(ui_node_name, ui_node);
	}
}
