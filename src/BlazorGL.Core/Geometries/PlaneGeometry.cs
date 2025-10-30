namespace BlazorGL.Core.Geometries;

/// <summary>
/// Plane geometry
/// </summary>
public class PlaneGeometry : Geometry
{
    public PlaneGeometry(float width, float height, int widthSegments = 1, int heightSegments = 1)
    {
        BuildPlane(width, height, widthSegments, heightSegments);
    }

    private void BuildPlane(float width, float height, int widthSegments, int heightSegments)
    {
        widthSegments = Math.Max(1, widthSegments);
        heightSegments = Math.Max(1, heightSegments);

        float widthHalf = width / 2;
        float heightHalf = height / 2;

        int gridX1 = widthSegments + 1;
        int gridY1 = heightSegments + 1;

        float segmentWidth = width / widthSegments;
        float segmentHeight = height / heightSegments;

        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        // Generate vertices, normals, and uvs
        for (int iy = 0; iy < gridY1; iy++)
        {
            float y = iy * segmentHeight - heightHalf;

            for (int ix = 0; ix < gridX1; ix++)
            {
                float x = ix * segmentWidth - widthHalf;

                vertices.Add(x);
                vertices.Add(-y);
                vertices.Add(0);

                normals.Add(0);
                normals.Add(0);
                normals.Add(1);

                uvs.Add((float)ix / widthSegments);
                uvs.Add(1 - ((float)iy / heightSegments));
            }
        }

        // Generate indices
        for (int iy = 0; iy < heightSegments; iy++)
        {
            for (int ix = 0; ix < widthSegments; ix++)
            {
                uint a = (uint)(ix + gridX1 * iy);
                uint b = (uint)(ix + gridX1 * (iy + 1));
                uint c = (uint)((ix + 1) + gridX1 * (iy + 1));
                uint d = (uint)((ix + 1) + gridX1 * iy);

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
