using Godot;
using System;
using FearTheCrow.Scripts.Enemy;

[GlobalClass]
public partial class BoidController : EnemyLogic
{
    [Export] public NodePath SteerCast;
    private Area3D _SteeringArea;
    private SteerCast _SteeriCast;

    public override void OnReady(Enemy parent)
    {
        base.OnReady(parent);
        
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnPhysicsUpdate()
    {
        base.OnPhysicsUpdate();
    }
}
