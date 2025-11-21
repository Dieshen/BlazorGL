using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Torus knot geometry - creates various knot configurations
/// </summary>
public class TorusKnotGeometry : Geometry
{
    public TorusKnotGeometry(float radius = 1, float tube = 0.4f, int tubularSegments = 64,
                            int radialSegments = 8, int p = 2, int q = 3)
    {
        BuildTorusKnot(radius, tube, tubularSegments, radialSegments, p, q);
    }

    private void BuildTorusKnot(float radius, float tube, int tubularSegments, int radialSegments, int p, int q)
    {
        tubularSegments = System.Math.Max(3, tubularSegments);
        radialSegments = System.Math.Max(3, radialSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        // Helper functions for torus knot calculations
        Vector3 CalculatePositionOnCurve(float u)
        {
            float pMul2Pi = p * 2 * MathF.PI;
            float qMul2Pi = q * 2 * MathF.PI;

            float cu = MathF.Cos(qMul2Pi * u);
            float su = MathF.Sin(qMul2Pi * u);
            float quOverP = q * u / p * 2 * MathF.PI;
            float cs = MathF.Cos(quOverP);

            float x = radius * (2 + cs) * 0.5f * cu;
            float y = radius * (2 + cs) * su * 0.5f;
            float z = radius * MathF.Sin(quOverP) * 0.5f;

            return new Vector3(x, y, z);
        }

        // Generate vertices
        for (int i = 0; i <= tubularSegments; i++)
        {
            float u = (float)i / tubularSegments;
            Vector3 p1 = CalculatePositionOnCurve(u);
            Vector3 p2 = CalculatePositionOnCurve(u + 0.01f);

            Vector3 tangent = Vector3.Normalize(p2 - p1);
            Vector3 normal = Vector3.Normalize(Vector3.Cross(tangent, p1));
            Vector3 binormal = Vector3.Normalize(Vector3.Cross(tangent, normal));

            for (int j = 0; j <= radialSegments; j++)
            {
                float v = (float)j / radialSegments * 2 * MathF.PI;
                float cx = -tube * MathF.Cos(v);
                float cy = tube * MathF.Sin(v);

                Vector3 pos = p1 + cx * normal + cy * binormal;

                vertices.Add(pos.X);
                vertices.Add(pos.Y);
                vertices.Add(pos.Z);

                Vector3 norm = Vector3.Normalize(pos - p1);
                normals.Add(norm.X);
                normals.Add(norm.Y);
                normals.Add(norm.Z);

                uvs.Add((float)i / tubularSegments);
                uvs.Add((float)j / radialSegments);
            }
        }

        // Generate indices
        for (int j = 1; j <= tubularSegments; j++)
        {
            for (int i = 1; i <= radialSegments; i++)
            {
                uint a = (uint)((radialSegments + 1) * (j - 1) + (i - 1));
                uint b = (uint)((radialSegments + 1) * j + (i - 1));
                uint c = (uint)((radialSegments + 1) * j + i);
                uint d = (uint)((radialSegments + 1) * (j - 1) + i);

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
