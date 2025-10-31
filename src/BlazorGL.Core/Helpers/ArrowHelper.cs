using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that displays a directional arrow (line + cone tip)
/// </summary>
public class ArrowHelper : Object3D
{
    public ArrowHelper(Vector3 direction, Vector3 origin, float length = 1.0f, Math.Color? color = null, float headLength = 0.2f, float headWidth = 0.2f)
    {
        Name = "ArrowHelper";

        var col = color ?? new Math.Color(1, 1, 0);

        // Normalize direction
        var dir = Vector3.Normalize(direction);

        // Create line for arrow shaft
        var lineGeom = new BufferGeometry();
        float shaftLength = length - headLength;
        var shaftEnd = dir * shaftLength;

        lineGeom.SetAttribute("position", new float[]
        {
            0, 0, 0,
            shaftEnd.X, shaftEnd.Y, shaftEnd.Z
        }, 3);

        var line = new Line(lineGeom, new LineBasicMaterial { Color = col });
        AddChild(line);

        // Create cone for arrow head
        var cone = new Mesh(
            new ConeGeometry(headWidth, headLength, 8, 1),
            new BasicMaterial { Color = col }
        );
        cone.Position = shaftEnd;
        cone.Rotation = new Vector3(MathF.PI / 2, 0, 0); // Point along direction
        AddChild(cone);

        Position = origin;
    }
}
