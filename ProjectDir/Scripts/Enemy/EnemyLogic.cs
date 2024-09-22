using System;
using Godot;

namespace FearTheCrow.Scripts.Enemy;

[GlobalClass]
public partial class EnemyLogic : Resource
{
    
    public global::Enemy _parent;
    
    [Export] public EnemyState state;
    [Export] public float Speed = 1.5f;
    [Export] public float TargetRefreshRate = 0.2f;
    
    
    [Export] protected float _timeSinceLastTargetCheck = 0;
    
    public virtual void OnUpdate()
    {
        switch (state)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Search:
                UpdateSearch();
                break;
            case EnemyState.Wander:
                UpdateWander();
                break;
            case EnemyState.Aggro:
                UpdateAggro();
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Dead:
                UpdateDead();
                break;
        }
    }



    public virtual void OnPhysicsUpdate()
    {
        switch (state)
        {
            case EnemyState.Idle:
                PhysicsUpdateIdle();
                break;
            case EnemyState.Search:
                PhysicsUpdateSearch();
                break;
            case EnemyState.Wander:
                PhysicsUpdateWander();
                break;
            case EnemyState.Aggro:
                PhysicsUpdateAggro();
                break;
            case EnemyState.Attack:
                PhysicsUpdateAttack();
                break;
            case EnemyState.Dead:
                PhysicsUpdateDead();
                break;
        }
    }

    public virtual void OnReady(global::Enemy parent)
    {
        _parent = parent;
        ReadyIdle();
    }

    public virtual void OnDestroy()
    {
    }
    
    
    public virtual void UpdateSearch()
    {
    }

    public virtual void UpdateWander()
    {
    }

    public virtual void UpdateAggro()
    {
    }

    public virtual void UpdateAttack()
    {
    }

    public virtual void UpdateIdle()
    {
    }
    public virtual void UpdateDead()
    {
    }

    
    public virtual void PhysicsUpdateSearch()
    {
    }

    public virtual void PhysicsUpdateWander()
    {
    }

    public virtual void PhysicsUpdateAggro()
    {
    }

    public virtual void PhysicsUpdateAttack()
    {
    }

    public virtual void PhysicsUpdateIdle()
    {
    }
    public virtual void PhysicsUpdateDead()
    {
    }

    public virtual void ReadyIdle()
    {
    }
    
    


}