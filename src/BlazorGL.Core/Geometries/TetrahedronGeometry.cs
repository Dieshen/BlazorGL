namespace BlazorGL.Core.Geometries;

/// <summary>
/// Tetrahedron geometry (4 triangular faces)
/// </summary>
public class TetrahedronGeometry : PolyhedronGeometry
{
    public TetrahedronGeometry(float radius = 1, int detail = 0)
    {
        float[] vertices = new float[]
        {
            1,  1,  1,   // 0
           -1, -1,  1,   // 1
           -1,  1, -1,   // 2
            1, -1, -1    // 3
        };

        uint[] indices = new uint[]
        {
            2, 1, 0,
            0, 3, 2,
            1, 3, 0,
            2, 3, 1
        };

        BuildPolyhedron(vertices, indices, radius, detail);
    }
}
