using Godot;
using System;

// TODO 
// 1) Agent script injection
// 2) Observer
// 3) Extends EnvEnemy (or Hostile) for Targeting
// 4) Handle saving the boss state

public partial class Boss : AnimatedCharacter{

    [Signal]
    public delegate void BossVanquishedEventHandler();

    [Export]
    private int health {get;set;} = 2000; 

    private bool alive = true;

    public override void _Ready()
	{
        base._Ready();
    }


    public override void _PhysicsProcess(double delta)
	{
        base._PhysicsProcess(delta);
        KinematicCollision3D collision = MoveAndCollide(Velocity * (float)delta, testOnly: true);
        if (collision != null)
        {
            GD.Print("Boss collided with ", ((Node)collision.GetCollider()).Name);
        
            if (alive){
                for (int i = 0; i < collision.GetCollisionCount(); i++){
				
        		    if (collision.GetCollider() is Weapon en_weapon)
        			{
            			int damage = en_weapon.Damage;
						this.Damage(damage);
        			}
				
        	        else
                    {
                        GD.Print(collision.GetCollider().ToString());
                    }
                }
            }
        }
        else{
            //If is dead, what do? 
        }
    }
    public void TeleportTo(Marker3D spot){
        Vector3 coordinates = spot.GlobalPosition;
		GD.Print("Teleporting player to " + coordinates.ToString());
		this.GlobalPosition = coordinates;
    }

    public int GetHealth(){
        return health;
    }

    public void Damage(int dmg){
        health -= dmg;
        if(health < 0){
            health = 0;
            OnDeath();
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

    public void Despawn(){
        this.Visible = false;
    }
}