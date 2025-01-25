// TODO character functionality
// Refactoring note:
//      Need to determine the character node organization, as it seems the transforms are getting fucked
//      Due to hidden engine logic
//      Also, for gameplay, need to figure out camera
// Refactoring note:
//      For movement executor, create file for shorthand moves (would allow for customization)
// Refactoring note:
//      Could move file logic to a separate space
// 1) Animation player (With an animation deck)                             // TODO - load in animations from external file
// 2) Move deck - Dictionary with string tokens and function calls          // Currently move aliases, uses executors
// 3) Collisions - Deal with collisions of the character                    // Doneish, relies on abstract methods         - Find a way to disable collisions with children
// 4) Movement - Deal with character movement and movement calculations     // Doneish, Fixed input reliance. Rotation is still offset, seems like origin might need to be moved, but movement works     - Need to add shorthand check for the movement executor 
// 5) Rescale                                                               // Buggy as hell, needs to be done either via animations or some better way, but should work with teleporting and markers - implement    
// 6) Spawn                                                                 // Testing, relies on abstract methods
// 7) Constrain                                                             // Constrain during pause or status effectsTODO - add
// 8) Add core marker                                                       // Add a marker to character layout the represents their core - add
// 9) Activity tracker                                                      // Track cooldowns, active moves and animations, might just need to use a status struct - add
// 10) For collision object, see if it possible to use deeper children instead (due to transforms) or if parent nodes should be made

using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Numerics;

public class MovementExecutor{
    
    public float Speed {set; get;} = 5.0f;
    public float JumpVelocity {set; get;} = 0.0f;
    public float FlySpeed {set; get;} = 0.0f;
    public float JumpImpulse { get; set; } = 20;
    public Godot.Vector3 velocity;
    public Transform3D transform3D;
    public float FallAcceleration { get; set; } = 75;
    private double last_delta;
    public bool gravity = true;
    public bool IsOnFloor { get; set; } = true;
    public bool CanFly { get; set; } = false;

    public Dictionary<string,string> shorthands;
    private Godot.Vector3 move_vector;
    private int counter;
    private int cooldown;
    private bool at_rest;

    

    public MovementExecutor(){
        last_delta = 0.0;
        shorthands = new Dictionary<string,string>();
        float delta_x = 0.15f;
        float delta_rot = 0.15f;
        counter = 0;
        cooldown = 1;
        at_rest = false;
        move_vector = Godot.Vector3.Zero;
        shorthands.Add("DeltaFWD", $"Move/Z/{Speed*delta_x}");
        shorthands.Add("DeltaBWD", $"Move/Z/{-Speed*delta_x}");
        shorthands.Add("DeltaTurnRight", $"Rotate/{-delta_rot}");
        shorthands.Add("DeltaTurnLeft", $"Rotate/{delta_rot}");
        shorthands.Add("SmallJump", $"Jump/{JumpImpulse}");
        shorthands.Add("BigJump", $"Jump/{5*JumpImpulse}");
        shorthands.Add("Fly", $"Fly/{FlySpeed}");
        shorthands.Add("Rest", $"Move/X/0/Z/0");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
    }

    public void Init(Godot.Vector3 v, Transform3D t3, float s = 0.0f, float jv = 0.0f, float fs = 0.0f, float ji = 20.0f, float fa = 75.0f, bool g = true, bool iof = true, bool cf = false){
        Speed = s;
        JumpVelocity = jv;
        FlySpeed = fs;
        JumpImpulse = ji;
        velocity = v;
        transform3D = t3;
        FallAcceleration = fa;
        gravity = g;
        IsOnFloor= iof;
        CanFly= cf;
    }

    public void SyncWithCharacter(Godot.Vector3 v, Transform3D t3, bool iof){
        //velocity = v;
        transform3D = t3;
        IsOnFloor= iof;
    }

    public void MoveHorizontal(Godot.Vector3 delta_mov){
        GD.Print($"Moving horizontal {delta_mov}");
        Transform3D transform = transform3D;
        var basis = transform.Basis;
		var scale_vec = new Godot.Vector3(1, 0, -1);
        var Y = velocity.Y;
		velocity = basis * ( scale_vec * delta_mov * Speed); //scale_vec *s
        velocity.Y = Y;
    }

    public void MoveVertical(Godot.Vector3 delta_mov){
        GD.Print($"Moving vertical {delta_mov}");
        Transform3D transform = transform3D;
        var basis = transform.Basis;
		var scale_vec = new Godot.Vector3(0, 1, 0);
		var velocity_local = ( scale_vec * delta_mov); //scale_vec *s
        velocity.Y = velocity_local.Y;
        GD.Print($"Moving vertical {velocity}");
    }

    public void Jump(){
        if (IsOnFloor){
            velocity.Y = JumpImpulse;
        }
    }

    public void Rotate(float delta){
        // TODO change to look at 
		Transform3D transform = transform3D;
		Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
		transform = transform.RotatedLocal(axis, delta);
		transform3D = transform;
	}

    public void Fly(float delta){ 
		Transform3D transform = transform3D;
		Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
		transform = transform.TranslatedLocal(new Godot.Vector3(0,0,delta));
		transform3D = transform;
	}

    public void ExecuteOrder(string order){
        if(!order.Contains("/")){
            foreach(var shorthand in shorthands.Keys){
                if (shorthand == order){
                    ExecuteOrder(shorthands[shorthand]);
                    return;
                }
            }
        }
        at_rest = false;
        string[] parts = order.Split(new[] { "/" }, StringSplitOptions.None);
        switch(parts[0]){
            
            case "Move":
                cooldown = 1; //FPS*seconds
                bool vertical = false, horizontal = false;
                bool x = false;
                bool y = false;
                bool z = false; 
                for(int i = 1; i< parts.Length-1; i+=2){
                    GD.Print($"Parts {parts[i]}");
                    
                    if(parts[i] == "X"){
                        GD.Print($"Parts {parts[i+1]}");
                        move_vector.X = float.Parse(parts[i+1]);
                        horizontal = true;
                        x = true;
                    }
                    if(parts[i] == "Y"){
                        move_vector.Y = float.Parse(parts[i+1]);
                        vertical = true;
                        y = true;
                    }
                    if(parts[i] == "Z"){
                        move_vector.Z = float.Parse(parts[i+1]);
                        horizontal = true;
                        z = true;
                    }
                    
                }
                if(!x) move_vector.X = 0;
                if(!y){
                        if(IsOnFloor){
                            GD.Print("Is on floor");
                            move_vector.Y = 0;
                        }else{
                            GD.Print($"falling {last_delta} {FallAcceleration}");
                            move_vector.Y -= FallAcceleration * (float)last_delta;
                            GD.Print($"falling {move_vector.Y}");
                        }
                }
                if(!z) move_vector.Z = 0;
                if (vertical || gravity){
                    MoveVertical(move_vector);
                }
                if (horizontal){
                    MoveHorizontal(move_vector);
                }
            break;
            case "Rotate":
                
                //for(int i = 1; i< parts.Length-1; i+=2){
                //    if(parts[i] == "X"){
                //        move_vector.X = float.Parse(parts[i+1]);
                //        horizontal = true;
                //    }
                //    if(parts[i] == "Y"){
                //        move_vector.Y = float.Parse(parts[i+1]);
                //        vertical = true;
                //    }
                //    if(parts[i] == "Z"){
                //        move_vector.Z = float.Parse(parts[i+1]);
                //        horizontal = true;
                //    }
                cooldown = 1; //FPS*seconds
                if (parts.Length > 1){
                    float delta_rot = 0.0f;
                    if (parts.Length == 2){
                        delta_rot = float.Parse(parts[1]); 
                    }else if(parts.Length == 3){
                        delta_rot = float.Parse(parts[2]); 
                    }else{
                        GD.Print("Problem with rotation format");
                    }
                    Rotate(delta_rot);
                }
            break;
            case "Jump":
                cooldown = 1; //FPS*seconds
                if (parts.Length > 1){ //Custom impulse
                    if (parts.Length == 2){
                        JumpImpulse = float.Parse(parts[1]); 
                    }else if(parts.Length == 3){
                        JumpImpulse = float.Parse(parts[2]); 
                    }else{
                        GD.Print("Problem with jump format");
                    }
                }    
                Jump();
            break;
            case "Fly":
                cooldown = 1; //FPS*seconds
                if (parts.Length > 1){ //Custom impulse
                    if (parts.Length == 2){
                        FlySpeed = float.Parse(parts[1]); 
                    }else if(parts.Length == 3){
                        FlySpeed = float.Parse(parts[2]); 
                    }else{
                        GD.Print("Problem with jump format");
                    }
                }
                Fly(FlySpeed);
            break;
            default:
            
            GD.Print($"Action not supported {parts[0]}");
            break;
        }
    }

    public void OnUpdate(double delta, Godot.Vector3 v, Transform3D t3, bool iof, bool input_happened){
        last_delta = delta;
        // Default move set
        // 1) Create a cooldown that resets velocity on no input
        if(!input_happened && !at_rest){
            counter++;
            if(counter > cooldown){
                // Do rest
                ExecuteOrder("Rest");
                counter = 0;
                at_rest = true;
            }
        }
        SyncWithCharacter(v, t3, iof);
        //Move gravity
    }

    public Godot.Vector3 GetVelocity(){
        return velocity;
    }

    public Transform3D GetTransform3D(){
        return transform3D;
    }

    public void SetVelocity(Godot.Vector3 velocity){
        this.velocity = velocity;
    }

    public void SetTransform3D(Transform3D transform3D){
        this.transform3D = transform3D;
    }
}

public abstract partial class Character : CharacterBody3D
{
    [Export]
	public float Speed = 5.0f;

	[Export]
    public float JumpVelocity = 4.5f;

    [Export]
    public float DefaultScaleFactor { set; get; } = 1f;

    [Export]
    public string scene_path { set; get; } = "res://NonPlayerGeneric.tscn";

    [Export]
    public string motion_mode { set; get; } = "Slide"; // TODO replace with a list of values

    [Export]
    public float FlySpeed {set; get;} = 0.0f;
    
    [Export]
    public float JumpImpulse { get; set; } = 20;

    [Export]
    public float FallAcceleration { get; set; } = 75;

    // Executors
    private AnimationPlayer animationPlayer;
    private MovementExecutor movementExecutor;
	Dictionary<string, Animation> animation_deck;
	Dictionary<string, Move> move_deck;
    List<string> move_keys;
    Dictionary<string, string> move_deck_aliases;
    public Dictionary<int, string> move_deck_ids;
    private double last_delta;
    private bool is_ready = false;
    private bool no_input = true;
    private float scale = 1.0f;
    private bool scale_changed = false;

    public override void _Ready()
	{
        base._Ready();
        animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if(animationPlayer==null){
            animationPlayer = CreateAnimationPlayer(this);
        }
        movementExecutor = new MovementExecutor();
        movementExecutor.Init(Velocity, Transform, Speed, JumpVelocity, FlySpeed, JumpImpulse, FallAcceleration);
        // Load animations
        // animation_deck = LoadAnimations("file_name");
        move_keys = LoadMoveKeys();
        move_deck_aliases = LoadAliases();
        move_deck_ids = new Dictionary<int, string>();
        LoadIds(move_deck_aliases);
        last_delta = 0.0;
        // Hardcoded deck should be replaced, but may exist in End chars until saving and loading is done
        //move_deck_aliases["LACTION"] = "LActionExtend";
        //move_deck_aliases["RACTION"] = "RActionSpin";
        //move_deck_aliases["ABILITY"] = "AbilityExpand";
        //move_deck_aliases["ULT"] = "Bloodcleave";
        // How it should be done
        // move_deck = LoadMoveDeck("file_name");
        is_ready = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        last_delta = delta;
        if(is_ready) OnUpdate(delta);
    }

    public virtual void OnUpdate(double delta){
		//string current_animation = animationPlayer.CurrentAnimation;
		//if(!move_deck_aliases.ContainsValue(current_animation)){ //change to default named animation
		//	Random rand = new Random();
		//	animationPlayer.Play(move_deck_aliases[move_deck_ids[rand.Next(0, move_deck_ids.Keys.Count)]]);
		//}
        //if(no_input && !idle){
        //    movementExecutor.Rest();
        //}
        Transform = movementExecutor.GetTransform3D();
        //Transform.ScaledLocal(scale);
        if(scale_changed) Rescale(scale);
        Velocity = movementExecutor.GetVelocity();
        DoMoveAndCollide();
        movementExecutor.OnUpdate(delta, Velocity, Transform, IsOnFloor(), !no_input);
        no_input = true;
        //int id = GetCurrentMove();
        //Move(id);
        //DoMoveAndCollide();
		//observation = get_observation(world_signal); //??
		//UpdateState(observation);
		//action = get_action();
		//DoAction(action);
	}

    public static Character Spawn(Node3D parent, Marker3D spot, string path){
		// Spawn instance at location
        var character_scene = ResourceLoader.Load<PackedScene>(path);       
        return Spawn(parent, spot, character_scene);                                      /* And add as child to */
	}

    public static Character Spawn(Node3D parent, Marker3D spot, PackedScene packedScene){
		// Spawn instance at location      
        Character character = packedScene.Instantiate() as Character;   /* Instantiate */
        character.TeleportTo(spot);  
        parent.AddChild(character);
                                       /* and teleport to spawn point */
        return character;                                         /* And add as child to */
	}

    public virtual Character Spawn(Node3D parent, Marker3D spot){
		return Character.Spawn(parent, spot, GetScenePath());
	}

    public virtual Character Spawn(Node3D parent){
		return Character.Spawn(parent, GetSpawn(), GetScenePath());
	}

    public void Despawn(){
		//Delete instance
	}

    public void TeleportTo(Marker3D spot){
        // Issue with teleporting after being added as child of the arena
        // Look into it
        if(spot == null) return; //Fail silently. Later may add a cancelled teleport effect.
        Godot.Vector3 coordinates = spot.GlobalPosition;
        string character_name = GetName();
		GD.Print($"Teleporting character {character_name} to " + coordinates.ToString());
		this.GlobalPosition = coordinates;
    }

    public void Move(int move_id){
        Move(move_deck_aliases[move_deck_ids[move_id]]);
    }

    public void Move(string move_alias){
        // Split string into two parts: Executor string and Order string
        // Executor string gets turned to enum
        // Order string is passed as the parameter to the execution function
        // Executor types (Actuators)
        // AnimationPlayer -- String: animation name in Animation player
        // MoveAndSlider -- String: Vectors to String
        // MoveAndCollider -- String: Vectors to String
        // SignalEmitter -- String: signal name in Signal dictionary
        // EffectApplicator -- String: Effect to apply, value in parenthesis
        no_input = false;
        if(!move_deck_aliases.ContainsKey(move_alias)){
            GD.Print("Key not in dict " + move_alias);
            return;
        }
        int split = move_deck_aliases[move_alias].Find("/");
        if(split == -1){
            GD.PrintErr("Bad move token " + move_alias);
            return;
        }
        string executor = move_deck_aliases[move_alias].Substring(0, split);
        string order = move_deck_aliases[move_alias].Substring(split+1);
        switch(executor){
            case "AnimationPlayer":
                animationPlayer.Play(order);
            break;
            case "TeleportTo":
                TeleportTo(FindMarker(order));
            break;
            case "Movement":
                movementExecutor.ExecuteOrder(order);
                
            break;
            default:
                Execute(executor, order);
            break;
        }
    }

    public string GetName(){
        return "character";
    }

    public void AddAnimationsToMoveKeys(){
        animationPlayer.GetAnimationList();
        foreach(var animation in animationPlayer.GetAnimationList()){
            move_keys.Add($"AnimationPlayer/{animation}");
        }
    }

    public List<string> GetMoveKeys(){
        //Currently using move keys from a separate list
        return move_keys;
    }

    public AnimationPlayer CreateAnimationPlayer(Node node_parent){
        var animation_player = new AnimationPlayer();
        node_parent.AddChild(animation_player);
        // If necessary, can set settings and import animations from node_parent
        return animation_player;
    }
    
    public void LoadIds(Dictionary<string, string> move_deck_aliases){
        int counter = 0;
        foreach(var item in move_deck_aliases.Keys){
            move_deck_ids[counter++] = item;
        }
    }

    public void SetScale(float factor){
        scale = factor;
        scale_changed = true;
    }

    public void Rescale(float factor){
        //TODO implement
        scale_changed = false;
		Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
		Transform = Transform.Translated(axis*scale*3);
		
        var scale_vector = new Godot.Vector3(1,1,1);
        Transform = Transform.ScaledLocal(scale_vector*scale);
    }

    public void DoMoveAndCollide()
    {
        switch(motion_mode){
            case "Slide":
                MoveAndSlide();
                SlideCollision();
            break;
            case "Collide":
                var collision = MoveAndCollide(Velocity*(float)last_delta);
                CollideCollision(collision);
            break;
            default:
                GD.Print("Do not handle collisions");
            break;
        }
    }

    public void SlideCollision(){

        for (int i = 0; i < GetSlideCollisionCount(); i++){
            var collision = GetSlideCollision(i);
            
            CollideCollision(collision);
		}
    }

    public void CollideCollision(KinematicCollision3D collision){
        if (collision != null)
        {
            if(collision.GetCollider() is StaticBody3D)
            {
                OnCollidedWithEnvironment(collision.GetCollider() as StaticBody3D);
            }

            if(collision.GetCollider() is Weapon)
            {
                OnCollidedWithObject(collision.GetCollider() as RigidBody3D);
            }

            if(collision.GetCollider() is Weapon)
            {
                OnCollidedWithCharacter(collision.GetCollider() as CharacterBody3D);
            }

            if(collision.GetCollider() is Weapon)
            {
                OnCollidedWithWeapon(collision.GetCollider() as Weapon);
            }
            
            if(collision.GetCollider() is Area3D)
            {
                OnAreaEntered(collision.GetCollider() as Area3D);
            }
		}
    }



    public abstract int GetCurrentMove();
    public abstract Marker3D FindMarker(string node_name);
    public abstract void Execute(string executor, string order);
    public abstract List<string> LoadMoveKeys();
    public abstract Dictionary<string, string> LoadAliases();
    public abstract Marker3D GetSpawn();
    public abstract string GetScenePath();
    

    //Collision handling
    // On area interactions
    public abstract void OnAreaEntered(Area3D area);
    //public abstract void OnTerrainAreaEntered(Terrain area);
    //public abstract void OnTerrainAreaExited(Terrain area);
    //public abstract void OnHazardAreaEntered(Hazard area); //Hazards
    //public abstract void OnHazardAreaExited(Hazard area);
    public abstract void OnAreaExited(Area3D area);
    // On collisions with Environment - Static body
    public abstract void OnCollidedWithEnvironment(StaticBody3D collider);
    // On collisions with objects - Rigid body
    public abstract void OnCollidedWithObject(RigidBody3D collider);
    // On collisions with characters
    public abstract void OnCollidedWithCharacter(CharacterBody3D collider);
    // On collisions with weapons
    public abstract void OnCollidedWithWeapon(Weapon collider);
    
}