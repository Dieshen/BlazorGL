using Xunit;
using BlazorGL.Core;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Tests.Core;

public class SkeletalAnimationTests
{
    [Fact]
    public void Bone_IsObject3D()
    {
        var bone = new Bone("TestBone");

        Assert.IsAssignableFrom<Object3D>(bone);
        Assert.Equal("TestBone", bone.Name);
        Assert.Equal("Bone", bone.Type);
    }

    [Fact]
    public void Bone_CanFormHierarchy()
    {
        var root = new Bone("root");
        var child1 = new Bone("child1");
        var child2 = new Bone("child2");

        root.AddChild(child1);
        child1.AddChild(child2);

        Assert.Equal(root, child1.Parent);
        Assert.Equal(child1, child2.Parent);
    }

    [Fact]
    public void Skeleton_InitializesWithBones()
    {
        var bones = new Bone[]
        {
            new Bone("bone1"),
            new Bone("bone2"),
            new Bone("bone3")
        };

        var skeleton = new Skeleton(bones);

        Assert.Equal(3, skeleton.Bones.Length);
        Assert.NotNull(skeleton.BoneMatrices);
        Assert.Equal(3, skeleton.BoneMatrices.Length);
    }

    [Fact]
    public void Skeleton_CalculatesBoneMatrices()
    {
        var bone = new Bone("bone1");
        bone.Position = new Vector3(5, 0, 0);
        bone.UpdateWorldMatrix(true, false);

        var skeleton = new Skeleton(new[] { bone });

        skeleton.Update();

        Assert.NotNull(skeleton.BoneMatrices);
        Assert.Single(skeleton.BoneMatrices);
    }

    [Fact]
    public void Skeleton_CreatesBoneTexture()
    {
        var bones = new Bone[]
        {
            new Bone("bone1"),
            new Bone("bone2")
        };

        var skeleton = new Skeleton(bones);

        Assert.NotNull(skeleton.BoneTexture);
        Assert.Equal(2 * 16, skeleton.BoneTexture.Length); // 2 bones * 16 floats per matrix
    }

    [Fact]
    public void Skeleton_GetBoneByName()
    {
        var bone1 = new Bone("leftArm");
        var bone2 = new Bone("rightArm");

        var skeleton = new Skeleton(new[] { bone1, bone2 });

        var found = skeleton.GetBoneByName("leftArm");

        Assert.Equal(bone1, found);
    }

    [Fact]
    public void SkinnedMesh_BindsSkeleton()
    {
        var geometry = new BoxGeometry();
        var material = new BasicMaterial();
        var bones = new Bone[] { new Bone("bone1") };
        var skeleton = new Skeleton(bones);

        var skinnedMesh = new SkinnedMesh(geometry, material, skeleton);

        Assert.Equal(skeleton, skinnedMesh.Skeleton);
    }

    [Fact]
    public void SkinnedMesh_UpdatesSkeleton()
    {
        var geometry = new BoxGeometry();
        var material = new BasicMaterial();
        var bone = new Bone("bone1");
        var skeleton = new Skeleton(new[] { bone });
        var skinnedMesh = new SkinnedMesh(geometry, material, skeleton);

        bone.Position = new Vector3(10, 0, 0);

        // Should not throw
        skinnedMesh.UpdateSkeleton();

        Assert.NotNull(skinnedMesh.Skeleton);
    }

    [Fact]
    public void SkinnedMesh_InheritsMeshProperties()
    {
        var skinnedMesh = new SkinnedMesh(
            new BoxGeometry(),
            new BasicMaterial()
        );

        Assert.IsAssignableFrom<Mesh>(skinnedMesh);
        Assert.Equal("SkinnedMesh", skinnedMesh.Type);
    }

    [Fact]
    public void Geometry_SupportsSkinAttributes()
    {
        var geometry = new BoxGeometry();

        var skinIndices = new float[24 * 4]; // 24 vertices * 4 bones per vertex
        var skinWeights = new float[24 * 4];

        geometry.SkinIndices = skinIndices;
        geometry.SkinWeights = skinWeights;

        Assert.Equal(skinIndices, geometry.SkinIndices);
        Assert.Equal(skinWeights, geometry.SkinWeights);
    }
}
