using Xunit;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Tests.Geometries;

public class BoxGeometryTests
{
    [Fact]
    public void Constructor_CreatesValidGeometry()
    {
        // Arrange & Act
        var geometry = new BoxGeometry(2, 3, 4);

        // Assert
        Assert.NotNull(geometry);
        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
        Assert.NotEmpty(geometry.UVs);
        Assert.NotEmpty(geometry.Indices);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_CreatesUnitCube()
    {
        // Arrange & Act
        var geometry = new BoxGeometry();

        // Assert
        Assert.NotEmpty(geometry.Vertices);
        // 24 vertices (6 faces * 4 vertices per face)
        Assert.Equal(24 * 3, geometry.Vertices.Length); // x3 for x,y,z
    }

    [Theory]
    [InlineData(2, 3, 4)]
    [InlineData(1, 1, 1)]
    [InlineData(0.5, 2.5, 1.5)]
    public void Constructor_WithDimensions_CreatesCorrectSize(float width, float height, float depth)
    {
        // Arrange & Act
        var geometry = new BoxGeometry(width, height, depth);

        // Assert
        Assert.NotEmpty(geometry.Vertices);

        // Check that vertices are within bounds
        for (int i = 0; i < geometry.Vertices.Length; i += 3)
        {
            float x = geometry.Vertices[i];
            float y = geometry.Vertices[i + 1];
            float z = geometry.Vertices[i + 2];

            Assert.InRange(x, -width / 2 - 0.001f, width / 2 + 0.001f);
            Assert.InRange(y, -height / 2 - 0.001f, height / 2 + 0.001f);
            Assert.InRange(z, -depth / 2 - 0.001f, depth / 2 + 0.001f);
        }
    }

    [Fact]
    public void Normals_AreNormalized()
    {
        // Arrange
        var geometry = new BoxGeometry(2, 3, 4);

        // Act & Assert
        for (int i = 0; i < geometry.Normals.Length; i += 3)
        {
            float x = geometry.Normals[i];
            float y = geometry.Normals[i + 1];
            float z = geometry.Normals[i + 2];

            float length = MathF.Sqrt(x * x + y * y + z * z);
            Assert.InRange(length, 0.99f, 1.01f); // Allow small floating point error
        }
    }

    [Fact]
    public void UVs_AreInValidRange()
    {
        // Arrange
        var geometry = new BoxGeometry(2, 3, 4);

        // Act & Assert
        for (int i = 0; i < geometry.UVs.Length; i += 2)
        {
            float u = geometry.UVs[i];
            float v = geometry.UVs[i + 1];

            Assert.InRange(u, 0f, 1f);
            Assert.InRange(v, 0f, 1f);
        }
    }

    [Fact]
    public void Indices_FormValidTriangles()
    {
        // Arrange
        var geometry = new BoxGeometry(2, 3, 4);

        // Act & Assert
        Assert.Equal(0, geometry.Indices.Length % 3); // Must be divisible by 3 (triangles)

        int vertexCount = geometry.Vertices.Length / 3;
        foreach (var index in geometry.Indices)
        {
            Assert.InRange(index, 0u, (uint)vertexCount - 1);
        }
    }

    [Fact]
    public void BoundingBox_IsCorrect()
    {
        // Arrange
        float width = 4, height = 6, depth = 8;
        var geometry = new BoxGeometry(width, height, depth);

        // Act
        var bbox = geometry.BoundingBox;

        // Assert
        Assert.InRange(bbox.Min.X, -width / 2 - 0.01f, -width / 2 + 0.01f);
        Assert.InRange(bbox.Min.Y, -height / 2 - 0.01f, -height / 2 + 0.01f);
        Assert.InRange(bbox.Min.Z, -depth / 2 - 0.01f, -depth / 2 + 0.01f);

        Assert.InRange(bbox.Max.X, width / 2 - 0.01f, width / 2 + 0.01f);
        Assert.InRange(bbox.Max.Y, height / 2 - 0.01f, height / 2 + 0.01f);
        Assert.InRange(bbox.Max.Z, depth / 2 - 0.01f, depth / 2 + 0.01f);
    }
}
