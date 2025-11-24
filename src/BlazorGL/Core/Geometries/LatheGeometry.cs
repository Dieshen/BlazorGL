using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Lathe geometry - creates shapes by rotating a 2D profile around an axis
/// </summary>
public class LatheGeometry : Geometry
{
    /// <summary>
    /// Creates a lathe geometry
    /// </summary>
    /// <param name="points">2D points (x, y) defining the profile to rotate</param>
    /// <param name="segments">Number of segments around the circumference</param>
    /// <param name="phiStart">Starting angle in radians</param>
    /// <param name="phiLength">Length of the rotation in radians</param>
    public LatheGeometry(Vector2[] points, int segments = 12, float phiStart = 0, float phiLength = MathF.PI * 2)
    {
        BuildLathe(points, segments, phiStart, phiLength);
    }

    private void BuildLathe(Vector2[] points, int segments, float phiStart, float phiLength)
    {
        if (points.Length < 2)
            throw new ArgumentException("Points array must contain at least 2 points");

        segments = System.Math.Max(3, segments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        // Generate vertices
        for (int i = 0; i <= segments; i++)
        {
            float phi = phiStart + ((float)i / segments) * phiLength;
            float sinPhi = MathF.Sin(phi);
            float cosPhi = MathF.Cos(phi);

            for (int j = 0; j < points.Length; j++)
            {
                Vector2 pt = points[j];

                // Position
                float x = pt.X * sinPhi;
                float y = pt.Y;
                float z = pt.X * cosPhi;

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                // Calculate normal
                Vector3 normal;
                if (j == 0)
                {
                    // First point - use direction to next point
                    Vector2 tangent = Vector2.Normalize(points[j + 1] - pt);
                    normal = new Vector3(-tangent.Y * sinPhi, tangent.X, -tangent.Y * cosPhi);
                }
                else if (j == points.Length - 1)
                {
                    // Last point - use direction from previous point
                    Vector2 tangent = Vector2.Normalize(pt - points[j - 1]);
                    normal = new Vector3(-tangent.Y * sinPhi, tangent.X, -tangent.Y * cosPhi);
                }
                else
                {
                    // Middle points - average of both directions
                    Vector2 tangent1 = Vector2.Normalize(pt - points[j - 1]);
                    Vector2 tangent2 = Vector2.Normalize(points[j + 1] - pt);
                    Vector2 tangent = Vector2.Normalize(tangent1 + tangent2);
                    normal = new Vector3(-tangent.Y * sinPhi, tangent.X, -tangent.Y * cosPhi);
                }

                normal = Vector3.Normalize(normal);
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                // UV coordinates
                uvs.Add((float)i / segments);
                uvs.Add((float)j / (points.Length - 1));
            }
        }

        // Generate indices
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < points.Length - 1; j++)
            {
                uint a = (uint)(i * points.Length + j);
                uint b = (uint)((i + 1) * points.Length + j);
                uint c = (uint)((i + 1) * points.Length + j + 1);
                uint d = (uint)(i * points.Length + j + 1);

                indices.Add(a);
                indices.Add(b);
                indices.Add(d);

                indices.Add(b);
                indices.Add(c);
                indices.Add(d);
            }
        }

        Vertices = vertices.ToArray();
        Normals = normals.ToArray();
        UVs = uvs.ToArray();
        Indices = indices.ToArray();

        ComputeBoundingBox();
        ComputeBoundingSphere();
    }
}
