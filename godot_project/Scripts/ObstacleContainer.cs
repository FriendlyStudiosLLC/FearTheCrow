using Godot;
using System;

public partial class ObstacleContainer : Node3D
{
    [Signal] public delegate void obstacleAddedEventHandler(Node3D obstacle);
    [Signal] public delegate void obstacleRemovedEventHandler(Node3D obstacle);

    [Export] private PackedScene OBSTACLE;

    public void create_obstacle(Vector3 location)
    {
        var obstacle_instance = OBSTACLE.Instantiate() as Node3D;
        AddChild(obstacle_instance);
        var globalTransform = obstacle_instance.GetGlobalTransform();
        globalTransform.Origin = location;
        obstacle_instance.SetGlobalTransform(globalTransform);
        EmitSignal("OnObstacleAdded", obstacle_instance);
    }

    public void remove_obstacle(StaticBody3D obstacle)
    {
        EmitSignal("OnObstacleRemoved", obstacle);
        obstacle.QueueFree();
    }
}
