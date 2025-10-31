using BlazorGL.Core.Textures;
using Microsoft.JSInterop;

namespace BlazorGL.Core.Loaders;

/// <summary>
/// Loader for data textures (procedural textures, lookup tables, etc.)
/// Creates textures from raw data arrays
/// </summary>
public class DataTextureLoader
{
    private readonly IJSRuntime? _jsRuntime;
    private readonly LoadingManager? _manager;

    public DataTextureLoader(IJSRuntime? jsRuntime = null, LoadingManager? manager = null)
    {
        _jsRuntime = jsRuntime;
        _manager = manager;
    }

    /// <summary>
    /// Creates a data texture from raw data
    /// </summary>
    public DataTexture Create(byte[] data, int width, int height, DataTextureFormat format = DataTextureFormat.RGBA)
    {
        var texture = new DataTexture
        {
            Data = data,
            Width = width,
            Height = height,
            Format = format
        };

        texture.NeedsUpdate = true;
        return texture;
    }

    /// <summary>
    /// Creates a data texture from float array (for HDR data)
    /// </summary>
    public DataTexture CreateFloat(float[] data, int width, int height, DataTextureFormat format = DataTextureFormat.RGBA)
    {
        var byteData = new byte[data.Length * sizeof(float)];
        Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

        var texture = new DataTexture
        {
            Data = byteData,
            Width = width,
            Height = height,
            Format = format,
            Type = DataTextureType.Float
        };

        texture.NeedsUpdate = true;
        return texture;
    }

    /// <summary>
    /// Loads data texture from URL
    /// </summary>
    public async Task<DataTexture?> LoadAsync(string url)
    {
        if (_jsRuntime == null)
            throw new InvalidOperationException("JSRuntime required for loading from URL");

        _manager?.ItemStart(url);

        try
        {
            var data = await _jsRuntime.InvokeAsync<byte[]>("blazorGL.loadDataTexture", url);

            // Parse dimensions from data or filename
            // This is simplified - actual implementation would parse file headers
            int width = 512;
            int height = 512;

            var texture = Create(data, width, height);

            _manager?.ItemEnd(url);
            return texture;
        }
        catch (Exception ex)
        {
            _manager?.ItemError(url);
            throw new Exception($"Failed to load data texture from {url}: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Data texture created from raw data
/// </summary>
public class DataTexture : Texture
{
    /// <summary>
    /// Raw texture data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Data format
    /// </summary>
    public DataTextureFormat Format { get; set; } = DataTextureFormat.RGBA;

    /// <summary>
    /// Data type
    /// </summary>
    public DataTextureType Type { get; set; } = DataTextureType.UnsignedByte;

    public DataTexture()
    {
        Name = "DataTexture";
    }
}

/// <summary>
/// Data texture formats
/// </summary>
public enum DataTextureFormat
{
    Alpha,
    RGB,
    RGBA,
    Luminance,
    LuminanceAlpha,
    Red,
    RG
}

/// <summary>
/// Data texture data types
/// </summary>
public enum DataTextureType
{
    UnsignedByte,
    Byte,
    Short,
    UnsignedShort,
    Int,
    UnsignedInt,
    Float,
    HalfFloat
}
