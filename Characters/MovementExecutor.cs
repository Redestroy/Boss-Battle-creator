using Godot;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
public class MovementExecutor
{

    public float Speed { set; get; } = 5.0f;
    public float JumpVelocity { set; get; } = 0.0f;
    public float FlySpeed { set; get; } = 0.0f;
    public float JumpImpulse { get; set; } = 20;
    public Godot.Vector3 velocity;
    public Transform3D transform3D;
    public float FallAcceleration { get; set; } = 75;
    private double last_delta;
    public bool gravity = true;
    public bool IsOnFloor { get; set; } = true;
    public bool CanFly { get; set; } = false;

    public Dictionary<string, string> shorthands;
    private Godot.Vector3 move_vector;
    private int counter;
    private int cooldown;
    private bool at_rest;



    public MovementExecutor()
    {
        last_delta = 0.0;
        shorthands = new Dictionary<string, string>();
        float delta_x = 0.15f;
        float delta_rot = 0.15f;
        counter = 0;
        cooldown = 1;
        at_rest = false;
        move_vector = Godot.Vector3.Zero;
        shorthands.Add("DeltaFWD", $"Move/Z/{Speed * delta_x}");
        shorthands.Add("DeltaBWD", $"Move/Z/{-Speed * delta_x}");
        shorthands.Add("DeltaTurnRight", $"Rotate/{-delta_rot}");
        shorthands.Add("DeltaTurnLeft", $"Rotate/{delta_rot}");
        shorthands.Add("SmallJump", $"Jump/{JumpImpulse}");
        shorthands.Add("BigJump", $"Jump/{5 * JumpImpulse}");
        shorthands.Add("Fly", $"Fly/{FlySpeed}");
        shorthands.Add("Rest", $"Move/X/0/Z/0");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
        //shorthands.Add("Rotate", "");
    }

    public void Init(Godot.Vector3 v, Transform3D t3, float s = 0.0f, float jv = 0.0f, float fs = 0.0f, float ji = 20.0f, float fa = 75.0f, bool g = true, bool iof = true, bool cf = false)
    {
        Speed = s;
        JumpVelocity = jv;
        FlySpeed = fs;
        JumpImpulse = ji;
        velocity = v;
        transform3D = t3;
        FallAcceleration = fa;
        gravity = g;
        IsOnFloor = iof;
        CanFly = cf;
    }

    public void SyncWithCharacter(Godot.Vector3 v, Transform3D t3, bool iof)
    {
        //velocity = v;
        transform3D = t3;
        IsOnFloor = iof;
    }

    public void MoveHorizontal(Godot.Vector3 delta_mov)
    {
        //GD.Print($"Moving horizontal {delta_mov}");
        Transform3D transform = transform3D;
        var basis = transform.Basis;
        var scale_vec = new Godot.Vector3(1, 0, -1);
        var Y = velocity.Y;
        velocity = basis * (scale_vec * delta_mov * Speed); //scale_vec *s
        velocity.Y = Y;
    }

    public void MoveVertical(Godot.Vector3 delta_mov)
    {
        //GD.Print($"Moving vertical {delta_mov}");
        Transform3D transform = transform3D;
        var basis = transform.Basis;
        var scale_vec = new Godot.Vector3(0, 1, 0);
        var velocity_local = (scale_vec * delta_mov); //scale_vec *s
        velocity.Y = velocity_local.Y;
        //GD.Print($"Moving vertical {velocity}");
    }

    public void Jump()
    {
        if (IsOnFloor)
        {
            velocity.Y = JumpImpulse;
        }
    }

    public void Rotate(float delta)
    {
        // TODO change to look at 
        Transform3D transform = transform3D;
        Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
        transform = transform.RotatedLocal(axis, delta);
        transform3D = transform;
    }

    public void Fly(float delta)
    {
        Transform3D transform = transform3D;
        Godot.Vector3 axis = new Godot.Vector3(0, 1, 0);
        transform = transform.TranslatedLocal(new Godot.Vector3(0, 0, delta));
        transform3D = transform;
    }

    public void ExecuteOrder(string order)
    {
        if (!order.Contains("/"))
        {
            foreach (var shorthand in shorthands.Keys)
            {
                if (shorthand == order)
                {
                    ExecuteOrder(shorthands[shorthand]);
                    return;
                }
            }
        }
        at_rest = false;
        string[] parts = order.Split(new[] { "/" }, StringSplitOptions.None);
        switch (parts[0])
        {

            case "Move":
                cooldown = 1; //FPS*seconds
                bool vertical = false, horizontal = false;
                bool x = false;
                bool y = false;
                bool z = false;
                for (int i = 1; i < parts.Length - 1; i += 2)
                {
                    //GD.Print($"Parts {parts[i]}");

                    if (parts[i] == "X")
                    {
                        //GD.Print($"Parts {parts[i+1]}");
                        move_vector.X = float.Parse(parts[i + 1]);
                        horizontal = true;
                        x = true;
                    }
                    if (parts[i] == "Y")
                    {
                        move_vector.Y = float.Parse(parts[i + 1]);
                        vertical = true;
                        y = true;
                    }
                    if (parts[i] == "Z")
                    {
                        move_vector.Z = float.Parse(parts[i + 1]);
                        horizontal = true;
                        z = true;
                    }

                }
                if (!x) move_vector.X = 0;
                if (!y)
                {
                    if (IsOnFloor)
                    {
                        //GD.Print("Is on floor");
                        move_vector.Y = 0;
                    }
                    else
                    {
                        //GD.Print($"falling {last_delta} {FallAcceleration}");
                        move_vector.Y -= FallAcceleration * (float)last_delta;
                        //GD.Print($"falling {move_vector.Y}");
                    }
                }
                if (!z) move_vector.Z = 0;
                if (vertical || gravity)
                {
                    MoveVertical(move_vector);
                }
                if (horizontal)
                {
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
                if (parts.Length > 1)
                {
                    float delta_rot = 0.0f;
                    if (parts.Length == 2)
                    {
                        delta_rot = float.Parse(parts[1]);
                    }
                    else if (parts.Length == 3)
                    {
                        delta_rot = float.Parse(parts[2]);
                    }
                    else
                    {
                        GD.Print("Problem with rotation format");
                    }
                    Rotate(delta_rot);
                }
                break;
            case "Jump":
                cooldown = 1; //FPS*seconds
                if (parts.Length > 1)
                { //Custom impulse
                    if (parts.Length == 2)
                    {
                        JumpImpulse = float.Parse(parts[1]);
                    }
                    else if (parts.Length == 3)
                    {
                        JumpImpulse = float.Parse(parts[2]);
                    }
                    else
                    {
                        GD.Print("Problem with jump format");
                    }
                }
                Jump();
                break;
            case "Fly":
                cooldown = 1; //FPS*seconds
                if (parts.Length > 1)
                { //Custom impulse
                    if (parts.Length == 2)
                    {
                        FlySpeed = float.Parse(parts[1]);
                    }
                    else if (parts.Length == 3)
                    {
                        FlySpeed = float.Parse(parts[2]);
                    }
                    else
                    {
                        GD.Print("Problem with jump format");
                    }
                }
                Fly(FlySpeed);
                break;
            default:

                GD.Print($"Action not supported {parts[0]}");
                OnExecuteFailed(order);
                break;
        }
    }

    public virtual void OnExecuteFailed(string order)
    {

    }

    public void OnUpdate(double delta, Godot.Vector3 v, Transform3D t3, bool iof, bool input_happened, bool teleport)
    {
        last_delta = delta;
        // Default move set
        // 1) Create a cooldown that resets velocity on no input
        if (!input_happened && !at_rest && !teleport)
        {
            counter++;
            if (counter > cooldown)
            {
                // Do rest
                ExecuteOrder("Rest");
                counter = 0;
                at_rest = true;
            }
        }
        SyncWithCharacter(v, t3, iof);
    }

    public Godot.Vector3 GetVelocity()
    {
        return velocity;
    }

    public Transform3D GetTransform3D()
    {
        return transform3D;
    }

    public void SetVelocity(Godot.Vector3 velocity)
    {
        this.velocity = velocity;
    }

    public void SetTransform3D(Transform3D transform3D)
    {
        this.transform3D = transform3D;
    }
}