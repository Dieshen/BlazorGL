using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that displays RGB axes (X=red, Y=green, Z=blue)
/// </summary>
public class AxesHelper : LineSegments
{
    public AxesHelper(float size = 1.0f)
    {
        Name = "AxesHelper";

        // Create geometry with 3 lines (6 vertices)
        var geometry = new BufferGeometry();

        float[] vertices = new float[]
        {
            // X axis (red)
            0, 0, 0,  size, 0, 0,
            // Y axis (green)
            0, 0, 0,  0, size, 0,
            // Z axis (blue)
            0, 0, 0,  0, 0, size
        };

        float[] colors = new float[]
        {
            // X axis - red
            1, 0, 0,  1, 0, 0,
            // Y axis - green
            0, 1, 0,  0, 1, 0,
            // Z axis - blue
            0, 0, 1,  0, 0, 1
        };

        geometry.SetAttribute("position", vertices, 3);
        geometry.SetAttribute("color", colors, 3);

        var material = new LineBasicMaterial
        {
            VertexColors = true
        };

        Geometry = geometry;
        Material = material;
    }
}
