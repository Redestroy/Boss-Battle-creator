using Godot;
using System;
using System.Collections.Generic;

// TODO 
// 1) Agent script injection
// 2) Observer
// 3) Extends EnvEnemy (or Hostile) for Targeting
// 4) Handle saving the boss state

public partial class Boss : Character, IEnvEnemy{

    [Signal]
    public delegate void BossVanquishedEventHandler();

    [Signal]
    public delegate void PlayerKilledEventHandler();

    public int CollisionDamage{get; set;} = 20;
    public string Warcry{get; set;} = "Fee Fii Foo";

    public const int MaxHealth = 2000;
    public int Health{get; set;} = MaxHealth;
    
    public Character Target{get; set;}

    private bool alive = true;

    public override void _Ready()
	{
        base._Ready();
    }

    public override void _Process(double delta)
	{
        base._Process(delta);
    }

    public override void _PhysicsProcess(double delta)
	{
        base._PhysicsProcess(delta);
    }

    public int GetHealth(){
        return Health;
    }

    public void Damage(int dmg){
        Health -= dmg;
        if(Health < 0){
            Health = 0;
            OnDeath();
        }else{
            OnDamage();
        }
    }

    public void OnDeath(){
        // Send Death signal
        EmitSignal(SignalName.BossVanquished);
        // Play Death animation
        GD.Print("You are victorious");
        // Enter state dead, for whatever that entails
        alive = false;
    }

    public override int GetCurrentMove(){
        // TODO add the random number picker code later
        bool is_idle = true;
		if(is_idle){
			Random rand = new Random();
			return rand.Next(0, move_deck_ids.Keys.Count);
		}
        return 0;
    }

    public override Marker3D FindMarker(string node_name){
        // TODO add the arena to pick the marker
        return null;
    }
    public override void Execute(string executor, string order){
        string boss_name = "boss";
        GD.Print($"{boss_name} should execute the following action: {executor} - {order}");
    }

    public override List<string> LoadMoveKeys(){
        //Todo implement
        return null;
    }

    public override Dictionary<string, string> LoadAliases(){
        //Todo implement
        return null;
    }
    public override Marker3D GetSpawn(){
        return FindMarker("SpawnBoss");
    }

    public override string GetScenePath(){
        return this.scene_path;
    }

    public override void Spawn(Marker3D spot){
        Character.Spawn(spot, GetScenePath());
    }

    //Collision handling
    // On area interactions
    public override void OnAreaEntered(Area3D area){
        GD.Print("Boss entered Area");
    }
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area){
        GD.Print("Boss exited Area");
    }
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider){
        if(!IsOnFloor()){
            GD.Print("Boss collided with Env");
        }
    }
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider){
        GD.Print("Boss collided with object");
    }
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider){
        GD.Print("Boss collided with character"); // Handled in player
    }
    // On collisions with weapons
    public override void OnCollidedWithWeapon(Weapon collider){
        this.Damage(collider.Damage);
    }

    public int GetCollisionDamage(){
        return CollisionDamage;
    }

    public void OnDamage(){
        //Add damage animation here
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
}