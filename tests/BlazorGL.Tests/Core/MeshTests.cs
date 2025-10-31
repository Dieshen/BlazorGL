using Xunit;
using BlazorGL.Core;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Tests.Core;

public class MeshTests
{
    [Fact]
    public void Constructor_AssignsGeometryAndMaterial()
    {
        // Arrange
        var geometry = new BoxGeometry();
        var material = new BasicMaterial();

        // Act
        var mesh = new Mesh(geometry, material);

        // Assert
        Assert.Equal(geometry, mesh.Geometry);
        Assert.Equal(material, mesh.Material);
    }

    [Fact]
    public void Constructor_SetsTypeName()
    {
        // Arrange
        var geometry = new BoxGeometry();
        var material = new BasicMaterial();

        // Act
        var mesh = new Mesh(geometry, material);

        // Assert
        Assert.Equal("Mesh", mesh.Type);
    }

    [Fact]
    public void Mesh_IsObject3D()
    {
        // Arrange
        var geometry = new BoxGeometry();
        var material = new BasicMaterial();

        // Act
        var mesh = new Mesh(geometry, material);

        // Assert
        Assert.IsAssignableFrom<Object3D>(mesh);
    }

    [Fact]
    public void Mesh_CanBeAddedToSceneGraph()
    {
        // Arrange
        var parent = new Object3D();
        var mesh = new Mesh(new BoxGeometry(), new BasicMaterial());

        // Act
        parent.AddChild(mesh);

        // Assert
        Assert.Contains(mesh, parent.Children);
        Assert.Equal(parent, mesh.Parent);
    }

    [Fact]
    public void Mesh_InheritsTransformProperties()
    {
        // Arrange
        var mesh = new Mesh(new BoxGeometry(), new BasicMaterial());

        // Act
        mesh.Position = new System.Numerics.Vector3(1, 2, 3);
        mesh.UpdateWorldMatrix(true, false);

        // Assert
        var pos = mesh.WorldMatrix.Translation;
        Assert.InRange(pos.X, 0.9f, 1.1f);
        Assert.InRange(pos.Y, 1.9f, 2.1f);
        Assert.InRange(pos.Z, 2.9f, 3.1f);
    }
}
