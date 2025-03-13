using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class HUD : Control
{
	[Signal]
	public delegate void SigEquipItemEventHandler(int index, string node_path);
	[Signal]
	public delegate string SigEquipMoveEventHandler(int index, string node_name);
	[Signal]
	public delegate void SigDeEquipItemEventHandler(string node_name);
	[Signal]
	public delegate string SigDeEquipMoveEventHandler(string node_name);

	Player player;
	Label label_player_health;
	TextureProgressBar bar_player_health;
	Label label_boss_health;
	TextureProgressBar bar_boss_health;
	Label label_help;
	Label label_Debug;
	Label label_control;

	public MoveDeckDisplay move_deck_display;

	ItemList item_list;
	ItemList move_list;
	Tree equip_tree;
	Timer help_timer; ///TODO Slight bug when changing scenes throws a warning (due to Timer being active due to help text change during door transitions)
	double text_hide_delay;

	public Godot.Collections.Array<int> inventory_ref_ui = new Godot.Collections.Array<int>();
    public Godot.Collections.Array<int> move_inventory_ref_ui = new Godot.Collections.Array<int>();

	
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
		move_list = hud.GetNode<ItemList>("TabContainer/MoveList");
		equip_tree = hud.GetNode<Tree>("TabContainer/EquipTree");
		TreeItem root = equip_tree.CreateItem();
		equip_tree.HideRoot = true;
		move_deck_display = hud.GetNode<MoveDeckDisplay>("MoveDeckDisplay");
		//move_deck_display.AttachPlayer(player);
		
		help_timer = hud.GetNode<Timer>("HelpTimer");
		item_list.ItemActivated += _on_item_list_item_activated;
		move_list.ItemActivated += _on_move_list_item_activated;
		equip_tree.ItemActivated += _on_equip_tree_item_activated;
		text_hide_delay = 4.0;
		UpdateInventoryLists();
		UpdateMoveDeckDisplay();
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
		string text = item_list.GetItemText((int)index);
		EquipItem((int)index, text);
		UpdateInventoryLists();
		UpdateMoveDeckDisplay();
	}

	private void _on_move_list_item_activated(long index){
		string text = move_list.GetItemText((int)index);
		EquipMove((int)index, text);
		UpdateInventoryLists();
		UpdateMoveDeckDisplay();
	}

	private void AddMoveToTree(TreeItem branch, string move_name){
		TreeItem move_item = equip_tree.CreateItem(branch);
		move_item.SetText(0, move_name);
	}

	private TreeItem GetTreeItemForEquippableString(string equippable_name){
		TreeItem current = equip_tree.GetRoot().GetFirstChild();
		while(current != null)
		{
			if(current.Visible && current.GetText != null){
				if(current.GetText(0) == equippable_name){
					return current;
				}
			}
			current = current.GetNext();
		}
		return null;
	}

	private TreeItem GetTreeItemForEquippable(Equippable equippable){
		TreeItem current = equip_tree.GetRoot().GetFirstChild();
		while(current != null)
		{
			if(current.Visible && current.GetText != null){
				if(current.GetText(0) == equippable.Name){
					return current;
				}
			}
			current = current.GetNext();
		}
		return null;
	}

	private void _on_equip_tree_item_activated(){
		TreeItem active_item = equip_tree.GetSelected();
		if(active_item != null){
			int item_depth = GetItemDepth(active_item);
			string selected_text = active_item.GetText(0);
			if(item_depth == 1){
				foreach(var item in active_item.GetChildren()){
				}
			}
			DeEquip(selected_text, item_depth);
			// Remove item from item list
			active_item.Free();
		}
		UpdateInventoryLists();
		UpdateMoveDeckDisplay();
	}

	private int GetItemDepth(TreeItem item)
	{
    	int depth = 0;
    	while (item.GetParent() != null) // Traverse up the tree
    	{
        	depth++;
        	item = item.GetParent();
    	}
    	return depth;
	}

	public void DeEquip(string selected_text, int depth){
		if(depth == 1){
			DeEquipItem(selected_text);
			item_list.AddItem(selected_text);
		}else if(depth == 2){
			DeEquipMove(selected_text);
			move_list.AddItem(selected_text);
		}
	}

	public void UpdateInventoryLists(){
		// TODO: Can optimize to only change the affected ones
		item_list.Clear();
		foreach(var item in player.inventory.items){
			item_list.AddItem(item.Key);
		}
		move_list.Clear();
		foreach(var move in player.inventory.moves){
			move_list.AddItem(move.Key);
		}
		equip_tree.Clear();
		IterateThroughEquipment();
	}

	public void IterateThroughEquipment(){
		var root = equip_tree.GetRoot();
		if(root == null){
			root = equip_tree.CreateItem();
		}
		foreach(var item in player.inventory.equipment){ // Can replace with the entry from global inventory
			TreeItem item_branch = AddItemInfoAsTreeItem(item.Value);
			foreach(var move in item.Value.slots){
				AddMoveToTree(item_branch, move.Alias_label);
			}
		}
	}

	public void Refresh(){
		UpdateInventoryLists();
		UpdateMoveDeckDisplay();
	}

	public TreeItem AddItemInfoAsTreeItem(EquippableInfo item){
		TreeItem equippable_item_slot = equip_tree.GetRoot().CreateChild();
		equippable_item_slot.SetText(0, item.Display_label);
		return equippable_item_slot;
	}

	public void UpdateMoveDeckDisplay(){
		move_deck_display.UpdateDisplaysFromInventory(player.inventory);
	}

	public int AddToItemList(string node_path){
		return item_list.AddItem(node_path);
	}

	public int AddToMoveList(string node_path){
		return move_list.AddItem(node_path);
	}

	public void EquipMove(int idx, string node_name){
		EmitSignal(SignalName.SigEquipMove, idx, node_name);
	}

	public void DeEquipMove(string node_name){
		EmitSignal(SignalName.SigDeEquipMove, node_name);
	}

	public void EquipItem(int idx, string node_path){
		EmitSignal(SignalName.SigEquipItem, idx, node_path);
	}

	public void DeEquipItem(string node_path){
		EmitSignal(SignalName.SigDeEquipItem, node_path);
	}
}
