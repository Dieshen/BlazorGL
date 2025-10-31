using System.Numerics;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Edges geometry - extracts edges from another geometry for wireframe rendering
/// Only includes edges where the angle between faces exceeds a threshold
/// </summary>
public class EdgesGeometry : Geometry
{
    /// <summary>
    /// Creates edges geometry from an existing geometry
    /// </summary>
    /// <param name="geometry">Source geometry to extract edges from</param>
    /// <param name="thresholdAngle">Threshold angle in degrees (edges with angle difference above this are included)</param>
    public EdgesGeometry(Geometry geometry, float thresholdAngle = 1)
    {
        BuildEdges(geometry, thresholdAngle);
    }

    private void BuildEdges(Geometry geometry, float thresholdAngle)
    {
        float thresholdDot = MathF.Cos(thresholdAngle * MathF.PI / 180);

        var edges = new HashSet<(uint, uint)>();
        var edgeNormals = new Dictionary<(uint, uint), List<Vector3>>();

        // Process each triangle
        for (int i = 0; i < geometry.Indices.Length; i += 3)
        {
            uint i0 = geometry.Indices[i];
            uint i1 = geometry.Indices[i + 1];
            uint i2 = geometry.Indices[i + 2];

            // Get triangle normal
            Vector3 v0 = new Vector3(
                geometry.Vertices[i0 * 3],
                geometry.Vertices[i0 * 3 + 1],
                geometry.Vertices[i0 * 3 + 2]
            );
            Vector3 v1 = new Vector3(
                geometry.Vertices[i1 * 3],
                geometry.Vertices[i1 * 3 + 1],
                geometry.Vertices[i1 * 3 + 2]
            );
            Vector3 v2 = new Vector3(
                geometry.Vertices[i2 * 3],
                geometry.Vertices[i2 * 3 + 1],
                geometry.Vertices[i2 * 3 + 2]
            );

            Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));

            // Store edges with their normals
            AddEdge(i0, i1, normal);
            AddEdge(i1, i2, normal);
            AddEdge(i2, i0, normal);
        }

        void AddEdge(uint a, uint b, Vector3 normal)
        {
            var key = a < b ? (a, b) : (b, a);
            if (!edgeNormals.ContainsKey(key))
                edgeNormals[key] = new List<Vector3>();
            edgeNormals[key].Add(normal);
        }

        // Filter edges by angle threshold
        var vertices = new List<float>();
        foreach (var kvp in edgeNormals)
        {
            if (kvp.Value.Count == 1 || // Border edge
                Vector3.Dot(kvp.Value[0], kvp.Value[1]) < thresholdDot) // Sharp edge
            {
                uint i0 = kvp.Key.Item1;
                uint i1 = kvp.Key.Item2;

                // Add both vertices
                vertices.Add(geometry.Vertices[i0 * 3]);
                vertices.Add(geometry.Vertices[i0 * 3 + 1]);
                vertices.Add(geometry.Vertices[i0 * 3 + 2]);

                vertices.Add(geometry.Vertices[i1 * 3]);
                vertices.Add(geometry.Vertices[i1 * 3 + 1]);
                vertices.Add(geometry.Vertices[i1 * 3 + 2]);
            }
        }

        Vertices = vertices.ToArray();
        // Edges don't need normals, UVs, or indices (rendered as line segments)
        Normals = new float[0];
        UVs = new float[0];
        Indices = new uint[0];

        ComputeBoundingBox();
        ComputeBoundingSphere();
    }
}
