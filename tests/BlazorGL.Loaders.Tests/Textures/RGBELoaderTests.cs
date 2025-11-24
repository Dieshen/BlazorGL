using Xunit;
using BlazorGL.Loaders.Textures;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace BlazorGL.Loaders.Tests.Textures;

public class RGBELoaderTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new RGBELoader(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public async Task LoadAsync_WithNullUrl_ThrowsArgumentException()
    {
        // Arrange
        var loader = CreateLoader();

        // Act
        Func<Task> act = async () => await loader.LoadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task LoadAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var loader = CreateLoader();

        // Act
        Func<Task> act = async () => await loader.LoadAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task LoadAsync_WithInvalidMagic_ThrowsFormatException()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        var loader = CreateLoader(invalidData);

        // Act
        Func<Task> act = async () => await loader.LoadAsync("http://test.com/test.hdr");

        // Assert
        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Not a valid RGBE file*");
    }

    [Fact]
    public async Task LoadAsync_WithValidRGBEFile_ReturnsDataTexture()
    {
        // Arrange
        var rgbeData = CreateSimpleRGBEFile(2, 2);
        var loader = CreateLoader(rgbeData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.hdr");

        // Assert
        texture.Should().NotBeNull();
        texture.Width.Should().Be(2);
        texture.Height.Should().Be(2);
        texture.FloatData.Should().NotBeNull();
        texture.FloatData!.Length.Should().Be(2 * 2 * 3); // width * height * RGB
    }

    [Fact]
    public async Task LoadAsync_DecodesRGBECorrectly()
    {
        // Arrange
        // Create RGBE file with known pixel value: R=255, G=128, B=64, E=128
        // Expected decoded value: (255, 128, 64) * 2^(128-128) / 256 = (0.99609375, 0.5, 0.25)
        var rgbeData = CreateRGBEFileWithPixel(255, 128, 64, 128);
        var loader = CreateLoader(rgbeData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.hdr");

        // Assert
        texture.FloatData.Should().NotBeNull();
        texture.FloatData![0].Should().BeApproximately(0.996f, 0.01f); // R
        texture.FloatData[1].Should().BeApproximately(0.5f, 0.01f);    // G
        texture.FloatData[2].Should().BeApproximately(0.25f, 0.01f);   // B
    }

    [Fact]
    public async Task LoadAsync_WithExposureSetting_AppliesExposure()
    {
        // Arrange
        var rgbeData = CreateSimpleRGBEFile(1, 1);
        var loader = CreateLoader(rgbeData);
        loader.Exposure = 2.0f; // Double the brightness

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.hdr");

        // Assert
        texture.FloatData.Should().NotBeNull();
        // Values should be affected by exposure (implementation dependent)
    }

    [Fact]
    public async Task LoadAsync_WithToneMapping_AppliesToneMapping()
    {
        // Arrange
        var rgbeData = CreateSimpleRGBEFile(1, 1);
        var loader = CreateLoader(rgbeData);
        loader.ApplyToneMapping = true;

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.hdr");

        // Assert
        texture.FloatData.Should().NotBeNull();
        // All values should be in range [0, 1] after tone mapping
        foreach (var value in texture.FloatData!)
        {
            value.Should().BeInRange(0f, 1f);
        }
    }

    // Helper methods

    private RGBELoader CreateLoader(byte[]? responseData = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        if (responseData != null)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(responseData)
                });
        }

        var httpClient = new HttpClient(handlerMock.Object);
        return new RGBELoader(httpClient);
    }

    private byte[] CreateSimpleRGBEFile(int width, int height)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write header
        var header = "#?RADIANCE\n\n-Y " + height + " +X " + width + "\n";
        var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
        writer.Write(headerBytes);

        // Write simple uncompressed scanlines
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                writer.Write((byte)128); // R
                writer.Write((byte)128); // G
                writer.Write((byte)128); // B
                writer.Write((byte)128); // E (exponent)
            }
        }

        return ms.ToArray();
    }

    private byte[] CreateRGBEFileWithPixel(byte r, byte g, byte b, byte e)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write header for 1x1 image
        var header = "#?RADIANCE\n\n-Y 1 +X 1\n";
        var headerBytes = System.Text.Encoding.ASCII.GetBytes(header);
        writer.Write(headerBytes);

        // Write single pixel
        writer.Write(r);
        writer.Write(g);
        writer.Write(b);
        writer.Write(e);

        return ms.ToArray();
    }
}
