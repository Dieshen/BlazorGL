# BlazorGL Texture Loaders

BlazorGL provides advanced texture loaders for loading compressed and HDR textures, enabling high-quality graphics with smaller file sizes and better performance.

## Overview

The texture loader system supports three specialized formats:

- **KTX2** - GPU-compressed textures with Basis Universal supercompression
- **RGBE (Radiance HDR)** - High Dynamic Range environment maps
- **DDS (DirectDraw Surface)** - DirectX compressed textures

## Why Use Compressed Textures?

### Benefits

1. **Reduced File Size** - 4:1 to 16:1 compression ratios
2. **Faster Loading** - Less data to download and transfer
3. **Lower Memory Usage** - Textures stay compressed in GPU memory
4. **Better Performance** - Reduced memory bandwidth during rendering
5. **Maintained Quality** - GPU-native formats with minimal quality loss

### Format Comparison

| Format | Compression | Quality | Platform | Use Case |
|--------|------------|---------|----------|----------|
| **PNG/JPG** | None/Lossy | High | All | Uncompressed fallback |
| **BC1 (DXT1)** | 6:1 | Good | Desktop | Opaque textures |
| **BC3 (DXT5)** | 4:1 | Good | Desktop | Textures with alpha |
| **BC7** | 4:1 | Excellent | Desktop | High-quality RGBA |
| **ETC2** | 4:1 | Good | Mobile | OpenGL ES 3.0+ |
| **ASTC 4x4** | 8:1 | Excellent | Mobile | Best mobile quality |
| **PVRTC** | 8:1 | Fair | iOS | Legacy iOS devices |

## KTX2Loader

KTX2 is a universal container format that uses Basis Universal transcoding to deliver optimal compressed textures for any GPU.

### Features

- Automatic GPU format detection (ASTC, BC7, ETC2, PVRTC)
- UASTC (high quality) and ETC1S (small size) compression
- Full mipmap chain support
- Alpha channel support
- sRGB vs linear color space detection

### Usage

```csharp
using BlazorGL.Loaders.Textures;
using Microsoft.JSInterop;

// Inject dependencies
@inject IJSRuntime JSRuntime
@inject HttpClient Http

// Initialize the loader (do this once at startup)
var ktx2Loader = new KTX2Loader(JSRuntime, Http);
await ktx2Loader.InitializeAsync();

// Load a KTX2 texture
var texture = await ktx2Loader.LoadAsync("assets/textures/wood.ktx2");

// Use in material
material.Map = texture;

// Dispose when done
await ktx2Loader.DisposeAsync();
```

### Creating KTX2 Textures

Use the Basis Universal command-line tools:

```bash
# High quality (UASTC)
basisu -ktx2 -uastc input.png -output_file output.ktx2

# Smaller file size (ETC1S)
basisu -ktx2 input.png -output_file output.ktx2

# With mipmaps
basisu -ktx2 -mipmap input.png -output_file output.ktx2
```

### Best Practices

1. Always call `InitializeAsync()` before loading textures
2. Reuse the loader instance for multiple textures
3. Dispose the loader when your component unmounts
4. Use UASTC for normal maps and detail textures
5. Use ETC1S for large environment maps and albedo textures

## RGBELoader

RGBE (Radiance HDR) format stores high dynamic range images for realistic lighting and reflections.

### Features

- Floating-point HDR data (unlimited brightness range)
- RLE compression support
- Exposure adjustment
- Gamma correction
- Optional tone mapping

### Usage

```csharp
using BlazorGL.Loaders.Textures;

// Inject HttpClient
@inject HttpClient Http

// Create loader
var rgbeLoader = new RGBELoader(Http);

// Configure exposure and gamma
rgbeLoader.Exposure = 1.5f;   // Increase brightness
rgbeLoader.Gamma = 2.2f;       // sRGB gamma
rgbeLoader.ApplyToneMapping = false; // Keep HDR values

// Load HDR environment map
var envMap = await rgbeLoader.LoadAsync("assets/hdri/studio.hdr");

// Use as environment map for PBR rendering
scene.Environment = envMap;
```

### Exposure and Tone Mapping

```csharp
// For display on screen (SDR)
rgbeLoader.ApplyToneMapping = true;
rgbeLoader.Exposure = 1.0f;

// For environment lighting (HDR)
rgbeLoader.ApplyToneMapping = false;
rgbeLoader.Exposure = 1.0f;

// For very bright scenes
rgbeLoader.Exposure = 0.5f; // Darker
// OR
rgbeLoader.Exposure = 2.0f; // Brighter
```

### Creating RGBE Files

Use `hdrgen` or convert with ImageMagick:

```bash
# Convert EXR to HDR
convert input.exr output.hdr

# Adjust exposure
hdrgen -e 1.5 input.hdr output.hdr
```

### Best Practices

1. Use `.hdr` files for environment maps (360° equirectangular)
2. Keep exposure at 1.0 for physically accurate lighting
3. Enable tone mapping only for preview/display purposes
4. Use 2K or 4K resolution for environment maps
5. Pre-blur environment maps for different roughness levels

## DDSLoader

DDS (DirectDraw Surface) is Microsoft's native format for DirectX compressed textures.

### Features

- BC1-BC7 compression formats (DirectX)
- Full mipmap chain support
- Cubemap support
- DX10 extended header support

### Usage

```csharp
using BlazorGL.Loaders.Textures;

// Inject HttpClient
@inject HttpClient Http

// Create loader
var ddsLoader = new DDSLoader(Http);

// Load DDS texture
var texture = await ddsLoader.LoadAsync("assets/textures/brick_wall.dds");

// Use in material
material.Map = texture;
```

### Supported Formats

| Format | Description | Best For |
|--------|-------------|----------|
| BC1 (DXT1) | RGB or 1-bit alpha | Opaque textures, masks |
| BC2 (DXT3) | RGBA explicit alpha | Sharp alpha transitions |
| BC3 (DXT5) | RGBA interpolated alpha | Smooth alpha gradients |
| BC4 | Single channel | Height maps, displacement |
| BC5 | Two channels | Normal maps |
| BC6H | HDR float | HDR textures |
| BC7 | High quality RGBA | High-quality color + alpha |

### Creating DDS Textures

Use NVIDIA Texture Tools or DirectXTex:

```bash
# Using texconv (DirectXTex)
texconv -f BC7_UNORM -m 10 input.png -o output.dds

# BC1 for opaque textures
texconv -f BC1_UNORM -m 10 input.png -o output.dds

# BC5 for normal maps
texconv -f BC5_UNORM -m 10 normal_map.png -o output.dds
```

### Best Practices

1. Use BC7 for highest quality (desktop only)
2. Use BC1 for opaque textures (smaller size)
3. Use BC5 for normal maps (best quality/size ratio)
4. Always include full mipmap chains
5. DDS is best for Windows/Desktop targets

## Format Selection Guide

### By Platform

**Desktop (Windows/Linux)**
- Primary: KTX2 (with BC7 transcoding)
- Fallback: DDS (BC1/BC3/BC7)
- HDR: RGBE or KTX2

**Mobile (iOS/Android)**
- Primary: KTX2 (with ASTC/ETC2 transcoding)
- Fallback: None (rely on KTX2 auto-detection)
- HDR: RGBE

**Web (All Browsers)**
- Primary: KTX2 (universal support via transcoding)
- Fallback: PNG/JPG
- HDR: RGBE

### By Texture Type

**Albedo/Diffuse Maps**
- Desktop: BC1 (opaque) or BC7 (alpha)
- Mobile: ASTC 4x4 or ETC2
- Format: KTX2 with ETC1S compression

**Normal Maps**
- Desktop: BC5 or BC7
- Mobile: ASTC 4x4
- Format: KTX2 with UASTC compression

**Environment Maps**
- Desktop: BC6H (HDR) or BC7 (LDR)
- Mobile: ASTC 4x4
- Format: RGBE (HDR) or KTX2 (compressed)

**UI Textures**
- All Platforms: BC1/BC7
- Format: KTX2 or PNG (if small)

## Performance Optimization

### File Size Reduction

1. **Use KTX2 with ETC1S** for maximum compression (up to 16:1)
2. **Reduce resolution** where possible (512x512 vs 2048x2048 = 16x smaller)
3. **Generate mipmaps** at export time (not runtime)
4. **Use appropriate format** (BC1 for opaque, BC3 for alpha)

### Loading Time Optimization

1. **Preload critical textures** during loading screen
2. **Use progressive loading** for large textures
3. **Load compressed formats** (4x faster than PNG)
4. **Cache loaded textures** (don't reload)

### Memory Optimization

1. **Keep textures compressed** (use GPU-compressed formats)
2. **Use mipmap chains** (reduces memory bandwidth)
3. **Dispose unused textures** explicitly
4. **Use texture atlases** for small textures

## Example: Complete Texture Pipeline

```csharp
@page "/textures"
@inject IJSRuntime JSRuntime
@inject HttpClient Http

<h3>Advanced Texture Demo</h3>

@code {
    private KTX2Loader? ktx2Loader;
    private RGBELoader? rgbeLoader;
    private DDSLoader? ddsLoader;

    protected override async Task OnInitializedAsync()
    {
        // Initialize loaders
        ktx2Loader = new KTX2Loader(JSRuntime, Http);
        await ktx2Loader.InitializeAsync();

        rgbeLoader = new RGBELoader(Http)
        {
            Exposure = 1.0f,
            ApplyToneMapping = false
        };

        ddsLoader = new DDSLoader(Http);

        // Load textures
        await LoadTexturesAsync();
    }

    private async Task LoadTexturesAsync()
    {
        // Load compressed albedo texture
        var albedo = await ktx2Loader!.LoadAsync("assets/textures/wood_albedo.ktx2");

        // Load normal map
        var normal = await ktx2Loader.LoadAsync("assets/textures/wood_normal.ktx2");

        // Load HDR environment map
        rgbeLoader!.Exposure = 1.2f;
        var envMap = await rgbeLoader.LoadAsync("assets/hdri/studio.hdr");

        // Load DDS roughness map
        var roughness = await ddsLoader!.LoadAsync("assets/textures/wood_roughness.dds");

        // Use textures in material
        var material = new PBRMaterial
        {
            AlbedoMap = albedo,
            NormalMap = normal,
            RoughnessMap = roughness,
            EnvironmentMap = envMap
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (ktx2Loader != null)
            await ktx2Loader.DisposeAsync();
    }
}
```

## Troubleshooting

### KTX2 Loader Not Working

**Problem**: "Basis transcoder not initialized" error

**Solution**: Ensure you call `InitializeAsync()` before loading:
```csharp
await ktx2Loader.InitializeAsync();
```

**Problem**: Module import fails

**Solution**: Verify `blazorgl.ktx2.js` is in `wwwroot/_content/BlazorGL.Loaders/`

### RGBE Files Look Too Dark/Bright

**Problem**: HDR environment looks incorrect

**Solution**: Adjust exposure:
```csharp
rgbeLoader.Exposure = 1.5f; // Try different values
```

### DDS Files Won't Load

**Problem**: "Unsupported format" error

**Solution**: Check the DDS format is supported (BC1-BC7). Re-export with correct format:
```bash
texconv -f BC7_UNORM input.png -o output.dds
```

### Textures Look Blocky

**Problem**: Compressed textures show artifacts

**Solution**:
1. Use higher quality compression (UASTC instead of ETC1S)
2. Increase source resolution before compression
3. Use BC7 instead of BC1 for better quality
4. Avoid compressing textures with fine details

## Additional Resources

- [Basis Universal](https://github.com/BinomialLLC/basis_universal) - KTX2 compression tools
- [DirectXTex](https://github.com/microsoft/DirectXTex) - DDS texture tools
- [HDR Image Viewers](http://www.hdrview.org/) - View and edit HDR images
- [Poly Haven](https://polyhaven.com/) - Free HDR environment maps

## API Reference

### KTX2Loader

```csharp
public class KTX2Loader : IAsyncDisposable
{
    public KTX2Loader(IJSRuntime jsRuntime, HttpClient httpClient);
    public Task InitializeAsync();
    public Task<CompressedTexture> LoadAsync(string url);
    public ValueTask DisposeAsync();
}
```

### RGBELoader

```csharp
public class RGBELoader
{
    public RGBELoader(HttpClient httpClient);
    public float Exposure { get; set; } // Default: 1.0
    public float Gamma { get; set; }    // Default: 2.2
    public bool ApplyToneMapping { get; set; } // Default: false
    public Task<DataTexture> LoadAsync(string url);
}
```

### DDSLoader

```csharp
public class DDSLoader
{
    public DDSLoader(HttpClient httpClient);
    public Task<CompressedTexture> LoadAsync(string url);
}
```

### CompressedTexture

```csharp
public class CompressedTexture : Texture
{
    public List<MipmapData> Mipmaps { get; set; }
    public CompressedTextureFormat CompressionFormat { get; set; }
    public bool IsCubemap { get; set; }
}
```

### DataTexture

```csharp
public class DataTexture : Texture
{
    public float[]? FloatData { get; set; }
    public int[]? IntData { get; set; }
    public TextureDataType DataType { get; set; }
    public TextureFormat TextureFormat { get; set; }
    public TextureEncoding Encoding { get; set; }
}
```
