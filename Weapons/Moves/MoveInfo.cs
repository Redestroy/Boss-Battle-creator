using System.Collections.Generic;
using System.Threading;
using Godot;

[GlobalClass]
public partial class MoveInfo : Resource{
    [Export]
    public string Input_label{get; set;} = "";
    [Export]
    public string Alias_label{get; set;} = "";
    [Export]
    public string MoveDescription{get; set;} = "";
    [Export]
    public float Cooldown{get; set;} = 0.01f;
    [Export]
    public Texture2D Card_image{get;set;}

    public MoveInfo() : this("", "", "") {}

    public MoveInfo(string input_l, string alias_l, string md){
        Input_label ??= input_l;
        Alias_label ??= alias_l;
        MoveDescription ??= md;
    }
}