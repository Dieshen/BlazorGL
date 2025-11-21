using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Ring geometry (flat 2D annulus/donut shape)
/// </summary>
public class RingGeometry : Geometry
{
    public RingGeometry(float innerRadius = 0.5f, float outerRadius = 1, int thetaSegments = 32,
                       int phiSegments = 1, float thetaStart = 0, float thetaLength = MathF.PI * 2)
    {
        BuildRing(innerRadius, outerRadius, thetaSegments, phiSegments, thetaStart, thetaLength);
    }

    private void BuildRing(float innerRadius, float outerRadius, int thetaSegments, int phiSegments,
                          float thetaStart, float thetaLength)
    {
        thetaSegments = System.Math.Max(3, thetaSegments);
        phiSegments = System.Math.Max(1, phiSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        // Generate vertices
        for (int j = 0; j <= phiSegments; j++)
        {
            for (int i = 0; i <= thetaSegments; i++)
            {
                float segment = thetaStart + ((float)i / thetaSegments) * thetaLength;
                float radius = innerRadius + ((float)j / phiSegments) * (outerRadius - innerRadius);

                float x = radius * MathF.Cos(segment);
                float y = radius * MathF.Sin(segment);

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(0);

                normals.Add(0);
                normals.Add(0);
                normals.Add(1);

                // UV coordinates
                float u = (float)i / thetaSegments;
                float v = (float)j / phiSegments;
                uvs.Add(u);
                uvs.Add(v);
            }
        }

        // Generate indices
        for (int j = 0; j < phiSegments; j++)
        {
            for (int i = 0; i < thetaSegments; i++)
            {
                uint a = (uint)((thetaSegments + 1) * j + i);
                uint b = (uint)((thetaSegments + 1) * (j + 1) + i);
                uint c = (uint)((thetaSegments + 1) * (j + 1) + i + 1);
                uint d = (uint)((thetaSegments + 1) * j + i + 1);

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
