using BlazorGL.Core.Textures;
using Microsoft.JSInterop;

namespace BlazorGL.Loaders.Textures;

/// <summary>
/// Loader for KTX2 texture container format with Basis Universal supercompression
/// Supports UASTC and ETC1S transcoding to GPU-native formats
/// </summary>
public class KTX2Loader : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;
    private IJSObjectReference? _module;
    private bool _isInitialized = false;

    /// <summary>
    /// Create KTX2 loader
    /// </summary>
    public KTX2Loader(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Initialize the Basis Universal transcoder
    /// Must be called before loading any textures
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorGL.Loaders/blazorgl.ktx2.js");

            await _module.InvokeVoidAsync("initialize");
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to initialize KTX2 loader. Ensure basis_universal WASM module is available.", ex);
        }
    }

    /// <summary>
    /// Load KTX2 texture from URL
    /// </summary>
    public async Task<CompressedTexture> LoadAsync(string url)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("KTX2Loader not initialized. Call InitializeAsync first.");

        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        if (_module == null)
            throw new InvalidOperationException("JavaScript module not loaded");

        // Download KTX2 file
        byte[] data = await _httpClient.GetByteArrayAsync(url);

        // Parse KTX2 container in JavaScript (better performance for binary parsing)
        var containerInfo = await _module.InvokeAsync<KTX2ContainerInfo>("parseKTX2", data);

        // Detect best GPU format for this device
        var targetFormat = await DetectBestFormatAsync();

        // Transcode to target format in JavaScript
        var transcodedData = await _module.InvokeAsync<List<TranscodedMipmap>>(
            "transcode", data, targetFormat);

        // Convert to MipmapData
        var mipmaps = transcodedData.Select(m => new MipmapData
        {
            Data = m.Data,
            Width = m.Width,
            Height = m.Height,
            Level = m.Level
        }).ToList();

        // Create compressed texture
        var texture = new CompressedTexture(mipmaps, MapToCompressedFormat(targetFormat))
        {
            Width = containerInfo.Width,
            Height = containerInfo.Height,
            GenerateMipmaps = false, // KTX2 includes mipmaps
            Name = Path.GetFileName(url)
        };

        return texture;
    }

    private async Task<GPUTextureFormat> DetectBestFormatAsync()
    {
        if (_module == null)
            throw new InvalidOperationException("JavaScript module not loaded");

        // Query WebGL extensions via JavaScript
        var capabilities = await _module.InvokeAsync<TextureCapabilities>("getCapabilities");

        // Prefer in order: ASTC > BC7 > ETC2 > PVRTC > RGB565
        if (capabilities.ASTC)
            return GPUTextureFormat.ASTC_4x4;

        if (capabilities.BC7)
            return GPUTextureFormat.BC7_RGBA;

        if (capabilities.ETC2)
            return GPUTextureFormat.ETC2_RGBA8;

        if (capabilities.PVRTC)
            return GPUTextureFormat.PVRTC_RGBA_4BPP;

        // Fallback to uncompressed
        return GPUTextureFormat.RGB565;
    }

    private CompressedTextureFormat MapToCompressedFormat(GPUTextureFormat gpuFormat)
    {
        return gpuFormat switch
        {
            GPUTextureFormat.ASTC_4x4 => CompressedTextureFormat.ASTC_4x4,
            GPUTextureFormat.BC7_RGBA => CompressedTextureFormat.BC7,
            GPUTextureFormat.ETC2_RGBA8 => CompressedTextureFormat.ETC2_RGBA,
            GPUTextureFormat.PVRTC_RGBA_4BPP => CompressedTextureFormat.PVRTC_RGBA_4BPP,
            GPUTextureFormat.RGB565 => CompressedTextureFormat.RGB565,
            _ => throw new NotSupportedException($"GPU format {gpuFormat} not supported")
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
            _module = null;
        }

        _isInitialized = false;
    }
}

/// <summary>
/// KTX2 container metadata
/// </summary>
public class KTX2ContainerInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Levels { get; set; }
    public bool IsUASTC { get; set; }
    public bool HasAlpha { get; set; }
    public bool IsSRGB { get; set; }
}

/// <summary>
/// GPU texture format capabilities
/// </summary>
public class TextureCapabilities
{
    public bool ASTC { get; set; }
    public bool BC7 { get; set; }
    public bool ETC2 { get; set; }
    public bool PVRTC { get; set; }
}

/// <summary>
/// Target GPU texture format for transcoding
/// </summary>
public enum GPUTextureFormat
{
    ASTC_4x4,
    BC7_RGBA,
    ETC2_RGBA8,
    PVRTC_RGBA_4BPP,
    RGB565
}

/// <summary>
/// Transcoded mipmap data from JavaScript
/// </summary>
public class TranscodedMipmap
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public int Width { get; set; }
    public int Height { get; set; }
    public int Level { get; set; }
}
