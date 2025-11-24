using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Tests.Extensions.PostProcessing;

public class LUTPassTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var pass = new LUTPass(1920, 1080);

        // Assert
        Assert.NotNull(pass);
        Assert.Equal(1.0f, pass.Intensity);
        Assert.Equal(16, pass.LUTSize);
        Assert.NotNull(pass.LUT); // Neutral LUT is created by default
    }

    [Fact]
    public void Intensity_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new LUTPass(1920, 1080);

        // Act
        pass.Intensity = 0.5f;

        // Assert
        Assert.Equal(0.5f, pass.Intensity);
    }

    [Fact]
    public void LUTSize_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new LUTPass(1920, 1080);

        // Act
        pass.LUTSize = 32;

        // Assert
        Assert.Equal(32, pass.LUTSize);
    }

    [Fact]
    public void LUT_CanBeSetAndRetrieved()
    {
        // Arrange
        var pass = new LUTPass(1920, 1080);
        var customLUT = new Texture();

        // Act
        pass.LUT = customLUT;

        // Assert
        Assert.Equal(customLUT, pass.LUT);
    }

    [Fact]
    public void SetLUT_UpdatesLUTAndSize()
    {
        // Arrange
        var pass = new LUTPass(1920, 1080);
        var customLUT = new Texture { Width = 1024, Height = 32 };

        // Act
        pass.SetLUT(customLUT, 32);

        // Assert
        Assert.Equal(customLUT, pass.LUT);
        Assert.Equal(32, pass.LUTSize);
    }

    [Fact]
    public void SetNeutralLUT_CreatesIdentityLUT()
    {
        // Arrange
        var pass = new LUTPass(1920, 1080);

        // Act
        pass.SetNeutralLUT();

        // Assert
        Assert.NotNull(pass.LUT);
        Assert.Equal(16, pass.LUTSize);
    }

    [Fact]
    public void LUT_CanBeSetToNull()
    {
        // Arrange
        var pass = new LUTPass(1920, 1080);

        // Act
        pass.LUT = null;

        // Assert
        Assert.Null(pass.LUT);
    }
}

public class LUTLoaderTests
{
    [Fact]
    public void LoadFromCubeFile_ThrowsException_WhenFileNotFound()
    {
        // Arrange
        var nonExistentPath = "nonexistent.cube";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => LUTLoader.LoadFromCubeFile(nonExistentPath));
    }

    [Fact]
    public void PresetLUTs_Warm_ReturnsTexture()
    {
        // Act
        var lut = LUTLoader.PresetLUTs.Warm();

        // Assert
        Assert.NotNull(lut);
    }

    [Fact]
    public void PresetLUTs_Cool_ReturnsTexture()
    {
        // Act
        var lut = LUTLoader.PresetLUTs.Cool();

        // Assert
        Assert.NotNull(lut);
    }

    [Fact]
    public void PresetLUTs_Sepia_ReturnsTexture()
    {
        // Act
        var lut = LUTLoader.PresetLUTs.Sepia();

        // Assert
        Assert.NotNull(lut);
    }
}
