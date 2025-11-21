using Xunit;
using BlazorGL.Core;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Math;
using System.Numerics;

namespace BlazorGL.Tests.Lights;

public class LightTests
{
    [Fact]
    public void AmbientLight_InitializesCorrectly()
    {
        var light = new AmbientLight(new Color(1, 1, 1), 0.5f);

        Assert.Equal(0.5f, light.Intensity);
        Assert.Equal("AmbientLight", light.Name);
    }

    [Fact]
    public void DirectionalLight_HasDirection()
    {
        var light = new DirectionalLight();

        Assert.NotEqual(Vector3.Zero, light.Direction);
        Assert.False(light.CastShadow); // Default
    }

    [Fact]
    public void DirectionalLight_CanCastShadows()
    {
        var light = new DirectionalLight { CastShadow = true };

        Assert.True(light.CastShadow);
        Assert.NotNull(light.Shadow);
    }

    [Fact]
    public void PointLight_HasDistanceAndDecay()
    {
        var light = new PointLight(new Color(1, 0, 0), 1.0f, 100f, 2.0f);

        Assert.Equal(100f, light.Distance);
        Assert.Equal(2.0f, light.Decay);
    }

    [Fact]
    public void PointLight_CanCastShadows()
    {
        var light = new PointLight { CastShadow = true };

        Assert.True(light.CastShadow);
        Assert.NotNull(light.Shadow);
        Assert.Equal(6, light.Shadow.Cameras.Length); // Cubemap: 6 faces
    }

    [Fact]
    public void SpotLight_HasAngleAndPenumbra()
    {
        var light = new SpotLight();

        Assert.True(light.Angle > 0);
        Assert.InRange(light.Penumbra, 0f, 1f);
    }

    [Fact]
    public void SpotLight_CanCastShadows()
    {
        var light = new SpotLight { CastShadow = true };

        Assert.True(light.CastShadow);
        Assert.NotNull(light.Shadow);
    }

    [Fact]
    public void HemisphereLight_HasSkyAndGroundColors()
    {
        var skyColor = new Color(0.5f, 0.7f, 1.0f);
        var groundColor = new Color(0.3f, 0.2f, 0.1f);

        var light = new HemisphereLight(skyColor, groundColor, 1.0f);

        Assert.Equal(skyColor, light.SkyColor);
        Assert.Equal(groundColor, light.GroundColor);
    }

    [Fact]
    public void RectAreaLight_HasWidthAndHeight()
    {
        var light = new RectAreaLight(new Color(1, 1, 1), 1.0f, 2.0f, 3.0f);

        Assert.Equal(2.0f, light.Width);
        Assert.Equal(3.0f, light.Height);
    }

    [Fact]
    public void LightProbe_HasSphericalHarmonics()
    {
        var light = new LightProbe();

        Assert.NotNull(light.SphericalHarmonics);
        Assert.Equal(9, light.SphericalHarmonics.Length);
    }

    [Fact]
    public void Light_InheritsFromObject3D()
    {
        var light = new PointLight();

        Assert.IsAssignableFrom<Object3D>(light);
        light.Position = new Vector3(5, 10, 15);
        Assert.Equal(new Vector3(5, 10, 15), light.Position);
    }

    [Fact]
    public void Light_CanBeAddedToScene()
    {
        var scene = new Scene();
        var light = new DirectionalLight();

        scene.AddChild(light);

        Assert.Contains(light, scene.Children);
    }
}
