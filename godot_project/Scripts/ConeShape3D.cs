using Godot;
using System;

[Tool][GlobalClass]
public partial class ConeShape3D : ConvexPolygonShape3D
{
    // The height of the cone
    [Export] public float Height
    {
        get => height;
        set
        {
            height = value;
            RequestResize();
        }
    }
    private float height = 2.0f;

    // The radius of the bottom of the cone
    [Export] public float Radius
    {
        get => radius;
        set
        {
            radius = value;
            RequestResize();
        }
    }
    private float radius = 0.5f;

    // The number of radial segments of the cone
    [Export] public int Resolution
    {
        get => resolution;
        set
        {
            resolution = value;
            RequestResize();
        }
    }
    private int resolution = 8;

    // Controls how rounded the base should be
    [Export] public int CapSegments
    {
        get => capSegments;
        set
        {
            capSegments = value;
            RequestResize();
        }
    }
    private int capSegments = 0;  // Adjust for more or fewer segments in the rounded base

    // Resize requested
    private bool pendingResize = false;

    public ConeShape3D()
    {
        RequestResize();
    }

    // Will only resize once per frame, during idle time
    private void RequestResize()
    {
        if (!pendingResize)
        {
            CallDeferred(nameof(UpdateSize));
            pendingResize = true;
        }
    }

    // Updates shape size
    private void UpdateSize()
    {
        Vector3[] points = MakeConePolygonPoints(resolution, height, radius, capSegments);
        SetPoints(points);
        pendingResize = false;
    }

    // Makes a cone polygon with a rounded base
    private Vector3[] MakeConePolygonPoints(int resolution, float height, float radius, int capSegments)
    {
        var pts = new System.Collections.Generic.List<Vector3>();
        Vector3 origin = Vector3.Zero;  // Use the node's position as the origin for the cone
        // Generate points for the sides of the cone
        for (int i = 0; i < resolution; i++)
        {
            float angle = (float)i * Mathf.Tau / resolution;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            pts.Add(new Vector3(x, -height / 2, z) + origin);
        }

        // Add points for the rounded base (circular arc)
        float baseHeight = -height / 2;
        for (int i = 1; i <= capSegments; i++)  // Start at 1 to avoid duplicating the base point
        {
            float t = (float)i / capSegments;
            float arcHeight = Mathf.Lerp(baseHeight, baseHeight + radius / 2, t); // Smooth transition up for the rounded cap
            float arcRadius = Mathf.Lerp(radius, 0, t);  // Gradually reduce the radius as you go up the cap
            for (int j = 0; j < resolution; j++)
            {
                float angle = (float)j * Mathf.Tau / resolution;
                float x = Mathf.Cos(angle) * arcRadius;
                float z = Mathf.Sin(angle) * arcRadius;
                pts.Add(new Vector3(x, arcHeight, z) + origin);
            }
        }

        // Add the top of the cone
        var top = new Vector3(0, height / 2, 0) + origin;
        pts.Add(top);

        return pts.ToArray();
    }
}
