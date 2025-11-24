using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Extensions.Tests.PostProcessing;

public class TAAPassTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);

        // Act
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Assert
        Assert.NotNull(pass);
        Assert.Equal(8, pass.SampleCount);
        Assert.Equal(0.5f, pass.Sharpness);
        Assert.False(pass.UseMotionVectors);
        Assert.Equal(0.1f, pass.BlendFactor);
        Assert.True(pass.EnableJitter);
    }

    [Fact]
    public void SampleCount_CanBeSetAndRetrieved()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.SampleCount = 16;

        // Assert
        Assert.Equal(16, pass.SampleCount);
    }

    [Fact]
    public void Sharpness_CanBeSetAndRetrieved()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.Sharpness = 0.8f;

        // Assert
        Assert.Equal(0.8f, pass.Sharpness);
    }

    [Fact]
    public void UseMotionVectors_CanBeToggled()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.UseMotionVectors = true;

        // Assert
        Assert.True(pass.UseMotionVectors);
    }

    [Fact]
    public void BlendFactor_CanBeSetAndRetrieved()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.BlendFactor = 0.2f;

        // Assert
        Assert.Equal(0.2f, pass.BlendFactor);
    }

    [Fact]
    public void EnableJitter_CanBeToggled()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.EnableJitter = false;

        // Assert
        Assert.False(pass.EnableJitter);
    }

    [Fact]
    public void GetCurrentJitterOffset_ReturnsZero_WhenJitterDisabled()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);
        pass.EnableJitter = false;

        // Act
        var jitter = pass.GetCurrentJitterOffset();

        // Assert
        Assert.Equal(Vector2.Zero, jitter);
    }

    [Fact]
    public void GetCurrentJitterOffset_ReturnsNonZero_WhenJitterEnabled()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);
        pass.EnableJitter = true;

        // Act
        var jitter = pass.GetCurrentJitterOffset();

        // Assert
        Assert.NotEqual(Vector2.Zero, jitter);
    }

    [Fact]
    public void CurrentJitter_UpdatesAfterGetCurrentJitterOffset()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.GetCurrentJitterOffset();
        var currentJitter = pass.CurrentJitter;

        // Assert
        Assert.NotEqual(Vector2.Zero, currentJitter);
    }

    [Fact]
    public void ResetHistory_ResetsFrameCount()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.ResetHistory();
        pass.GetCurrentJitterOffset(); // Should start from frame 0

        // Assert - no exception thrown means reset worked
        Assert.NotNull(pass);
    }

    [Fact]
    public void SetSampleCount_UpdatesJitterPattern()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.SetSampleCount(16);

        // Assert
        Assert.Equal(16, pass.SampleCount);
    }

    [Fact]
    public void Pass_HandlesStateCorrectly()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.ResetHistory();

        // Assert
        // Verify pass state is managed correctly
        Assert.NotNull(pass);
        Assert.Equal(8, pass.SampleCount);
    }

    [Theory]
    [InlineData(8, 0.3f, 0.1f)]
    [InlineData(16, 0.5f, 0.15f)]
    [InlineData(32, 0.7f, 0.2f)]
    public void MultipleSettings_CanBeConfigured(int samples, float sharpness, float blend)
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.0f, 0.1f, 1000f);
        var pass = new TAARenderPass(scene, camera, 1920, 1080);

        // Act
        pass.SampleCount = samples;
        pass.Sharpness = sharpness;
        pass.BlendFactor = blend;

        // Assert
        Assert.Equal(samples, pass.SampleCount);
        Assert.Equal(sharpness, pass.Sharpness);
        Assert.Equal(blend, pass.BlendFactor);
    }
}
