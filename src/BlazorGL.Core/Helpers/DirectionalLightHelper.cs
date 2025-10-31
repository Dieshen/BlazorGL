using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Lights;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a directional light's direction
/// </summary>
public class DirectionalLightHelper : Object3D
{
    private DirectionalLight _light;
    private LineSegments _lightPlane;
    private Line _targetLine;

    public DirectionalLightHelper(DirectionalLight light, float size = 1.0f, Math.Color? color = null)
    {
        Name = "DirectionalLightHelper";
        _light = light;

        var col = color ?? light.Color;

        // Create light plane (representing the light source)
        var planeGeom = new BufferGeometry();
        float halfSize = size * 0.5f;
        float[] planeVerts = new float[]
        {
            -halfSize, 0, -halfSize,  halfSize, 0, -halfSize,
            halfSize, 0, -halfSize,  halfSize, 0, halfSize,
            halfSize, 0, halfSize,  -halfSize, 0, halfSize,
            -halfSize, 0, halfSize,  -halfSize, 0, -halfSize,
        };
        planeGeom.SetAttribute("position", planeVerts, 3);

        _lightPlane = new LineSegments(planeGeom, new LineBasicMaterial { Color = col });
        AddChild(_lightPlane);

        // Create line showing direction
        var lineGeom = new BufferGeometry();
        float[] lineVerts = new float[]
        {
            0, 0, 0,
            0, -size, 0
        };
        lineGeom.SetAttribute("position", lineVerts, 3);

        _targetLine = new Line(lineGeom, new LineBasicMaterial { Color = col });
        AddChild(_targetLine);

        Update();
    }

    public void Update()
    {
        // Update helper position and orientation based on light
        // TODO: Position based on light's direction
    }
}
