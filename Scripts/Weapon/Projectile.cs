using Godot;

public partial class Projectile : RigidBody3D
{
    [Export] public int Damage = 10;
    [Export] public float Lifespan = 5.0f; // Seconds
    [Export] public float Speed = 50.0f; // Adjust as needed
    [Export] public float BlastRadius = 2.0f; // Radius of the explosion
    [Export] public float BlastForce = 100.0f; // Force applied to objects within the radius
    [Export] public Area3D ProjectileArea;

    private float _timeAlive = 0.0f;

    public override void _Ready()
    {
        // Apply initial velocity based on the direction it was fired in
        // (Assuming you set the projectile's rotation in ProjectileFire())
        LinearVelocity = -GlobalTransform.Basis.Z * Speed; 
    }

    public override void _PhysicsProcess(double delta)
    {
        _timeAlive += (float)delta;
        if (_timeAlive > Lifespan)
        {
            QueueFree(); // Destroy the projectile after its lifespan
        }
    }

    private void _on_body_entered(Node body)
    {
        if (body is CharacterBody3D enemy)
        {
            // Apply damage to the enemy
           // enemy.ApplyDamage(Damage);
        }

        // Apply blast force to nearby objects
        ApplyBlastForce();

        QueueFree(); // Destroy the projectile on impact
    }

    private void ApplyBlastForce()
    {
        // Get all overlapping bodies within the blast radius
        var overlappingBodies = ProjectileArea.GetOverlappingBodies();

        foreach (var body in overlappingBodies)
        {
            if (body is RigidBody3D rigidBody)
            {
                // Calculate direction and distance from the explosion center
                var direction = rigidBody.GlobalTransform.Origin - GlobalTransform.Origin;
                var distance = direction.Length();

                if (distance <= BlastRadius)
                {
                    // Apply force based on distance (closer objects get more force)
                    var force = BlastForce * (1.0f - distance / BlastRadius);
                    rigidBody.ApplyCentralImpulse(direction.Normalized() * force);
                }
            }
        }
    }
}