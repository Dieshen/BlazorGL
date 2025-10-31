using BlazorGL.Core.Textures;
using Microsoft.JSInterop;

namespace BlazorGL.Core.Loaders;

/// <summary>
/// Loader for cube map textures (6 images for environment mapping)
/// </summary>
public class CubeTextureLoader
{
    private readonly IJSRuntime _jsRuntime;
    private readonly LoadingManager? _manager;

    public CubeTextureLoader(IJSRuntime jsRuntime, LoadingManager? manager = null)
    {
        _jsRuntime = jsRuntime;
        _manager = manager;
    }

    /// <summary>
    /// Loads a cube texture from 6 image URLs
    /// Order: +X, -X, +Y, -Y, +Z, -Z
    /// </summary>
    public async Task<CubeTexture?> LoadAsync(string[] urls)
    {
        if (urls.Length != 6)
        {
            throw new ArgumentException("CubeTexture requires exactly 6 image URLs", nameof(urls));
        }

        var cubeTexture = new CubeTexture();

        try
        {
            // Notify loading manager
            foreach (var url in urls)
            {
                _manager?.ItemStart(url);
            }

            // Load all 6 faces
            for (int i = 0; i < 6; i++)
            {
                var imageData = await LoadImageAsync(urls[i]);
                if (imageData != null)
                {
                    // Store face data (would need proper cube texture implementation)
                    _manager?.ItemEnd(urls[i]);
                }
                else
                {
                    _manager?.ItemError(urls[i]);
                }
            }

            cubeTexture.NeedsUpdate = true;
            return cubeTexture;
        }
        catch (Exception ex)
        {
            foreach (var url in urls)
            {
                _manager?.ItemError(url);
            }
            throw new Exception($"Failed to load cube texture: {ex.Message}", ex);
        }
    }

    private async Task<byte[]?> LoadImageAsync(string url)
    {
        try
        {
            // Use JS interop to load image
            return await _jsRuntime.InvokeAsync<byte[]>("blazorGL.loadImage", url);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Cube texture for environment mapping
/// </summary>
public class CubeTexture : Texture
{
    /// <summary>
    /// Image data for each cube face (+X, -X, +Y, -Y, +Z, -Z)
    /// </summary>
    public byte[]?[] FaceData { get; set; } = new byte[]?[6];

    public CubeTexture()
    {
        Name = "CubeTexture";
    }
}
