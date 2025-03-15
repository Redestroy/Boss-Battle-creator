using Godot;
using System;
using System.Collections.Generic;

// Level handles the system menu on paused (maybe the saving and loading of inventories as well)
// Arena and Pathway classes extend the level class 

public abstract partial class Level : Node3D
{
    [Export]
    public string pause_menu_path{get;set;} = "res://UI/pause_menu.tscn";

    PauseMenu pause_menu{get;set;}

    public override async void _Ready(){
        base._Ready();
        pause_menu = ResourceLoader.Load<PackedScene>(pause_menu_path).Instantiate() as PauseMenu;
        AddChild(pause_menu);
        pause_menu.Hide();
        this.pause_menu.Resume += _on_resume;
    }

    public void _on_pause(){
        //Pause game
        GetTree().Paused = true;
        pause_menu.Show();
    }

    public void _on_resume(){
        //Resume game
        pause_menu.Hide();
        GetTree().Paused = false;
    }
}