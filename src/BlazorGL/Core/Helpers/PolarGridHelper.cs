using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that displays a polar coordinate grid
/// </summary>
public class PolarGridHelper : LineSegments
{
    public PolarGridHelper(float radius = 10, int radials = 16, int circles = 8, int divisions = 64, Math.Color? color1 = null, Math.Color? color2 = null)
    {
        Name = "PolarGridHelper";

        var col1 = color1 ?? new Math.Color(0.5f, 0.5f, 0.5f);
        var col2 = color2 ?? new Math.Color(0.25f, 0.25f, 0.25f);

        var vertices = new List<float>();
        var colors = new List<float>();

        // Radial lines
        for (int i = 0; i < radials; i++)
        {
            float angle = (i / (float)radials) * MathF.PI * 2;
            float x = MathF.Cos(angle) * radius;
            float z = MathF.Sin(angle) * radius;

            vertices.AddRange(new[] { 0f, 0f, 0f });
            vertices.AddRange(new[] { x, 0f, z });

            colors.AddRange(new[] { col1.R, col1.G, col1.B });
            colors.AddRange(new[] { col1.R, col1.G, col1.B });
        }

        // Circular rings
        for (int i = 1; i <= circles; i++)
        {
            float r = (i / (float)circles) * radius;

            for (int j = 0; j < divisions; j++)
            {
                float angle1 = (j / (float)divisions) * MathF.PI * 2;
                float angle2 = ((j + 1) / (float)divisions) * MathF.PI * 2;

                float x1 = MathF.Cos(angle1) * r;
                float z1 = MathF.Sin(angle1) * r;
                float x2 = MathF.Cos(angle2) * r;
                float z2 = MathF.Sin(angle2) * r;

                vertices.AddRange(new[] { x1, 0f, z1 });
                vertices.AddRange(new[] { x2, 0f, z2 });

                colors.AddRange(new[] { col2.R, col2.G, col2.B });
                colors.AddRange(new[] { col2.R, col2.G, col2.B });
            }
        }

        var geometry = new BufferGeometry();
        geometry.SetAttribute("position", vertices.ToArray(), 3);
        geometry.SetAttribute("color", colors.ToArray(), 3);

        var material = new LineBasicMaterial
        {
            VertexColors = true
        };

        Geometry = geometry;
        Material = material;
    }
}
