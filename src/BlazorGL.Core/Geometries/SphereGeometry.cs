using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Sphere geometry
/// </summary>
public class SphereGeometry : Geometry
{
    public SphereGeometry(float radius, int widthSegments = 32, int heightSegments = 16,
                         float phiStart = 0, float phiLength = MathF.PI * 2,
                         float thetaStart = 0, float thetaLength = MathF.PI)
    {
        BuildSphere(radius, widthSegments, heightSegments, phiStart, phiLength, thetaStart, thetaLength);
    }

    private void BuildSphere(float radius, int widthSegments, int heightSegments,
                            float phiStart, float phiLength, float thetaStart, float thetaLength)
    {
        widthSegments = Math.Max(3, widthSegments);
        heightSegments = Math.Max(2, heightSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        var grid = new List<List<uint>>();

        for (int iy = 0; iy <= heightSegments; iy++)
        {
            var verticesRow = new List<uint>();
            float v = (float)iy / heightSegments;

            for (int ix = 0; ix <= widthSegments; ix++)
            {
                float u = (float)ix / widthSegments;

                // Position
                float x = -radius * MathF.Cos(phiStart + u * phiLength) * MathF.Sin(thetaStart + v * thetaLength);
                float y = radius * MathF.Cos(thetaStart + v * thetaLength);
                float z = radius * MathF.Sin(phiStart + u * phiLength) * MathF.Sin(thetaStart + v * thetaLength);

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                // Normal
                var normal = Vector3.Normalize(new Vector3(x, y, z));
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                // UV
                uvs.Add(u);
                uvs.Add(1 - v);

                verticesRow.Add((uint)(vertices.Count / 3 - 1));
            }

            grid.Add(verticesRow);
        }

        // Indices
        for (int iy = 0; iy < heightSegments; iy++)
        {
            for (int ix = 0; ix < widthSegments; ix++)
            {
                uint a = grid[iy][ix + 1];
                uint b = grid[iy][ix];
                uint c = grid[iy + 1][ix];
                uint d = grid[iy + 1][ix + 1];

                if (iy != 0 || thetaStart > 0)
                {
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(d);
                }

                if (iy != heightSegments - 1 || thetaStart + thetaLength < MathF.PI)
                {
                    indices.Add(b);
                    indices.Add(c);
                    indices.Add(d);
                }
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
