using Godot;

public interface IDamageable{
    // may add critical later
    public void OnDamage(int damage);
    public void OnDeath();
}

public partial class Heart : Area3D{
    
    //TODO add emit death signal
    [Signal]
    public delegate void DamagedEventHandler(int damage);

    [Signal]
    public delegate void DiedEventHandler();

    [Export]
    public int MaxHealth{get; set;} = 10;

    [Export]
    public int Health{get; set;} = 10;

    [Export]
    public bool Alive{get; private set;} = true; // Could change to ENUM
    public IDamageable creature;

    public Heart(IDamageable creature, int health){ // Or could just pass it into constructor
        Health = health;
        this.creature = creature; //GetParent<IDamageable>(); // Replace with find or similar
        Alive = true;
    }

    public void OnDeath(){
        EmitSignal(SignalName.Died);
        Alive = false;
    }

    public void OnDamage(int damage){
        if(Alive){
            Health -= damage;
            if(Health>MaxHealth){
                Health = MaxHealth; //May add overheal later
            }
            EmitSignal(SignalName.Damaged);
            if(Health < 0){
                Health = 0;
                OnDeath();
            }
        }else{
            GD.Print("Beating a dead horse, huh?");
            //May add necromancy here
        }
    }
}