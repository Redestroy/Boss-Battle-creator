using Godot;
using System;

public partial class CharacterInformation : Node
{
    public static bool InformationInitiated = false;
    public static string ActivePlayer = "";
    public static string Inventory = "";

    public static string previous_scene = "";
    public static string active_scene = "";

    public static Godot.Collections.Dictionary<string, EquippableInfo> Equippables; // All equipables In inventory 

    public static Godot.Collections.Dictionary<string, MoveInfo> MoveInfos;         // All moves in inventory

    public static void InitializeCharacterInformation(){
        Equippables = new Godot.Collections.Dictionary<string, EquippableInfo>();
        MoveInfos = new Godot.Collections.Dictionary<string, MoveInfo>();
        InformationInitiated = true;
    }
}
