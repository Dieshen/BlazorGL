using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a plane with its normal
/// </summary>
public class PlaneHelper : LineSegments
{
    public PlaneHelper(float size = 1.0f, Math.Color? color = null)
    {
        Name = "PlaneHelper";

        var col = color ?? new Math.Color(1, 1, 0);

        float halfSize = size * 0.5f;

        var geometry = new BufferGeometry();

        // Create plane outline + center cross
        float[] vertices = new float[]
        {
            // Outline
            -halfSize, 0, -halfSize,  halfSize, 0, -halfSize,
            halfSize, 0, -halfSize,  halfSize, 0, halfSize,
            halfSize, 0, halfSize,  -halfSize, 0, halfSize,
            -halfSize, 0, halfSize,  -halfSize, 0, -halfSize,
            // Center cross
            -halfSize, 0, 0,  halfSize, 0, 0,
            0, 0, -halfSize,  0, 0, halfSize
        };

        geometry.SetAttribute("position", vertices, 3);

        var material = new LineBasicMaterial { Color = col };

        Geometry = geometry;
        Material = material;
    }
}
