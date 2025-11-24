namespace BlazorGL.Core.Geometries;

/// <summary>
/// Wireframe geometry - converts all triangle edges to line segments
/// </summary>
public class WireframeGeometry : Geometry
{
    /// <summary>
    /// Creates wireframe geometry from an existing geometry
    /// </summary>
    /// <param name="geometry">Source geometry to convert to wireframe</param>
    public WireframeGeometry(Geometry geometry)
    {
        BuildWireframe(geometry);
    }

    private void BuildWireframe(Geometry geometry)
    {
        var vertices = new List<float>();
        var edges = new HashSet<(uint, uint)>();

        // Process each triangle and extract unique edges
        for (int i = 0; i < geometry.Indices.Length; i += 3)
        {
            uint i0 = geometry.Indices[i];
            uint i1 = geometry.Indices[i + 1];
            uint i2 = geometry.Indices[i + 2];

            // Add the three edges of the triangle
            AddEdge(i0, i1);
            AddEdge(i1, i2);
            AddEdge(i2, i0);
        }

        void AddEdge(uint a, uint b)
        {
            // Ensure consistent ordering
            var key = a < b ? (a, b) : (b, a);

            // Only add if not already added
            if (edges.Add(key))
            {
                // Add both vertices
                vertices.Add(geometry.Vertices[a * 3]);
                vertices.Add(geometry.Vertices[a * 3 + 1]);
                vertices.Add(geometry.Vertices[a * 3 + 2]);

                vertices.Add(geometry.Vertices[b * 3]);
                vertices.Add(geometry.Vertices[b * 3 + 1]);
                vertices.Add(geometry.Vertices[b * 3 + 2]);
            }
        }

        Vertices = vertices.ToArray();
        // Wireframes don't need normals, UVs, or indices (rendered as line segments)
        Normals = new float[0];
        UVs = new float[0];
        Indices = new uint[0];

        ComputeBoundingBox();
        ComputeBoundingSphere();
    }
}
