using System.Numerics;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Core;

/// <summary>
/// Mesh that renders multiple instances efficiently using GPU instancing
/// Each instance can have its own transformation matrix
/// </summary>
public class InstancedMesh : Mesh
{
    private Matrix4x4[] _instanceMatrices;
    private bool _matricesNeedUpdate = true;

    /// <summary>
    /// Number of instances to render
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Instance transformation matrices
    /// </summary>
    public Matrix4x4[] InstanceMatrices
    {
        get => _instanceMatrices;
        set
        {
            _instanceMatrices = value;
            _matricesNeedUpdate = true;
        }
    }

    /// <summary>
    /// Whether matrices need to be uploaded to GPU
    /// </summary>
    public bool MatricesNeedUpdate
    {
        get => _matricesNeedUpdate;
        set => _matricesNeedUpdate = value;
    }

    /// <summary>
    /// Instance colors (optional, per-instance coloring)
    /// </summary>
    public Vector3[]? InstanceColors { get; set; }

    /// <summary>
    /// Whether instance colors need update
    /// </summary>
    public bool ColorsNeedUpdate { get; set; } = false;

    public InstancedMesh(Geometry geometry, Material material, int count) : base(geometry, material)
    {
        Count = count;
        _instanceMatrices = new Matrix4x4[count];

        // Initialize with identity matrices
        for (int i = 0; i < count; i++)
        {
            _instanceMatrices[i] = Matrix4x4.Identity;
        }

        Name = "InstancedMesh";
    }

    /// <summary>
    /// Sets the transformation matrix for a specific instance
    /// </summary>
    public void SetMatrixAt(int index, Matrix4x4 matrix)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _instanceMatrices[index] = matrix;
        _matricesNeedUpdate = true;
    }

    /// <summary>
    /// Gets the transformation matrix for a specific instance
    /// </summary>
    public Matrix4x4 GetMatrixAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _instanceMatrices[index];
    }

    /// <summary>
    /// Sets the color for a specific instance
    /// </summary>
    public void SetColorAt(int index, Vector3 color)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (InstanceColors == null)
            InstanceColors = new Vector3[Count];

        InstanceColors[index] = color;
        ColorsNeedUpdate = true;
    }

    /// <summary>
    /// Gets the color for a specific instance
    /// </summary>
    public Vector3? GetColorAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return InstanceColors?[index];
    }
}
