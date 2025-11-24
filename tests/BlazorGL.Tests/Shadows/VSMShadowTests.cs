using Xunit;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Shaders.ShaderChunks;
using System;

namespace BlazorGL.Tests.Shadows;

/// <summary>
/// Tests for VSM (Variance Shadow Maps) implementation
/// </summary>
public class VSMShadowTests
{
    [Fact]
    public void VSMShadowMap_CanBeCreated()
    {
        // Act
        var vsm = new VSMShadowMap(1024, 1024);

        // Assert
        Assert.NotNull(vsm);
        Assert.Equal(1024, vsm.Width);
        Assert.Equal(1024, vsm.Height);
    }

    [Fact]
    public void VSMShadowMap_CreatesThreeRenderTargets()
    {
        // Act
        var vsm = new VSMShadowMap(512, 512);

        // Assert
        Assert.NotNull(vsm.ShadowMapTarget);
        Assert.NotNull(vsm.HorizontalBlurTarget);
        Assert.NotNull(vsm.BlurredShadowMapTarget);
    }

    [Fact]
    public void MinVariance_DefaultValue_IsTiny()
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(0.00001f, shadow.MinVariance);
    }

    [Fact]
    public void MinVariance_CanBeAdjusted()
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.MinVariance = 0.0001f;

        // Assert
        Assert.Equal(0.0001f, shadow.MinVariance);
    }

    [Fact]
    public void LightBleedingReduction_DefaultValue_Is01()
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(0.1f, shadow.LightBleedingReduction);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.1f)]
    [InlineData(0.3f)]
    [InlineData(0.5f)]
    public void LightBleedingReduction_AcceptsValidRange(float reduction)
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.LightBleedingReduction = reduction;

        // Assert
        Assert.Equal(reduction, shadow.LightBleedingReduction);
    }

    [Fact]
    public void BlurSize_DefaultValue_Is3()
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(3, shadow.BlurSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    public void BlurSize_CanBeSet_ToOddValues(int size)
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.BlurSize = size;

        // Assert
        Assert.Equal(size, shadow.BlurSize);
    }

    [Fact]
    public void VSMShaderFunction_IsDefinedInChunks()
    {
        // Assert
        Assert.Contains("getShadowVSM", ShadowMapChunks.VSMShadowMap);
        Assert.Contains("moments", ShadowMapChunks.VSMShadowMap);
        Assert.Contains("variance", ShadowMapChunks.VSMShadowMap);
    }

    [Fact]
    public void VSMShader_ImplementsChebyshevInequality()
    {
        // Assert - VSM should use statistical filtering
        Assert.Contains("variance", ShadowMapChunks.VSMShadowMap);
        Assert.Contains("moments", ShadowMapChunks.VSMShadowMap);
        Assert.Contains("pMax", ShadowMapChunks.VSMShadowMap);
    }

    [Fact]
    public void VSMDepthPacking_StoresDepthAndDepthSquared()
    {
        // Assert
        Assert.Contains("packDepthToVSM", ShadowMapChunks.VSMDepthPacking);
        Assert.Contains("moment1", ShadowMapChunks.VSMDepthPacking);
        Assert.Contains("moment2", ShadowMapChunks.VSMDepthPacking);
    }

    [Fact]
    public void VSMDepthFragmentShader_OutputsMoments()
    {
        // Assert
        Assert.Contains("packDepthToVSM", ShadowMapChunks.VSMDepthFragmentShader);
        Assert.Contains("vec4(moments", ShadowMapChunks.VSMDepthFragmentShader);
    }

    [Fact]
    public void VSM_SupportsLightBleedingReduction()
    {
        // Assert
        Assert.Contains("lightBleedingReduction", ShadowMapChunks.VSMShadowMap);
        Assert.Contains("linstep", ShadowMapChunks.VSMShadowMap);
    }

    [Fact]
    public void GaussianWeights_CalculationIsCorrect()
    {
        // Act
        var weights = VSMShadowMap.CalculateGaussianWeights(3, 2.0f);

        // Assert
        Assert.Equal(7, weights.Length); // 3*2+1
        Assert.True(weights[3] > weights[0]); // Center weight is largest
        Assert.True(weights[3] > weights[6]); // Center weight is largest

        // Weights should sum to approximately 1
        float sum = 0;
        foreach (var w in weights)
        {
            sum += w;
        }
        Assert.True(Math.Abs(sum - 1.0f) < 0.001f);
    }

    [Fact]
    public void GaussianWeights_AreSymmetric()
    {
        // Act
        var weights = VSMShadowMap.CalculateGaussianWeights(5, 2.0f);

        // Assert
        int size = weights.Length;
        for (int i = 0; i < size / 2; i++)
        {
            Assert.True(Math.Abs(weights[i] - weights[size - 1 - i]) < 0.0001f);
        }
    }

    [Fact]
    public void VSMShadowMap_DisposesResources()
    {
        // Arrange
        var vsm = new VSMShadowMap(512, 512);

        // Act
        vsm.Dispose();

        // Assert - should not throw
        Assert.NotNull(vsm);
    }

    [Fact]
    public void VSMShadowMap_HasConfigurableBlurSigma()
    {
        // Arrange
        var vsm = new VSMShadowMap(512, 512);

        // Act
        vsm.BlurSigma = 3.0f;

        // Assert
        Assert.Equal(3.0f, vsm.BlurSigma);
    }
}
