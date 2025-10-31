namespace BlazorGL.Core;

/// <summary>
/// Bone in a skeletal hierarchy
/// Used for skeletal animation and skinned mesh deformation
/// </summary>
public class Bone : Object3D
{
    public Bone()
    {
        Name = "Bone";
        Type = "Bone";
    }

    /// <summary>
    /// Creates a bone with a name
    /// </summary>
    public Bone(string name)
    {
        Name = name;
        Type = "Bone";
    }
}
