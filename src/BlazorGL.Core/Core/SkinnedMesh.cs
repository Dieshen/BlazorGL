using System.Numerics;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Core;

/// <summary>
/// Mesh with skeletal animation support
/// Vertices are deformed based on bone transformations and skin weights
/// </summary>
public class SkinnedMesh : Mesh
{
    /// <summary>
    /// The skeleton controlling this skinned mesh
    /// </summary>
    public Skeleton? Skeleton { get; set; }

    /// <summary>
    /// Bind matrix - the inverse of the mesh's world matrix at bind time
    /// </summary>
    public Matrix4x4 BindMatrix { get; set; } = Matrix4x4.Identity;

    /// <summary>
    /// The inverse of the bind matrix
    /// </summary>
    public Matrix4x4 BindMatrixInverse { get; set; } = Matrix4x4.Identity;

    /// <summary>
    /// Root bone of the skeleton
    /// </summary>
    public Bone? BindMode { get; set; }

    public SkinnedMesh(Geometry geometry, Material material) : base(geometry, material)
    {
        Name = "SkinnedMesh";
        Type = "SkinnedMesh";
    }

    public SkinnedMesh(Geometry geometry, Material material, Skeleton skeleton) : base(geometry, material)
    {
        Name = "SkinnedMesh";
        Type = "SkinnedMesh";
        Skeleton = skeleton;
    }

    /// <summary>
    /// Binds the skeleton to this mesh
    /// </summary>
    public void Bind(Skeleton skeleton, Matrix4x4? bindMatrix = null)
    {
        Skeleton = skeleton;

        if (bindMatrix.HasValue)
        {
            BindMatrix = bindMatrix.Value;
        }
        else
        {
            // Use current world matrix as bind matrix
            UpdateWorldMatrix(true, false);
            BindMatrix = WorldMatrix;
        }

        Matrix4x4.Invert(BindMatrix, out var inverse);
        BindMatrixInverse = inverse;
    }

    /// <summary>
    /// Updates the skeleton (should be called before rendering)
    /// </summary>
    public void UpdateSkeleton()
    {
        if (Skeleton != null)
        {
            // Update bone world matrices
            foreach (var bone in Skeleton.Bones)
            {
                bone.UpdateWorldMatrix(false, false);
            }

            // Update skeleton bone matrices
            Skeleton.Update();
        }
    }

    /// <summary>
    /// Gets the bone by name from the skeleton
    /// </summary>
    public Bone? GetBoneByName(string name)
    {
        return Skeleton?.GetBoneByName(name);
    }
}
