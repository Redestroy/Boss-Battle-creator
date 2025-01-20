using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

// TODO player functionality
// 1) Input mapping
// 2) HUD
// 3) Camera viewport
// 4) Inventory
// 5) Ext. Targetable/ish, Character
// 6) 

public partial class Player : Character{

	[Signal]
    public delegate void PlayerKilledEventHandler();

	[Signal]
    public delegate void SaveStageEventHandler();

	[Export] 
	public bool is_active = false;

	[Export]
    private int health {get;set;} = 100; 

    private bool alive = true;

	public static bool player_selected = false;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	private Camera3D camera;
	private PhysicsDirectSpaceState3D _space_state;
	private static Player active_player;
	private bool constrained; 

	private Weapon weapon;
	private RayCast3D raycast;
	private float rayLength = 10.0f; // Length of the ray

	public override void _Ready()
	{
		if(!player_selected){
			// Select first player as active
			GD.Print("Player selected as active");
			is_active = true;
			player_selected = true;
			active_player = this;
		}
		constrained = false;
		camera = GetNode<Camera3D>("Marker3D/Camera3D");
		weapon = GetNode<Weapon>("Pivot/WeaponPivot/Weapon");
		//raycast = GetNode<RayCast3D>("RayCast3D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		_space_state = GetWorld3D().DirectSpaceState;


		if (is_active && !constrained && alive){
		if (Input.IsActionPressed("FWD"))
    	{
			play_action("FWD");
    	}


		if (Input.IsActionPressed("BWD"))
    	{
        	play_action("BWD");
		}

		if (Input.IsActionPressed("LEFT"))
    	{
        	play_action("LEFT");
		}

		if (Input.IsActionPressed("RIGHT"))
    	{
        	play_action("RIGHT");
		}

		if (Input.IsActionPressed("JUMP"))
    	{
        	play_action("JUMP");
		}

		if (Input.IsActionPressed("ULT"))
    	{
        	play_action("ULT");
		}

		if (Input.IsActionPressed("ABILITY"))
    	{
        	play_action("ABILITY");
		}

		if (Input.IsActionPressed("LACTION"))
    	{
        	play_action("LACTION");
		}

		if (Input.IsActionPressed("RACTION"))
    	{
        	play_action("RACTION");
		}
		
		//raycast. .CastTo = new Vector3(0, -rayLength, 0); // Pointing downwards
        //raycast.Enabled = true;

        // Check if the ray is colliding
        //if (raycast.IsColliding())
        //{
            //var collider = (CollisionObject3D)raycast.GetCollider();
            // Optionally, check if it's terrain (if your terrain is in a specific group)
            //if (collider.HasGroup("terrain"))
            //{
            //    GD.Print("Colliding with terrain!");
            //}
        //}

		}
        OnPhysicsProcess(delta);
	}


	public Player GetActivePlayer(){
		return active_player;
	}

	public void play_action(string action_tag){
		float d_rot = 0.15f;
		float d_mov = 0.15f;
		if (action_tag == "LEFT"){
			rotate(d_rot);
		}
		else if (action_tag == "RIGHT"){
			rotate(-d_rot);
		}
		else if (action_tag == "FWD"){
			move(d_mov);
		}
		else if (action_tag == "BWD"){
			move(-d_mov);
		}
		else if (action_tag == "RACTION"){
			//raycast to get player
			string player_tag = get_player_tag_from_cast();
			//call change player
			if (player_tag != "MISS"){
				change_player(player_tag);
			}else{
				GD.Print("No player: ");
			}
		}
		else if (action_tag == "SaveArena"){
			SaveArena();
		}
		GD.Print(action_tag);
        OnPlayAction(action_tag);
	}

	public void rotate(float delta){
		Transform3D transform = Transform;
		Vector3 axis = new Vector3(0, 1, 0);
		transform.Basis = transform.Basis.Rotated(axis, delta);
		Transform = transform;
	}

	public void move(float delta){
		Transform3D transform = Transform;
		transform = transform.TranslatedLocal(new Vector3(0,0,delta));
		Transform = transform;
	}

	public string get_player_tag_from_cast(){
		return "MISS";
	}

	public void change_player(string player_tag){
		//Get node from player tag or pass it as arg, then set it active
		//GetTree().UniqueName(player_tag).set_active;
		is_active = false;
	}

	public void _set_active(){
		active_player.change_player("PLACEHOLDER");
		active_player = this;
		camera.MakeCurrent();
		is_active = true;
	}

	public virtual void _on_input_event(){
        GD.Print("Clicked on player");
    }

    public virtual void OnPlayAction(string tag){

    }

     public virtual void OnPhysicsProcess(double delta){

        // We get one of the collisions with the player.
       		KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta, testOnly: true);
			if (collision != null)
        	{
            	GD.Print("I collided with ", ((Node)collision.GetCollider()).Name);
        
				for (int i = 0; i < collision.GetCollisionCount(); i++){
				
        		if (collision.GetCollider(i).IsClass(this.weapon.GetClass()))
        			{
						Weapon enemy_weapon = (Weapon)collision.GetCollider(i);
            			int damage = enemy_weapon.Damage;
						this.Damage(damage);
        			}
				}
			}
    }

	//public void TeleportTo(Marker3D spot){
	//	Vector3 coordinates = spot.GlobalPosition;
//		GD.Print("Teleporting player to " + coordinates.ToString());
//		this.GlobalPosition = coordinates;
//	}

	public void StoreState(){
		//load health, weapon and modifiers to file/singleton to store info between scenes
	}

	public void RestoreState(){
		// Read info (for variables) from file
	}

	public void SaveArena(){
		EmitSignal(SignalName.SaveStage);
		//load health, weapon and modifiers to file/singleton to store info between scenes
	}

	public void Constrain(){
		constrained = true;
	}

	public void Release(){
		constrained = false;
	}

	
    public int GetHealth(){
        return health;
    }

	public void Damage(int dmg){
		GD.Print("Ouch! That hurt");
        health -= dmg;
        if(health < 0){
            health = 0;
            OnDeath();
        }
    }

	public void OnFled(){
		GD.Print("Player ran like a chicken");
		OnDeath();
	}

    public void OnDeath(){
        // Send Death signal
		EmitSignal(SignalName.PlayerKilled);
        // Play Death animation
        // Enter state dead, for whatever that entails
        alive = false;
    }

	public override int GetCurrentMove(){
		return current_move; //current move is set on input
	}
    public override Marker3D FindMarker(string node_name){
		//Find child in arena object
		return new Marker3D();
	}
    public override void Execute(string executor, string order){
		GD.Print($"Player should execute the following action: {executor} - {order}");
	}

	public override void LoadAliases(Dictionary<string, string> move_deck_aliases){

	}

    public override Marker3D GetSpawn(){
		return FindMarker("SpawnPlayer");
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
		GD.Print("Entered area");
	}
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public override void OnAreaExited(Area3D area){
		GD.Print("Exited area");
	}
    // On collisions with Environment - Static body
    public override void OnCollidedWithEnvironment(StaticBody3D collider){
		if(!IsOnFloor()){
			GD.Print("Hit environment wall");
		}
	}
    // On collisions with objects - Rigid body
    public override void OnCollidedWithObject(RigidBody3D collider){
		GD.Print("Hit object");
	}
    // On collisions with characters
    public override void OnCollidedWithCharacter(CharacterBody3D collider){
		GD.Print("Ran into another character");
		if(collider is EnvEnemy){
			GD.Print("ITs ENEMY!!!");
			enemy = collider as EnvEnemy;
			this.Damage(enemy.GetCollisionDamage);
		}
	}
    // On collisions with weapons
    public override void OnCollidedWithWeapon(Weapon collider){
		GD.Print("Hit a weapon");
		this.Damage(collider.Damage);
	}



	public Control GetHUD(){
		return GetNode<Control>("HUD");
	}
}