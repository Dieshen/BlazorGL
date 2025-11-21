using BlazorGL.Core.WebGL;

namespace BlazorGL.Core.Textures;

/// <summary>
/// Render target for off-screen rendering
/// </summary>
public class RenderTarget : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Width of the render target
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Height of the render target
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// WebGL framebuffer handle
    /// </summary>
    public uint FramebufferId { get; internal set; }

    /// <summary>
    /// Color texture attachment
    /// </summary>
    public Texture Texture { get; }

    /// <summary>
    /// Depth buffer handle
    /// </summary>
    public uint DepthBufferId { get; internal set; }

    /// <summary>
    /// Whether to use depth buffer
    /// </summary>
    public bool DepthBuffer { get; set; } = true;

    /// <summary>
    /// Whether to use stencil buffer
    /// </summary>
    public bool StencilBuffer { get; set; } = false;

    public RenderTarget(int width, int height)
    {
        Width = width;
        Height = height;
        Texture = new Texture
        {
            Width = width,
            Height = height,
            MinFilter = TextureMinFilter.Linear,
            MagFilter = TextureMagFilter.Linear,
            WrapS = TextureWrapMode.ClampToEdge,
            WrapT = TextureWrapMode.ClampToEdge,
            GenerateMipmaps = false
        };
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Texture?.Dispose();
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
