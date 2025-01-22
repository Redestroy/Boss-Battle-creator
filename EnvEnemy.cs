// TODO
// 1) Extends Env Enemy
// 2) Have signals like: OnSpawn, OnDeath, OnDamage                                     // TODO
// 3) Add weapon collision mask                                                         // TODO
// 4) Add hostility                                                                     //Done via Target marker
// 5) Add Health                                                                        //Done
// 6) Add Collision damage                                                              //Done
// 7) Add player sense? (Active target: player, other entity (If gimmick) or null)      //Done via Target marker

using Godot;

public interface IEnvEnemy{
    public int CollisionDamage{get; set;}
    public string Warcry{get; set;}
    protected int Health{get; set;}
    protected Character Target{get; set;}

    public int GetCollisionDamage();
    public void OnDeath();

    public void OnDamage();
    public void OnPlayerKilled();
    public Marker3D GetTargetMarker();
}

public partial class EnvEnemy : CharacterBody3D, IEnvEnemy{
    
    //TODO add emit death signal
    [Signal]
    public delegate void PlayerKilledEventHandler();

    [Signal]
    public delegate void EnemyDiedEventHandler();

    public int CollisionDamage{get; set;} = 1;
    public int Health{get; set;} = 10;
    public string Warcry{get; set;} = "Puny player";
    public Character Target{get; set;}

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
        if(Target != null){
            return Target.FindMarker("Self");
        }
        else{
            return null;
        }
    }

    public void OnDamage(){
        //Play damaged animation
        GD.Print("Got Damaged");
    }
}