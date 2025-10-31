using System.Numerics;

namespace BlazorGL.Core;

/// <summary>
/// Skeleton that manages a hierarchy of bones for skeletal animation
/// Calculates bone matrices used for vertex skinning
/// </summary>
public class Skeleton
{
    private Matrix4x4[] _boneMatrices;
    private Matrix4x4[] _boneInverses;
    private bool _matricesNeedUpdate = true;

    /// <summary>
    /// Array of bones in the skeleton
    /// </summary>
    public Bone[] Bones { get; set; }

    /// <summary>
    /// Inverse bind matrices for each bone (rest pose inverse)
    /// </summary>
    public Matrix4x4[] BoneInverses
    {
        get => _boneInverses;
        set
        {
            _boneInverses = value;
            _matricesNeedUpdate = true;
        }
    }

    /// <summary>
    /// Computed bone matrices ready for shader upload
    /// </summary>
    public Matrix4x4[] BoneMatrices => _boneMatrices;

    /// <summary>
    /// Texture containing bone matrices (for WebGL uniform size limits)
    /// </summary>
    public float[]? BoneTexture { get; private set; }

    public Skeleton(Bone[] bones, Matrix4x4[]? boneInverses = null)
    {
        Bones = bones;
        _boneMatrices = new Matrix4x4[bones.Length];
        _boneInverses = boneInverses ?? CreateDefaultBoneInverses();

        CalculateBoneMatrices();
    }

    /// <summary>
    /// Creates default bone inverse matrices (identity)
    /// </summary>
    private Matrix4x4[] CreateDefaultBoneInverses()
    {
        var inverses = new Matrix4x4[Bones.Length];
        for (int i = 0; i < Bones.Length; i++)
        {
            Matrix4x4.Invert(Bones[i].WorldMatrix, out inverses[i]);
        }
        return inverses;
    }

    /// <summary>
    /// Updates bone matrices based on current bone world transforms
    /// </summary>
    public void Update()
    {
        CalculateBoneMatrices();
    }

    /// <summary>
    /// Calculates final bone matrices (world * inverse bind)
    /// </summary>
    private void CalculateBoneMatrices()
    {
        for (int i = 0; i < Bones.Length; i++)
        {
            // Bone matrix = BoneWorld * BoneInverseBindMatrix
            _boneMatrices[i] = _boneInverses[i] * Bones[i].WorldMatrix;
        }

        // Convert to texture data (4x4 matrices as floats)
        BoneTexture = new float[Bones.Length * 16];
        for (int i = 0; i < Bones.Length; i++)
        {
            var matrix = _boneMatrices[i];
            int offset = i * 16;

            BoneTexture[offset + 0] = matrix.M11;
            BoneTexture[offset + 1] = matrix.M12;
            BoneTexture[offset + 2] = matrix.M13;
            BoneTexture[offset + 3] = matrix.M14;
            BoneTexture[offset + 4] = matrix.M21;
            BoneTexture[offset + 5] = matrix.M22;
            BoneTexture[offset + 6] = matrix.M23;
            BoneTexture[offset + 7] = matrix.M24;
            BoneTexture[offset + 8] = matrix.M31;
            BoneTexture[offset + 9] = matrix.M32;
            BoneTexture[offset + 10] = matrix.M33;
            BoneTexture[offset + 11] = matrix.M34;
            BoneTexture[offset + 12] = matrix.M41;
            BoneTexture[offset + 13] = matrix.M42;
            BoneTexture[offset + 14] = matrix.M43;
            BoneTexture[offset + 15] = matrix.M44;
        }

        _matricesNeedUpdate = false;
    }

    /// <summary>
    /// Gets the bone by name
    /// </summary>
    public Bone? GetBoneByName(string name)
    {
        return Array.Find(Bones, bone => bone.Name == name);
    }

    /// <summary>
    /// Gets the bone index by name
    /// </summary>
    public int GetBoneIndexByName(string name)
    {
        return Array.FindIndex(Bones, bone => bone.Name == name);
    }
}
