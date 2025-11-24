namespace BlazorGL.Core.Geometries;

/// <summary>
/// Dodecahedron geometry (12 pentagonal faces)
/// </summary>
public class DodecahedronGeometry : PolyhedronGeometry
{
    public DodecahedronGeometry(float radius = 1, int detail = 0)
    {
        float t = (1 + MathF.Sqrt(5)) / 2; // Golden ratio
        float r = 1 / t;

        float[] vertices = new float[]
        {
            // (±1, ±1, ±1)
            1,  1,  1,   // 0
            1,  1, -1,   // 1
            1, -1,  1,   // 2
            1, -1, -1,   // 3
           -1,  1,  1,   // 4
           -1,  1, -1,   // 5
           -1, -1,  1,   // 6
           -1, -1, -1,   // 7

            // (0, ±1/φ, ±φ)
            0,  r,  t,   // 8
            0,  r, -t,   // 9
            0, -r,  t,   // 10
            0, -r, -t,   // 11

            // (±1/φ, ±φ, 0)
            r,  t,  0,   // 12
            r, -t,  0,   // 13
           -r,  t,  0,   // 14
           -r, -t,  0,   // 15

            // (±φ, 0, ±1/φ)
            t,  0,  r,   // 16
            t,  0, -r,   // 17
           -t,  0,  r,   // 18
           -t,  0, -r    // 19
        };

        uint[] indices = new uint[]
        {
            3, 11, 7,   3, 7, 15,   3, 15, 13,
            7, 19, 17,  7, 17, 6,   7, 6, 15,
            17, 4, 8,   17, 8, 10,  17, 10, 6,
            8, 0, 16,   8, 16, 2,   8, 2, 10,
            0, 12, 1,   0, 1, 16,
            16, 1, 17,  17, 1, 3,
            2, 16, 13,  16, 3, 13,
            2, 13, 15,  2, 15, 6,
            15, 7, 19,  19, 7, 5,
            19, 5, 14,  19, 14, 18,
            18, 14, 4,  18, 4, 6,
            18, 6, 10,
            4, 14, 12,  4, 12, 0,
            4, 0, 8,
            14, 5, 9,   14, 9, 12,
            12, 9, 1,   1, 9, 11,
            1, 11, 3,   5, 7, 11,   5, 11, 9
        };

        BuildPolyhedron(vertices, indices, radius, detail);
    }
}
