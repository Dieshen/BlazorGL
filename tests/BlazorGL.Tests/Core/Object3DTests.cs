using Xunit;
using BlazorGL.Core;
using System.Numerics;

namespace BlazorGL.Tests.Core;

public class Object3DTests
{
    [Fact]
    public void Constructor_InitializesWithIdentityTransform()
    {
        // Arrange & Act
        var obj = new Object3D();

        // Assert
        Assert.Equal(Vector3.Zero, obj.Position);
        Assert.Equal(Vector3.Zero, obj.Rotation);
        Assert.Equal(Vector3.One, obj.Scale);
        Assert.Equal(Matrix4x4.Identity, obj.LocalMatrix);
    }

    [Fact]
    public void AddChild_EstablishesParentChildRelationship()
    {
        // Arrange
        var parent = new Object3D();
        var child = new Object3D();

        // Act
        parent.AddChild(child);

        // Assert
        Assert.Contains(child, parent.Children);
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void RemoveChild_BreaksParentChildRelationship()
    {
        // Arrange
        var parent = new Object3D();
        var child = new Object3D();
        parent.AddChild(child);

        // Act
        parent.RemoveChild(child);

        // Assert
        Assert.DoesNotContain(child, parent.Children);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void UpdateWorldMatrix_WithoutParent_UsesLocalMatrix()
    {
        // Arrange
        var obj = new Object3D();
        obj.Position = new Vector3(1, 2, 3);

        // Act
        obj.UpdateWorldMatrix(true, false);

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, obj.WorldMatrix);
        var translation = obj.WorldMatrix.Translation;
        Assert.Equal(1, translation.X, 2);
        Assert.Equal(2, translation.Y, 2);
        Assert.Equal(3, translation.Z, 2);
    }

    [Fact]
    public void UpdateWorldMatrix_WithParent_CombinesTransforms()
    {
        // Arrange
        var parent = new Object3D();
        var child = new Object3D();
        parent.AddChild(child);

        parent.Position = new Vector3(10, 0, 0);
        child.Position = new Vector3(5, 0, 0);

        // Act
        parent.UpdateWorldMatrix(true, true);

        // Assert - child should be at (15, 0, 0) in world space
        var childWorldPos = child.WorldMatrix.Translation;
        Assert.InRange(childWorldPos.X, 14.9f, 15.1f);
        Assert.InRange(childWorldPos.Y, -0.1f, 0.1f);
        Assert.InRange(childWorldPos.Z, -0.1f, 0.1f);
    }

    [Fact]
    public void Position_UpdatesLocalMatrix()
    {
        // Arrange
        var obj = new Object3D();

        // Act
        obj.Position = new Vector3(5, 10, 15);
        obj.UpdateWorldMatrix(true, false);

        // Assert
        var pos = obj.WorldMatrix.Translation;
        Assert.Equal(5, pos.X, 2);
        Assert.Equal(10, pos.Y, 2);
        Assert.Equal(15, pos.Z, 2);
    }

    [Fact]
    public void Scale_AffectsWorldMatrix()
    {
        // Arrange
        var obj = new Object3D();
        obj.Scale = new Vector3(2, 2, 2);

        // Act
        obj.UpdateWorldMatrix(true, false);

        // Assert
        // Check if scaling is applied (matrix M11, M22, M33 should be ~2)
        Assert.InRange(obj.WorldMatrix.M11, 1.9f, 2.1f);
        Assert.InRange(obj.WorldMatrix.M22, 1.9f, 2.1f);
        Assert.InRange(obj.WorldMatrix.M33, 1.9f, 2.1f);
    }

    [Fact]
    public void Visible_DefaultsToTrue()
    {
        // Arrange & Act
        var obj = new Object3D();

        // Assert
        Assert.True(obj.Visible);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var obj = new Object3D();

        // Act
        obj.Name = "TestObject";

        // Assert
        Assert.Equal("TestObject", obj.Name);
    }

    [Fact]
    public void MultipleChildren_AreAllStoredCorrectly()
    {
        // Arrange
        var parent = new Object3D();
        var child1 = new Object3D();
        var child2 = new Object3D();
        var child3 = new Object3D();

        // Act
        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);

        // Assert
        Assert.Equal(3, parent.Children.Count);
        Assert.Contains(child1, parent.Children);
        Assert.Contains(child2, parent.Children);
        Assert.Contains(child3, parent.Children);
    }

    [Fact]
    public void NestedHierarchy_TransformsPropagate()
    {
        // Arrange - Create A -> B -> C hierarchy
        var objA = new Object3D { Position = new Vector3(10, 0, 0) };
        var objB = new Object3D { Position = new Vector3(10, 0, 0) };
        var objC = new Object3D { Position = new Vector3(10, 0, 0) };

        objA.AddChild(objB);
        objB.AddChild(objC);

        // Act
        objA.UpdateWorldMatrix(true, true);

        // Assert - C should be at (30, 0, 0) in world space
        var cWorldPos = objC.WorldMatrix.Translation;
        Assert.InRange(cWorldPos.X, 29.9f, 30.1f);
    }
}
