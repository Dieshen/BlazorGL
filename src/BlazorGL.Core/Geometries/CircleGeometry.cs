using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Circular disc geometry (flat 2D circle)
/// </summary>
public class CircleGeometry : Geometry
{
    public CircleGeometry(float radius = 1, int segments = 32, float thetaStart = 0, float thetaLength = MathF.PI * 2)
    {
        BuildCircle(radius, segments, thetaStart, thetaLength);
    }

    private void BuildCircle(float radius, int segments, float thetaStart, float thetaLength)
    {
        segments = Math.Max(3, segments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        // Center vertex
        vertices.Add(0);
        vertices.Add(0);
        vertices.Add(0);

        normals.Add(0);
        normals.Add(0);
        normals.Add(1);

        uvs.Add(0.5f);
        uvs.Add(0.5f);

        // Generate vertices around the circle
        for (int s = 0; s <= segments; s++)
        {
            float segment = thetaStart + ((float)s / segments) * thetaLength;

            float x = radius * MathF.Cos(segment);
            float y = radius * MathF.Sin(segment);

            vertices.Add(x);
            vertices.Add(y);
            vertices.Add(0);

            normals.Add(0);
            normals.Add(0);
            normals.Add(1);

            uvs.Add((x / radius + 1) / 2);
            uvs.Add((y / radius + 1) / 2);
        }

        // Generate indices
        for (int i = 1; i <= segments; i++)
        {
            indices.Add(0);
            indices.Add((uint)i);
            indices.Add((uint)i + 1);
        }

        Vertices = vertices.ToArray();
        Normals = normals.ToArray();
        UVs = uvs.ToArray();
        Indices = indices.ToArray();

        ComputeBoundingBox();
        ComputeBoundingSphere();
    }
}
