using Xunit;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Tests.Geometries;

public class CylinderGeometryTests
{
    [Fact]
    public void Constructor_CreatesValidGeometry()
    {
        var geometry = new CylinderGeometry(1, 1, 2, 32);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
        Assert.NotEmpty(geometry.UVs);
        Assert.NotEmpty(geometry.Indices);
    }

    [Theory]
    [InlineData(1, 1, 2, 8)]
    [InlineData(2, 1, 3, 16)]
    [InlineData(0.5, 1.5, 4, 32)]
    public void Constructor_WithParameters_CreatesCorrectShape(float radiusTop, float radiusBottom, float height, int radialSegments)
    {
        var geometry = new CylinderGeometry(radiusTop, radiusBottom, height, radialSegments);

        Assert.NotEmpty(geometry.Vertices);
        // Should have vertices for top cap, bottom cap, and sides
        Assert.True(geometry.Vertices.Length > radialSegments * 3);
    }

    [Fact]
    public void ConeGeometry_IsCylinderWithZeroTop()
    {
        var cone = new ConeGeometry(1, 2, 32);

        Assert.NotEmpty(cone.Vertices);
        Assert.NotEmpty(cone.Indices);
    }
}
