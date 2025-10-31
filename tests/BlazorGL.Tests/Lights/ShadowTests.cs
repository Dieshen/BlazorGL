using Xunit;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Cameras;

namespace BlazorGL.Tests.Lights;

public class ShadowTests
{
    [Fact]
    public void DirectionalLightShadow_HasOrthographicCamera()
    {
        var light = new DirectionalLight { CastShadow = true };

        Assert.NotNull(light.Shadow);
        Assert.NotNull(light.Shadow.Camera);
        Assert.IsType<OrthographicCamera>(light.Shadow.Camera);
    }

    [Fact]
    public void SpotLightShadow_HasPerspectiveCamera()
    {
        var light = new SpotLight { CastShadow = true };

        Assert.NotNull(light.Shadow);
        Assert.NotNull(light.Shadow.Camera);
        Assert.IsType<PerspectiveCamera>(light.Shadow.Camera);
    }

    [Fact]
    public void PointLightShadow_HasSixCameras()
    {
        var light = new PointLight { CastShadow = true };

        Assert.NotNull(light.Shadow);
        Assert.Equal(6, light.Shadow.Cameras.Length);

        // All cameras should be perspective for cubemap
        foreach (var camera in light.Shadow.Cameras)
        {
            Assert.IsType<PerspectiveCamera>(camera);
        }
    }

    [Fact]
    public void LightShadow_HasDefaultMapSize()
    {
        var light = new DirectionalLight { CastShadow = true };

        Assert.Equal(512, light.Shadow.Width);
        Assert.Equal(512, light.Shadow.Height);
    }

    [Fact]
    public void LightShadow_CanSetBias()
    {
        var light = new DirectionalLight { CastShadow = true };

        light.Shadow.Bias = 0.001f;

        Assert.Equal(0.001f, light.Shadow.Bias);
    }

    [Fact]
    public void LightShadow_CanSetRadius()
    {
        var light = new DirectionalLight { CastShadow = true };

        light.Shadow.Radius = 2.0f;

        Assert.Equal(2.0f, light.Shadow.Radius);
    }

    [Fact]
    public void LightShadow_CanInitializeRenderTarget()
    {
        var light = new DirectionalLight { CastShadow = true };

        light.Shadow.Initialize();

        Assert.NotNull(light.Shadow.Map);
    }
}
