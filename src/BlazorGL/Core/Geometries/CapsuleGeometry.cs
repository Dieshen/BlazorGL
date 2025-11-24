using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Capsule geometry (cylinder with hemispherical caps) - also known as a pill shape
/// </summary>
public class CapsuleGeometry : Geometry
{
    public CapsuleGeometry(float radius = 1, float length = 1, int capSegments = 4, int radialSegments = 8)
    {
        BuildCapsule(radius, length, capSegments, radialSegments);
    }

    private void BuildCapsule(float radius, float length, int capSegments, int radialSegments)
    {
        capSegments = System.Math.Max(1, capSegments);
        radialSegments = System.Math.Max(3, radialSegments);

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        float halfLength = length / 2;

        // Generate top hemisphere
        for (int lat = 0; lat <= capSegments; lat++)
        {
            float theta = (float)lat / capSegments * (MathF.PI / 2);
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= radialSegments; lon++)
            {
                float phi = (float)lon / radialSegments * MathF.PI * 2;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;

                vertices.Add(x * radius);
                vertices.Add(y * radius + halfLength);
                vertices.Add(z * radius);

                normals.Add(x);
                normals.Add(y);
                normals.Add(z);

                float u = (float)lon / radialSegments;
                float v = (float)lat / capSegments * 0.5f;
                uvs.Add(u);
                uvs.Add(v);
            }
        }

        // Generate cylinder body
        for (int i = 0; i <= 1; i++)
        {
            float y = i == 0 ? halfLength : -halfLength;

            for (int lon = 0; lon <= radialSegments; lon++)
            {
                float phi = (float)lon / radialSegments * MathF.PI * 2;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi;
                float z = sinPhi;

                vertices.Add(x * radius);
                vertices.Add(y);
                vertices.Add(z * radius);

                normals.Add(x);
                normals.Add(0);
                normals.Add(z);

                float u = (float)lon / radialSegments;
                float v = 0.5f + (i == 0 ? 0 : 0.5f);
                uvs.Add(u);
                uvs.Add(v);
            }
        }

        // Generate bottom hemisphere
        for (int lat = 0; lat <= capSegments; lat++)
        {
            float theta = (float)lat / capSegments * (MathF.PI / 2);
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= radialSegments; lon++)
            {
                float phi = (float)lon / radialSegments * MathF.PI * 2;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = -cosTheta;
                float z = sinPhi * sinTheta;

                vertices.Add(x * radius);
                vertices.Add(y * radius - halfLength);
                vertices.Add(z * radius);

                normals.Add(x);
                normals.Add(y);
                normals.Add(z);

                float u = (float)lon / radialSegments;
                float v = 0.5f + (float)lat / capSegments * 0.5f;
                uvs.Add(u);
                uvs.Add(v);
            }
        }

        // Generate indices for top hemisphere
        for (int lat = 0; lat < capSegments; lat++)
        {
            for (int lon = 0; lon < radialSegments; lon++)
            {
                uint first = (uint)(lat * (radialSegments + 1) + lon);
                uint second = (uint)(first + radialSegments + 1);

                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                indices.Add(second);
                indices.Add(second + 1);
                indices.Add(first + 1);
            }
        }

        // Generate indices for cylinder
        uint cylinderStart = (uint)((capSegments + 1) * (radialSegments + 1));
        for (int lon = 0; lon < radialSegments; lon++)
        {
            uint first = cylinderStart + (uint)lon;
            uint second = first + (uint)(radialSegments + 1);

            indices.Add(first);
            indices.Add(second);
            indices.Add(first + 1);

            indices.Add(second);
            indices.Add(second + 1);
            indices.Add(first + 1);
        }

        // Generate indices for bottom hemisphere
        uint bottomStart = cylinderStart + (uint)(2 * (radialSegments + 1));
        for (int lat = 0; lat < capSegments; lat++)
        {
            for (int lon = 0; lon < radialSegments; lon++)
            {
                uint first = bottomStart + (uint)(lat * (radialSegments + 1) + lon);
                uint second = first + (uint)(radialSegments + 1);

                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                indices.Add(second);
                indices.Add(second + 1);
                indices.Add(first + 1);
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
