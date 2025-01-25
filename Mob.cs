// TODO
// 1) Extends Env Enemy
// 2) Adds simple observer                  TODO
// 3) Add simple movement controller        TODO
// 4) Maybe can use as projectile           TODO

using Godot;
using System;
using System.Collections.Generic;

public partial class Mob : Character, IEnvEnemy, IDamageable{

    [Signal]
    public delegate void PlayerKilledEventHandler();

    [Signal]
    public delegate void EnemyDiedEventHandler();

    private Heart heart;
    [Export]
    public int CollisionDamage{get; set;}

    [Export]
    public int MaxHealth{get; set;} = 10;

    [Export]
    public int Health{
        get{
            return heart.Health;
        }
        set{
            heart.Health = value;
        }}
    [Export]
    public string Warcry{get; set;}
    public Character Target{get; set;}
    

    public override void _Ready()
	{
        heart = new Heart(this, MaxHealth);
		heart.Damaged += OnDamage;
		heart.Died += OnDeath;
	}

    public override int GetCurrentMove(){
        return 0; // TODO add real logic
    }
    public override Marker3D FindMarker(string node_name){
        //Have a collision area and get marker from there;
        return null;
    }

    public override void Execute(string executor, string order){
        string mob_name = "mob";
        GD.Print($"{mob_name} should execute the following action: {executor} - {order}");
    }

    public override List<string> LoadMoveKeys(){
        //Todo implement
        return null;
    }

    public override Dictionary<string, string> LoadAliases(){
        GD.Print("");
        return null;
    }
    public override Marker3D GetSpawn(){
        return FindMarker("MobSpawn");
    }
    public override string GetScenePath(){
        return this.scene_path;
    }

    //Collision handling
    // On area interactions
    public override void OnAreaEntered(Area3D area){
        GD.Print($"Mob entered area");
    }
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area){
        GD.Print($"Mob exited area");
    }
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider){
        if(!IsOnFloor()){
            GD.Print($"Mob hit a wall");
        }
    }
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider){
        GD.Print($"Mob hit an object");
    }
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider){
        GD.Print($"Mob hit another character");
        if(collider is Boss){

        }
    }
    // On collisions with weapons
    public override void OnCollidedWithWeapon(Weapon collider){
        GD.Print($"Mob hit weapon");

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

    public void OnDamage(int Damage){
        //Play OnDamage
        OnDamage();
    }

    public void OnDamage(){
        //Play OnDamage
    }

}