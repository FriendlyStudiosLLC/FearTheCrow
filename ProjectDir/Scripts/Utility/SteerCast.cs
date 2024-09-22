using Godot;
using System;

[Tool][GlobalClass]
public partial class SteerCast : Node3D
{
    [Export] public Vector3 Origin = Vector3.Zero;
    [Export] public Vector3 BaseDirection = Vector3.Forward;
    [Export] public Vector3 SteerDirection;
    [Export] public float Length = 10.0f;
    [Export] public float Angle = Mathf.Pi / 4; // 45 degrees in radians

    // Optional: Collision layers and masks
    [Export] public uint CollisionMask = 1; // Adjust as needed
    
    [Export] private float castInterval = 0.2f; // Adjust as needed
    [Export] private float _timeSinceLastRaycast = 0;
    public bool _hitObject = false;
    private bool _debug = true;
    public bool enabled = true;

    [Export]
    public bool debug
    {
        get => _debug;
        set
        {
            _debug = value;
        }
    }

    public override void _Ready()
    {
        
    }
    public override void _Process(double delta)
    {
        if(!enabled) return;
        _timeSinceLastRaycast += (float)delta;
        if (_timeSinceLastRaycast >= castInterval) 
        {
            
            SteerDirection = BaseDirection;
            PerformHitscan();
            _timeSinceLastRaycast = 0;
        }
    }

    


    public void PerformHitscan() 
    {
        
        var spaceState = GetWorld3D().DirectSpaceState;

        var from = Origin;
        var to = Origin + (Origin + new Vector3(
            (float)GD.RandRange(-Angle, Angle),
            (float)GD.RandRange(-Angle, Angle),
            Length));
        
            
        var query = new PhysicsRayQueryParameters3D();
        query.From = from;
        query.To = to;
        query.CollisionMask = CollisionMask;
        var result = spaceState.IntersectRay(query);
        SteerDirection = to;

        if (result.Keys.Contains("collider"))
        {
            var hitPoint = (Vector3)result["position"];

            if (IsPointInCone(hitPoint))
            {
                GD.Print("Hit detected within the cone at: ", hitPoint);

                // ... (Your hit handling logic here, similar to the provided HitscanFire)
                // Example:
                var hit = result["collider"].AsGodotObject() as Node;
                if (hit != null)
                {
                    _hitObject = true;
                    // ... (Apply decal, impulse, etc.)
                }
                else
                {
                    _hitObject = false;
                }
            }
        }

    }

    private bool IsPointInCone(Vector3 point)
    {
        var pointDirection = (point - Origin).Normalized();
        var angleToPoint = BaseDirection.AngleTo(pointDirection);

        return angleToPoint <= Angle / 2;
    }
}