using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Tube geometry - extrudes a circular cross-section along a path
/// </summary>
public class TubeGeometry : Geometry
{
    public TubeGeometry(Vector3[] path, float radius = 1, int tubularSegments = 64,
                       int radialSegments = 8, bool closed = false)
    {
        BuildTube(path, radius, tubularSegments, radialSegments, closed);
    }

    private void BuildTube(Vector3[] path, float radius, int tubularSegments, int radialSegments, bool closed)
    {
        if (path.Length < 2)
            throw new ArgumentException("Path must contain at least 2 points");

        tubularSegments = Math.Min(tubularSegments, path.Length - 1);
        radialSegments = Math.Max(3, radialSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        // Generate tube segments
        for (int i = 0; i <= tubularSegments; i++)
        {
            float u = (float)i / tubularSegments;
            int pathIndex = (int)(u * (path.Length - 1));
            pathIndex = Math.Min(pathIndex, path.Length - 2);

            float t = u * (path.Length - 1) - pathIndex;
            Vector3 p1 = path[pathIndex];
            Vector3 p2 = path[pathIndex + 1];
            Vector3 pos = Vector3.Lerp(p1, p2, t);

            // Calculate tangent
            Vector3 tangent = Vector3.Normalize(p2 - p1);

            // Create perpendicular vectors (Frenet frame)
            Vector3 normal;
            if (Math.Abs(tangent.Y) < 0.9f)
                normal = Vector3.Normalize(Vector3.Cross(tangent, Vector3.UnitY));
            else
                normal = Vector3.Normalize(Vector3.Cross(tangent, Vector3.UnitX));

            Vector3 binormal = Vector3.Normalize(Vector3.Cross(tangent, normal));

            // Generate circle of vertices
            for (int j = 0; j <= radialSegments; j++)
            {
                float v = (float)j / radialSegments * 2 * MathF.PI;
                float cx = radius * MathF.Cos(v);
                float cy = radius * MathF.Sin(v);

                Vector3 vertex = pos + cx * normal + cy * binormal;
                Vector3 norm = Vector3.Normalize(cx * normal + cy * binormal);

                vertices.Add(vertex.X);
                vertices.Add(vertex.Y);
                vertices.Add(vertex.Z);

                normals.Add(norm.X);
                normals.Add(norm.Y);
                normals.Add(norm.Z);

                uvs.Add(u);
                uvs.Add((float)j / radialSegments);
            }
        }

        // Generate indices
        for (int i = 0; i < tubularSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                uint a = (uint)((radialSegments + 1) * i + j);
                uint b = (uint)((radialSegments + 1) * (i + 1) + j);
                uint c = (uint)((radialSegments + 1) * (i + 1) + j + 1);
                uint d = (uint)((radialSegments + 1) * i + j + 1);

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
