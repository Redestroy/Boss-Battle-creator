using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MoveDeckDisplay : Control
{
    public Godot.Collections.Dictionary<string, EquippableInfo> equippables;
    public Godot.Collections.Dictionary<int, string> equippables_by_index;

    Label move_deck_name;
    HBoxContainer horizontalContainer;
    PackedScene EquippableDisplayScene;

    Player player;

    bool changed = true;

    public override void _Ready()
	{
        base._Ready();
        EquippableDisplayScene = ResourceLoader.Load<PackedScene>("res://UI/equippable_display.tscn");
        move_deck_name = GetNode<Label>("VBoxContainer/MoveDeckLabel");
        horizontalContainer = GetNode<HBoxContainer>("VBoxContainer/HBoxContainer");
        equippables = new Godot.Collections.Dictionary<string, EquippableInfo>();
        equippables_by_index = new Godot.Collections.Dictionary<int, string>();
    } 

    public void AttachPlayer(Player player_){
        this.player = player_;
        move_deck_name.Visible = true; 
        move_deck_name.Text = player.Name; 
        equippables = player.inventory.equipment;
    }

    public void DeattachPlayer(){
        move_deck_name.Visible = false; 
        move_deck_name.Text = ""; 
        equippables = null;
        //foreach(var item in player.equipment){
        //    Remove(item.Value);
        //}
    }

    public EquippableInfo TryGetEquippable(string type){
        foreach(var item in equippables){
            if(item.Value.EquippableType == type){
                return item.Value;
            }
        }
        return null;
    }

    public EquippableDisplay TryGetEquippableDisplay(string type){
        foreach(var item in horizontalContainer.GetChildren()){
            var item_as_display = item as EquippableDisplay;
            if(item_as_display.equippable.EquippableType == type){
                return item_as_display;
            }
        }
        return null;
    }

    public void _on_slot_item(string key){
        GD.Print($"Slotted an equippable {key}");
        //Add(player.equipment[key]);
    }

    public void _on_deslot_item(string key){
        GD.Print($"DESlotted an equippable {key}");
        //Remove(player.equipment[key]);
    }

    public void Add(EquippableInfo equippable){
        equippables.Add(equippable.Display_label, equippable);
        changed = true;
    }

    public void Remove(Equippable equippable){
        equippables.Remove(equippable.equippableInfo.Display_label);
        changed = true;
    }

    public void UpdateDisplaysFromInventory(Inventory inventory){
        int counter = 0;
        equippables_by_index.Clear();
        foreach(var item in inventory.equipment){
            equippables.TryAdd<string, EquippableInfo>(item.Key, item.Value);
            equippables_by_index.TryAdd<int,string>(counter, item.Key);
            counter++;
        }
        changed = true;
    }

    public void UpdateDisplays(){
        RedrawDisplays();
        move_deck_name.Visible = true;
        move_deck_name.Text = player.Name;
        foreach(var display in horizontalContainer.GetChildren()){
            var as_equip_display = display as EquippableDisplay;
            GD.Print($"Update Display {as_equip_display}");
            as_equip_display.UpdateDisplay();
        }
    }

    public void RedrawDisplays(){
        //TODO add the trivial delete all children and add new ones like is done in with the item lists
        var display_count = player.inventory.GetSlotCounts();
        GD.Print($"Display count {display_count.Count}");
        //if(display_count.Count == 0) return;
        int n_delta = horizontalContainer.GetChildCount() - display_count.Count;
        
        if(n_delta < 0){
            for(int i=0; i < -n_delta; i++){
                var move_ui = EquippableDisplayScene.Instantiate<EquippableDisplay>();
                horizontalContainer.AddChild(move_ui);
            }
        }else if(n_delta > 0){
            for(int i=0; i < n_delta; i++){
                horizontalContainer.RemoveChild(horizontalContainer.GetChild(horizontalContainer.GetChildCount()-1));
            }
        }
        var children = horizontalContainer.GetChildren();
        for(int i = 0; i<display_count.Count; i++){
            int count = display_count[i];
            var display = children[i] as EquippableDisplay;
            display.equippable = player.inventory.equipment[equippables_by_index[i]];
            display.RedrawDisplay(count);
        }
    }
    
    public override void _Process(double delta)
	{
        base._Process(delta);
        if(changed){
            UpdateDisplays();
            changed = false;
        }
    }
}