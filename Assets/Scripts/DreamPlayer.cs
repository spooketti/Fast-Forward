using Godot;
using System;

public partial class DreamPlayer : CharacterBody3D
{
	private Node3D Head;
	private Node3D CamRot;
	private Camera3D camera;
	private float acceleration = 0.5f;
	private float gravity = 30f;
	private static float speed  = 5f;
	private float backSpeed = speed/2;
	private bool isInVR = false;

	public float frequnecy = 0.01f;
	public float amplitude = 0.15f;
	private float swayTurn = 0;
	private float mouseDelta = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		CamRot = GetNode<Node3D>("Head").GetNode<Node3D>("CamRot");
		camera = CamRot.GetNode<Camera3D>("Camera3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		cameraSway(delta);
		mouseDelta = 0;
	}

	public override void _Input(InputEvent @event)
	{
		if(@event is InputEventMouseMotion eventMouseMotion)
		{
			mouseDelta = eventMouseMotion.Relative.X;
			RotateY(-eventMouseMotion.Relative.X * (float)(Math.PI / 180));
			camera.RotateX(-eventMouseMotion.Relative.Y * (float)(Math.PI / 180));
		}	
	}


	private float swayLerp(double a, double b, double t)
	{
		return (float)(a + (b-a) * t);
	}
	
	private void cameraSway(double dt)
	{
		swayTurn = swayLerp(swayTurn,Math.Clamp(mouseDelta,-7.5,7.5),7*dt);
		Vector3 camRot = camera.Rotation;
		camRot.X = Mathf.Clamp(camRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
		camRot.Z = Mathf.DegToRad(swayTurn);
		camera.Rotation = camRot;
	}

	private Vector3 velocity = Vector3.Zero;
    public override void _PhysicsProcess(double delta)
    {
        Vector2 moveInput = Input.GetVector("left","right","backward","forward");
		Vector3 moveVector = (Transform.Basis * new Vector3(0, 0, Math.Abs(moveInput.Y))).Normalized();
		if(moveVector != Vector3.Zero)
		{
			velocity.X  = moveVector.X * speed;
			velocity.Z = moveVector.Z * speed;
			double tick = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			float BobbleX = (float)Mathf.Cos(tick*frequnecy / 2)*amplitude;
			float BobbleY = (float)Mathf.Abs(Math.Sin(tick*frequnecy))*amplitude;
			Vector3 bobbleOffset = new Vector3(BobbleX,BobbleY,0);
			if(moveInput.Y < 0)
			{
				velocity.X = moveVector.X * speed * 0.5f;
				velocity.Z = moveVector.Z * speed * 0.5f;
				bobbleOffset.X /= 2;
				bobbleOffset.Y /= 2;
			}
			camera.Position = camera.Position.Lerp(bobbleOffset,0.75f);
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X,0,acceleration);
			velocity.Z = Mathf.MoveToward(velocity.Z,0,acceleration);
			camera.Position = camera.Position.Lerp(new Vector3(0,0,0),0.025f);
		}
		if(!IsOnFloor())
		{
			velocity.Y -= gravity * (float)delta;
		}
		Velocity = velocity;
		MoveAndSlide();
    }
}
