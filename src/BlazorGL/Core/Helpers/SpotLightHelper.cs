using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Lights;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a spot light's cone
/// </summary>
public class SpotLightHelper : Object3D
{
    private SpotLight _light;
    private LineSegments _cone;

    public SpotLightHelper(SpotLight light, Math.Color? color = null)
    {
        Name = "SpotLightHelper";
        _light = light;

        var col = color ?? light.Color;

        // Create cone wireframe
        var geometry = new BufferGeometry();

        int segments = 16;
        var vertices = new List<float>();

        // Tip of cone
        vertices.AddRange(new[] { 0f, 0f, 0f });

        // Base circle vertices
        float angle = light.Angle;
        float height = 1.0f;
        float radius = MathF.Tan(angle) * height;

        for (int i = 0; i <= segments; i++)
        {
            float theta = (i / (float)segments) * MathF.PI * 2;
            float x = MathF.Cos(theta) * radius;
            float z = MathF.Sin(theta) * radius;

            // Line from tip to base
            vertices.AddRange(new[] { 0f, 0f, 0f });
            vertices.AddRange(new[] { x, -height, z });

            // Base circle segments
            if (i > 0)
            {
                float prevTheta = ((i - 1) / (float)segments) * MathF.PI * 2;
                float prevX = MathF.Cos(prevTheta) * radius;
                float prevZ = MathF.Sin(prevTheta) * radius;

                vertices.AddRange(new[] { prevX, -height, prevZ });
                vertices.AddRange(new[] { x, -height, z });
            }
        }

        geometry.SetAttribute("position", vertices.ToArray(), 3);

        _cone = new LineSegments(geometry, new LineBasicMaterial { Color = col });
        AddChild(_cone);

        Update();
    }

    public void Update()
    {
        // Update helper position and orientation based on light
        Position = _light.Position;
    }
}
