using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a skeleton's bone hierarchy
/// </summary>
public class SkeletonHelper : LineSegments
{
    private Object3D _root;

    public SkeletonHelper(Object3D root)
    {
        Name = "SkeletonHelper";
        _root = root;

        var geometry = new BufferGeometry();
        var vertices = new List<float>();

        // Recursively build bone lines
        BuildBoneLines(root, vertices);

        geometry.SetAttribute("position", vertices.ToArray(), 3);

        var material = new LineBasicMaterial
        {
            Color = new Math.Color(0, 1, 1), // Cyan
            VertexColors = false
        };

        Geometry = geometry;
        Material = material;
    }

    private void BuildBoneLines(Object3D bone, List<float> vertices)
    {
        if (bone.Children.Count == 0)
            return;

        var bonePos = Vector3.Transform(Vector3.Zero, bone.WorldMatrix);

        foreach (var child in bone.Children)
        {
            if (child is Bone)
            {
                var childPos = Vector3.Transform(Vector3.Zero, child.WorldMatrix);

                // Line from parent to child
                vertices.AddRange(new[] { bonePos.X, bonePos.Y, bonePos.Z });
                vertices.AddRange(new[] { childPos.X, childPos.Y, childPos.Z });

                // Recurse
                BuildBoneLines(child, vertices);
            }
        }
    }

    public void Update()
    {
        // Rebuild bone lines
        var vertices = new List<float>();
        BuildBoneLines(_root, vertices);

        if (Geometry is BufferGeometry bufferGeom)
        {
            bufferGeom.SetAttribute("position", vertices.ToArray(), 3);
        }
    }
}
