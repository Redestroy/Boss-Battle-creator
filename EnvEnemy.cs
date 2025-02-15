// TODO
// 1) Extends Env Enemy
// 2) Have signals like: OnSpawn, OnDeath, OnDamage                                     // Kinda done
// 3) Add weapon collision mask                                                         // TODO
// 4) Add hostility                                                                     //Done via Target marker
// 5) Add Health                                                                        //Done
// 6) Add Collision damage                                                              //Done
// 7) Add player sense? (Active target: player, other entity (If gimmick) or null)      //Done via Target marker

using Godot;

public interface IEnvEnemy : IArenaObject{

    [Export]
    Weapon Weapon{ get; set; }
    public int CollisionDamage{get; set;}
    public string Warcry{get; set;}
    protected int Health{get; set;}
    protected Observer Target{get; set;}

    public int GetCollisionDamage();
    public void OnDeath();

    public void OnDamage();
    public void OnPlayerKilled();
    public void SetObserver(Observer observer);
    public Marker3D GetTargetMarker();
}

public partial class EnvEnemy : CharacterBody3D, IEnvEnemy{
    
    //TODO add emit death signal
    [Signal]
    public delegate void PlayerKilledEventHandler();

    [Signal]
    public delegate void EnemyDiedEventHandler();

    [Export]
    public Weapon Weapon{ get; set; }

    [Export]
    public string ArenaAlias { get; set; }
    public int CollisionDamage{get; set;} = 1;
    public int Health{get; set;} = 10;
    public string Warcry{get; set;} = "Puny player";
    public Observer Target{get; set;}

        	

    public EnvEnemy(){

    }

    public int GetCollisionDamage(){
        return CollisionDamage;
    }

    public void OnDeath(){
        EmitSignal(SignalName.EnemyDied);
    }

    public void OnPlayerKilled(){
        GD.Print(this.Warcry);
        EmitSignal(SignalName.PlayerKilled);
    }

    public Marker3D GetTargetMarker(){
        if(Target is TargetObserver){
            TargetObserver targetObserver = this.Target as TargetObserver;
            return targetObserver.GetTargetMarker();
        }
        return null;
    }

    public void SetObserver(Observer observer){
        this.Target = observer;
    }

    public void OnDamage(){
        //Play damaged animation
        GD.Print("Got Damaged");
    }

	public Marker3D GetSelfPositionTargetMarket(){
        return null;
    }
}


public partial class Vitriol : CharacterBody3D, IEnvEnemy{ //Added as child to IEnvEnemy to implement standard functionality 
    
    //TODO add emit death signal
    [Signal]
    public delegate void PlayerKilledEventHandler();

    [Signal]
    public delegate void EnemyDiedEventHandler();

    [Export]
    public Weapon Weapon{ get; set; }
    [Export]
    public string ArenaAlias { get; set; }
    [Export]
    public int CollisionDamage{get; set;} = 1;
    [Export]
    public int Health{get; set;} = 10;
    [Export]
    public string Warcry{get; set;} = "Puny player";

    public Observer Target{get; set;}
    private Character parent;

    public Vitriol(Character parent, int col_dmg){
        CollisionDamage = col_dmg;
        this.parent = parent;
    }

    public void SetObserver(Observer observer){
        this.Target = observer;
    }

    public int GetCollisionDamage(){
        return CollisionDamage;
    }

    public void OnDeath(){
        EmitSignal(SignalName.EnemyDied);
    }

    public void OnPlayerKilled(){
        GD.Print(this.Warcry);
        EmitSignal(SignalName.PlayerKilled);
    }

    public Marker3D GetTargetMarker(){
        if(Target is TargetObserver){
            TargetObserver targetObserver = this.Target as TargetObserver;
            return targetObserver.GetTargetMarker();
        }
        return null;
    }

    public void OnDamage(){
        //Play damaged animation
        GD.Print("Got Damaged");
    }

    public Marker3D GetSelfPositionTargetMarket(){
        return parent.GetNode<Marker3D>("Self");
    }
}


