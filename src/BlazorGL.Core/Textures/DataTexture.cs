namespace BlazorGL.Core.Textures;

/// <summary>
/// Texture created from raw data arrays (float, int, byte)
/// Used for procedural textures, HDR data, data visualization, etc.
/// </summary>
public class DataTexture : Texture
{
    /// <summary>
    /// Float data (for HDR, height maps, data textures)
    /// </summary>
    public float[]? FloatData { get; set; }

    /// <summary>
    /// Integer data (for lookup tables, indices)
    /// </summary>
    public int[]? IntData { get; set; }

    /// <summary>
    /// Data type for this texture
    /// </summary>
    public TextureDataType DataType { get; set; } = TextureDataType.UnsignedByte;

    /// <summary>
    /// Texture format (RGB, RGBA, etc.)
    /// </summary>
    public TextureFormat TextureFormat { get; set; } = TextureFormat.RGBA;

    /// <summary>
    /// Color space encoding
    /// </summary>
    public TextureEncoding Encoding { get; set; } = TextureEncoding.LinearEncoding;

    /// <summary>
    /// Create data texture from float array
    /// </summary>
    public DataTexture(float[] data, int width, int height)
    {
        FloatData = data ?? throw new ArgumentNullException(nameof(data));
        Width = width;
        Height = height;
        DataType = TextureDataType.Float;
        NeedsUpdate = true;
    }

    /// <summary>
    /// Create data texture from byte array
    /// </summary>
    public DataTexture(byte[] data, int width, int height)
    {
        ImageData = data ?? throw new ArgumentNullException(nameof(data));
        Width = width;
        Height = height;
        DataType = TextureDataType.UnsignedByte;
        NeedsUpdate = true;
    }

    /// <summary>
    /// Create data texture from int array
    /// </summary>
    public DataTexture(int[] data, int width, int height)
    {
        IntData = data ?? throw new ArgumentNullException(nameof(data));
        Width = width;
        Height = height;
        DataType = TextureDataType.Int;
        NeedsUpdate = true;
    }

    /// <summary>
    /// Create empty data texture
    /// </summary>
    public DataTexture()
    {
    }
}

/// <summary>
/// Data type for texture data
/// </summary>
public enum TextureDataType
{
    /// <summary>
    /// 8-bit unsigned byte (0-255)
    /// </summary>
    UnsignedByte,

    /// <summary>
    /// 8-bit signed byte (-128 to 127)
    /// </summary>
    Byte,

    /// <summary>
    /// 16-bit unsigned short (0-65535)
    /// </summary>
    UnsignedShort,

    /// <summary>
    /// 16-bit signed short (-32768 to 32767)
    /// </summary>
    Short,

    /// <summary>
    /// 32-bit unsigned integer
    /// </summary>
    UnsignedInt,

    /// <summary>
    /// 32-bit signed integer
    /// </summary>
    Int,

    /// <summary>
    /// 16-bit half float
    /// </summary>
    HalfFloat,

    /// <summary>
    /// 32-bit float
    /// </summary>
    Float
}

/// <summary>
/// Texture pixel format
/// </summary>
public enum TextureFormat
{
    /// <summary>
    /// Alpha channel only
    /// </summary>
    Alpha,

    /// <summary>
    /// Red channel only
    /// </summary>
    Red,

    /// <summary>
    /// Red and green channels
    /// </summary>
    RG,

    /// <summary>
    /// RGB (no alpha)
    /// </summary>
    RGB,

    /// <summary>
    /// RGBA with alpha
    /// </summary>
    RGBA,

    /// <summary>
    /// Luminance (grayscale)
    /// </summary>
    Luminance,

    /// <summary>
    /// Luminance with alpha
    /// </summary>
    LuminanceAlpha,

    /// <summary>
    /// Depth component
    /// </summary>
    Depth,

    /// <summary>
    /// Depth and stencil
    /// </summary>
    DepthStencil
}

/// <summary>
/// Color space encoding
/// </summary>
public enum TextureEncoding
{
    /// <summary>
    /// Linear color space (no gamma correction)
    /// </summary>
    LinearEncoding,

    /// <summary>
    /// sRGB color space (gamma 2.2)
    /// </summary>
    sRGBEncoding
}

/// <summary>
/// Texture filtering modes
/// </summary>
public enum TextureFilter
{
    /// <summary>
    /// Nearest neighbor (pixelated)
    /// </summary>
    Nearest,

    /// <summary>
    /// Linear interpolation (smooth)
    /// </summary>
    Linear,

    /// <summary>
    /// Nearest with nearest mipmap
    /// </summary>
    NearestMipmapNearest,

    /// <summary>
    /// Linear with nearest mipmap
    /// </summary>
    LinearMipmapNearest,

    /// <summary>
    /// Nearest with linear mipmap
    /// </summary>
    NearestMipmapLinear,

    /// <summary>
    /// Linear with linear mipmap (trilinear)
    /// </summary>
    LinearMipmapLinear
}
