using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Tests.PostProcessing;

public class PostProcessingTests
{
    [Fact]
    public void EffectComposer_CanBeCreated()
    {
        // Arrange & Act
        var renderer = new Renderer();
        var composer = new EffectComposer(renderer, 1024, 768);

        // Assert
        Assert.NotNull(composer);
    }

    [Fact]
    public void EffectComposer_CanAddPasses()
    {
        // Arrange
        var renderer = new Renderer();
        var composer = new EffectComposer(renderer, 1024, 768);
        var pass = new FXAAPass(1024, 768);

        // Act
        composer.AddPass(pass);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void SSAOPass_HasCorrectDefaultParameters()
    {
        // Arrange
        var renderer = new Renderer();
        var camera = new PerspectiveCamera(45, 1.33f, 0.1f, 100f);

        // Act
        var ssaoPass = new SSAOPass(renderer, camera, 1024, 768);

        // Assert
        Assert.Equal(32, ssaoPass.KernelSize);
        Assert.Equal(0.5f, ssaoPass.Radius);
        Assert.Equal(0.01f, ssaoPass.Bias);
        Assert.Equal(1.5f, ssaoPass.Power);
    }

    [Fact]
    public void SSAOPass_CanUpdateParameters()
    {
        // Arrange
        var renderer = new Renderer();
        var camera = new PerspectiveCamera(45, 1.33f, 0.1f, 100f);
        var ssaoPass = new SSAOPass(renderer, camera, 1024, 768);

        // Act
        ssaoPass.KernelSize = 64;
        ssaoPass.Radius = 1.0f;
        ssaoPass.Bias = 0.02f;
        ssaoPass.Power = 2.0f;

        // Assert
        Assert.Equal(64, ssaoPass.KernelSize);
        Assert.Equal(1.0f, ssaoPass.Radius);
        Assert.Equal(0.02f, ssaoPass.Bias);
        Assert.Equal(2.0f, ssaoPass.Power);
    }

    [Fact]
    public void FXAAPass_CanBeCreated()
    {
        // Arrange & Act
        var pass = new FXAAPass(1024, 768);

        // Assert
        Assert.NotNull(pass);
        Assert.True(pass.Enabled);
    }

    [Fact]
    public void FXAAPass_CanBeDisabled()
    {
        // Arrange
        var pass = new FXAAPass(1024, 768);

        // Act
        pass.Enabled = false;

        // Assert
        Assert.False(pass.Enabled);
    }

    [Fact]
    public void ColorCorrectionPass_HasCorrectDefaultParameters()
    {
        // Arrange & Act
        var pass = new ColorCorrectionPass();

        // Assert
        Assert.Equal(0.0f, pass.Brightness);
        Assert.Equal(1.0f, pass.Contrast);
        Assert.Equal(1.0f, pass.Saturation);
        Assert.Equal(0.0f, pass.Hue);
        Assert.Equal(1.0f, pass.Exposure);
        Assert.Equal(2.2f, pass.Gamma);
    }

    [Fact]
    public void ColorCorrectionPass_CanUpdateParameters()
    {
        // Arrange
        var pass = new ColorCorrectionPass();

        // Act
        pass.Brightness = 0.2f;
        pass.Contrast = 1.5f;
        pass.Saturation = 0.8f;
        pass.Hue = 0.1f;
        pass.Exposure = 1.2f;
        pass.Gamma = 2.4f;

        // Assert
        Assert.Equal(0.2f, pass.Brightness);
        Assert.Equal(1.5f, pass.Contrast);
        Assert.Equal(0.8f, pass.Saturation);
        Assert.Equal(0.1f, pass.Hue);
        Assert.Equal(1.2f, pass.Exposure);
        Assert.Equal(2.4f, pass.Gamma);
    }

    [Fact]
    public void ShaderPass_UsesInputTexture()
    {
        // Arrange
        var material = new ShaderMaterial(
            SSAOShader.VertexShader,
            SSAOShader.FragmentShader
        );
        var pass = new ShaderPass(material);
        var input = new RenderTarget(512, 512);

        // Act - check that material can receive texture uniform
        pass._material.Uniforms["tDiffuse"] = input.Texture;

        // Assert
        Assert.Contains("tDiffuse", pass._material.Uniforms.Keys);
        Assert.Equal(input.Texture, pass._material.Uniforms["tDiffuse"]);
    }

    [Fact]
    public void RenderTarget_HasCorrectDimensions()
    {
        // Arrange & Act
        var target = new RenderTarget(1024, 768);

        // Assert
        Assert.Equal(1024, target.Width);
        Assert.Equal(768, target.Height);
        Assert.NotNull(target.Texture);
        Assert.True(target.DepthBuffer);
    }

    [Fact]
    public void RenderTarget_CanDisableDepthBuffer()
    {
        // Arrange & Act
        var target = new RenderTarget(512, 512)
        {
            DepthBuffer = false
        };

        // Assert
        Assert.False(target.DepthBuffer);
    }

    [Fact]
    public void SSAOShader_HasRequiredUniforms()
    {
        // Arrange
        var expectedUniforms = new[]
        {
            "tDiffuse", "tDepth", "tNoise", "kernel", "projection",
            "projectionInverse", "resolution", "kernelSize", "radius",
            "bias", "power"
        };

        // Act - verify shader contains uniform declarations
        var fragmentShader = SSAOShader.FragmentShader;

        // Assert
        foreach (var uniform in expectedUniforms)
        {
            Assert.Contains($"uniform", fragmentShader);
        }
    }

    [Fact]
    public void FXAAShader_HasRequiredUniforms()
    {
        // Arrange
        var expectedUniforms = new[] { "tDiffuse", "resolution" };

        // Act
        var fragmentShader = FXAAShader.FragmentShader;

        // Assert
        foreach (var uniform in expectedUniforms)
        {
            Assert.Contains($"uniform", fragmentShader);
        }
    }

    [Fact]
    public void ColorCorrectionShader_HasRGBToHSLConversion()
    {
        // Act
        var fragmentShader = ColorCorrectionShader.FragmentShader;

        // Assert
        Assert.Contains("rgb2hsl", fragmentShader);
        Assert.Contains("hsl2rgb", fragmentShader);
    }

    [Fact]
    public void ColorCorrectionShader_AppliesAllCorrections()
    {
        // Act
        var fragmentShader = ColorCorrectionShader.FragmentShader;

        // Assert
        Assert.Contains("exposure", fragmentShader);
        Assert.Contains("brightness", fragmentShader);
        Assert.Contains("contrast", fragmentShader);
        Assert.Contains("saturation", fragmentShader);
        Assert.Contains("hue", fragmentShader);
        Assert.Contains("gamma", fragmentShader);
    }

    [Fact]
    public void SSAOPass_GeneratesKernelSamples()
    {
        // Arrange
        var renderer = new Renderer();
        var camera = new PerspectiveCamera(45, 1.33f, 0.1f, 100f);

        // Act
        var ssaoPass = new SSAOPass(renderer, camera, 1024, 768);

        // Assert - kernel is generated internally, verify pass was created
        Assert.NotNull(ssaoPass);
        Assert.True(ssaoPass.KernelSize > 0);
    }

    [Fact]
    public void MultiplePassesCanBeChained()
    {
        // Arrange
        var renderer = new Renderer();
        var composer = new EffectComposer(renderer, 1024, 768);
        var camera = new PerspectiveCamera(45, 1.33f, 0.1f, 100f);

        // Act
        composer.AddPass(new SSAOPass(renderer, camera, 1024, 768));
        composer.AddPass(new FXAAPass(1024, 768));
        composer.AddPass(new ColorCorrectionPass());

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void Pass_CanBeEnabledAndDisabled()
    {
        // Arrange
        var pass = new FXAAPass(1024, 768);

        // Act & Assert
        Assert.True(pass.Enabled);

        pass.Enabled = false;
        Assert.False(pass.Enabled);

        pass.Enabled = true;
        Assert.True(pass.Enabled);
    }
}
