using Xunit;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Tests.Geometries;

public class PlaneGeometryTests
{
    [Fact]
    public void Constructor_CreatesValidGeometry()
    {
        // Arrange & Act
        var geometry = new PlaneGeometry(10, 5);

        // Assert
        Assert.NotNull(geometry);
        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
        Assert.NotEmpty(geometry.UVs);
        Assert.NotEmpty(geometry.Indices);
    }

    [Theory]
    [InlineData(1, 1, 1, 1)]
    [InlineData(10, 5, 2, 3)]
    [InlineData(4, 4, 4, 4)]
    public void Constructor_CreatesCorrectVertexCount(float width, float height, int widthSegments, int heightSegments)
    {
        // Arrange & Act
        var geometry = new PlaneGeometry(width, height, widthSegments, heightSegments);

        // Assert
        int expectedVertexCount = (widthSegments + 1) * (heightSegments + 1);
        Assert.Equal(expectedVertexCount * 3, geometry.Vertices.Length);
    }

    [Fact]
    public void Vertices_ArePlanar()
    {
        // Arrange
        var geometry = new PlaneGeometry(10, 5);

        // Act & Assert - all vertices should have z = 0 (or close to it)
        for (int i = 2; i < geometry.Vertices.Length; i += 3)
        {
            float z = geometry.Vertices[i];
            Assert.InRange(z, -0.001f, 0.001f);
        }
    }

    [Fact]
    public void Normals_PointUpward()
    {
        // Arrange
        var geometry = new PlaneGeometry(10, 5);

        // Act & Assert - normals should point in +Z direction
        for (int i = 0; i < geometry.Normals.Length; i += 3)
        {
            float nx = geometry.Normals[i];
            float ny = geometry.Normals[i + 1];
            float nz = geometry.Normals[i + 2];

            Assert.InRange(nx, -0.001f, 0.001f);
            Assert.InRange(ny, -0.001f, 0.001f);
            Assert.InRange(nz, 0.999f, 1.001f);
        }
    }

    [Fact]
    public void WithSegments_CreatesSubdividedPlane()
    {
        // Arrange
        int segments = 4;
        var geometry = new PlaneGeometry(10, 10, segments, segments);

        // Act
        int expectedVertices = (segments + 1) * (segments + 1);
        int expectedTriangles = segments * segments * 2;

        // Assert
        Assert.Equal(expectedVertices * 3, geometry.Vertices.Length);
        Assert.Equal(expectedTriangles * 3, geometry.Indices.Length);
    }
}
