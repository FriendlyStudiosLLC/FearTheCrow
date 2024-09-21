using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Godot.Collections;
using Array = Godot.Collections.Array;

public partial class AstarTest : Node3D
{
    [Export] public bool debug = true;
    [Export] public float grid_step = 1.5f;
    [Export] public AStar3D AStar = new();
    [Export] public Godot.Collections.Dictionary<String, long> Points;
    [Export] public float grid_y = 1.1f;

    [Signal]
    public delegate void OnObstacleAddedEventHandler(Node3D obstacle);
    [Signal]
    public delegate void OnObstacleRemovedEventHandler(Node3D obstacle);
    
    
    private StandardMaterial3D red_mat = new StandardMaterial3D();
    private StandardMaterial3D green_mat = new StandardMaterial3D();
    public override void _Ready()
    {
        OnObstacleAdded += _OnObstacleAdded;
        OnObstacleRemoved += _OnObstacleRemoved;
        
        
        green_mat.AlbedoColor = Colors.Green;
        red_mat.AlbedoColor = Colors.Red;
        base._Ready();
        var pathables = GetTree().GetNodesInGroup("pathable");
        var obstacles = GetTree().GetNodesInGroup("obstacle");
        //Points["0,0,0"] = 0;
        AddPoints(pathables);
        
        ConnectPoints();
    }
    
    public void AddPoints(Array<Node> pathables)
    {
        foreach (var pathable in pathables)
        {
            var _object = pathable.GetNode<MeshInstance3D>("MeshInstance3D");
            if (_object == null) return;
            var aabb = _object.Mesh.GetAabb();
            var startPoint = _object.ToGlobal(aabb.Position) ;

            var x_step = aabb.Size.X / grid_step;
            var y_step = aabb.Size.Y / grid_step;
            var z_step = aabb.Size.Z / grid_step;

            for (int x = 0; x < x_step; x++)
            {
                for (int y = 0; y < y_step; y++)
                {
                    for (int z = 0; z < z_step; z++)
                    {
                        var nextPoint = startPoint + new Vector3(x * grid_step, y * grid_step, z * grid_step);
                        AddPoint(nextPoint);
                    }
                }
            }
        }
    }

    public void AddPoint(Vector3 point)
    {
        
        var id = AStar.GetAvailablePointId();


        Points[WorldToStar(point)] = id;
        AStar.AddPoint(id, point);
        _create_nav_cube(point);
    }


    private void _create_nav_cube(Vector3 position)
    {
        if (!debug) return;
        var cube = new MeshInstance3D();
        cube.Mesh = new BoxMesh();
        (cube.Mesh as BoxMesh).Size = new Vector3(0.125f, 0.125f, 0.125f);
        cube.MaterialOverride = red_mat;
        AddChild(cube);
        position.Y += grid_y;
        cube.GlobalTransform = cube.GlobalTransform with { Origin = position };
    }

    public void ConnectPoints()
    {
        foreach (var point in Points)
        {
            var pos_str = point.Key.Split(',');
            var world_pos = new Vector3(pos_str[0].ToFloat(), pos_str[1].ToFloat(), pos_str[2].ToFloat());
            var search_coords = new[] {-1, 0, 1};
            var adjacent_points = get_adjacent_points(world_pos);
            var currentID = point.Value;

            foreach (var neighbor_id in adjacent_points)
            {
                if (!AStar.ArePointsConnected(currentID, neighbor_id))
                {
                    AStar.ConnectPoints(currentID, neighbor_id);
                    if (debug)
                    {
                        ((MeshInstance3D)GetChild((int)currentID)).MaterialOverride = green_mat;
                        ((MeshInstance3D)GetChild((int)neighbor_id)).MaterialOverride = green_mat;
                    }
                }
            }
        }
    }
    
    public Vector3 GetRandomPoint()
    {
        
        Random random = new Random();
        int randomIndex = random.Next(Points.Count);
        var point = Points.Keys.ElementAt(randomIndex);
        var pos_str = point.Split(',');
        Vector3 randomKey = new Vector3(pos_str[0].ToFloat(), pos_str[1].ToFloat(), pos_str[2].ToFloat());
        return randomKey;
    }


    public long[] get_adjacent_points(Vector3 world_point)
    {
        // Use a List<long> for building the array dynamically
        var adjacent_points = new List<long>();
        var search_coords = new[] { -grid_step, 0, grid_step };
        foreach (var x in search_coords)
        {
            foreach (var y in search_coords)
            {
                foreach (var z in search_coords)
                {
                    var search_offset = new Vector3(x, y, z);
                    if (search_offset == Vector3.Zero)
                        continue;
                    var potential_neighbor = WorldToStar(world_point + search_offset);
                    if (Points.Keys.Contains(potential_neighbor))
                    {
                        adjacent_points.Add(Points.GetValueOrDefault(potential_neighbor));
                    }
                }
            }
        }

        // Convert the List to an array before returning
        return adjacent_points.ToArray();
    }

    void _OnObstacleAdded(Node3D obstacle)
    {
        var normalized_origin = obstacle.GlobalTransform.Origin;
        var adjacent_points = new long[] { };
        var point_key = WorldToStar(normalized_origin);
        var astar_id = Points[point_key];
        adjacent_points.Append(astar_id);

        foreach (var point in adjacent_points)
        {
            if (!AStar.IsPointDisabled(point))
            {
                AStar.SetPointDisabled(point, true);
                if (debug)
                {
                    ((MeshInstance3D)GetChild((int)point)).MaterialOverride = red_mat;
                }
            }
        }
    }

    void _OnObstacleRemoved(Node3D obstacle)
    {
        var normalized_origin = obstacle.GlobalTransform.Origin;
        var adjacent_points = new long[] { };
        var point_key = WorldToStar(normalized_origin);
        var astar_id = Points[point_key];
        adjacent_points.Append(astar_id);
        foreach (var point in adjacent_points)
        {
            if (AStar.IsPointDisabled(point))
            {
                AStar.SetPointDisabled(point, true);
                if (debug)
                {
                    ((MeshInstance3D)GetChild((int)point)).MaterialOverride = green_mat;
                }
            }
        }
    }


    public string WorldToStar(Vector3 worldPosition)
    {
        var x = Mathf.Snapped(worldPosition.X, grid_step);
        var y = Mathf.Snapped(worldPosition.Y, grid_step);
        var z = Mathf.Snapped(worldPosition.Z, grid_step);
        return $"{x},{y},{z}";
    }
    
    public Vector3[] FindPath(Vector3 from, Vector3 to)
    {
        var startID = AStar.GetClosestPoint(from);
        var toID = AStar.GetClosestPoint(to);

        Console.WriteLine($"Finding path from point {startID} to {toID}");

        if (!IsPointInGraph(startID))
        {
            Console.WriteLine($"Start point {startID} is not part of the graph.");
            return System.Array.Empty<Vector3>();
        }

        if (!IsPointInGraph(toID))
        {
            Console.WriteLine($"End point {toID} is not part of the graph.");
            return System.Array.Empty<Vector3>();
        }

        LogPointConnections(startID);
        LogPointConnections(toID);

        var path = AStar.GetPointPath(startID, toID);

        if (path == null || path.Length == 0)
        {
            GD.Print("No path found between the given points.");
        }
        else
        {
            GD.Print($"Path found with {path.Length} points.");
        }

        return path;
    }

    private Vector3 GetVectorKeyAsString(long id)
    {
        foreach (var point in Points)
        {
            if (point.Value == id)
            {
                var pos_str = point.Key.Split(',');
                Vector3 pos = new Vector3(pos_str[0].ToFloat(), pos_str[1].ToFloat(), pos_str[2].ToFloat());
                return pos;
                
            }
        }

        return Vector3.Zero;
    }

    private void LogPointConnections(long pointID)
    {
        
        var neighbors = get_adjacent_points(GetVectorKeyAsString(pointID));
        GD.Print($"Point {pointID} has {neighbors.Length} neighbors:");
        foreach (var neighbor in neighbors)
        {
            GD.Print($" - Neighbor ID: {neighbor}");
        }
    }

    private bool IsPointInGraph(long pointID)
    {
        // Assuming you have a way to get all point IDs in the A* graph
        var allPoints = Points;
        return allPoints.Values.Contains(pointID);
    }
}
