namespace BlazorGL.Core.Geometries;

/// <summary>
/// Octahedron geometry (8 triangular faces)
/// </summary>
public class OctahedronGeometry : PolyhedronGeometry
{
    public OctahedronGeometry(float radius = 1, int detail = 0)
    {
        float[] vertices = new float[]
        {
            1, 0, 0,   // 0
           -1, 0, 0,   // 1
            0, 1, 0,   // 2
            0,-1, 0,   // 3
            0, 0, 1,   // 4
            0, 0,-1    // 5
        };

        uint[] indices = new uint[]
        {
            0, 2, 4,
            0, 4, 3,
            0, 3, 5,
            0, 5, 2,
            1, 2, 5,
            1, 5, 3,
            1, 3, 4,
            1, 4, 2
        };

        BuildPolyhedron(vertices, indices, radius, detail);
    }
}
