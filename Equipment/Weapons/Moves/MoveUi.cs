using Godot;
using System;

public partial class MoveUi : Control
{
	[Signal]
	public delegate void CooldownEndedEventHandler();

	[Export]
	public MoveInfo moveInfo{get; set;}

	public string Alias{
		get{
			return moveInfo.Alias_label;
		}
		set{
			moveInfo.Alias_label = value;
		}
	}

	Texture2D blank_texture; // TODO: later replace with a blank card

	Timer cooldown;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		blank_texture = ResourceLoader.Load<Texture2D>("res://Assets/Paint/Blank.png");
		var move_card = GetNodeOrNull("Move_UI") as MoveUi;
		if(move_card == null) move_card = this;
		if(moveInfo != null){
			move_card.GetNode<Label>("VBoxContainer/Label_Ctrl").Text = moveInfo.Input_label;
			move_card.GetNode<Label>("VBoxContainer/Label_Alias").Text = moveInfo.Alias_label;
			if(moveInfo.Card_image != null) move_card.GetNode<TextureRect>("VBoxContainer/TextureRect").Texture = moveInfo.Card_image;
		}
		cooldown = move_card.GetNode<Timer>("Timer");
		if(moveInfo != null) cooldown.WaitTime = moveInfo.Cooldown;
		cooldown.Timeout += _on_cooldown_timer_timeout;
	}

	public void UpdateCardView(MoveInfo moveInfo){
		var move_card = GetNodeOrNull("Move_UI") as MoveUi;
		GD.Print($"Updating card view {move_card}");
		if(move_card == null) move_card = this;
		if(moveInfo != null){
			move_card.GetNode<Label>("VBoxContainer/Label_Ctrl").Text = moveInfo.Input_label;
			move_card.GetNode<Label>("VBoxContainer/Label_Alias").Text = moveInfo.Alias_label;
			if(moveInfo.Card_image != null) move_card.GetNode<TextureRect>("VBoxContainer/TextureRect").Texture = moveInfo.Card_image;
			cooldown.WaitTime = moveInfo.Cooldown;
		}
	}

	public void ResetToEmpty(){
		moveInfo = new MoveInfo();
		moveInfo.Card_image = blank_texture;
		UpdateCardView(moveInfo);
	}

	public void UpdateInformation(MoveInfo moveInfo){
		//TODO update info
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

	public void _on_trigger_cooldown(){
		GetNode<TextureProgressBar>("Cooldown").Visible = true;
		cooldown.Start();
	}
}
