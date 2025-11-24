using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core;
using System.Numerics;
using System.Text;

namespace BlazorGL.Loaders;

/// <summary>
/// Loads STL (Stereolithography) 3D models
/// Supports both ASCII and binary STL formats
/// </summary>
public class STLLoader
{
    /// <summary>
    /// Loads an STL file from a URL
    /// </summary>
    public async Task<Mesh> LoadAsync(string url)
    {
        using var httpClient = new HttpClient();
        var data = await httpClient.GetByteArrayAsync(url);
        return Load(data);
    }

    /// <summary>
    /// Loads an STL file from byte array
    /// </summary>
    public Mesh Load(byte[] data)
    {
        // Detect format by checking if it starts with "solid" (ASCII) or not (binary)
        bool isAscii = data.Length > 5 &&
                       Encoding.ASCII.GetString(data, 0, 5).ToLower() == "solid";

        Geometry geometry;
        if (isAscii)
        {
            geometry = ParseASCII(Encoding.UTF8.GetString(data));
        }
        else
        {
            geometry = ParseBinary(data);
        }

        return new Mesh
        {
            Geometry = geometry,
            Material = new StandardMaterial
            {
                Color = Core.Math.Color.White,
                Roughness = 0.5f,
                Metalness = 0.3f
            }
        };
    }

    private Geometry ParseBinary(byte[] data)
    {
        // Binary STL format:
        // 80 bytes - header
        // 4 bytes - number of triangles (uint32)
        // For each triangle:
        //   12 bytes - normal (3 floats)
        //   12 bytes - vertex 1 (3 floats)
        //   12 bytes - vertex 2 (3 floats)
        //   12 bytes - vertex 3 (3 floats)
        //   2 bytes - attribute byte count (unused)

        if (data.Length < 84)
            throw new Exception("Invalid binary STL file: too small");

        uint triangleCount = BitConverter.ToUInt32(data, 80);

        var vertices = new List<float>();
        var normals = new List<float>();
        var indices = new List<uint>();

        int offset = 84; // Skip header and triangle count

        for (uint i = 0; i < triangleCount; i++)
        {
            if (offset + 50 > data.Length)
                throw new Exception("Invalid binary STL file: unexpected end of data");

            // Read normal
            float nx = BitConverter.ToSingle(data, offset);
            float ny = BitConverter.ToSingle(data, offset + 4);
            float nz = BitConverter.ToSingle(data, offset + 8);
            offset += 12;

            // Read 3 vertices
            for (int v = 0; v < 3; v++)
            {
                float x = BitConverter.ToSingle(data, offset);
                float y = BitConverter.ToSingle(data, offset + 4);
                float z = BitConverter.ToSingle(data, offset + 8);
                offset += 12;

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);

                normals.Add(nx);
                normals.Add(ny);
                normals.Add(nz);

                indices.Add((uint)(i * 3 + v));
            }

            offset += 2; // Skip attribute byte count
        }

        var geometry = new CustomGeometry(vertices.ToArray(), indices.ToArray());
        geometry.Normals = normals.ToArray();
        geometry.ComputeBoundingBox();
        geometry.ComputeBoundingSphere();

        return geometry;
    }

    private Geometry ParseASCII(string content)
    {
        var vertices = new List<float>();
        var normals = new List<float>();
        var indices = new List<uint>();

        var lines = content.Split('\n');
        Vector3 currentNormal = Vector3.Zero;
        var triangleVertices = new List<Vector3>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) continue;

            switch (parts[0].ToLower())
            {
                case "facet":
                    if (parts.Length >= 5 && parts[1].ToLower() == "normal")
                    {
                        currentNormal = new Vector3(
                            float.Parse(parts[2]),
                            float.Parse(parts[3]),
                            float.Parse(parts[4])
                        );
                        triangleVertices.Clear();
                    }
                    break;

                case "vertex":
                    if (parts.Length >= 4)
                    {
                        triangleVertices.Add(new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        ));
                    }
                    break;

                case "endfacet":
                    if (triangleVertices.Count == 3)
                    {
                        uint baseIndex = (uint)(vertices.Count / 3);

                        foreach (var v in triangleVertices)
                        {
                            vertices.Add(v.X);
                            vertices.Add(v.Y);
                            vertices.Add(v.Z);

                            normals.Add(currentNormal.X);
                            normals.Add(currentNormal.Y);
                            normals.Add(currentNormal.Z);
                        }

                        indices.Add(baseIndex);
                        indices.Add(baseIndex + 1);
                        indices.Add(baseIndex + 2);
                    }
                    break;
            }
        }

        var geometry = new CustomGeometry(vertices.ToArray(), indices.ToArray());
        geometry.Normals = normals.ToArray();
        geometry.ComputeBoundingBox();
        geometry.ComputeBoundingSphere();

        return geometry;
    }
}

/// <summary>
/// Custom geometry created from raw vertex and index data
/// </summary>
public class CustomGeometry : Geometry
{
    public CustomGeometry(float[] vertices, uint[] indices)
    {
        Vertices = vertices;
        Indices = indices;

        // Generate UVs if not provided (planar projection)
        if (UVs == null || UVs.Length == 0)
        {
            GenerateDefaultUVs();
        }
    }

    private void GenerateDefaultUVs()
    {
        int vertexCount = Vertices.Length / 3;
        UVs = new float[vertexCount * 2];

        // Simple planar UV mapping
        for (int i = 0; i < vertexCount; i++)
        {
            float x = Vertices[i * 3];
            float y = Vertices[i * 3 + 1];

            UVs[i * 2] = (x + 1) * 0.5f;
            UVs[i * 2 + 1] = (y + 1) * 0.5f;
        }
    }
}
