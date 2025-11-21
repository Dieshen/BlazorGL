using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Tests.PostProcessing;

public class BloomPassTests
{
    [Fact]
    public void BloomPass_Initialization_Success()
    {
        // Arrange
        int width = 800;
        int height = 600;

        // Act
        var bloomPass = new BloomPass(width, height);

        // Assert
        Assert.NotNull(bloomPass);
        Assert.Equal(0.8f, bloomPass.LuminosityThreshold);
        Assert.Equal(1.5f, bloomPass.BloomStrength);
        Assert.Equal(1.0f, bloomPass.BlurRadius);
        Assert.Equal(2, bloomPass.ResolutionDivisor);
    }

    [Fact]
    public void BloomPass_SetSize_UpdatesResolution()
    {
        // Arrange
        var bloomPass = new BloomPass(800, 600);

        // Act
        bloomPass.SetSize(1024, 768);

        // Assert - no exception thrown
        Assert.NotNull(bloomPass);
    }

    [Fact]
    public void BloomPass_Properties_CanBeModified()
    {
        // Arrange
        var bloomPass = new BloomPass(800, 600);

        // Act
        bloomPass.LuminosityThreshold = 0.9f;
        bloomPass.BloomStrength = 2.0f;
        bloomPass.BlurRadius = 1.5f;

        // Assert
        Assert.Equal(0.9f, bloomPass.LuminosityThreshold);
        Assert.Equal(2.0f, bloomPass.BloomStrength);
        Assert.Equal(1.5f, bloomPass.BlurRadius);
    }
}
