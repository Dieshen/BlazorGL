using System.Numerics;
using BlazorGL.Core.Math;

namespace BlazorGL.Core.Geometries;

/// <summary>
/// Base class for all geometry types
/// </summary>
public abstract class Geometry : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Vertex positions (x, y, z, x, y, z, ...)
    /// </summary>
    public float[] Vertices { get; protected set; } = Array.Empty<float>();

    /// <summary>
    /// Vertex normals (x, y, z, x, y, z, ...)
    /// </summary>
    public float[] Normals { get; protected set; } = Array.Empty<float>();

    /// <summary>
    /// Texture coordinates (u, v, u, v, ...)
    /// </summary>
    public float[] UVs { get; protected set; } = Array.Empty<float>();

    /// <summary>
    /// Vertex indices for indexed drawing
    /// </summary>
    public uint[] Indices { get; protected set; } = Array.Empty<uint>();

    /// <summary>
    /// Tangent vectors for normal mapping (x, y, z, w, ...)
    /// </summary>
    public float[]? Tangents { get; protected set; }

    /// <summary>
    /// Vertex colors (r, g, b, r, g, b, ...)
    /// </summary>
    public float[]? Colors { get; protected set; }

    private BoundingBox _boundingBox;
    private BoundingSphere _boundingSphere;
    private bool _boundingBoxNeedsUpdate = true;
    private bool _boundingSphereNeedsUpdate = true;

    /// <summary>
    /// Axis-aligned bounding box
    /// </summary>
    public BoundingBox BoundingBox
    {
        get
        {
            if (_boundingBoxNeedsUpdate)
                ComputeBoundingBox();
            return _boundingBox;
        }
    }

    /// <summary>
    /// Bounding sphere
    /// </summary>
    public BoundingSphere BoundingSphere
    {
        get
        {
            if (_boundingSphereNeedsUpdate)
                ComputeBoundingSphere();
            return _boundingSphere;
        }
    }

    /// <summary>
    /// Computes vertex normals from faces
    /// </summary>
    public virtual void ComputeNormals()
    {
        if (Indices.Length == 0 || Vertices.Length == 0)
            return;

        int vertexCount = Vertices.Length / 3;
        Normals = new float[Vertices.Length];

        // For each face
        for (int i = 0; i < Indices.Length; i += 3)
        {
            uint i0 = Indices[i];
            uint i1 = Indices[i + 1];
            uint i2 = Indices[i + 2];

            Vector3 v0 = new(Vertices[i0 * 3], Vertices[i0 * 3 + 1], Vertices[i0 * 3 + 2]);
            Vector3 v1 = new(Vertices[i1 * 3], Vertices[i1 * 3 + 1], Vertices[i1 * 3 + 2]);
            Vector3 v2 = new(Vertices[i2 * 3], Vertices[i2 * 3 + 1], Vertices[i2 * 3 + 2]);

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            // Accumulate normals for each vertex
            for (int j = 0; j < 3; j++)
            {
                uint idx = Indices[i + j];
                Normals[idx * 3] += normal.X;
                Normals[idx * 3 + 1] += normal.Y;
                Normals[idx * 3 + 2] += normal.Z;
            }
        }

        // Normalize all normals
        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 normal = new(Normals[i * 3], Normals[i * 3 + 1], Normals[i * 3 + 2]);
            normal = Vector3.Normalize(normal);
            Normals[i * 3] = normal.X;
            Normals[i * 3 + 1] = normal.Y;
            Normals[i * 3 + 2] = normal.Z;
        }
    }

    /// <summary>
    /// Computes tangent vectors for normal mapping
    /// </summary>
    public virtual void ComputeTangents()
    {
        if (Indices.Length == 0 || Vertices.Length == 0 || UVs.Length == 0)
            return;

        int vertexCount = Vertices.Length / 3;
        Tangents = new float[vertexCount * 4];
        var tan1 = new Vector3[vertexCount];
        var tan2 = new Vector3[vertexCount];

        for (int i = 0; i < Indices.Length; i += 3)
        {
            uint i0 = Indices[i];
            uint i1 = Indices[i + 1];
            uint i2 = Indices[i + 2];

            Vector3 v0 = new(Vertices[i0 * 3], Vertices[i0 * 3 + 1], Vertices[i0 * 3 + 2]);
            Vector3 v1 = new(Vertices[i1 * 3], Vertices[i1 * 3 + 1], Vertices[i1 * 3 + 2]);
            Vector3 v2 = new(Vertices[i2 * 3], Vertices[i2 * 3 + 1], Vertices[i2 * 3 + 2]);

            Vector2 uv0 = new(UVs[i0 * 2], UVs[i0 * 2 + 1]);
            Vector2 uv1 = new(UVs[i1 * 2], UVs[i1 * 2 + 1]);
            Vector2 uv2 = new(UVs[i2 * 2], UVs[i2 * 2 + 1]);

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            Vector3 tangent = new(
                f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X),
                f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y),
                f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z)
            );

            Vector3 bitangent = new(
                f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X),
                f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y),
                f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z)
            );

            tan1[i0] += tangent;
            tan1[i1] += tangent;
            tan1[i2] += tangent;

            tan2[i0] += bitangent;
            tan2[i1] += bitangent;
            tan2[i2] += bitangent;
        }

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 n = new(Normals[i * 3], Normals[i * 3 + 1], Normals[i * 3 + 2]);
            Vector3 t = tan1[i];

            // Gram-Schmidt orthogonalize
            Vector3 tangent = Vector3.Normalize(t - n * Vector3.Dot(n, t));

            // Calculate handedness
            float w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;

            Tangents[i * 4] = tangent.X;
            Tangents[i * 4 + 1] = tangent.Y;
            Tangents[i * 4 + 2] = tangent.Z;
            Tangents[i * 4 + 3] = w;
        }
    }

    /// <summary>
    /// Computes the axis-aligned bounding box
    /// </summary>
    public void ComputeBoundingBox()
    {
        if (Vertices.Length == 0)
        {
            _boundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
            _boundingBoxNeedsUpdate = false;
            return;
        }

        var points = new Vector3[Vertices.Length / 3];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(Vertices[i * 3], Vertices[i * 3 + 1], Vertices[i * 3 + 2]);
        }

        _boundingBox = BoundingBox.FromPoints(points);
        _boundingBoxNeedsUpdate = false;
    }

    /// <summary>
    /// Computes the bounding sphere
    /// </summary>
    public void ComputeBoundingSphere()
    {
        if (Vertices.Length == 0)
        {
            _boundingSphere = new BoundingSphere(Vector3.Zero, 0);
            _boundingSphereNeedsUpdate = false;
            return;
        }

        var points = new Vector3[Vertices.Length / 3];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(Vertices[i * 3], Vertices[i * 3 + 1], Vertices[i * 3 + 2]);
        }

        _boundingSphere = BoundingSphere.FromPoints(points);
        _boundingSphereNeedsUpdate = false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Managed cleanup
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
