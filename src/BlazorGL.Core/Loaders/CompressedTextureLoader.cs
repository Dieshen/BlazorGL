using BlazorGL.Core.Textures;
using Microsoft.JSInterop;

namespace BlazorGL.Core.Loaders;

/// <summary>
/// Loader for compressed texture formats (DXT, ETC, PVRTC, ASTC, etc.)
/// Compressed textures reduce memory usage and improve performance
/// </summary>
public class CompressedTextureLoader
{
    private readonly IJSRuntime _jsRuntime;
    private readonly LoadingManager? _manager;

    public CompressedTextureLoader(IJSRuntime jsRuntime, LoadingManager? manager = null)
    {
        _jsRuntime = jsRuntime;
        _manager = manager;
    }

    /// <summary>
    /// Loads a compressed texture from URL
    /// </summary>
    public async Task<CompressedTexture?> LoadAsync(string url)
    {
        _manager?.ItemStart(url);

        try
        {
            // Load compressed texture data
            var data = await LoadCompressedDataAsync(url);

            if (data != null)
            {
                var texture = new CompressedTexture
                {
                    CompressedData = data.Data,
                    Format = data.Format,
                    Width = data.Width,
                    Height = data.Height,
                    MipmapCount = data.MipmapCount
                };

                texture.NeedsUpdate = true;
                _manager?.ItemEnd(url);

                return texture;
            }

            _manager?.ItemError(url);
            return null;
        }
        catch (Exception ex)
        {
            _manager?.ItemError(url);
            throw new Exception($"Failed to load compressed texture from {url}: {ex.Message}", ex);
        }
    }

    private async Task<CompressedTextureData?> LoadCompressedDataAsync(string url)
    {
        try
        {
            // Use JS interop to load and parse compressed texture format
            return await _jsRuntime.InvokeAsync<CompressedTextureData>("blazorGL.loadCompressedTexture", url);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Compressed texture data structure
/// </summary>
public class CompressedTextureData
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public CompressedTextureFormat Format { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int MipmapCount { get; set; } = 1;
}

/// <summary>
/// Compressed texture with specific format
/// </summary>
public class CompressedTexture : Texture
{
    /// <summary>
    /// Compressed texture data
    /// </summary>
    public byte[] CompressedData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Compression format
    /// </summary>
    public CompressedTextureFormat Format { get; set; }

    /// <summary>
    /// Number of mipmap levels
    /// </summary>
    public int MipmapCount { get; set; } = 1;

    public CompressedTexture()
    {
        Name = "CompressedTexture";
    }
}

/// <summary>
/// Compressed texture formats
/// </summary>
public enum CompressedTextureFormat
{
    // S3TC/DXT formats (desktop)
    RGB_S3TC_DXT1,
    RGBA_S3TC_DXT1,
    RGBA_S3TC_DXT3,
    RGBA_S3TC_DXT5,

    // ETC formats (mobile)
    RGB_ETC1,
    RGB_ETC2,
    RGBA_ETC2_EAC,

    // PVRTC formats (iOS)
    RGB_PVRTC_4BPPV1,
    RGB_PVRTC_2BPPV1,
    RGBA_PVRTC_4BPPV1,
    RGBA_PVRTC_2BPPV1,

    // ASTC formats (modern mobile/desktop)
    RGBA_ASTC_4x4,
    RGBA_ASTC_8x8,
    RGBA_ASTC_12x12
}
