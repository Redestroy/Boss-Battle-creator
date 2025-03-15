using System.ComponentModel;
using System.Threading;
using Godot;

public partial class EquippableDisplay : Control
{
    [Export]
    public string move_ui_scene_path{get;set;} = "res://Equipment/Weapons/Moves/Move_QuickAttack.tscn";
    [Export]
    public string move_ui_scene_empty_path{get;set;} = "res://Equipment/Weapons/Moves/Move_Empty.tscn";
    public Godot.Collections.Dictionary<string, MoveInfo> moveInfos;
    Label equippable_name;

    HBoxContainer horizontalContainer;
    public EquippableInfo equippable;

    public string Display_label{
        get{
            return equippable.Display_label;
        }
        set{
            equippable.Display_label = value;
        }
    }

    PackedScene moveUiScene;
    PackedScene moveUiSceneEmpty;
    bool changed = true;

    public override void _Ready()
	{
        base._Ready();
        foreach(var node in this.GetChildren()){
            GD.Print(node);
        }
        equippable_name = this.GetNode<Label>("VBoxContainer/LabelEquippable");
        horizontalContainer = this.GetNode<HBoxContainer>("VBoxContainer/HBoxContainer");
        moveInfos = new Godot.Collections.Dictionary<string, MoveInfo>();
        moveUiScene = ResourceLoader.Load<PackedScene>(move_ui_scene_path);
        moveUiSceneEmpty = ResourceLoader.Load<PackedScene>(move_ui_scene_empty_path);
    }

    public void _on_equip_move_card(MoveInfo move_ui){
        Add(move_ui);
    }

    public void _on_deequip_move_card(MoveInfo move_ui){
        Remove(move_ui);
    }

    public void AttachEquippable(EquippableInfo equippable_){
        this.equippable = equippable_;
        GD.Print(equippable);
        equippable_name ??= new Label();
        equippable_name.Visible = true; 
        equippable_name.Text = equippable.Display_label; 
        foreach(var move in equippable.slots){
            Add(move);
        }
    }

    public void DeattachEquippable(){
        equippable_name.Visible = false;
        equippable_name.Text = ""; 
        foreach(var move in equippable.slots){
            Remove(move);
        }
    }

    public void Add(MoveInfo moveInfo){
        moveInfos.Add(moveInfo.Alias_label, moveInfo);
        changed = true;
    }

    public void Remove(MoveInfo moveInfo){
        moveInfos.Remove(moveInfo.Alias_label);
        changed = true;
    }

    public override void _Process(double delta)
	{
        base._Process(delta);
        if(changed){
            UpdateDisplay();
            UpdateCooldowns();
            changed = false;
        }
    }

    public void UpdateEquippableInfo(EquippableInfo info){
        equippable = info;
    }

    public void RedrawDisplay(int count){
        GD.Print($"Redraw display to have {count} moves");
        int n_delta = horizontalContainer.GetChildCount() - count;
        if(n_delta < 0){
            for(int i=0; i < -n_delta; i++){
                var move_ui = moveUiSceneEmpty.Instantiate<MoveUi>();
                move_ui.Set("size_flags_horizontal", (int)Control.SizeFlags.Expand);
                move_ui.Set("size_flags_vertical", (int)Control.SizeFlags.ShrinkCenter);
                move_ui.Set("custom_minimum_size", new Vector2(200, 250));
                move_ui.SetPosition(Vector2.Zero);
                horizontalContainer.AddChild(move_ui);
                
                GD.Print($"horizontalContainer {horizontalContainer}");
                GD.Print($"parent of move ui {move_ui.GetParent()}");
            }
        }else if(n_delta > 0){
            for(int i=0; i < n_delta; i++){
                //Possible memleak
                horizontalContainer.RemoveChild(horizontalContainer.GetChild(horizontalContainer.GetChildCount()-1));
            }
        }
        horizontalContainer.ResetSize();
    }

    public void UpdateDisplay(){
        var child_list = horizontalContainer.GetChildren();
        equippable_name ??= new Label();
        equippable_name.Visible = true; 
        if(equippable != null){
        equippable_name.Text = equippable.Display_label; 
        GD.Print($"     Update Display Ui {equippable.Display_label}");
        for(int i=0; i< child_list.Count; i++){
            var move_ui = child_list[i] as MoveUi;
            GD.Print($"     Update move Ui {move_ui}");
            if(equippable.slots.Count > i){
                var move = equippable.slots[i];
                move_ui.UpdateCardView(move);
            }else{
                move_ui.ResetToEmpty();
            }
        }
        }
    }

    public void UpdateCooldowns(){
        //foreach(var move_ui in moveUis)
    }
}