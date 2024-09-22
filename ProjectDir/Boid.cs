using Godot;

public partial class Boid : CharacterBody3D
{
    // Boid Parameters
    [Export] public float MaxSpeed = 5.0f;
    [Export] public float MaxForce = 0.5f;
    [Export] public float SeparationRadius = 2.0f;
    [Export] public float AlignmentRadius = 5.0f;
    [Export] public float CohesionRadius = 10.0f;

    // Internal Variables
    public override void _Ready()
    {
        SetVelocity(new Vector3(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1, GD.Randf() * 2 - 1));
    }

    public override void _PhysicsProcess(double delta)
    {
        
        // Find Nearby Boids
        var nearbyBoids = GetNearbyBoids();
        // Calculate Boid Forces
        var separationForce = CalculateSeparation(nearbyBoids);
        var alignmentForce = CalculateAlignment(nearbyBoids);
        var cohesionForce = CalculateCohesion(nearbyBoids);

        // Apply Forces to Velocity
        var v = GetVelocity();
        v += (separationForce + alignmentForce + cohesionForce) * MaxSpeed;
        SetVelocity(v.Clamp(Vector3.Zero, new Vector3(MaxSpeed, MaxSpeed, MaxSpeed)));
        GD.Print("New Velocity:", GetVelocity());
        // Move the Boid
        MoveAndSlide();

        // Optional: Visualize Velocity (for debugging)
        if (Engine.IsEditorHint())
        {
            //DebugDraw.DrawLine3D(Transform.Origin, Transform.Origin + Velocity, Colors.Red);
        }
    }

    private Godot.Collections.Array<Boid> GetNearbyBoids()
    {
        var spaceState = GetWorld3D().DirectSpaceState;

        // Create a SphereShape3D for the query
        var shape = new SphereShape3D();
        shape.Radius = CohesionRadius;

        var query = new PhysicsShapeQueryParameters3D();
        query.Transform = Transform;
        query.SetShape(shape);
        query.CollideWithBodies = true;
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() }; // Exclude self

        var result = spaceState.IntersectShape(query);
        var nearbyBoids = new Godot.Collections.Array<Boid>();

        foreach (var item in result)
        {
            var collider = (Node3D)item["collider"];
            if (collider != null && collider.GetParent() is Boid boid)
            {
                nearbyBoids.Add(boid);
            }
        }

        return nearbyBoids;
    }

    private Vector3 CalculateSeparation(Godot.Collections.Array<Boid> boids)
    {
        var steer = Vector3.Zero;
        int count = 0;
        foreach (var other in boids)
        {
            float d = Transform.Origin.DistanceTo(other.Transform.Origin);
            if (d > 0 && d < SeparationRadius)
            {
                var diff = Transform.Origin - other.Transform.Origin;
                diff /= d; // Weight by distance
                steer += diff;
                count++;
            }
        }
        if (count > 0)
        {
            steer /= count;
        }

        if (steer.LengthSquared() > 0)
        {
            steer = steer.Normalized() * MaxSpeed - Velocity;
            
            steer = steer.Clamp(Vector3.Zero, new Vector3(MaxForce,MaxForce,MaxForce));
        }
        return steer;
    }

    private Vector3 CalculateAlignment(Godot.Collections.Array<Boid> boids)
    {
        var sum = Vector3.Zero;
        int count = 0;
        foreach (var other in boids)
        {
            float d = Transform.Origin.DistanceTo(other.Transform.Origin);
            if (d > 0 && d < AlignmentRadius)
            {
                sum += other.Velocity;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            sum = sum.Normalized() * MaxSpeed;
            var steer = sum - Velocity;
            
            steer = steer.Clamp(Vector3.Zero, new Vector3(MaxForce,MaxForce,MaxForce));
            return steer;
        }
        else
        {
            return Vector3.Zero;
        }
    }

    private Vector3 CalculateCohesion(Godot.Collections.Array<Boid> boids)
    {
        var sum = Vector3.Zero;
        int count = 0;
        foreach (var other in boids)
        {
            float d = Transform.Origin.DistanceTo(other.Transform.Origin);
            if (d > 0 && d < CohesionRadius)
            {
                sum += other.Transform.Origin;
                count++;
            }
        }
        if (count > 0)
        {
            sum /= count;
            return Seek(sum);
        }
        else
        {
            return Vector3.Zero;
        }
    }

    private Vector3 Seek(Vector3 target)
    {
        var desired = target - Transform.Origin;
        desired = desired.Normalized() * MaxSpeed;
        var steer = desired - Velocity;
        steer = steer.Clamp(Vector3.Zero, new Vector3(MaxForce,MaxForce,MaxForce));
        return steer;
    }
}