using BlazorGL.Core.Textures;
using BlazorGL.Loaders.Textures;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace BlazorGL.Tests.Loaders.Textures;

public class DDSLoaderTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new DDSLoader(null!);

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
    public async Task LoadAsync_WithInvalidMagic_ThrowsFormatException()
    {
        // Arrange
        var invalidData = new byte[] { 0x00, 0x00, 0x00, 0x00 };
        var loader = CreateLoader(invalidData);

        // Act
        Func<Task> act = async () => await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        await act.Should().ThrowAsync<FormatException>()
            .WithMessage("Not a valid DDS file*");
    }

    [Fact]
    public async Task LoadAsync_WithDXT1Format_ReturnsCompressedTexture()
    {
        // Arrange
        var ddsData = CreateSimpleDDSFile(4, 4, CompressedTextureFormat.BC1);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.Should().NotBeNull();
        texture.Width.Should().Be(4);
        texture.Height.Should().Be(4);
        texture.CompressionFormat.Should().Be(CompressedTextureFormat.BC1);
        texture.Mipmaps.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoadAsync_WithDXT5Format_ReturnsBC3CompressedTexture()
    {
        // Arrange
        var ddsData = CreateSimpleDDSFile(8, 8, CompressedTextureFormat.BC3);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.CompressionFormat.Should().Be(CompressedTextureFormat.BC3);
    }

    [Fact]
    public async Task LoadAsync_WithMipmaps_LoadsAllLevels()
    {
        // Arrange
        var ddsData = CreateDDSFileWithMipmaps(64, 64, 7); // 64x64 with 7 mip levels
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.Mipmaps.Should().HaveCount(7);
        texture.Mipmaps[0].Width.Should().Be(64);
        texture.Mipmaps[0].Height.Should().Be(64);
        texture.Mipmaps[1].Width.Should().Be(32);
        texture.Mipmaps[1].Height.Should().Be(32);
        texture.Mipmaps[6].Width.Should().Be(1);
        texture.Mipmaps[6].Height.Should().Be(1);
    }

    [Fact]
    public async Task LoadAsync_CalculatesCompressedSizeCorrectly()
    {
        // Arrange
        // BC1/DXT1: 8 bytes per 4x4 block
        // 4x4 texture = 1 block = 8 bytes
        var ddsData = CreateSimpleDDSFile(4, 4, CompressedTextureFormat.BC1);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.Mipmaps[0].Data.Length.Should().Be(8); // 1 block * 8 bytes
    }

    [Fact]
    public async Task LoadAsync_WithBC1_CalculatesSizeCorrectly()
    {
        // Arrange
        // BC1: 8 bytes per 4x4 block
        // 8x8 = 2x2 blocks = 4 blocks = 32 bytes
        var ddsData = CreateSimpleDDSFile(8, 8, CompressedTextureFormat.BC1);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.Mipmaps[0].Data.Length.Should().Be(32);
    }

    [Fact]
    public async Task LoadAsync_WithBC3_CalculatesSizeCorrectly()
    {
        // Arrange
        // BC3/DXT5: 16 bytes per 4x4 block
        // 8x8 = 2x2 blocks = 4 blocks = 64 bytes
        var ddsData = CreateSimpleDDSFile(8, 8, CompressedTextureFormat.BC3);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.Mipmaps[0].Data.Length.Should().Be(64);
    }

    [Fact]
    public async Task LoadAsync_WithNonPowerOfTwo_HandlesCorrectly()
    {
        // Arrange
        // Non-power-of-two dimensions should still work
        var ddsData = CreateSimpleDDSFile(100, 50, CompressedTextureFormat.BC1);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.Width.Should().Be(100);
        texture.Height.Should().Be(50);
        // 100x50 pixels = 25x13 blocks (rounded up) = 325 blocks * 8 bytes = 2600 bytes
        texture.Mipmaps[0].Data.Length.Should().Be(2600);
    }

    [Fact]
    public async Task LoadAsync_SetsTextureName()
    {
        // Arrange
        var ddsData = CreateSimpleDDSFile(4, 4, CompressedTextureFormat.BC1);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/myTexture.dds");

        // Assert
        texture.Name.Should().Be("myTexture.dds");
    }

    [Fact]
    public async Task LoadAsync_DisablesGenerateMipmaps()
    {
        // Arrange
        var ddsData = CreateSimpleDDSFile(4, 4, CompressedTextureFormat.BC1);
        var loader = CreateLoader(ddsData);

        // Act
        var texture = await loader.LoadAsync("http://test.com/test.dds");

        // Assert
        texture.GenerateMipmaps.Should().BeFalse();
    }

    // Helper methods

    private DDSLoader CreateLoader(byte[]? responseData = null)
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
        return new DDSLoader(httpClient);
    }

    private byte[] CreateSimpleDDSFile(int width, int height, CompressedTextureFormat format)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Magic number: "DDS "
        writer.Write(0x20534444u);

        // DDS_HEADER (124 bytes)
        writer.Write(124u); // dwSize
        writer.Write(0x1007u); // dwFlags (CAPS | HEIGHT | WIDTH | PIXELFORMAT)
        writer.Write((uint)height);
        writer.Write((uint)width);
        writer.Write(0u); // dwPitchOrLinearSize
        writer.Write(0u); // dwDepth
        writer.Write(1u); // dwMipMapCount

        // Reserved1[11]
        for (int i = 0; i < 11; i++)
            writer.Write(0u);

        // DDS_PIXELFORMAT (32 bytes)
        writer.Write(32u); // dwSize
        writer.Write(0x4u); // dwFlags (FOURCC)

        // FourCC based on format
        uint fourCC = format switch
        {
            CompressedTextureFormat.BC1 => 0x31545844u, // "DXT1"
            CompressedTextureFormat.BC2 => 0x33545844u, // "DXT3"
            CompressedTextureFormat.BC3 => 0x35545844u, // "DXT5"
            CompressedTextureFormat.BC4 => 0x31495441u, // "ATI1"
            CompressedTextureFormat.BC5 => 0x32495441u, // "ATI2"
            _ => 0x31545844u
        };
        writer.Write(fourCC);

        writer.Write(0u); // dwRGBBitCount
        writer.Write(0u); // dwRBitMask
        writer.Write(0u); // dwGBitMask
        writer.Write(0u); // dwBBitMask
        writer.Write(0u); // dwABitMask

        // dwCaps, dwCaps2, dwCaps3, dwCaps4
        writer.Write(0x1000u); // DDSCAPS_TEXTURE
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);

        // dwReserved2
        writer.Write(0u);

        // Write compressed data
        int dataSize = CalculateCompressedSize(width, height, format);
        byte[] data = new byte[dataSize];
        writer.Write(data);

        return ms.ToArray();
    }

    private byte[] CreateDDSFileWithMipmaps(int width, int height, int mipCount)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Magic number
        writer.Write(0x20534444u);

        // DDS_HEADER with mipmaps
        writer.Write(124u);
        writer.Write(0x20007u); // dwFlags (CAPS | HEIGHT | WIDTH | PIXELFORMAT | MIPMAPCOUNT)
        writer.Write((uint)height);
        writer.Write((uint)width);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write((uint)mipCount); // dwMipMapCount

        // Reserved1[11]
        for (int i = 0; i < 11; i++)
            writer.Write(0u);

        // DDS_PIXELFORMAT
        writer.Write(32u);
        writer.Write(0x4u);
        writer.Write(0x31545844u); // DXT1
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);

        // Caps
        writer.Write(0x401008u); // TEXTURE | MIPMAP | COMPLEX
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(0u);

        // Write mipmap data
        int w = width;
        int h = height;
        for (int level = 0; level < mipCount; level++)
        {
            int size = CalculateCompressedSize(w, h, CompressedTextureFormat.BC1);
            byte[] data = new byte[size];
            writer.Write(data);

            w = System.Math.Max(1, w / 2);
            h = System.Math.Max(1, h / 2);
        }

        return ms.ToArray();
    }

    private int CalculateCompressedSize(int width, int height, CompressedTextureFormat format)
    {
        int blockSize = format switch
        {
            CompressedTextureFormat.BC1 => 8,
            CompressedTextureFormat.BC2 => 16,
            CompressedTextureFormat.BC3 => 16,
            CompressedTextureFormat.BC4 => 8,
            CompressedTextureFormat.BC5 => 16,
            _ => 8
        };

        int blocksWide = System.Math.Max(1, (width + 3) / 4);
        int blocksHigh = System.Math.Max(1, (height + 3) / 4);

        return blocksWide * blocksHigh * blockSize;
    }
}
