using Xunit;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Tests.Geometries;

public class SphereGeometryTests
{
    [Fact]
    public void Constructor_CreatesValidGeometry()
    {
        // Arrange & Act
        var geometry = new SphereGeometry(1, 32, 16);

        // Assert
        Assert.NotNull(geometry);
        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
        Assert.NotEmpty(geometry.UVs);
        Assert.NotEmpty(geometry.Indices);
    }

    [Theory]
    [InlineData(1, 8, 6)]
    [InlineData(2, 16, 8)]
    [InlineData(0.5, 32, 16)]
    public void Constructor_WithParameters_CreatesCorrectVertexCount(float radius, int widthSegments, int heightSegments)
    {
        // Arrange & Act
        var geometry = new SphereGeometry(radius, widthSegments, heightSegments);

        // Assert
        int expectedVertexCount = (widthSegments + 1) * (heightSegments + 1);
        Assert.Equal(expectedVertexCount * 3, geometry.Vertices.Length);
    }

    [Fact]
    public void Vertices_AreOnSphereSurface()
    {
        // Arrange
        float radius = 5.0f;
        var geometry = new SphereGeometry(radius, 32, 16);

        // Act & Assert
        for (int i = 0; i < geometry.Vertices.Length; i += 3)
        {
            float x = geometry.Vertices[i];
            float y = geometry.Vertices[i + 1];
            float z = geometry.Vertices[i + 2];

            float distance = MathF.Sqrt(x * x + y * y + z * z);
            Assert.InRange(distance, radius - 0.01f, radius + 0.01f);
        }
    }

    [Fact]
    public void Normals_PointOutward()
    {
        // Arrange
        var geometry = new SphereGeometry(1, 16, 8);

        // Act & Assert - normals should point away from center
        for (int i = 0; i < geometry.Normals.Length; i += 3)
        {
            float nx = geometry.Normals[i];
            float ny = geometry.Normals[i + 1];
            float nz = geometry.Normals[i + 2];

            float vx = geometry.Vertices[i];
            float vy = geometry.Vertices[i + 1];
            float vz = geometry.Vertices[i + 2];

            // Dot product should be positive (normal points away from origin)
            float dot = nx * vx + ny * vy + nz * vz;
            Assert.True(dot > 0, $"Normal at vertex {i / 3} points inward");
        }
    }

    [Fact]
    public void BoundingSphere_IsCorrect()
    {
        // Arrange
        float radius = 3.5f;
        var geometry = new SphereGeometry(radius, 32, 16);

        // Act
        var bsphere = geometry.BoundingSphere;

        // Assert
        Assert.InRange(bsphere.Radius, radius - 0.01f, radius + 0.01f);
        Assert.InRange(bsphere.Center.X, -0.01f, 0.01f);
        Assert.InRange(bsphere.Center.Y, -0.01f, 0.01f);
        Assert.InRange(bsphere.Center.Z, -0.01f, 0.01f);
    }
}
