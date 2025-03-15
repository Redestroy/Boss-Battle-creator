using Godot;
using System;

public partial class PauseMenu : Control
{
    [Signal]
    public delegate void ResumeEventHandler();

     public override async void _Ready(){
        base._Ready();
    }

    public void _on_exit_button_pressed(){
        // maybe add saving here
        GD.Print("Back");
		GetTree().ChangeSceneToFile("res://Menus/Menu_Start.tscn");
    }

    public void _on_resume_button_pressed(){
        EmitSignal(SignalName.Resume);
    }
}
