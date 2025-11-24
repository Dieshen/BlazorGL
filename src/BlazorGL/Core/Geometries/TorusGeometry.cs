using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Torus (donut) geometry
/// </summary>
public class TorusGeometry : Geometry
{
    public TorusGeometry(float radius, float tube, int radialSegments = 8, int tubularSegments = 6,
                        float arc = MathF.PI * 2)
    {
        BuildTorus(radius, tube, radialSegments, tubularSegments, arc);
    }

    private void BuildTorus(float radius, float tube, int radialSegments, int tubularSegments, float arc)
    {
        radialSegments = System.Math.Max(3, radialSegments);
        tubularSegments = System.Math.Max(3, tubularSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        for (int j = 0; j <= radialSegments; j++)
        {
            for (int i = 0; i <= tubularSegments; i++)
            {
                float u = (float)i / tubularSegments * arc;
                float v = (float)j / radialSegments * MathF.PI * 2;

                // Vertex
                float x = (radius + tube * MathF.Cos(v)) * MathF.Cos(u);
                float y = (radius + tube * MathF.Cos(v)) * MathF.Sin(u);
                float z = tube * MathF.Sin(v);

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                // Normal
                Vector3 center = new(radius * MathF.Cos(u), radius * MathF.Sin(u), 0);
                Vector3 vertex = new(x, y, z);
                Vector3 normal = Vector3.Normalize(vertex - center);

                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                // UV
                uvs.Add((float)i / tubularSegments);
                uvs.Add((float)j / radialSegments);
            }
        }

        // Generate indices
        for (int j = 1; j <= radialSegments; j++)
        {
            for (int i = 1; i <= tubularSegments; i++)
            {
                uint a = (uint)((tubularSegments + 1) * j + i - 1);
                uint b = (uint)((tubularSegments + 1) * (j - 1) + i - 1);
                uint c = (uint)((tubularSegments + 1) * (j - 1) + i);
                uint d = (uint)((tubularSegments + 1) * j + i);

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
