using Godot;
using System;
using System.Threading.Tasks;

public partial class HUD : Control
{
	[Signal]
	public delegate void SigEquipItemEventHandler(int index, string node_path);
	[Signal]
	public delegate void SigDeEquipItemEventHandler(int index, string node_path);

	Player player;
	Label label_player_health;
	TextureProgressBar bar_player_health;
	Label label_boss_health;
	TextureProgressBar bar_boss_health;
	Label label_help;
	Label label_Debug;
	Label label_control;

	ItemList item_list;
	ItemList equip_list;
	Timer help_timer; ///TODO Slight bug when changing scenes throws a warning (due to Timer being active due to help text change during door transitions)
	double text_hide_delay;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var hud = this.GetNode<Control>("AspectRatioContainer/cHUD");
		player = GetParent<Player>(); 
		label_player_health = hud.GetNode<Label>("Health");
		bar_player_health = hud.GetNode<TextureProgressBar>("HealthBar");
		label_boss_health = hud.GetNode<Label>("BossHealth");
		bar_boss_health = hud.GetNode<TextureProgressBar>("BossHealthBar");
		label_help = hud.GetNode<Label>("Help");
		label_Debug = hud.GetNode<Label>("Debug");
		label_control = hud.GetNode<Label>("Controls");
		item_list = hud.GetNode<ItemList>("TabContainer/ItemList");
		equip_list = hud.GetNode<ItemList>("TabContainer/EquipList");
		
		help_timer = hud.GetNode<Timer>("HelpTimer");
		item_list.ItemActivated += _on_item_list_item_activated;
		equip_list.ItemActivated += _on_equip_list_item_activated;
		text_hide_delay = 4.0;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

	public void UpdateHelpText(string text, bool time_fade = true){
		label_help.Text = text;
		label_help.Show();
		if(time_fade) help_timer.Start(text_hide_delay);
	}

	public void UpdateLifeBar(){
		label_player_health.Text = $"{player.heart.Health}";
		bar_player_health.SetValueNoSignal(100 * player.heart.RelativeHealth);
	}

	public void _on_help_timer_timeout(){
		label_help.Hide();
	}

	private void _on_item_list_item_activated(long index){
		EquipItem((int)index, item_list.GetItemText((int)index));
		// Add item to equipment tab
		equip_list.AddItem(item_list.GetItemText((int)index));
		// Remove item from item list
		item_list.RemoveItem((int)index);
	}

	private void _on_equip_list_item_activated(long index){
		DeEquipItem((int)index, equip_list.GetItemText((int)index));
		// Add item to equipment tab
		item_list.AddItem(equip_list.GetItemText((int)index));
		// Remove item from item list
		equip_list.RemoveItem((int)index);
	}

	public int AddToItemList(string node_path){
		//TODO add handling for duplicates
		return item_list.AddItem(node_path);
	}

	public void EquipItem(int idx, string node_path){
		EmitSignal(SignalName.SigEquipItem, idx, node_path);
	}

	public void DeEquipItem(int idx, string node_path){
		EmitSignal(SignalName.SigDeEquipItem, idx, node_path);
	}
}
