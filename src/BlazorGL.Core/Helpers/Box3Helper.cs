using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Math;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes an axis-aligned bounding box (Box3)
/// </summary>
public class Box3Helper : LineSegments
{
    private BoundingBox _box;

    public Box3Helper(BoundingBox box, Math.Color? color = null)
    {
        Name = "Box3Helper";
        _box = box;

        var col = color ?? new Math.Color(1, 1, 0); // Yellow

        UpdateGeometry();

        var material = new LineBasicMaterial { Color = col };
        Material = material;
    }

    private void UpdateGeometry()
    {
        var min = _box.Min;
        var max = _box.Max;

        var geometry = new BufferGeometry();

        // Create box wireframe edges
        float[] vertices = new float[]
        {
            // Bottom face
            min.X, min.Y, min.Z,  max.X, min.Y, min.Z,
            max.X, min.Y, min.Z,  max.X, min.Y, max.Z,
            max.X, min.Y, max.Z,  min.X, min.Y, max.Z,
            min.X, min.Y, max.Z,  min.X, min.Y, min.Z,
            // Top face
            min.X, max.Y, min.Z,  max.X, max.Y, min.Z,
            max.X, max.Y, min.Z,  max.X, max.Y, max.Z,
            max.X, max.Y, max.Z,  min.X, max.Y, max.Z,
            min.X, max.Y, max.Z,  min.X, max.Y, min.Z,
            // Vertical edges
            min.X, min.Y, min.Z,  min.X, max.Y, min.Z,
            max.X, min.Y, min.Z,  max.X, max.Y, min.Z,
            max.X, min.Y, max.Z,  max.X, max.Y, max.Z,
            min.X, min.Y, max.Z,  min.X, max.Y, max.Z
        };

        geometry.SetAttribute("position", vertices, 3);
        Geometry = geometry;
    }

    public void Update(BoundingBox box)
    {
        _box = box;
        UpdateGeometry();
    }
}
