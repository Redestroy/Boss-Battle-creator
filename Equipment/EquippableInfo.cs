using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Godot;

[GlobalClass]
public partial class EquippableInfo : Resource{
    [Export]
    public string Display_label{get; set;} = "";
    [Export]
    public string Alias_label{get; set;} = "";
    [Export]
    public string EquippableDescription{get; set;} = "";
    [Export]
    public string EquippableType{get; set;} = "";
    [Export]
    public string ItemPath{get;set;} = "";
    [Export]
    public int number_of_slots{get; set;} = 4;
    [Export]
    public Godot.Collections.Array<MoveInfo> slots{get; set;}
    [Export]
    public Godot.Collections.Dictionary<string, int> slot_names{get; set;}

    public EquippableInfo() : this("", "", "", "", null) {}

    public EquippableInfo(string display_label_, string alias_l, string md, string et, Godot.Collections.Array<MoveInfo> arr, int ns = 4){
        Display_label ??= display_label_;
        Alias_label ??= alias_l;
        EquippableDescription ??= md;
        EquippableType = et;
        number_of_slots = ns;
        slots ??= arr;
        slots ??= new Godot.Collections.Array<MoveInfo>();
        slot_names = new Godot.Collections.Dictionary<string,int>(){
            {"LMB", 0},
            {"RMB", 1},
            {"ABILITY", 2},
            {"ULT", 3}
        };
    }

    public EquippableInfo(Equippable item): this(item.Name, item.equippableInfo.Alias_label, item.equippableInfo.EquippableDescription, item.GetMarkerTag(), item.moveInfos){

    }

    public void AddMove(MoveInfo move){
        var tmp_slot_names = new Godot.Collections.Dictionary<string,int>(){
            {"LMB", 0},
            {"RMB", 1},
            {"ABILITY", 2},
            {"ULT", 3}
        };
        GD.Print($"Input_label {move.Input_label}");
        foreach(var slot in tmp_slot_names){
            GD.Print(
                $"Input {slot.Key} index {slot.Value}"
            );
        }
        int spot = tmp_slot_names[move.Input_label];
        slots.Insert(spot, move);
    }

    public string RemoveMove(string move_string){
        foreach(var move in slots){
            if(move.Alias_label == move_string){
                GD.Print($"Removed move {move_string}");
                string move_path = move.MovePath;
                slots.Remove(move);
                return move_path;
            }
        }
        return "";
    }
}