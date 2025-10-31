using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Cylinder geometry (can also create cones by setting radiusTop to 0)
/// </summary>
public class CylinderGeometry : Geometry
{
    public CylinderGeometry(float radiusTop, float radiusBottom, float height,
                           int radialSegments = 32, int heightSegments = 1, bool openEnded = false)
    {
        BuildCylinder(radiusTop, radiusBottom, height, radialSegments, heightSegments, openEnded);
    }

    private void BuildCylinder(float radiusTop, float radiusBottom, float height,
                              int radialSegments, int heightSegments, bool openEnded)
    {
        radialSegments = Math.Max(3, radialSegments);
        heightSegments = Math.Max(1, heightSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        var indexArray = new List<List<uint>>();

        float halfHeight = height / 2;

        // Generate torso
        for (int y = 0; y <= heightSegments; y++)
        {
            var indexRow = new List<uint>();
            float v = (float)y / heightSegments;
            float radius = v * (radiusBottom - radiusTop) + radiusTop;

            for (int x = 0; x <= radialSegments; x++)
            {
                float u = (float)x / radialSegments;
                float theta = u * MathF.PI * 2;

                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                // Vertex
                float vx = radius * sinTheta;
                float vy = -v * height + halfHeight;
                float vz = radius * cosTheta;

                vertices.Add(vx);
                vertices.Add(vy);
                vertices.Add(vz);

                // Normal
                var normal = Vector3.Normalize(new Vector3(sinTheta, (radiusBottom - radiusTop) / height, cosTheta));
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                // UV
                uvs.Add(u);
                uvs.Add(1 - v);

                indexRow.Add((uint)(vertices.Count / 3 - 1));
            }

            indexArray.Add(indexRow);
        }

        // Generate indices
        for (int y = 0; y < heightSegments; y++)
        {
            for (int x = 0; x < radialSegments; x++)
            {
                uint a = indexArray[y][x];
                uint b = indexArray[y + 1][x];
                uint c = indexArray[y + 1][x + 1];
                uint d = indexArray[y][x + 1];

                indices.Add(a);
                indices.Add(b);
                indices.Add(d);

                indices.Add(b);
                indices.Add(c);
                indices.Add(d);
            }
        }

        // Generate caps
        if (!openEnded)
        {
            if (radiusTop > 0)
                GenerateCap(true, radiusTop, halfHeight);
            if (radiusBottom > 0)
                GenerateCap(false, radiusBottom, -halfHeight);
        }

        void GenerateCap(bool top, float radius, float yPos)
        {
            uint centerIndex = (uint)(vertices.Count / 3);

            // Center vertex
            vertices.Add(0);
            vertices.Add(yPos);
            vertices.Add(0);

            normals.Add(0);
            normals.Add(top ? 1 : -1);
            normals.Add(0);

            uvs.Add(0.5f);
            uvs.Add(0.5f);

            // Generate cap vertices
            for (int x = 0; x <= radialSegments; x++)
            {
                float u = (float)x / radialSegments;
                float theta = u * MathF.PI * 2;

                float cosTheta = MathF.Cos(theta);
                float sinTheta = MathF.Sin(theta);

                vertices.Add(radius * sinTheta);
                vertices.Add(yPos);
                vertices.Add(radius * cosTheta);

                normals.Add(0);
                normals.Add(top ? 1 : -1);
                normals.Add(0);

                uvs.Add((cosTheta * 0.5f) + 0.5f);
                uvs.Add((sinTheta * 0.5f) + 0.5f);
            }

            // Generate cap indices
            for (int x = 0; x < radialSegments; x++)
            {
                uint c = centerIndex;
                uint i = centerIndex + (uint)x + 1;
                uint j = centerIndex + (uint)x + 2;

                if (top)
                {
                    indices.Add(i);
                    indices.Add(c);
                    indices.Add(j);
                }
                else
                {
                    indices.Add(c);
                    indices.Add(i);
                    indices.Add(j);
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
