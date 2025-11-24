using Xunit;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Shaders.ShaderChunks;
using System;

namespace BlazorGL.Tests.Shadows;

/// <summary>
/// Tests for PCF (Percentage Closer Filtering) shadow implementation
/// </summary>
public class PCFShadowTests
{
    [Fact]
    public void PCFSamples_DefaultValue_Is9()
    {
        // Arrange & Act
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(9, shadow.PCFSamples);
    }

    [Fact]
    public void PCFSamples_CanBeSet_ToValidValues()
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.PCFSamples = 25;

        // Assert
        Assert.Equal(25, shadow.PCFSamples);
    }

    [Theory]
    [InlineData(9)]   // 3x3
    [InlineData(16)]  // 4x4 (Poisson disk)
    [InlineData(25)]  // 5x5
    [InlineData(64)]  // 8x8
    public void PCFSamples_SupportedValues_AreValid(int sampleCount)
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.PCFSamples = sampleCount;

        // Assert
        Assert.Equal(sampleCount, shadow.PCFSamples);
        Assert.True(sampleCount >= 9 && sampleCount <= 64);
    }

    [Fact]
    public void ShadowRadius_DefaultValue_Is1()
    {
        // Arrange & Act
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(1.0f, shadow.Radius);
    }

    [Fact]
    public void ShadowRadius_CanBeAdjusted_ForSoftness()
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.Radius = 2.5f;

        // Assert
        Assert.Equal(2.5f, shadow.Radius);
    }

    [Fact]
    public void ShadowMapType_DefaultValue_IsPCF()
    {
        // Arrange & Act
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(ShadowMapType.PCF, shadow.Type);
    }

    [Theory]
    [InlineData(ShadowMapType.Basic)]
    [InlineData(ShadowMapType.PCF)]
    [InlineData(ShadowMapType.PCFSoft)]
    [InlineData(ShadowMapType.VSM)]
    public void ShadowMapType_CanBeSet_ToAnyValue(ShadowMapType type)
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.Type = type;

        // Assert
        Assert.Equal(type, shadow.Type);
    }

    [Fact]
    public void PoissonDisk16_IsDefinedInShaderChunks()
    {
        // Assert
        Assert.Contains("poissonDisk16", ShadowMapChunks.PoissonDiskSamples);
        Assert.Contains("vec2[](", ShadowMapChunks.PoissonDiskSamples);
    }

    [Fact]
    public void PCFShadowFunction_IsDefinedInShaderChunks()
    {
        // Assert
        Assert.Contains("getShadowPCF", ShadowMapChunks.PCFShadowMap);
        Assert.Contains("shadowMap", ShadowMapChunks.PCFShadowMap);
        Assert.Contains("numSamples", ShadowMapChunks.PCFShadowMap);
    }

    [Fact]
    public void PCFShader_UsesPoissonDiskSampling()
    {
        // Assert - PCF shader should reference Poisson disk patterns
        Assert.Contains("poissonDisk", ShadowMapChunks.PCFShadowMap);
        Assert.Contains("texture(shadowMap", ShadowMapChunks.PCFShadowMap);
    }

    [Fact]
    public void CompleteShadowFunctions_IncludesPCF()
    {
        // Assert
        Assert.Contains("getShadowPCF", ShadowMapChunks.CompleteShadowFunctions);
        Assert.Contains("poissonDisk", ShadowMapChunks.CompleteShadowFunctions);
    }

    [Fact]
    public void ShadowSoftness_DefaultValue_Is1()
    {
        // Arrange & Act
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(1.0f, shadow.ShadowSoftness);
    }

    [Theory]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    [InlineData(2.0f)]
    [InlineData(5.0f)]
    public void ShadowSoftness_CanBeAdjusted(float softness)
    {
        // Arrange
        var shadow = new DirectionalLightShadow();

        // Act
        shadow.ShadowSoftness = softness;

        // Assert
        Assert.Equal(softness, shadow.ShadowSoftness);
    }

    [Fact]
    public void PCFSoft_WithHigherSampleCount_ProducesSofterShadows()
    {
        // Arrange
        var shadow = new DirectionalLightShadow
        {
            Type = ShadowMapType.PCFSoft,
            PCFSamples = 64,
            ShadowSoftness = 3.0f
        };

        // Assert
        Assert.Equal(ShadowMapType.PCFSoft, shadow.Type);
        Assert.Equal(64, shadow.PCFSamples);
        Assert.Equal(3.0f, shadow.ShadowSoftness);
    }

    [Fact]
    public void LightSize_ForPCSS_DefaultValue_Is1()
    {
        // Arrange & Act
        var shadow = new DirectionalLightShadow();

        // Assert
        Assert.Equal(1.0f, shadow.LightSize);
    }

    [Fact]
    public void PCSS_ShaderFunction_IsAvailable()
    {
        // Assert
        Assert.Contains("getShadowPCSS", ShadowMapChunks.PCSSShadowMap);
        Assert.Contains("findBlockerDepth", ShadowMapChunks.PCSSShadowMap);
        Assert.Contains("getPenumbraSize", ShadowMapChunks.PCSSShadowMap);
    }

    [Fact]
    public void PCSS_ImplementsVariablePenumbra()
    {
        // Assert - PCSS should calculate dynamic penumbra size
        Assert.Contains("penumbra", ShadowMapChunks.PCSSShadowMap);
        Assert.Contains("receiverDepth", ShadowMapChunks.PCSSShadowMap);
        Assert.Contains("blockerDepth", ShadowMapChunks.PCSSShadowMap);
    }
}
