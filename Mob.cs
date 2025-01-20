// TODO
// 1) Extends Env Enemy
// 2) Adds simple observer
// 3) Add simple movement controller
// 4) Maybe can use as projectile

using Godot;
using System;
using System.Collections.Generic;

public partial class Mob : Character, IEnvEnemy{

    public override void _Ready()
	{
	}

    public override int GetCurrentMove(){
        return 0; // TODO add real logic
    }
    public override Marker3D FindMarker(string node_name){
        //Have a collidion area and get marker from there;
        return null;
    }

    public override void Execute(string executor, string order){
        GD.Print($"Player should execute the following action: {executor} - {order}");
    }

    public override void LoadAliases(Dictionary<string, string> move_deck_aliases);
    public override Marker3D GetSpawn();
    public override string GetScenePath();
    public override void Spawn(Marker3D spot);

    //Collision handling
    // On area interactions
    public override void OnAreaEntered(Area3D area);
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area);
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider);
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider);
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider);
    // On collisions with weapons
   