using Godot;
using System;

public partial class PlayerGeneric : Player
{

	[Export]
	public const double JumpVelocity = 4.5f;
	
	[Export]
	public float Speed { get; set; } = 1.00f;
	// The downward acceleration when in the air, in meters per second squared.
	[Export]
	public int FallAcceleration { get; set; } = 75;

	private Vector3 _targetVelocity = Vector3.Zero;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Input(InputEvent @event)
    {
        GD.Print(@event.AsText());
    }

	public override void _PhysicsProcess(double delta)
	{
		float delta_rot = 0;
		float delta_mov = 0;
		if (Input.IsActionPressed("playerMoveRight"))
		{
			delta_rot -= 1.0f;
			//GD.Print("PRight");
		}
		if (Input.IsActionPressed("playerMoveLeft"))
		{
			delta_rot += 1.0f;
			//GD.Print("PLeft");
		}
		if (Input.IsActionPressed("playerMoveBackward"))
		{
			delta_mov -= 1.0f;
		}
		if (Input.IsActionPressed("playerMoveForward"))
		{
			delta_mov += 1.0f;
		}

		rotate(delta_rot);
		move_planar(delta_mov);

		if (!IsOnFloor()) // If in the air, fall towards the floor. Literally gravity
		{
			move_vertical(delta);
		}
		// Moving the character
		Velocity = _targetVelocity;
		MoveAndSlide();
		//MoveAndCollide(Velocity*(float)delta);
	}

	public void rotate(float delta_rot){
		//Replace with look at
		Transform3D transform = Transform;
		Vector3 axis = new Vector3(0, 1, 0);
		float rot_coif = 0.10f;
		transform.Basis = transform.Basis.Rotated(axis, rot_coif*delta_rot);
		Transform = transform;
		//if (direction != Vector3.Zero)
		//{
		//	direction = direction.Normalized();
			// Setting the basis property will affect the rotation of the node.
		//	GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);
		//

	}

	public void move_planar(float delta_mov){
		Transform3D transform = Transform;
		var base_pos = transform.Basis;
		var scale_vec = new Vector3(0, 0, -1);
		_targetVelocity = base_pos * ( scale_vec* (Speed * delta_mov));
	}

	public void move_vertical(double delta){
		_targetVelocity.Y -= FallAcceleration * (float)delta;
	}
}
