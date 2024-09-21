using Godot;
using System;
using System.Linq;


public static class BoidHelper {

    const int numViewDirections = 300;
    public static readonly Vector3[] directions;

    static BoidHelper () {
        directions = new Vector3[BoidHelper.numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt (5)) / 2;
        float angleIncrement = Mathf.Pi * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++) {
            float t = (float) i / numViewDirections;
            float inclination = Mathf.Acos (1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin (inclination) * Mathf.Cos (azimuth);
            float y = Mathf.Sin (inclination) * Mathf.Sin (azimuth);
            float z = Mathf.Cos (inclination);
            directions[i] = new Vector3 (x, y, z);
        }
    }

}


public partial class FlyingEnemy : CharacterBody3D
{
    [Export] private float Speed = 5.0f; // Increased speed for more natural movement
    [Export] private float TargetFactor = 1.0f;
    [Export] private float SeperationFactor = 1.0f;
    [Export] private float AlignmentFactor = 1.0f;
    [Export] private float CohesionFactor = 1.0f;
    [Export] private float MaxSteerForce = 5.0f; // Increased max steer force
    [Export] private Area3D ProximityArea;
    [Export] private ShapeCast3D ForwardCast;
    [Export] private float radius = 10.0f; // Adjust the radius as needed
    [Export] private Node3D mesh;
    private Vector3[] path;
    [Export]private Vector3 acceleration;
    private Vector3 avgFlockHeading;
    private Vector3 avgAvoidanceHeading;
    private Vector3 centerOfFlock;
    private int numPerceivedInFlock;
    private AstarTest astar;
    
    [Export] private int currentPathIndex = 0;

    [Export]private Vector3 targetPosition;
    [Export] private float targetMoveInterval = 2.0f; // Interval to move target position in seconds
    private float targetMoveTimer = 0.0f;

    public override void _Process(double delta)
    {
        // Update the target position randomly over time
        UpdateTargetPosition((float)delta);
        MoveForward();
        // Reset acceleration for the frame
     
        
        // Update velocity and move the enemy
        Velocity = acceleration;
        MoveAndSlide();
        
        // Make the enemy face the direction of its velocity
        FaceVelocity();
    }

    private void UpdateTargetPosition(float delta)
    {
        targetMoveTimer += delta;

        if (targetMoveTimer >= targetMoveInterval)
        {
            var nav = GetParent().GetNode<AstarTest>("Astar");
            
            GD.Print(nav.GetRandomPoint());
            UpdatePath(nav.FindPath(GlobalPosition, nav.GetRandomPoint()));
            targetMoveTimer = 0.0f;
        }

    }

    private void UpdatePath(Vector3[] newPath)
    {
        path = newPath;
        FindNextPointInPath();
    }

    private void FindNextPointInPath()
    {
        if (path != null && currentPathIndex < path.Length)
        {
            var new_target = path[currentPathIndex];
            currentPathIndex++;
            targetPosition = new_target;
            // Velocity.MoveToward(new_target, Speed * (float)GetProcessDeltaTime());
        }
        else
        {
            currentPathIndex = 0;
            acceleration = Vector3.Zero;
            targetPosition = GlobalPosition;
        }
    }


    private void MoveForward()
    {
        if (GlobalTransform.Origin.DistanceTo(targetPosition) < 0.1f)
        {
            FindNextPointInPath();
        }
        acceleration = SteerTorwards(targetPosition) * Speed;
    }

    private void FaceVelocity()
    {
        if (Velocity.Length() > 0) // Ensure there's velocity to face
        {
            Vector3 direction = Velocity;
            LookAt(GlobalPosition + targetPosition, Vector3.Up);
        }
    }

    void CalculateAverageFlockCenter()
    {
        Vector3 sum = Vector3.Zero;
        int neighborCount = 0;

        foreach (var b in ProximityArea.GetOverlappingBodies())
        {
            if (b is FlyingEnemy enemy)
            {
                sum += enemy.GlobalPosition;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            centerOfFlock = sum / neighborCount; // Calculate the average position of the flock
        }
    }

    void CalculateAverageHeading()
    {
        Vector3 sum = Vector3.Zero;
        int neighborCount = 0;

        foreach (var b in ProximityArea.GetOverlappingBodies())
        {
            if (b is FlyingEnemy enemy)
            {
                sum += enemy.Velocity.Normalized(); // Use velocity direction for alignment
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            avgFlockHeading = sum / neighborCount; // Calculate the average heading of the flock
        }
    }

    void CalculateAvoidance()
    {
        Vector3 avoidance = Vector3.Zero;
        int neighborCount = 0;

        foreach (var b in ProximityArea.GetOverlappingBodies())
        {
            if (b is FlyingEnemy enemy && enemy != this) // Avoid itself
            {
                Vector3 toBoid = GlobalPosition - enemy.GlobalPosition; // Direction away from the other boid
                float distance = toBoid.Length();
                var shape = ProximityArea.GetChild(0) as CollisionShape3D;
                var sphere = shape.Shape as SphereShape3D;

                if (distance > 0 && distance < sphere.Radius) // Only avoid boids within a certain radius
                {
                    avoidance += toBoid.Normalized() / distance; // Weight by inverse of the distance
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            avgAvoidanceHeading = avoidance / neighborCount; // Average the avoidance direction
        }
    }

    private Vector3 SteerTorwards(Vector3 target)
    {
        Vector3 desiredVelocity = (target - GlobalPosition).Normalized() * Speed; // Desired velocity towards the target
        Vector3 steer = desiredVelocity - Velocity;

        if (steer.Length() > MaxSteerForce) // Limit steering force
        {
            steer = steer.Normalized() * MaxSteerForce;
        }

        return steer;
    }

    private void Seperation()
    {
        acceleration += SteerTorwards(Position + avgAvoidanceHeading) * SeperationFactor; // Use avoidance for separation
    }

    private void Alignment()
    {
        acceleration += SteerTorwards(Position + avgFlockHeading) * AlignmentFactor; // Steer towards the flock's average heading
    }

    private void Cohesion()
    {
        acceleration += SteerTorwards(centerOfFlock) * CohesionFactor; // Steer towards the flock's center
    }
}