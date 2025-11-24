using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that displays a grid on the ground plane
/// </summary>
public class GridHelper : LineSegments
{
    public GridHelper(int size = 10, int divisions = 10, Math.Color? centerColor = null, Math.Color? gridColor = null)
    {
        Name = "GridHelper";

        var center = centerColor ?? new Math.Color(0.5f, 0.5f, 0.5f);
        var grid = gridColor ?? new Math.Color(0.25f, 0.25f, 0.25f);

        var step = (float)size / divisions;
        var halfSize = size / 2.0f;

        var vertexCount = (divisions + 1) * 2 * 2; // 2 lines per division, 2 directions
        var vertices = new List<float>();
        var colors = new List<float>();

        // Create grid lines
        for (int i = 0; i <= divisions; i++)
        {
            float pos = -halfSize + (i * step);
            var color = (i == divisions / 2) ? center : grid;

            // X direction lines
            vertices.AddRange(new[] { -halfSize, 0f, pos });
            vertices.AddRange(new[] { halfSize, 0f, pos });
            colors.AddRange(new[] { color.R, color.G, color.B });
            colors.AddRange(new[] { color.R, color.G, color.B });

            // Z direction lines
            vertices.AddRange(new[] { pos, 0f, -halfSize });
            vertices.AddRange(new[] { pos, 0f, halfSize });
            colors.AddRange(new[] { color.R, color.G, color.B });
            colors.AddRange(new[] { color.R, color.G, color.B });
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
