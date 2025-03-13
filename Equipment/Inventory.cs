using Godot;
using System;
using System.Collections.Generic;

// TODO
// FIX INVENTORY SAVING and LOADING
// 1) Resolve the null errors

public partial class Inventory : Node
{
    public bool InformationInitiated = false;
    public Godot.Collections.Dictionary<string, string> items= new Godot.Collections.Dictionary<string, string>();
    public Godot.Collections.Dictionary<string, string> moves= new Godot.Collections.Dictionary<string, string>();
    public Godot.Collections.Dictionary<string, string> move_types= new Godot.Collections.Dictionary<string, string>();
    public Godot.Collections.Dictionary<string, string> move_attachment= new Godot.Collections.Dictionary<string, string>();
    public Godot.Collections.Dictionary<string, EquippableInfo> equipment= new Godot.Collections.Dictionary<string, EquippableInfo>();

    public override void _Ready(){
        PrintContents();
        InitiateInformation();
    }

    public void InitiateInformation(){
        InformationInitiated = true;
    }

    public void PrintContents(){
        foreach(var item in items){
            GD.Print($"Item {item.Key} :: {item.Value}");
        }
        foreach(var move in moves){
            GD.Print($"Move {move.Key} :: {move.Value}");
        }
        foreach(var move in move_types){
            GD.Print($"move type {move.Key} :: {move.Value}");
        }
        foreach(var move in move_attachment){
            GD.Print($"move type {move.Key} :: {move.Value}");
        }
        foreach(var item in equipment){
            GD.Print($"Item {item.Key} :: {item.Value}");
        }
    }


    public Dictionary<int, Tuple<string, string>> GetItemRef(){
        // Generate a item reference the same way it is generated in HUD
        // TODO generalize for different inventory schemas
        var dict = new Dictionary<int, Tuple<string, string>>();
        int counter = 0;
        foreach(var item in items){ 
            dict.Add(counter, new Tuple<string, string>(item.Key, item.Value));
            counter++;
        }
        return dict;
    }
    public Dictionary<int, Tuple<string, string>> GetMoveRef(){
        // Generate a move reference the same way it is generated in HUD
        // TODO generalize for different inventory schemas
        var dict = new Dictionary<int, Tuple<string, string>>();
        int counter = 0;
        foreach(var item in moves){ 
            dict.Add(counter, new Tuple<string, string>(item.Key, item.Value));
            counter++;
        }
        return dict;
    }

    public void UpdateInventory(Inventory inventory){
        PrintContents();
        items = inventory.items.Duplicate();
        moves = inventory.moves.Duplicate();
        move_types = inventory.move_types.Duplicate();
        move_attachment = inventory.move_attachment.Duplicate();
        equipment = inventory.equipment.Duplicate();
    }

    public void LoadInventory(string name){
        PrintContents();
        GD.Print("Load inv"+name);
        //
        // if(GlobalInventory.inventories.TryGetValue(name, out Inventory inventory)){
           // 
           // if(!InformationInitiated){
           //     InitiateInformation();
           // }
           // GD.Print(inventory);
           // GD.Print(items);
           // GD.Print(inventory.items);
           // items = inventory.items.Duplicate();
           // moves = inventory.moves.Duplicate();
           // move_types = inventory.move_types.Duplicate();
           // move_attachment = inventory.move_attachment.Duplicate();
           // equipment = inventory.equipment.Duplicate();
           // GD.Print(items);
           // GD.Print(inventory.items);
        //}
    }

    public EquippableInfo GetEquippableByName(string key){
        foreach(var item in equipment){
            if(item.Value.Display_label == key){
                return item.Value;
            }
        }
        return null;
    }

    public string GetMoveEquippable(string move_text){
        if(move_types.ContainsKey(move_text))
            return move_types[move_text];
        return null;
    }

    public string GetEquippableNameForMove(string move_alias){
        if(!move_types.ContainsKey(move_alias)) return null;
        if(!equipment.ContainsKey(move_types[move_alias])) return null;
        return equipment[move_types[move_alias]].Display_label;
    }

    public void OnPickup(string item_ref, string item_path, string type){
        if(type == "Move"){
            if(moves.ContainsKey(item_ref)){
                GD.Print($"Key already in move deck {item_ref}");
            }
            moves.Add(item_ref, item_path);
        }else{
            if(items.ContainsKey(item_ref)){
                GD.Print($"Key already in item deck {item_ref}");
            }
            items.Add(item_ref, item_path);
        }
    }

    public void OnSlot(){
        //TODO Emit signal to update displays 
    }

    public void OnDeSlot(){
        //TODO Emit signal to update displays 
    }

    public void Slot(Equippable equippable){
        // First, check if the item can be equipped (return if not)
        //if(false) return;
        if(equipment.TryGetValue(equippable.GetMarkerTag(), out EquippableInfo old_item)){
            DeSlot(old_item);
        }
        equippable.equippableInfo.ItemPath = items[equippable.equippableInfo.Display_label];
        equipment.Add(equippable.GetMarkerTag(), equippable.equippableInfo);
        items.Remove(equippable.equippableInfo.Display_label);
        equippable.Slot();
        OnSlot();
    }


    public void DeSlot(EquippableInfo equippable){
        // First, check if the weapon can be deequipped (return if not and trigger animation)
        //if(false) return;
        equipment.Remove(equippable.EquippableType);
        items.Add(equippable.Display_label, equippable.ItemPath);
        OnDeSlot();
    }

    public void SlotMoveOnEquippable(MoveInfo move, EquippableInfo item){
        move_types.Add(move.Alias_label, move.EquippableType);
        move_attachment.Add(move.Alias_label, item.Display_label);
        equipment[move.EquippableType].AddMove(move);
        move.MovePath = moves[move.Alias_label];
        moves.Remove(move.Alias_label);
    }

    public void RemoveMoveFromItem(string move_string){
        GD.Print($"Move string {move_string}");
        GD.Print($"Move attachment {move_types[move_string]}");
        GD.Print($"Equipment {equipment[move_types[move_string]]}");
        var path = equipment[move_types[move_string]].RemoveMove(move_string); // Key not found on second remove, meaning the equip info might not be read in on reslot
        moves.Add(move_string, path);
        move_types.Remove(move_string);
        move_attachment.Remove(move_string);
    }

    public string GetEquippableNameForMoveByAttachment(string move_string){
        return move_attachment[move_string];
    }

    public Godot.Collections.Array<int> GetSlotCounts(){
        Godot.Collections.Array<int> counts = new Godot.Collections.Array<int>();
        foreach(var item in equipment){
            counts.Add(item.Value.number_of_slots);
        }
        return counts;
    }

    public void Drop(string item_key){
        //EquippableInfo item = items[item_key];
        //items.Remove(item_key);
        //item.DropItem();
        //item.QueueFree();
    }

}