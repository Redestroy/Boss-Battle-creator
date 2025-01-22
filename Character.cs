// TODO character functionality
// 1) Animation player (With an animation deck)                             // TODO - load in animations from external file
// 2) Move deck - Dictionary with string tokens and function calls          // Currently move aliases, uses executors
// 3) Collisions - Deal with collisions of the character                    // Kinda done, relies on abstract methods         - Find a way to disable collisions with children
// 4) Movement - Deal with character movement and movement calculations     // Testing, needs testing and may need changes    - Need to add shorthand check for the movement executor 
// 5) Rescale                                                               // TODO - implement    
// 6) Spawn                                                                 // Kinda done, relies on abstract methods
// 7) Constrain                                                             // TODO - add
// 8) Add core marker                                                       // TODO - add
// 9) Activity tracker                                                      // TODO - add


using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

public class MovementExecutor{
    
    public float Speed {set; get;} = 0.0f;
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

    public MovementExecutor(){
        last_delta = 0.0;
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
        velocity = v;
        transform3D = t3;
        IsOnFloor= iof;
    }

    public void MoveHorizontal(Godot.Vector3 delta_mov){
        Transform3D transform = transform3D;
		var base_pos = transform.Basis;
		var scale_vec = new Godot.Vector3(1, 0, -1);
		velocity = base_pos * ( scale_vec * delta_mov * Speed);
    }

    public void MoveVertical(double delta){
        velocity.Y -= FallAcceleration * (float)delta;
    }

    public void Jump(){
        //every argument is a key value pair in form of key: value
		//type is inferred, if needed, it can be added after underscore
		// make functions to handle parsing, but should be as simple as possible
		//Jump calculation
		//double gravity = 0.0; //Used for ballistic calculation
		//Transform3D transform = transform3D;
		// integrate the jump
		//if(!is_moving){
		//	is_moving = true;
		//  target_velocity.y += jump_impulse;
		//}
		//transform3D = transform;
        if (IsOnFloor){
            velocity.Y = JumpImpulse;
        }
    }

    public void Rotate(float delta){
        // TODO change to look at 
		Transform3D transform = transform3D;
		Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
		transform.Basis = transform.Basis.Rotated(axis, delta);
		transform3D = transform;
	}

    public void Fly(float delta){
        // TODO change to look at 
		Transform3D transform = transform3D;
		Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
		transform = transform.TranslatedLocal(new Godot.Vector3(0,0,delta));
		transform3D = transform;
	}

    public void ExecuteOrder(string order){
        string[] parts = order.Split(new[] { "/" }, StringSplitOptions.None);
        switch(parts[0]){
            case "Move":
                Godot.Vector3 move_vector = new Godot.Vector3();
                bool vertical = false, horizontal = false; 
                for(int i = 1; i< parts.Length-1; i+=2){
                    if(parts[i] == "X"){
                        move_vector.X = float.Parse(parts[i+1]);
                        horizontal = true;
                    }
                    if(parts[i] == "Y"){
                        move_vector.Y = float.Parse(parts[i+1]);
                        vertical = true;
                    }
                    if(parts[i] == "Z"){
                        move_vector.Z = float.Parse(parts[i+1]);
                        horizontal = true;
                    }
                }
                if (vertical || gravity){
                    MoveVertical(last_delta);
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

    public void OnUpdate(double delta, Godot.Vector3 v, Transform3D t3, bool iof){
        last_delta = delta;
        SyncWithCharacter(v, t3, iof);
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
        //move_keys = new List<string>();
        move_keys = LoadMoveKeys();
        move_deck_aliases = new Dictionary<string, string>();
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
        movementExecutor.OnUpdate(delta, Velocity, Transform, IsOnFloor());
        //int id = GetCurrentMove();
        //Move(id);
        //DoMoveAndCollide();
		//observation = get_observation(world_signal); //??
		//UpdateState(observation);
		//action = get_action();
		//DoAction(action);
	}

    public static void Spawn(Marker3D spot, string path){
		// Spawn instance at location
        // Instantiate
        var character = ResourceLoader.Load<Character>(path);
        // Teleport to
        character.TeleportTo(character.GetSpawn());
	}

    public void Despawn(){
		//Delete instance
	}

    public void TeleportTo(Marker3D spot){
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
                Transform = movementExecutor.GetTransform3D();
                Velocity = movementExecutor.GetVelocity();
                DoMoveAndCollide();
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

    public void Rescale(float factor){
        //TODO implement
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
    public abstract void Spawn(Marker3D spot);

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