using Godot;

public interface IEquippable{
    public void Slot();
    public void DeSlot();
    public string MarkerTag{get;set;}
}

public abstract partial class Equippable : AnimatableBody3D, IEquippable
{
    [Export]
    public EquippableInfo equippableInfo{get;set;}
    
    [Export]
    public Godot.Collections.Array<MoveInfo> moveInfos{get;set;}
    
    public Godot.Collections.Array<MoveUi> moveUIs{get;set;} // Check if actually required

    public virtual string MarkerTag{get;set;} // Slightly redundant

    public bool IsEquipped{get;private set;} = false;

    public Equippable() : this(null, null){
        this.moveInfos ??= new Godot.Collections.Array<MoveInfo>();
        this.moveUIs ??= new Godot.Collections.Array<MoveUi>();
        this.equippableInfo ??= new EquippableInfo();
    }

    public Equippable(Godot.Collections.Array<MoveInfo> moveInfos_, Godot.Collections.Array<MoveUi> moveUis_){
        this.moveInfos ??= moveInfos_;
        this.moveUIs ??= moveUis_;
        this.equippableInfo ??= new EquippableInfo();
    }

    public virtual void Slot(){
        IsEquipped = true;
        foreach(var move in CharacterInformation.MoveInfos){
            if(move.Value.EquippableType == GetMarkerTag()){
                this.AddMove(move.Value);
            }
        }
    }

    public virtual void DeSlot(){
        //foreach (var moveInfo in item.moveInfos)
        //{
        //        DisableMove(moveInfo.Alias_label);
        //}
        IsEquipped = false;
    }

    public void Stow(){
        SaveInfo();
        QueueFree();
    }

    public void SaveInfo(){
        this.equippableInfo = new EquippableInfo(this);
        CharacterInformation.Equippables[GetMarkerTag()] = this.equippableInfo;
        foreach(var move in moveInfos){
            CharacterInformation.MoveInfos[move.Alias_label] = move;
        }
    }

    public virtual void AddMove(MoveUi move){
        if(move.moveInfo.EquippableType == this.GetMarkerTag()){
            FreeSlot(move.moveInfo.EquippableSlot);
            moveUIs.Add(move);
            moveInfos.Add(move.moveInfo);
        }
    }

    public virtual void AddMove(MoveInfo move){
        if(move.EquippableType == this.GetMarkerTag()){
            FreeSlot(move.EquippableSlot);
            //moveUIs.Add(new MoveUi(move));
            moveInfos.Add(move);
        }
    }

    public void FreeSlot(string slot_name){ //TODO: modify to use Equippable info
        foreach(var move in moveUIs){
            if(slot_name == move.moveInfo.EquippableSlot){
                RemoveMove(move);
                return;
            }
        }
    }

    public virtual void RemoveMove(MoveUi move){ //TODO: modify to use Equippable info
        moveUIs.Remove(move);
        moveInfos.Remove(move.moveInfo);
    }

    public virtual void RemoveMoveByName(string move_name){ //TODO: modify to use Equippable info
        for(int i = 0; i<moveInfos.Count; i++){
            if(move_name == moveInfos[i].Alias_label){
                moveUIs.RemoveAt(i);
                moveInfos.RemoveAt(i);
                return;
            }
        }
    }

    public void TeleportTo(Marker3D spot){
        //this.Transform = spot.GlobalTransform;
        this.Position = spot.Position;
    }

    public abstract string GetParentNodeTag();

    public virtual string GetMarkerTag(){
		return MarkerTag;
	}

    public virtual void DropItem(){
		//var reward = reward_card.Instantiate() as Item;
		//Marker3D rew_pos = GetNodeOrNull<Marker3D>("Skymove/SpawnRewardCard");
		//if(rew_pos == null){
		//	rew_pos = spawn_markers["SpawnRewardCard"];
        //}
		//reward.DropItem(rew_pos);
		//this.AddChild(reward);
	}

    public void SetCollisionsTo(uint layer, uint mask){
        // Player weapon    (0b00000000_00000000_00000000_01000000, 0b00000000_00000000_00000001_10111100)
        // Mob weapon       (0b00000000_00000000_00000001_00000000, 0b00000000_00000000_00000001_10110010)
        // Boss weapon      (0b00000000_00000000_00000010_00000000, 0b00000000_00000000_00000001_10111010)
        // Body             (0b00000000_00000000_00000000_01000000, 0b00000000_00000000_00000001_10111100) NOT SET
        // Passive          (0b00000000_00000000_00000000_01000000, 0b00000000_00000000_00000001_10111100) NOT SET
        // Consumable       (0b00000000_00000000_00000000_01000000, 0b00000000_00000000_00000001_10111100) NOT SET
        this.CollisionLayer = 0;
        this.CollisionMask = 0;
        this.SetCollisionLayerValue(7, true);
        for(int i = 0; i<16; i++){
            this.SetCollisionMaskValue(i+1, ((mask>>i)&0b1)==1);
        }
        GD.Print(this.CollisionLayer);
        GD.Print(this.CollisionMask);
        //this.CollisionMask = mask;
    }
}