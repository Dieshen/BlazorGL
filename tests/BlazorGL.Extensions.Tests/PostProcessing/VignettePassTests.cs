using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Extensions.Tests.PostProcessing;

public class VignettePassTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var pass = new VignettePass(1920, 1080);

        // Assert
        Assert.NotNull(pass);
        Assert.Equal(1.0f, pass.Offset);
        Assert.Equal(1.0f, pass.Darkness);
        Assert.Equal(0.5f, pass.Smoothness);
    }

    [Fact]
    public void Offset_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.Offset = 1.5f;

        // Assert
        Assert.Equal(1.5f, pass.Offset);
    }

    [Fact]
    public void Darkness_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.Darkness = 0.7f;

        // Assert
        Assert.Equal(0.7f, pass.Darkness);
    }

    [Fact]
    public void Smoothness_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.Smoothness = 0.3f;

        // Assert
        Assert.Equal(0.3f, pass.Smoothness);
    }

    [Fact]
    public void SetPreset_Subtle_SetsCorrectValues()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.SetPreset(VignettePreset.Subtle);

        // Assert
        Assert.Equal(1.2f, pass.Offset);
        Assert.Equal(0.3f, pass.Darkness);
        Assert.Equal(0.8f, pass.Smoothness);
    }

    [Fact]
    public void SetPreset_Medium_SetsCorrectValues()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.SetPreset(VignettePreset.Medium);

        // Assert
        Assert.Equal(1.0f, pass.Offset);
        Assert.Equal(0.6f, pass.Darkness);
        Assert.Equal(0.5f, pass.Smoothness);
    }

    [Fact]
    public void SetPreset_Strong_SetsCorrectValues()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.SetPreset(VignettePreset.Strong);

        // Assert
        Assert.Equal(0.8f, pass.Offset);
        Assert.Equal(0.9f, pass.Darkness);
        Assert.Equal(0.3f, pass.Smoothness);
    }

    [Fact]
    public void SetPreset_Dramatic_SetsCorrectValues()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.SetPreset(VignettePreset.Dramatic);

        // Assert
        Assert.Equal(0.6f, pass.Offset);
        Assert.Equal(1.0f, pass.Darkness);
        Assert.Equal(0.2f, pass.Smoothness);
    }

    [Fact]
    public void SetPreset_Cinematic_SetsCorrectValues()
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.SetPreset(VignettePreset.Cinematic);

        // Assert
        Assert.Equal(1.1f, pass.Offset);
        Assert.Equal(0.7f, pass.Darkness);
        Assert.Equal(0.6f, pass.Smoothness);
    }

    [Theory]
    [InlineData(0.5f, 0.3f, 0.2f)]
    [InlineData(1.0f, 0.6f, 0.5f)]
    [InlineData(1.5f, 0.9f, 0.8f)]
    public void MultipleSettings_CanBeConfigured(float offset, float darkness, float smoothness)
    {
        // Arrange
        var pass = new VignettePass(1920, 1080);

        // Act
        pass.Offset = offset;
        pass.Darkness = darkness;
        pass.Smoothness = smoothness;

        // Assert
        Assert.Equal(offset, pass.Offset);
        Assert.Equal(darkness, pass.Darkness);
        Assert.Equal(smoothness, pass.Smoothness);
    }
}
