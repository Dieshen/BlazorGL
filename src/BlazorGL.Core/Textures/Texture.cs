using Silk.NET.WebGL;

namespace BlazorGL.Core.Textures;

/// <summary>
/// Texture for materials
/// </summary>
public class Texture : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// WebGL texture handle
    /// </summary>
    public uint TextureId { get; internal set; }

    /// <summary>
    /// Texture width
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Texture height
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Texture wrapping mode for S coordinate
    /// </summary>
    public TextureWrapMode WrapS { get; set; } = TextureWrapMode.Repeat;

    /// <summary>
    /// Texture wrapping mode for T coordinate
    /// </summary>
    public TextureWrapMode WrapT { get; set; } = TextureWrapMode.Repeat;

    /// <summary>
    /// Minification filter
    /// </summary>
    public TextureMinFilter MinFilter { get; set; } = TextureMinFilter.LinearMipmapLinear;

    /// <summary>
    /// Magnification filter
    /// </summary>
    public TextureMagFilter MagFilter { get; set; } = TextureMagFilter.Linear;

    /// <summary>
    /// Generate mipmaps
    /// </summary>
    public bool GenerateMipmaps { get; set; } = true;

    /// <summary>
    /// Anisotropic filtering level
    /// </summary>
    public float Anisotropy { get; set; } = 1.0f;

    /// <summary>
    /// Texture format
    /// </summary>
    public InternalFormat Format { get; set; } = InternalFormat.Rgba;

    /// <summary>
    /// Whether the texture needs to be uploaded to GPU
    /// </summary>
    public bool NeedsUpdate { get; set; } = true;

    /// <summary>
    /// Image data (before upload)
    /// </summary>
    public byte[]? ImageData { get; set; }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Texture cleanup happens in RenderContext
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Texture wrapping modes
/// </summary>
public enum TextureWrapMode
{
    Repeat,
    ClampToEdge,
    MirroredRepeat
}

/// <summary>
/// Texture minification filters
/// </summary>
public enum TextureMinFilter
{
    Nearest,
    Linear,
    NearestMipmapNearest,
    LinearMipmapNearest,
    NearestMipmapLinear,
    LinearMipmapLinear
}

/// <summary>
/// Texture magnification filters
/// </summary>
public enum TextureMagFilter
{
    Nearest,
    Linear
}
