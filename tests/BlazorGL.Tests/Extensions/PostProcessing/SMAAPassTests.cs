using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Tests.Extensions.PostProcessing;

public class SMAAPassTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var pass = new SMAAPass(1920, 1080);

        // Assert
        Assert.NotNull(pass);
        Assert.Equal(SMAAQuality.High, pass.Quality);
        Assert.Equal(0.1f, pass.EdgeDetectionThreshold);
    }

    [Fact]
    public void Quality_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new SMAAPass(1920, 1080);

        // Act
        pass.Quality = SMAAQuality.Ultra;

        // Assert
        Assert.Equal(SMAAQuality.Ultra, pass.Quality);
    }

    [Fact]
    public void EdgeDetectionThreshold_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new SMAAPass(1920, 1080);

        // Act
        pass.EdgeDetectionThreshold = 0.05f;

        // Assert
        Assert.Equal(0.05f, pass.EdgeDetectionThreshold);
    }

    [Fact]
    public void SetQuality_Low_SetsAppropriateThreshold()
    {
        // Arrange
        var pass = new SMAAPass(1920, 1080);

        // Act
        pass.SetQuality(SMAAQuality.Low);

        // Assert
        Assert.Equal(SMAAQuality.Low, pass.Quality);
        Assert.Equal(0.15f, pass.EdgeDetectionThreshold);
    }

    [Fact]
    public void SetQuality_Medium_SetsAppropriateThreshold()
    {
        // Arrange
        var pass = new SMAAPass(1920, 1080);

        // Act
        pass.SetQuality(SMAAQuality.Medium);

        // Assert
        Assert.Equal(SMAAQuality.Medium, pass.Quality);
        Assert.Equal(0.1f, pass.EdgeDetectionThreshold);
    }

    [Fact]
    public void SetQuality_High_SetsAppropriateThreshold()
    {
        // Arrange
        var pass = new SMAAPass(1920, 1080);

        // Act
        pass.SetQuality(SMAAQuality.High);

        // Assert
        Assert.Equal(SMAAQuality.High, pass.Quality);
        Assert.Equal(0.05f, pass.EdgeDetectionThreshold);
    }

    [Fact]
    public void SetQuality_Ultra_SetsAppropriateThreshold()
    {
        // Arrange
        var pass = new SMAAPass(1920, 1080);

        // Act
        pass.SetQuality(SMAAQuality.Ultra);

        // Assert
        Assert.Equal(SMAAQuality.Ultra, pass.Quality);
        Assert.Equal(0.025f, pass.EdgeDetectionThreshold);
    }

    [Fact]
    public void Pass_CanBeConstructed()
    {
        // Arrange & Act
        var pass = new SMAAPass(1920, 1080);

        // Assert
        // Verify pass was created successfully
        Assert.NotNull(pass);
        Assert.Equal(SMAAQuality.High, pass.Quality);
    }
}
