using Xunit;
using BlazorGL.Core;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Tests.Core;

public class InstancedMeshTests
{
    [Fact]
    public void Constructor_InitializesWithCount()
    {
        var geometry = new BoxGeometry();
        var material = new BasicMaterial();

        var instancedMesh = new InstancedMesh(geometry, material, 100);

        Assert.Equal(100, instancedMesh.Count);
        Assert.NotNull(instancedMesh.InstanceMatrices);
        Assert.Equal(100, instancedMesh.InstanceMatrices.Length);
    }

    [Fact]
    public void InstanceMatrices_InitializeToIdentity()
    {
        var instancedMesh = new InstancedMesh(
            new BoxGeometry(),
            new BasicMaterial(),
            10
        );

        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(Matrix4x4.Identity, instancedMesh.GetMatrixAt(i));
        }
    }

    [Fact]
    public void SetMatrixAt_UpdatesMatrix()
    {
        var instancedMesh = new InstancedMesh(
            new BoxGeometry(),
            new BasicMaterial(),
            5
        );

        var matrix = Matrix4x4.CreateTranslation(5, 10, 15);
        instancedMesh.SetMatrixAt(2, matrix);

        Assert.Equal(matrix, instancedMesh.GetMatrixAt(2));
        Assert.True(instancedMesh.MatricesNeedUpdate);
    }

    [Fact]
    public void SetColorAt_UpdatesInstanceColor()
    {
        var instancedMesh = new InstancedMesh(
            new BoxGeometry(),
            new BasicMaterial(),
            5
        );

        var color = new Vector3(1, 0, 0); // Red
        instancedMesh.SetColorAt(3, color);

        Assert.Equal(color, instancedMesh.GetColorAt(3));
        Assert.True(instancedMesh.ColorsNeedUpdate);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void SetMatrixAt_WithInvalidIndex_ThrowsException(int index)
    {
        var instancedMesh = new InstancedMesh(
            new BoxGeometry(),
            new BasicMaterial(),
            10
        );

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            instancedMesh.SetMatrixAt(index, Matrix4x4.Identity)
        );
    }

    [Fact]
    public void InstancedMesh_InheritsMeshProperties()
    {
        var instancedMesh = new InstancedMesh(
            new BoxGeometry(),
            new BasicMaterial(),
            50
        );

        Assert.IsAssignableFrom<Mesh>(instancedMesh);
        Assert.NotNull(instancedMesh.Geometry);
        Assert.NotNull(instancedMesh.Material);
    }
}
