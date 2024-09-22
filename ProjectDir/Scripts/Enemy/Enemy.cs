using Godot;
using System;
using FearTheCrow.Scripts.Enemy;

public enum EnemyState
{
	Idle,
	Search,
	Wander,
	Aggro,
	Attack,
	Dead,
}
public partial class Enemy : CharacterBody3D
{
	public Vector3 Target;
	[Export] public EnemyLogic Logic;
	[Export] public Marker3D SearchMarker;
	[Export] public Player TargetPlayer;
	[Export] public Area3D OverlapArea;
	[Export] public ShapeCast3D hitbox;
	
	[Export] public RayCast3D TargetRay = null;
	
	[Export] private float _featureUpdateInterval = 0.2f; 
	[Export] private float _featurePhysicsUpdateInterval = 0.2f; 
	
	private float _timeSinceLastUpdate = 0;
	private float _timeSinceLastPhysicsUpdate = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Logic.OnReady(this);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_timeSinceLastUpdate += (float)delta;
		if (_timeSinceLastUpdate >= _featureUpdateInterval) 
		{
			Logic.OnUpdate();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		_timeSinceLastPhysicsUpdate += (float)delta;
		if (_timeSinceLastPhysicsUpdate >= _featurePhysicsUpdateInterval)
		{
			Logic.OnPhysicsUpdate();
		}

	}
}
