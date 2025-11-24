using Xunit;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Shaders.ShaderChunks;
using System;
using System.Numerics;

namespace BlazorGL.Tests.Shadows;

/// <summary>
/// Tests for CSM (Cascaded Shadow Maps) implementation
/// </summary>
public class CSMTests
{
    [Fact]
    public void DirectionalLightCSM_CanBeCreated()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);

        // Act
        var csm = new DirectionalLightCSM(light, camera);

        // Assert
        Assert.NotNull(csm);
        Assert.Equal(light, csm.Light);
    }

    [Fact]
    public void CSM_DefaultCascadeCount_Is3()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Assert
        Assert.Equal(3, csm.CascadeCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CSM_CascadeCount_CanBeSet(int count)
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);

        // Act
        var csm = new DirectionalLightCSM(light, camera)
        {
            CascadeCount = count
        };

        // Assert
        Assert.Equal(count, csm.CascadeCount);
    }

    [Fact]
    public void CSM_CreatesCorrectNumberOfCascades()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera)
        {
            CascadeCount = 4
        };

        // Act
        csm.UpdateCascades(camera);

        // Assert
        Assert.Equal(4, csm.Cascades.Count);
    }

    [Fact]
    public void CSM_EachCascade_HasShadowMap()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Act
        csm.UpdateCascades(camera);

        // Assert
        foreach (var cascade in csm.Cascades)
        {
            Assert.NotNull(cascade.ShadowMap);
            Assert.NotNull(cascade.ShadowCamera);
        }
    }

    [Fact]
    public void CSM_EachCascade_HasOrthographicCamera()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Act
        csm.UpdateCascades(camera);

        // Assert
        foreach (var cascade in csm.Cascades)
        {
            Assert.IsType<OrthographicCamera>(cascade.ShadowCamera);
        }
    }

    [Fact]
    public void CSM_Lambda_DefaultValue_Is05()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Assert
        Assert.Equal(0.5f, csm.Lambda);
    }

    [Theory]
    [InlineData(0.0f)] // Uniform splitting
    [InlineData(0.5f)] // Balanced
    [InlineData(1.0f)] // Logarithmic splitting
    public void CSM_Lambda_AcceptsValidRange(float lambda)
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);

        // Act
        var csm = new DirectionalLightCSM(light, camera)
        {
            Lambda = lambda
        };

        // Assert
        Assert.Equal(lambda, csm.Lambda);
    }

    [Fact]
    public void CSM_SplitDistances_AreIncreasing()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera)
        {
            CascadeCount = 3,
            MaxDistance = 500f
        };

        // Act
        csm.UpdateCascades(camera);

        // Assert
        var splits = csm.SplitDistances;
        for (int i = 0; i < splits.Count - 1; i++)
        {
            Assert.True(splits[i] < splits[i + 1], $"Split {i} ({splits[i]}) should be less than split {i+1} ({splits[i+1]})");
        }
    }

    [Fact]
    public void CSM_FirstSplit_EqualsCameraNear()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Act
        csm.UpdateCascades(camera);

        // Assert
        Assert.Equal(camera.Near, csm.SplitDistances[0]);
    }

    [Fact]
    public void CSM_LastSplit_EqualsMaxDistance()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        float maxDist = 800f;
        var csm = new DirectionalLightCSM(light, camera)
        {
            MaxDistance = maxDist
        };

        // Act
        csm.UpdateCascades(camera);

        // Assert
        Assert.Equal(maxDist, csm.SplitDistances[csm.SplitDistances.Count - 1]);
    }

    [Fact]
    public void CSM_GetCascadeIndex_ReturnsCorrectIndex()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera)
        {
            CascadeCount = 3,
            MaxDistance = 100f
        };
        csm.UpdateCascades(camera);

        // Act & Assert
        Assert.Equal(0, csm.GetCascadeIndex(-5f));  // Near
        Assert.Equal(2, csm.GetCascadeIndex(-95f)); // Far
    }

    [Fact]
    public void CSM_BlendingEnabled_ByDefault()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Assert
        Assert.True(csm.EnableCascadeBlending);
    }

    [Fact]
    public void CSM_BlendFactor_IsZero_FarFromTransition()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera)
        {
            CascadeCount = 3,
            MaxDistance = 100f
        };
        csm.UpdateCascades(camera);

        // Act
        float blend = csm.GetCascadeBlendFactor(-5f, 0);

        // Assert
        Assert.Equal(0.0f, blend);
    }

    [Fact]
    public void CSM_CascadeResolution_DefaultIs1024()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Assert
        Assert.Equal(1024, csm.CascadeResolution);
    }

    [Fact]
    public void CSM_ShaderFunction_IsDefinedInChunks()
    {
        // Assert
        Assert.Contains("getShadowCSM", ShadowMapChunks.CSMShadowMap);
        Assert.Contains("selectCascade", ShadowMapChunks.CSMShadowMap);
        Assert.Contains("cascadeShadowMaps", ShadowMapChunks.CSMShadowMap);
    }

    [Fact]
    public void CSM_ShaderSupportsBlending()
    {
        // Assert
        Assert.Contains("getShadowCSMBlended", ShadowMapChunks.CSMShadowMap);
        Assert.Contains("blendFactor", ShadowMapChunks.CSMShadowMap);
        Assert.Contains("mix(shadow, nextShadow", ShadowMapChunks.CSMShadowMap);
    }

    [Fact]
    public void DirectionalLight_CanEnableCSM()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);

        // Act
        light.EnableCSM(camera, 3, 500f);

        // Assert
        Assert.True(light.UseCSM);
        Assert.NotNull(light.CSM);
        Assert.Equal(3, light.CSM.CascadeCount);
        Assert.Equal(500f, light.CSM.MaxDistance);
    }

    [Fact]
    public void DirectionalLight_CanDisableCSM()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        light.EnableCSM(camera);

        // Act
        light.DisableCSM();

        // Assert
        Assert.False(light.UseCSM);
        Assert.Null(light.CSM);
    }

    [Fact]
    public void CSM_DisposesAllCascades()
    {
        // Arrange
        var light = new DirectionalLight();
        var camera = new PerspectiveCamera(45, 16.0f / 9.0f, 0.1f, 1000f);
        var csm = new DirectionalLightCSM(light, camera);

        // Act
        csm.Dispose();

        // Assert - should not throw
        Assert.Empty(csm.Cascades);
    }
}
