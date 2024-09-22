using Godot;
using System;

[Tool][GlobalClass]
public partial class TestFlyingEnemy : CharacterBody3D
{
    [Export] public  float Speed = 0.01f;
    [Export] public  float rotationSpeed = 2f;

    private float _visionLength = 5f;

    [Export]
    public float visionLength
    {
        get
        {
            return _visionLength;
        }
        set
        {
            if(visionCone == null)
                Init();
            if (visionCone != null)
                (visionCone.GetChild(0) as Node3D)?.SetScale(new Vector3(_visionAngle, _visionAngle, value));
            _visionLength = value;
            visionRaycast.TargetPosition = new Vector3(0,0,_visionLength);
            steerRaycast.TargetPosition = new Vector3(0,0,_visionLength);
        }
    }

    private float _visionAngle = 5f;
    [Export]
    public float visionAngle
    {
        get
        {
            return _visionAngle;
        }
        set
        {
            if(visionCone == null)
                Init();
            if (visionCone != null)
                (visionCone.GetChild(0) as Node3D)?.SetScale(new Vector3(value, value, _visionLength));
            _visionAngle = value;
        }
    }
    public RayCast3D visionRaycast = null;
    public RayCast3D steerRaycast = null;
    public Area3D visionCone = null;
    public Area3D proximitySphere = null;
    public Timer steerRefreshTimer = null;
    public Player player = null;
    public KinematicCollision3D collision = null;
    public Vector3 targetPosition;

    [Export] public float rayCorrection = 5f;
    private double _steerRefresh = 1.0f;
    [Export] public double steerRefresh    
    {
        get
        {
            return _steerRefresh;
        }
        set
        {
            if (steerRefreshTimer != null)
                steerRefreshTimer.WaitTime = value;
            _steerRefresh = value;
        }
    }

    public override void _Ready()
    {
        Init();
    }

    public void Init()
    {
        steerRefreshTimer = GetNode<Timer>("steerRefreshTimer");
        visionCone = GetNode<Area3D>("VisionCone");
        proximitySphere = GetNode<Area3D>("ProximitySphere");
        steerRaycast = GetNode<RayCast3D>("SteerRaycast");
        visionRaycast = GetNode<RayCast3D>("VisionRaycast");
        visionLength = _visionLength;
        visionAngle = _visionAngle;
        steerRefresh = _steerRefresh;
    }

    public void Steer()
    {
        float randomAngle = steerRaycast.IsColliding()
            ? (float)GD.RandRange(-_visionAngle * 10, _visionAngle * 10)
            : (float)GD.RandRange(-_visionAngle, _visionAngle);
        var direction = GlobalTransform.Basis.Z;
        var velocity = direction * Speed;
       if(steerRaycast != null) 
           steerRaycast.TargetPosition = new Vector3(randomAngle, randomAngle, _visionLength);
       else
       {
           steerRaycast = GetNode<RayCast3D>("SteerRaycast");
       }
       
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint())
            return;

        
        UpdateSteer();
        var target_position = (Transform.Origin + Velocity) * Speed;

        Transform.Basis.Z.MoveToward(target_position, (float)(rotationSpeed * delta));

// Apply the new basis 

       // SetBasis(new_basis);
        
        MoveAndCollide(Velocity);
        if(steerRaycast.IsColliding())
            Steer();
    }

    private void UpdateSteer()
    {
        if (!TestMove(GetTransform(), steerRaycast.TargetPosition, collision, 0))
        {
            targetPosition = steerRaycast.TargetPosition;
            
            Velocity = Velocity.MoveToward(targetPosition, (float)(Speed * GetProcessDeltaTime()));
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector3.Zero, (float)(Speed * GetProcessDeltaTime()));
        }
    }


    public void VisionTimeout()
    {
        var overlaps = visionCone.GetOverlappingBodies();
        if (overlaps.Count > 0)
        {
            foreach (var overlap in overlaps)
            {
                
            }
        }
    }
}
