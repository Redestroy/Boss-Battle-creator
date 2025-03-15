// TODO character functionality
// Refactoring note:
//      Need to determine the character node organization, as it seems the transforms are getting fucked
//      Due to hidden engine logic
//      Also, for gameplay, need to figure out camera
// Refactoring note:
//      For movement executor, create file for shorthand moves (would allow for customization)
// Refactoring note:
//      Could move file logic to a separate space
// 1) Animation player (With an animation deck)                             // Doneish, Testing
// 2) Move deck - Dictionary with string tokens and function calls          // Currently move aliases, uses executors
// 3) Collisions - Deal with collisions of the character                    // Doneish, relies on abstract methods         - Find a way to disable collisions with children
// 4) Movement - Deal with character movement and movement calculations     // Doneish, Fixed input reliance. Rotation is still offset, seems like origin might need to be moved, but movement works     - Need to add shorthand check for the movement executor 
// 5) Rescale                                                               // Buggy as hell, needs to be done either via animations or some better way, but should work with teleporting and markers - implement    
// 6) Spawn                                                                 // Done
// 7) Constrain                                                             // Constrain during pause or status effects TODO - add
// 8) Add core marker                                                       // Done
// 10) For collision object - possible to use other collision objects, but it comes with custom code.
// 11) Problem with teleporting player after adding it as child of arena    // Resolved
//TODO: Create a function to switch around two move keys, related to move deck editor UI
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;

public abstract partial class Character : CharacterBody3D, IArenaObject
{
    [Signal]
	public delegate void SigSlotItemEventHandler(string item_key);
	[Signal]
	public delegate void SigDeSlotItemEventHandler(string item_key);

    [Export]
    public float Speed = 5.0f;

    [Export]
    public float JumpVelocity = 4.5f;

    [Export]
    public float DefaultScaleFactor { set; get; } = 1f;

    [Export]
    public string scene_path { set; get; } = "res://NonPlayerGeneric.tscn";

    [Export]
    public string motion_mode { set; get; } = "Slide";

    [Export]
    public float FlySpeed { set; get; } = 0.0f;

    [Export]
    public float JumpImpulse { get; set; } = 20;

    [Export]
    public float FallAcceleration { get; set; } = 75;

    [Export]
    public string ArenaAlias { get; set; } = "character";

    // Executors
    public AnimationPlayer animationPlayer; // Maybe add methods instead of making public?

    private AnimationExecutor animationExecutor = new AnimationExecutor();
    private MovementExecutor movementExecutor = new MovementExecutor();
    Dictionary<string, Animation> animation_deck;
    
    public Dictionary<string, Equippable> item_instances = new Dictionary<string, Equippable>();
    public Inventory inventory = new Inventory();
    //public Dictionary<string, Equippable> equipment;
    //public Dictionary<string, Equippable> move_attachment_to_equipment;

    Dictionary<string, MoveInfo> move_deck = new Dictionary<string, MoveInfo>();
    List<string> move_keys;
    List<string> default_moves = new List<string>{
                                                        "Movement/DeltaFWD",
                                                        "Movement/DeltaBWD",
                                                        "Movement/DeltaTurnLeft",
                                                        "Movement/DeltaTurnRight",
                                                        };
    Dictionary<string, string> move_deck_aliases;
    public Dictionary<int, string> move_deck_ids = new Dictionary<int, string>();
    private Dictionary<string, bool> move_mask = new Dictionary<string, bool>();

    // Reference to access items by index

    public Dictionary<int, Tuple<string, PackedScene>> inventory_ref_ = new Dictionary<int, Tuple<string, PackedScene>>();
    public Dictionary<int, Tuple<string, PackedScene>> move_inventory_ref_ = new Dictionary<int, Tuple<string, PackedScene>>();

    protected Weapon weapon;
    //private Item item;
    //private Body body; 
    private double last_delta;
    private bool is_ready = false;
    private bool no_input = true;
    private float scale = 1.0f;
    private bool scale_changed = false;
    private bool teleport = false;

    public override void _Ready()
    {
        
        base._Ready();
        animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animationPlayer == null)
        {
            animationPlayer = CreateAnimationPlayer(this);
        }
        movementExecutor.Init(Velocity, Transform, Speed, JumpVelocity, FlySpeed, JumpImpulse, FallAcceleration);
        animationExecutor.Init();
        
        // Load animations
        // animation_deck = LoadAnimations("file_name");
        
        //Inventory
        this.AddChild(inventory);
        LoadInventory();
        var global_inv = (GlobalInventory) GetNode("/root/GlobalInventory");
        global_inv.CopyInventory(this.Name, inventory);
        move_keys = LoadMoveKeys();
        move_deck_aliases = LoadAliases();
        LoadIds(move_deck_aliases);
        last_delta = 0.0;
        animationExecutor.GetPlayersFromCharacter(this);
        // Hardcoded deck should be replaced, but may exist in End chars until saving and loading is done
        // Slime example
        // move_deck_aliases["QuickAttack"] = "AnimationPlayer/Idle";
        // move_deck_aliases["HeavyAttack"] = "AnimationPlayer/Recoil";
        // move_deck_aliases["AbilityTrigger"] = "AnimationPlayer/Shrug";
        // move_deck_aliases["UltimateMove"] = "AnimationPlayer/BigJump";
        // Shade example
        // move_deck_aliases["QuickAttack"] = "AnimationPlayer/LActionExtend";
        // move_deck_aliases["HeavyAttack"] = "AnimationPlayer/RActionSpin";
        // move_deck_aliases["AbilityTrigger"] = "AnimationPlayer/AbilityExpand";
        // move_deck_aliases["UltimateMove"] = "AnimationPlayer/Bloodcleave";
        // How it should be done
        // move_deck = LoadMoveDeck("file_name");
        is_ready = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        last_delta = delta;

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (is_ready) OnUpdate(delta);
    }

    public virtual void OnUpdate(double delta)
    {
        //string current_animation = animationPlayer.CurrentAnimation;
        //if(!move_deck_aliases.ContainsValue(current_animation)){ //change to default named animation
        //	Random rand = new Random();
        //	animationPlayer.Play(move_deck_aliases[move_deck_ids[rand.Next(0, move_deck_ids.Keys.Count)]]);
        //}
        //if(no_input && !idle){
        //    movementExecutor.Rest();
        //}
        if (!teleport) Transform = movementExecutor.GetTransform3D();

        //Transform.ScaledLocal(scale);
        if (scale_changed) Rescale(scale);
        Velocity = movementExecutor.GetVelocity();
        DoMoveAndCollide();
        movementExecutor.OnUpdate(delta, Velocity, Transform, IsOnFloor(), !no_input, teleport);
        no_input = true;
        if (teleport)
        {
            teleport = false;
            //return;
        }
        //int id = GetCurrentMove();
        //Move(id);
        //DoMoveAndCollide();
        //observation = get_observation(world_signal); //??
        //UpdateState(observation);
        //action = get_action();
        //DoAction(action);
    }

    public static Character Spawn(Node3D parent, Marker3D spot, string path)
    {
        // Spawn instance at location
        var character_scene = ResourceLoader.Load<PackedScene>(path);
        return Spawn(parent, spot, character_scene);                                      /* And add as child to */
    }

    public static Character Spawn(Node3D parent, Marker3D spot, PackedScene packedScene)
    {
        // Spawn instance at location      
        Character character = packedScene.Instantiate() as Character;   /* Instantiate */
        GD.Print($"Spawning character {character}");
        character.TeleportTo(spot);
        parent.AddChild(character);                                 /* and teleport to spawn point */
        GD.Print($"Done Spawning character {character}");
        return character;                                         /* And add as child to */
    }

    public virtual Character Spawn(Node3D parent, Marker3D spot)
    {
        return Character.Spawn(parent, spot, GetScenePath());
    }

    public virtual Character Spawn(Node3D parent)
    {
        return Character.Spawn(parent, GetSpawn(), GetScenePath());
    }

    public void Despawn()
    {
        this.QueueFree();
    }

    public void TeleportTo(Marker3D spot)
    {
        // Issue with teleporting after being added as child of the arena
        // Look into it
        if (spot == null)
        {
            GD.Print("No spot given");
            return;
        } //Fail silently. Later may add a cancelled teleport effect.
        //Godot.Transform3D coordinates = spot.GlobalTransform;
        teleport = true;
        Godot.Vector3 coordinates = spot.GlobalPosition;
        string character_name = GetName();
        GD.Print($"Teleporting character {character_name} to " + coordinates.ToString());
        if (!this.IsInsideTree())
        {
            //this.GlobalTransform = coordinates;
            this.Position = coordinates;
        }
        else
        {
            this.LookAtFromPosition(coordinates, this.GetParent<Node3D>().GlobalTransform.Origin);
            this.ForceUpdateTransform();
        }
        //
    }

    public bool IsAvailableMove(int id)
    {
        return move_mask.GetValueOrDefault(move_deck_ids.GetValueOrDefault(id, ""), false);
    }

    public bool IsAvailableMove(string key)
    {
        return move_mask.GetValueOrDefault(key, false);
    }

    public void Move(int move_id)
    {
        if (IsAvailableMove(move_id)) Move(move_deck_aliases[move_deck_ids[move_id]]);
        else
        {
            GD.Print("Move disabled or not in deck");
        }
    }

    public void Move(string move_alias)
    {
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
        if (!move_deck_aliases.ContainsKey(move_alias))
        {
            GD.Print("Key not in dict " + move_alias);
            return;
        }
        int split = move_deck_aliases[move_alias].Find("/");
        if (split == -1)
        {
            GD.PrintErr("Bad move token " + move_alias);
            return;
        }
        if (!IsAvailableMove(move_alias))
        {
            GD.Print($"Move disabled {move_alias}");
            return;
        }
        string executor = move_deck_aliases[move_alias].Substring(0, split);
        string order = move_deck_aliases[move_alias].Substring(split + 1);
        switch (executor)
        {
            case "Animation":
                animationExecutor.ExecuteOrder(order);
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

    public string GetAlias()
    {
        return ArenaAlias;
    }

    public void AddAnimationsToMoveKeys(List<string> move_keys)
    {
        animationPlayer.GetAnimationList();
        foreach (var animation in animationPlayer.GetAnimationList())
        {
            GD.Print();
            move_keys.Add($"AnimationPlayer/{animation}");
            GD.Print(move_keys.Last());
        }
    }

    public virtual Inventory LoadInventory(){
        // Load inventory from database if entry exists, otherwise create an empty inventory
        var global_inv = (GlobalInventory) GetNode("/root/GlobalInventory");
        var inventory = global_inv.LoadInventory(this.Name) ?? new Inventory();
        LoadInvRefFromDict(inventory.GetItemRef());
        LoadMoveRefFromDict(inventory.GetMoveRef());
        return inventory;
        // Override for mob, boss and player separately, as there are different ways to load it for each one. 
        // Can make a static loader function to load the inventory from the database based on key 
    }

    public void LoadInvRefFromDict(Dictionary<int, Tuple<string, string>> paths){
        foreach(var item in paths){
            var packed_scene = ResourceLoader.Load<PackedScene>(item.Value.Item2);
            inventory_ref_.Add(item.Key, new Tuple<string, PackedScene>(item.Value.Item1, packed_scene));
        }
    }

    public void LoadMoveRefFromDict(Dictionary<int, Tuple<string, string>> paths){
        foreach(var item in paths){
            var packed_scene = ResourceLoader.Load<PackedScene>(item.Value.Item2);
            move_inventory_ref_.Add(item.Key, new Tuple<string, PackedScene>(item.Value.Item1, packed_scene));
        }
    }

    public List<string> GetMoveKeys()
    {
        //Currently using move keys from a separate list
        return move_keys;
    }

    public AnimationPlayer CreateAnimationPlayer(Node node_parent)
    {
        var animation_player = new AnimationPlayer();
        node_parent.AddChild(animation_player);
        // If necessary, can set settings and import animations from node_parent
        return animation_player;
    }

    public void LoadIds(Dictionary<string, string> move_deck_aliases)
    {
        int counter = 0;
        foreach (var item in move_deck_aliases.Keys)
        {
            move_deck_ids[counter++] = item;
            EnableMove(item);
        }
    }

    public void SetScale(float factor)
    {
        scale = factor;
        scale_changed = true;
    }

    public void Rescale(float factor)
    {
        //TODO implement properly (currently launches into stratosphere)
        scale_changed = false;
        Scale *= factor;
    }

    public void DoMoveAndCollide()
    {
        //GD.Print($"Doing move and Collide {ArenaAlias}");
        switch (motion_mode)
        {
            case "Slide":
                MoveAndSlide();
                SlideCollision();
                break;
            case "Collide":
                var collision = MoveAndCollide(Velocity * (float)last_delta);
                CollideCollision(collision);
                break;
            default:
                GD.Print("Do not handle collisions");
                break;
        }
    }

    public void SlideCollision()
    {

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);

            CollideCollision(collision);
        }
    }

    public void CollideCollision(KinematicCollision3D collision)
    {
        if (collision != null)
        {
            if (collision.GetCollider() is StaticBody3D)
            {
                OnCollidedWithEnvironment(collision.GetCollider() as StaticBody3D);
            }

            if (collision.GetCollider() is RigidBody3D)
            {
                OnCollidedWithObject(collision.GetCollider() as RigidBody3D);
            }

            if (collision.GetCollider() is CharacterBody3D)
            {
                //GD.Print($"On collided with another character {ArenaAlias}");
                OnCollidedWithCharacter(collision.GetCollider() as CharacterBody3D);
            }

            if (collision.GetCollider() is Player)
            {
                //GD.Print($"On collided with another player {ArenaAlias}");
                OnCollidedWithCharacter(collision.GetCollider() as CharacterBody3D);
            }

            if (collision.GetCollider() is Weapon)
            {
                OnCollidedWithWeapon(collision.GetCollider() as Weapon);
            }

            if (collision.GetCollider() is Area3D)
            {
                OnAreaEntered(collision.GetCollider() as Area3D);
            }
        }
    }

    public Marker3D GetSelfPositionTargetMarket()
    {
        return FindMarker("Self");
    }

    public static Node GetRoot(Node myself)
    {
        Node root = myself.GetParent();
        //GD.Print($"Looking for arena1 {root}");
        if (root == null)
        {
            return myself;
        }
        Node temp = root;
        //GD.Print($"Looking for arena2 {temp}");
        while (temp != null)
        {
            //GD.Print($"Looking for arena3 {temp}");
            root = temp;
            temp = temp.GetParent();
        }
        return root;
    }


    public abstract int GetCurrentMove();
    public abstract Marker3D FindMarker(string node_name);
    public abstract void Execute(string executor, string order);
    public abstract List<string> LoadMoveKeys();
    public abstract Dictionary<string, string> LoadAliases();
    public abstract Marker3D GetSpawn();
    public abstract string GetScenePath();

    // Move deck
    public void AddToMoveDeck(MoveInfo move){
        move_deck.TryAdd<string, MoveInfo>(move.Alias_label, move);
        move_deck_aliases.TryAdd<string,string>(move.Alias_label, move.move_order);
        EnableMove(move.Alias_label);
    }

    public void RemoveFromMoveDeck(MoveInfo move){
        if(move_deck.ContainsKey(move.Alias_label)){
            move_deck.Remove(move.Alias_label);
            move_deck_aliases.Remove(move.Alias_label);
            DisableMove(move.Alias_label);
        }
    }

    public void RefreshMoveDeck(){
        ClearCardMoves();
        foreach(var item in inventory.equipment){
            foreach(var move in item.Value.slots){
                animationExecutor.AddBundleFromMove(move);
                AddToMoveDeck(move);
            }
        }
    }

    public void ClearCardMoves(){
        for(int i = 0; i<move_deck_aliases.Count; i++){
            string move_alias = move_deck_aliases.ElementAt(i).Key;
            if(!default_moves.Contains(move_deck_aliases[move_alias])){
                if(move_deck.TryGetValue(move_alias, out MoveInfo move))
                    RemoveFromMoveDeck(move);
            }
        }
    }

    // Item functions for slots
    // General
    public PackedScene LoadItemScene(int idx){
        return inventory_ref_[idx].Item2;
    }

    public PackedScene LoadMoveScene(int idx){
        return move_inventory_ref_[idx].Item2;
    }

    public PackedScene LoadItemScene(string node_path)
    {
        return ResourceLoader.Load<PackedScene>(node_path); //TODO: replace with dict reference to invref
    }

    
    public void SlotItem(int idx, string item_string)
    {
        item_instances[item_string] = LoadItemScene(idx).Instantiate() as Equippable;
        SlotEquippable(item_instances[item_string]);
        EmitSignal(SignalName.SigSlotItem, item_string);
    }

    public void ReinstanceEquipment(){
        foreach(var eqquipable in inventory.equipment){
            item_instances[eqquipable.Value.Display_label] = LoadItemScene(eqquipable.Value.ItemPath).Instantiate() as Equippable;
            InstanceEquippable(item_instances[eqquipable.Value.Display_label]);
        }
        //animationExecutor.GetPlayersFromCharacter(this);
    }

    public void InstanceEquippable(Equippable equippable){
        this.SetPhysicsProcess(false);
        uint layer = 7;
        uint mask = 0b00000000_00000000_00000001_10111100;
        equippable.Scale = equippable.Scale*0.1f;
        equippable.SetCollisionsTo(layer, mask);
        equippable.TeleportTo(FindMarker(equippable.GetMarkerTag())); //weapon.Transform = FindMarker("Weapon").GlobalTransform; //Teleport weapon to its spot (check if collisions cause problems)
        GetNode<Node3D>(equippable.GetParentNodeTag()).AddChild(equippable);
        if(equippable is Weapon){
            GD.Print("Equipped weapon");
            weapon = equippable as Weapon;
        }
        animationExecutor.AddAnimationPlayer(equippable.equippableInfo.EquippableType, equippable.animationPlayer);
        RefreshMoveDeck();
        this.SetPhysicsProcess(true);
    }

    public void SlotEquippable(Equippable equippable){
        GD.Print($"Slotting item {equippable}");
        if (equippable.IsEquipped) return; // Might have errors, for switching weapons, hiding might work better
        inventory.Slot(equippable);

        // Separate into a different function
        InstanceEquippable(equippable);
        // Load all moves
        // Separate into a different function

        //var move_deck_equippable = equippable.moveInfos;
        //foreach (var moveInfo in move_deck_equippable)
        //{
        //    move_deck_aliases.Add(moveInfo.Alias_label, $"AnimationPlayer/{moveInfo.MoveDescription}"); //weapon.GetAttackAnimation(move_el.moveInfo.Input_label)
        //    CheckEnabledMoves(moveInfo.Alias_label);
        //}
    }

    public void SlotMove(int idx, string item_string){
        // add move to move deck
        var move = LoadMoveScene(idx).Instantiate() as MoveUi;
        if(inventory.equipment.TryGetValue(move.moveInfo.EquippableType, out EquippableInfo item)){
            item_instances[item.Display_label].AddMove(move);
            animationExecutor.AddBundleFromMove(move.moveInfo);
            inventory.SlotMoveOnEquippable(move.moveInfo, item);
            AddToMoveDeck(move.moveInfo);
        }
    }

    public void DeSlotItem(string item_string)
    {
        DeSlotEquippable(inventory.GetEquippableByName(item_string));
        EmitSignal(SignalName.SigDeSlotItem, item_string);
    }

    public void DeSlotEquippable(EquippableInfo item){
        //GetNode<Node3D>(item.GetParentNodeTag()).RemoveChild(item);
        if(item != null){
            inventory.DeSlot(item);
            if(item_instances[item.Display_label] is Weapon){
                weapon = null;
            }
            animationExecutor.RemoveAnimationPlayer(item.EquippableType);
            RefreshMoveDeck();
            item_instances[item.Display_label].Stow();
            item_instances.Remove(item.Display_label);
        }
    }

    public void DeSlotMove(string item_string){
        // Remove move from weapon
        var move = item_instances[inventory.GetEquippableNameForMoveByAttachment(item_string)].GetMoveByName(item_string);
        animationExecutor.RemoveBundleFromMove(move);
        RemoveFromMoveDeck(move);
        item_instances[inventory.GetEquippableNameForMoveByAttachment(item_string)].RemoveMoveByName(item_string);
        // Remove move from move deck
        inventory.RemoveMoveFromItem(item_string);
    }

    public string GetTypeFromItemString(string item_string){
        var token_list = new List<string>(){
            "Weapon",
            "Item",
            "Body"
        };
        foreach(var token in token_list){
            if(item_string.Find(token)!= -1){
                return token;
            }
        }
        return "no equip";
    }

    public void CheckEnabledMoves(string alias)
    {
        //Check which moves are present in slots (and enable them)
        if (!IsAvailableMove(alias) && move_deck_aliases.ContainsKey(alias)) EnableMove(alias); // Also add a check for the move card unlock
    }

    public void EnableMove(string alias)
    {
        move_mask[alias] = true;
    }

    public void DisableMove(string alias)
    {
        move_mask[alias] = false;
    }

    public void DeSlotWeapon(int idx, string node_path)
    {
        //TODO: move weapon unloading code here?
    }


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