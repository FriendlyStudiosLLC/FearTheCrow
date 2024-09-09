using Godot;
using System;

[Tool]
public partial class Cone : Node3D
{
    [Export] public Vector3 Origin = Vector3.Zero;
    [Export] public Vector3 Direction = Vector3.Forward;
    [Export] public float Length = 10.0f;
    [Export] public float Angle = Mathf.Pi / 4; // 45 degrees in radians

    [Export] private RayCast3D ray = new RayCast3D();
    // Optional: Collision layers and masks
    [Export] public uint CollisionMask = 1; // Adjust as needed
    
    [Export] private float castInterval = 0.2f; // Adjust as needed
    [Export] private float _timeSinceLastRaycast = 0;

    public override void _Ready()
    {
        AddChild(ray);
    }
    public override void _Process(double delta)
    {
        _timeSinceLastRaycast += (float)delta;
        if (_timeSinceLastRaycast >= castInterval) 
        {
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
        ray.Position = from;
        ray.TargetPosition = to;
            
        var query = new PhysicsRayQueryParameters3D();
        query.From = from;
        query.To = to;
        query.CollisionMask = CollisionMask;
        var result = spaceState.IntersectRay(query);

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
                    // ... (Apply decal, impulse, etc.)
                }
            }
        }

        
    }

    private bool IsPointInCone(Vector3 point)
    {
        var pointDirection = (point - Origin).Normalized();
        var angleToPoint = Direction.AngleTo(pointDirection);

        return angleToPoint <= Angle / 2;
    }
}