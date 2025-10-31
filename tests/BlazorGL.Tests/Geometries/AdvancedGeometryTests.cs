using Xunit;
using BlazorGL.Core.Geometries;
using System.Numerics;

namespace BlazorGL.Tests.Geometries;

public class AdvancedGeometryTests
{
    [Fact]
    public void TorusGeometry_CreatesValidGeometry()
    {
        var geometry = new TorusGeometry(2, 0.5f, 16, 32);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
        Assert.NotEmpty(geometry.UVs);
        Assert.NotEmpty(geometry.Indices);
    }

    [Fact]
    public void TorusKnotGeometry_CreatesValidGeometry()
    {
        var geometry = new TorusKnotGeometry(1, 0.4f, 64, 8);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
    }

    [Fact]
    public void CapsuleGeometry_CreatesValidGeometry()
    {
        var geometry = new CapsuleGeometry(1, 2, 8, 16);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
    }

    [Fact]
    public void CircleGeometry_CreatesValidGeometry()
    {
        var geometry = new CircleGeometry(1, 32);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.UVs);
    }

    [Fact]
    public void RingGeometry_CreatesValidGeometry()
    {
        var geometry = new RingGeometry(0.5f, 1.5f, 32);

        Assert.NotEmpty(geometry.Vertices);
        Assert.True(geometry.Vertices.Length > 0);
    }

    [Fact]
    public void IcosahedronGeometry_CreatesValidGeometry()
    {
        var geometry = new IcosahedronGeometry(1, 0);

        Assert.NotEmpty(geometry.Vertices);
        // Icosahedron has 12 vertices at detail level 0
        Assert.True(geometry.Vertices.Length >= 12 * 3);
    }

    [Fact]
    public void OctahedronGeometry_CreatesValidGeometry()
    {
        var geometry = new OctahedronGeometry(1, 0);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Indices);
    }

    [Fact]
    public void TetrahedronGeometry_CreatesValidGeometry()
    {
        var geometry = new TetrahedronGeometry(1, 0);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Indices);
    }

    [Fact]
    public void DodecahedronGeometry_CreatesValidGeometry()
    {
        var geometry = new DodecahedronGeometry(1, 0);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Indices);
    }

    [Fact]
    public void TubeGeometry_CreatesValidGeometry()
    {
        var path = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0)
        };

        var geometry = new TubeGeometry(path, 16, 0.2f, 8, false);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
    }

    [Fact]
    public void LatheGeometry_CreatesValidGeometry()
    {
        var points = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0.5f, 0.5f),
            new Vector2(0, 1)
        };

        var geometry = new LatheGeometry(points, 32);

        Assert.NotEmpty(geometry.Vertices);
        Assert.NotEmpty(geometry.Normals);
    }

    [Fact]
    public void EdgesGeometry_ExtractsEdges()
    {
        var box = new BoxGeometry(1, 1, 1);
        var edges = new EdgesGeometry(box, 1);

        Assert.NotEmpty(edges.Vertices);
        // Edges should have fewer vertices than original
    }

    [Fact]
    public void WireframeGeometry_CreatesWireframe()
    {
        var sphere = new SphereGeometry(1, 8, 6);
        var wireframe = new WireframeGeometry(sphere);

        Assert.NotEmpty(wireframe.Vertices);
    }

    [Fact]
    public void BufferGeometry_CanSetCustomAttributes()
    {
        var geometry = new BufferGeometry();

        var positions = new float[] { 0, 0, 0, 1, 0, 0, 0, 1, 0 };
        var normals = new float[] { 0, 0, 1, 0, 0, 1, 0, 0, 1 };

        geometry.SetAttribute("position", positions, 3);
        geometry.SetAttribute("normal", normals, 3);

        Assert.Equal(positions, geometry.Vertices);
        Assert.Equal(normals, geometry.Normals);
    }
}
