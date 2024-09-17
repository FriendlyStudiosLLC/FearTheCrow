using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace FearTheCrow.Scripts.Enemy;

[GlobalClass]
public partial class FlyingEnemyLogic : EnemyLogic
{
    // ... (your existing code) ...

    // New properties to control boid behavior
    [Export] public float SeparationDistance = 1f;
    [Export] public float AlignmentStrength = 200.5f;
    [Export] public float CohesionStrength = -405f;
    [Export] public float WanderRadius = 15f;
    [Export] public float WanderJitter = 0.1f;
    [Export] public float MinTargetChangeInterval = 2f;
    [Export] public float MaxTargetChangeInterval = 5f;
    [Export] public float TargetChangeMultiplier = 30.0f;
    
    private float _timeUntilNextTargetChange;
    //[Export] public Area3D SearchArea; // The volume to confine the search

    private Vector3 _wanderTarget;
    private Vector3 _avoidanceVector;

    public override void OnReady(global::Enemy parent)
    {
        _parent = parent;
        _timeUntilNextTargetChange = (float)GD.RandRange(MinTargetChangeInterval, MaxTargetChangeInterval);
        ReadyIdle();
    }

    public override void OnUpdate()
    {
        // ... (your existing code) ...

        switch (state)
        {
            case EnemyState.Search:
                UpdateSearch();
                break;
            // ... (rest of your switch statement) ...
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void OnPhysicsUpdate()
    {
        // ... (your existing code) ...

        switch (state)
        {
            case EnemyState.Search:
                PhysicsUpdateSearch();
                break;
            // ... (rest of your switch statement) ...
        }
    }

    public override void UpdateSearch()
    {
        _timeSinceLastTargetCheck += (float)_parent.GetPhysicsProcessDeltaTime();
        if (_timeSinceLastTargetCheck >= TargetRefreshRate)
        {
            _timeSinceLastTargetCheck = 0;

            // Apply boid flocking rules
            var neighbors = _parent.OverlapArea.GetOverlappingBodies()
                .OfType<Node3D>()
                .Where(body => body is global::Enemy enemy && enemy.Logic == this)
                .Select(body => body.GlobalPosition)
                .ToList();

            var separation = CalculateSeparation(neighbors);
            var alignment = CalculateAlignment(neighbors);
            var cohesion = CalculateCohesion(neighbors);

            var   flockingForce = separation + alignment + cohesion;

            GD.Print("Separation Force:", separation);
            GD.Print("Alignment Force:", alignment);
            GD.Print("Cohesion Force:", cohesion);
            GD.Print("Flocking Force:", flockingForce);

            // Wander even when there are flocking forces
            Wander(); 

            // Apply forces and move
            var desiredVelocity = (_parent.Target - _parent.GlobalPosition + flockingForce + _avoidanceVector).Normalized() * Speed;
            _parent.Velocity = _parent.Velocity.Lerp(desiredVelocity, 0.2f); 
            
            _timeUntilNextTargetChange -= (float)_parent.GetPhysicsProcessDeltaTime();

            // If it's time, find a new target
            if (_timeUntilNextTargetChange <= 0f)
            {
                FindNewTarget();
                _timeUntilNextTargetChange = (float)GD.RandRange(MinTargetChangeInterval, MaxTargetChangeInterval);
            }
            
          
        }
    }

    public override void PhysicsUpdateSearch()
    {
        // Obstacle avoidance during movement
        
        if (_parent.hitbox.IsColliding()) 
        {
            _avoidanceVector = Vector3.Zero;

            for (int i = 0; i < _parent.hitbox.GetCollisionCount(); i++)
            {
                if (_parent.hitbox.GetCollider(i) != _parent)
                {
                    _avoidanceVector -= _parent.hitbox.GetCollisionNormal(i); // Use -= for accumulation
                    FindNewTarget();
                }
            }

            if (_avoidanceVector.LengthSquared() > 0) 
            {
                _avoidanceVector = _avoidanceVector.Normalized();
            }

            _parent.Velocity += _avoidanceVector * 0.5f; 
        }
        else
        {
            _avoidanceVector = Vector3.Zero;
        }

        _parent.MoveAndSlide();
    }


    private void FindNewTarget()
    {
        // Generate a random direction within a hemisphere above the enemy
        var randomDirection = new Vector3(
            (float)GD.RandRange(-1f, 1f),
            (float)GD.RandRange(0f, 1f), // Ensure Y is positive or zero
            (float)GD.RandRange(-1f, 1f)
        ).Normalized();

        // Scale the direction by a random distance within the wander radius
        var randomDistance = (float)GD.RandRange(WanderRadius / 2f, WanderRadius); // At least half the radius

        _parent.Target = _parent.GlobalPosition + randomDirection * randomDistance * TargetChangeMultiplier;
    }


    private void Wander()
    {
        // Add more variation to wander behavior
        _wanderTarget += new Vector3(
            (float)GD.RandRange(-1f, 1f),
            (float)GD.RandRange(-1f, 1f),
            (float)GD.RandRange(-1f, 1f)
        ) * WanderJitter;
        _wanderTarget = _wanderTarget.Normalized() * WanderRadius;

        var targetLocal = _parent.GlobalTransform.Basis * (_wanderTarget); // Use Xform for local to global
        _parent.Target = _parent.GlobalPosition + targetLocal;
    }
    

    private Vector3 CalculateSeparation(List<Vector3> neighbors)
    {
        var separationForce = Vector3.Zero;
        foreach (var neighbor in neighbors)
        {
            var toNeighbor = _parent.GlobalPosition - neighbor;
            var distance = toNeighbor.Length();

            // Add a small epsilon to avoid division by zero or near-zero
            if (distance < SeparationDistance && distance > 0.001f) 
            {
                separationForce += toNeighbor.Normalized() / (distance + 0.001f); 
            }
        }
        return separationForce;
    }

    private Vector3 CalculateAlignment(List<Vector3> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.Zero;

        var averageVelocity = neighbors.Aggregate(Vector3.Zero, (acc, pos) => acc + pos) / neighbors.Count;
        return (averageVelocity - _parent.Velocity) * AlignmentStrength;
    }
    
    

    private Vector3 CalculateCohesion(List<Vector3> neighbors)
    {
        if (neighbors.Count == 0) return Vector3.Zero;

        var centerOfMass = neighbors.Aggregate(Vector3.Zero, (acc, pos) => acc + pos) / neighbors.Count;
        var toCenter = centerOfMass - _parent.GlobalPosition;
        return toCenter * CohesionStrength;
    }

}