using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Base class for generating polyhedron geometries from vertex and face data
/// </summary>
public class PolyhedronGeometry : Geometry
{
    protected PolyhedronGeometry()
    {
    }

    public PolyhedronGeometry(float[] vertexData, uint[] indexData, float radius = 1, int detail = 0)
    {
        BuildPolyhedron(vertexData, indexData, radius, detail);
    }

    protected void BuildPolyhedron(float[] vertexData, uint[] indexData, float radius, int detail)
    {
        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        // Convert vertex data to Vector3
        for (int i = 0; i < vertexData.Length; i += 3)
        {
            vertices.Add(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]));
        }

        // Process each face
        for (int i = 0; i < indexData.Length; i += 3)
        {
            uint v1 = indexData[i];
            uint v2 = indexData[i + 1];
            uint v3 = indexData[i + 2];

            Subdivide(vertices, indices, vertices[(int)v1], vertices[(int)v2], vertices[(int)v3], detail);
        }

        // Convert to arrays and apply radius
        var vertList = new List<float>();
        var normList = new List<float>();
        var uvList = new List<float>();

        foreach (var v in vertices)
        {
            Vector3 normalized = Vector3.Normalize(v);

            // Position (normalized and scaled by radius)
            vertList.Add(normalized.X * radius);
            vertList.Add(normalized.Y * radius);
            vertList.Add(normalized.Z * radius);

            // Normal (same as normalized position for spherical surfaces)
            normList.Add(normalized.X);
            normList.Add(normalized.Y);
            normList.Add(normalized.Z);

            // UV coordinates (spherical mapping)
            float u = 0.5f + MathF.Atan2(normalized.Z, normalized.X) / (2 * MathF.PI);
            float v = 0.5f - MathF.Asin(normalized.Y) / MathF.PI;
            uvList.Add(u);
            uvList.Add(v);
        }

        Vertices = vertList.ToArray();
        Normals = normList.ToArray();
        UVs = uvList.ToArray();
        Indices = indices.ToArray();

        ComputeBoundingBox();
        ComputeBoundingSphere();
    }

    private void Subdivide(List<Vector3> vertices, List<uint> indices, Vector3 v1, Vector3 v2, Vector3 v3, int detail)
    {
        if (detail == 0)
        {
            // Add triangle without subdivision
            uint i1 = (uint)vertices.Count;
            uint i2 = i1 + 1;
            uint i3 = i1 + 2;

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            indices.Add(i1);
            indices.Add(i2);
            indices.Add(i3);
        }
        else
        {
            // Subdivide triangle
            Vector3 v12 = Vector3.Normalize((v1 + v2) / 2);
            Vector3 v23 = Vector3.Normalize((v2 + v3) / 2);
            Vector3 v31 = Vector3.Normalize((v3 + v1) / 2);

            Subdivide(vertices, indices, v1, v12, v31, detail - 1);
            Subdivide(vertices, indices, v2, v23, v12, detail - 1);
            Subdivide(vertices, indices, v3, v31, v23, detail - 1);
            Subdivide(vertices, indices, v12, v23, v31, detail - 1);
        }
    }
}
