using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that displays a wireframe box around an object
/// </summary>
public class BoxHelper : LineSegments
{
    private Object3D? _object;

    public BoxHelper(Object3D? obj = null, Math.Color? color = null)
    {
        Name = "BoxHelper";
        _object = obj;

        var col = color ?? new Math.Color(1, 1, 0); // Yellow default

        // Create box wireframe (12 edges = 24 vertices)
        var geometry = new BufferGeometry();

        float[] vertices = new float[]
        {
            // Bottom face
            -0.5f, -0.5f, -0.5f,  0.5f, -0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,  0.5f, -0.5f, 0.5f,
            0.5f, -0.5f, 0.5f,  -0.5f, -0.5f, 0.5f,
            -0.5f, -0.5f, 0.5f,  -0.5f, -0.5f, -0.5f,
            // Top face
            -0.5f, 0.5f, -0.5f,  0.5f, 0.5f, -0.5f,
            0.5f, 0.5f, -0.5f,  0.5f, 0.5f, 0.5f,
            0.5f, 0.5f, 0.5f,  -0.5f, 0.5f, 0.5f,
            -0.5f, 0.5f, 0.5f,  -0.5f, 0.5f, -0.5f,
            // Vertical edges
            -0.5f, -0.5f, -0.5f,  -0.5f, 0.5f, -0.5f,
            0.5f, -0.5f, -0.5f,  0.5f, 0.5f, -0.5f,
            0.5f, -0.5f, 0.5f,  0.5f, 0.5f, 0.5f,
            -0.5f, -0.5f, 0.5f,  -0.5f, 0.5f, 0.5f
        };

        geometry.SetAttribute("position", vertices, 3);

        var material = new LineBasicMaterial
        {
            Color = col
        };

        Geometry = geometry;
        Material = material;

        Update();
    }

    /// <summary>
    /// Updates the helper to match the object's bounding box
    /// </summary>
    public void Update()
    {
        if (_object != null)
        {
            // TODO: Update box size and position based on object's bounding box
            // This would require computing the bounding box from the object's geometry
        }
    }
}
