namespace BlazorGL.Core.Textures;

/// <summary>
/// Texture with pre-compressed data (DDS, KTX2, etc.)
/// </summary>
public class CompressedTexture : Texture
{
    /// <summary>
    /// Mipmap chain for this texture
    /// </summary>
    public List<MipmapData> Mipmaps { get; set; } = new();

    /// <summary>
    /// GPU compression format
    /// </summary>
    public CompressedTextureFormat CompressionFormat { get; set; }

    /// <summary>
    /// Whether this is a cubemap texture
    /// </summary>
    public bool IsCubemap { get; set; }

    /// <summary>
    /// Create compressed texture from mipmap chain
    /// </summary>
    public CompressedTexture(List<MipmapData> mipmaps, CompressedTextureFormat format)
    {
        Mipmaps = mipmaps ?? throw new ArgumentNullException(nameof(mipmaps));
        CompressionFormat = format;
        GenerateMipmaps = false; // Compressed textures include mipmaps

        if (mipmaps.Count > 0)
        {
            Width = mipmaps[0].Width;
            Height = mipmaps[0].Height;
        }
    }

    /// <summary>
    /// Create empty compressed texture
    /// </summary>
    public CompressedTexture()
    {
        GenerateMipmaps = false;
    }
}

/// <summary>
/// Single mipmap level data
/// </summary>
public class MipmapData
{
    /// <summary>
    /// Compressed or raw pixel data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Width of this mipmap level
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of this mipmap level
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Mipmap level (0 = base level)
    /// </summary>
    public int Level { get; set; }
}

/// <summary>
/// GPU texture compression formats
/// </summary>
public enum CompressedTextureFormat
{
    /// <summary>
    /// BC1 / DXT1 - RGB or RGBA with 1-bit alpha (4:1 compression)
    /// </summary>
    BC1,

    /// <summary>
    /// BC2 / DXT3 - RGBA with explicit alpha (4:1 compression)
    /// </summary>
    BC2,

    /// <summary>
    /// BC3 / DXT5 - RGBA with interpolated alpha (4:1 compression)
    /// </summary>
    BC3,

    /// <summary>
    /// BC4 - Single channel compression (2:1 compression)
    /// </summary>
    BC4,

    /// <summary>
    /// BC5 - Two channel compression for normal maps (2:1 compression)
    /// </summary>
    BC5,

    /// <summary>
    /// BC6H - HDR float compression (6:1 compression)
    /// </summary>
    BC6H,

    /// <summary>
    /// BC7 - High quality RGBA compression (4:1 compression)
    /// </summary>
    BC7,

    /// <summary>
    /// ETC1 - Mobile RGB compression (6:1 compression)
    /// </summary>
    ETC1,

    /// <summary>
    /// ETC2 RGB - Mobile RGB compression (6:1 compression)
    /// </summary>
    ETC2_RGB,

    /// <summary>
    /// ETC2 RGBA - Mobile RGBA compression (4:1 compression)
    /// </summary>
    ETC2_RGBA,

    /// <summary>
    /// ASTC 4x4 - Adaptive Scalable Texture Compression (8:1 compression)
    /// </summary>
    ASTC_4x4,

    /// <summary>
    /// ASTC 6x6 - Adaptive Scalable Texture Compression (5.33:1 compression)
    /// </summary>
    ASTC_6x6,

    /// <summary>
    /// ASTC 8x8 - Adaptive Scalable Texture Compression (4:1 compression)
    /// </summary>
    ASTC_8x8,

    /// <summary>
    /// PVRTC RGBA 4BPP - PowerVR compression (8:1 compression)
    /// </summary>
    PVRTC_RGBA_4BPP,

    /// <summary>
    /// PVRTC RGBA 2BPP - PowerVR compression (16:1 compression)
    /// </summary>
    PVRTC_RGBA_2BPP,

    /// <summary>
    /// RGB565 - Uncompressed 16-bit fallback
    /// </summary>
    RGB565
}
