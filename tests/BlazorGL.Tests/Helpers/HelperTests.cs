using Xunit;
using BlazorGL.Core.Helpers;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Math;
using System.Numerics;

namespace BlazorGL.Tests.Helpers;

public class HelperTests
{
    [Fact]
    public void AxesHelper_CreatesThreeAxes()
    {
        var helper = new AxesHelper(5.0f);

        Assert.NotNull(helper.Geometry);
        Assert.NotNull(helper.Material);
        // 3 axes * 2 vertices per line = 6 vertices * 3 components
        Assert.Equal(18, helper.Geometry.Vertices.Length);
    }

    [Fact]
    public void GridHelper_CreatesGrid()
    {
        var helper = new GridHelper(10, 10);

        Assert.NotNull(helper.Geometry);
        Assert.NotNull(helper.Material);
        Assert.NotEmpty(helper.Geometry.Vertices);
    }

    [Fact]
    public void GridHelper_WithCustomColors()
    {
        var centerColor = new Color(1, 0, 0);
        var gridColor = new Color(0, 1, 0);

        var helper = new GridHelper(10, 10, centerColor, gridColor);

        Assert.NotNull(helper);
        Assert.NotEmpty(helper.Geometry.Vertices);
    }

    [Fact]
    public void PolarGridHelper_CreatesRadialGrid()
    {
        var helper = new PolarGridHelper(10, 16, 8, 64);

        Assert.NotNull(helper.Geometry);
        Assert.NotEmpty(helper.Geometry.Vertices);
    }

    [Fact]
    public void BoxHelper_CreatesWireframeBox()
    {
        var helper = new BoxHelper();

        Assert.NotNull(helper.Geometry);
        Assert.NotNull(helper.Material);
        // 12 edges * 2 vertices per edge * 3 components
        Assert.Equal(72, helper.Geometry.Vertices.Length);
    }

    [Fact]
    public void Box3Helper_CreatesFromBoundingBox()
    {
        var bbox = new BoundingBox
        {
            Min = new Vector3(-1, -1, -1),
            Max = new Vector3(1, 1, 1)
        };

        var helper = new Box3Helper(bbox);

        Assert.NotNull(helper.Geometry);
        Assert.Equal(72, helper.Geometry.Vertices.Length); // 12 edges
    }

    [Fact]
    public void ArrowHelper_CreatesArrow()
    {
        var direction = new Vector3(0, 1, 0);
        var origin = Vector3.Zero;

        var helper = new ArrowHelper(direction, origin, 5.0f);

        Assert.NotNull(helper);
        Assert.Equal(2, helper.Children.Count); // Line + cone
    }

    [Fact]
    public void PlaneHelper_CreatesPlane()
    {
        var helper = new PlaneHelper(10.0f);

        Assert.NotNull(helper.Geometry);
        Assert.NotEmpty(helper.Geometry.Vertices);
    }

    [Fact]
    public void DirectionalLightHelper_VisualizesLight()
    {
        var light = new DirectionalLight();
        var helper = new DirectionalLightHelper(light, 2.0f);

        Assert.NotNull(helper);
        Assert.Equal(2, helper.Children.Count); // Plane + line
    }

    [Fact]
    public void PointLightHelper_VisualizesLight()
    {
        var light = new PointLight();
        var helper = new PointLightHelper(light, 1.0f);

        Assert.NotNull(helper);
        Assert.NotNull(helper.Geometry);
    }

    [Fact]
    public void SpotLightHelper_VisualizesLight()
    {
        var light = new SpotLight();
        var helper = new SpotLightHelper(light);

        Assert.NotNull(helper);
        Assert.Single(helper.Children); // Cone
    }

    [Fact]
    public void HemisphereLightHelper_VisualizesLight()
    {
        var light = new HemisphereLight();
        var helper = new HemisphereLightHelper(light, 1.0f);

        Assert.NotNull(helper);
        Assert.Single(helper.Children); // Octahedron mesh
    }

    [Fact]
    public void CameraHelper_VisualizesFrustum()
    {
        var camera = new PerspectiveCamera();
        var helper = new CameraHelper(camera);

        Assert.NotNull(helper);
        Assert.NotNull(helper.Geometry);
    }

    [Fact]
    public void SkeletonHelper_VisualizeBones()
    {
        var root = new Bone("root");
        var child1 = new Bone("child1");
        var child2 = new Bone("child2");

        root.AddChild(child1);
        root.AddChild(child2);

        var helper = new SkeletonHelper(root);

        Assert.NotNull(helper);
        Assert.NotNull(helper.Geometry);
    }
}
