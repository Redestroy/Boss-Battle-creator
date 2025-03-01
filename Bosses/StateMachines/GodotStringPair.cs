using System.Collections.Generic;
using System.Threading;
using Godot;

[GlobalClass]
public partial class GodotStringPair : Resource{
    [Export]
    public string From{get; set;} = "";
    [Export]
    public string To{get; set;} = "";

    public string Key{
        get{
            return From;
        } 
        set{
            From = value;
        }
    }
    public string Value{
        get{
            return To;
        } 
        set{
            To = value;
        }
    }

    public GodotStringPair() : this("", "") {}

    public GodotStringPair(string from = "", string to = ""){
        From = from;
        To = to;
    }
}