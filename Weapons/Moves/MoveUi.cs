using Godot;
using System;

public partial class MoveUi : Control
{
	[Signal]
	public delegate void CooldownEndedEventHandler();

	[Export]
	public MoveInfo moveInfo{get; set;}

	Timer cooldown;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(moveInfo != null){
			GetNode<Label>("VBoxContainer/Label_Ctrl").Text = moveInfo.Input_label;
			GetNode<Label>("VBoxContainer/Label_Alias").Text = moveInfo.Alias_label;
			if(moveInfo.Card_image != null) GetNode<TextureRect>("VBoxContainer/TextureRect").Texture = moveInfo.Card_image;
		}
		cooldown = GetNode<Timer>("Timer");
		if(moveInfo != null) cooldown.WaitTime = moveInfo.Cooldown;
		cooldown.Timeout += _on_cooldown_timer_timeout;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!cooldown.IsStopped()) GetNode<TextureProgressBar>("Cooldown").Value = 100*cooldown.TimeLeft/cooldown.WaitTime;
		EmitSignal(SignalName.CooldownEnded);
	}

	public void _on_cooldown_timer_timeout(){
		GetNode<TextureProgressBar>("Cooldown").Visible = false;
	}

	public void _on_triger_cooldown(){
		GetNode<TextureProgressBar>("Cooldown").Visible = true;
		cooldown.Start();
	}
}
