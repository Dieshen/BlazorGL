namespace BlazorGL.Core.Geometries;

/// <summary>
/// Box (cube) geometry
/// </summary>
public class BoxGeometry : Geometry
{
    public BoxGeometry(float width, float height, float depth,
                       int widthSegments = 1, int heightSegments = 1, int depthSegments = 1)
    {
        BuildBox(width, height, depth, widthSegments, heightSegments, depthSegments);
    }

    private void BuildBox(float width, float height, float depth,
                          int widthSegments, int heightSegments, int depthSegments)
    {
        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();
        var indices = new List<uint>();

        uint indexOffset = 0;

        // Build each face
        BuildPlane(2, 1, 0, -1, -1, depth, height, width, depthSegments, heightSegments, 0); // px
        BuildPlane(2, 1, 0, 1, -1, depth, height, -width, depthSegments, heightSegments, 1); // nx
        BuildPlane(0, 2, 1, 1, 1, width, depth, height, widthSegments, depthSegments, 2); // py
        BuildPlane(0, 2, 1, 1, -1, width, depth, -height, widthSegments, depthSegments, 3); // ny
        BuildPlane(0, 1, 2, 1, -1, width, height, depth, widthSegments, heightSegments, 4); // pz
        BuildPlane(0, 1, 2, -1, -1, width, height, -depth, widthSegments, heightSegments, 5); // nz

        void BuildPlane(int u, int v, int w, int udir, int vdir,
                       float pwidth, float pheight, float pdepth,
                       int gridX, int gridY, int side)
        {
            float segmentWidth = pwidth / gridX;
            float segmentHeight = pheight / gridY;

            float widthHalf = pwidth / 2;
            float heightHalf = pheight / 2;
            float depthHalf = pdepth / 2;

            int gridX1 = gridX + 1;
            int gridY1 = gridY + 1;

            uint vertexCounter = 0;

            for (int iy = 0; iy < gridY1; iy++)
            {
                float y = iy * segmentHeight - heightHalf;

                for (int ix = 0; ix < gridX1; ix++)
                {
                    float x = ix * segmentWidth - widthHalf;

                    float[] vertex = new float[3];
                    vertex[u] = x * udir;
                    vertex[v] = y * vdir;
                    vertex[w] = depthHalf;

                    vertices.Add(vertex[0]);
                    vertices.Add(vertex[1]);
                    vertices.Add(vertex[2]);

                    float[] normal = new float[3];
                    normal[u] = 0;
                    normal[v] = 0;
                    normal[w] = pdepth > 0 ? 1 : -1;

                    normals.Add(normal[0]);
                    normals.Add(normal[1]);
                    normals.Add(normal[2]);

                    uvs.Add((float)ix / gridX);
                    uvs.Add(1 - ((float)iy / gridY));

                    vertexCounter++;
                }
            }

            for (int iy = 0; iy < gridY; iy++)
            {
                for (int ix = 0; ix < gridX; ix++)
                {
                    uint a = indexOffset + (uint)(ix + gridX1 * iy);
                    uint b = indexOffset + (uint)(ix + gridX1 * (iy + 1));
                    uint c = indexOffset + (uint)((ix + 1) + gridX1 * (iy + 1));
                    uint d = indexOffset + (uint)((ix + 1) + gridX1 * iy);

                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(d);

                    indices.Add(b);
                    indices.Add(c);
                    indices.Add(d);
                }
            }

            indexOffset += vertexCounter;
        }

        Vertices = vertices.ToArray();
        Normals = normals.ToArray();
        UVs = uvs.ToArray();
        Indices = indices.ToArray();

        ComputeBoundingBox();
        ComputeBoundingSphere();
    }
}
