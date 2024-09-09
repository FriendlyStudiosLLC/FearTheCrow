using System;
using Godot;

namespace FearTheCrow.Scripts.Enemy;

public enum EnemyState
{
    Idle,
    Alert,
    Stunned,
    LowHealth,
    Attacking,
    Roaming,
    Searching
}

[GlobalClass]
public partial class EnemyStateMachine : Node
{
    private AStar3D astar = new AStar3D();
    [Export] public EnemyState State;
    public override void _Process(double delta)
    {
    }

    public void StateBranch()
    {
        switch(State)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Alert:
                break;
            case EnemyState.Stunned:
                break;
            case EnemyState.LowHealth:
                break;
            case EnemyState.Attacking:
                break;
            case EnemyState.Roaming:
                break;
            case EnemyState.Searching:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}