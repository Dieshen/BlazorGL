namespace BlazorGL.Core.Geometries;

/// <summary>
/// Icosahedron geometry (20 triangular faces)
/// </summary>
public class IcosahedronGeometry : PolyhedronGeometry
{
    public IcosahedronGeometry(float radius = 1, int detail = 0)
    {
        float t = (1 + MathF.Sqrt(5)) / 2; // Golden ratio

        float[] vertices = new float[]
        {
           -1,  t,  0,   // 0
            1,  t,  0,   // 1
           -1, -t,  0,   // 2
            1, -t,  0,   // 3
            0, -1,  t,   // 4
            0,  1,  t,   // 5
            0, -1, -t,   // 6
            0,  1, -t,   // 7
            t,  0, -1,   // 8
            t,  0,  1,   // 9
           -t,  0, -1,   // 10
           -t,  0,  1    // 11
        };

        uint[] indices = new uint[]
        {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,
            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,
            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,
            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };

        BuildPolyhedron(vertices, indices, radius, detail);
    }
}
