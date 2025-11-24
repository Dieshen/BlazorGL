using BlazorGL.Core.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace BlazorGL.Loaders;

/// <summary>
/// Loads textures from various sources
/// </summary>
public static class TextureLoader
{
    /// <summary>
    /// Loads a texture from a URL
    /// </summary>
    public static async Task<Texture> LoadAsync(string url, Action<float>? onProgress = null)
    {
        using var httpClient = new HttpClient();

        onProgress?.Invoke(0);

        // Download image data
        var imageData = await httpClient.GetByteArrayAsync(url);

        onProgress?.Invoke(0.5f);

        // Load with ImageSharp
        var texture = LoadFromBytes(imageData);

        onProgress?.Invoke(1.0f);

        return texture;
    }

    /// <summary>
    /// Loads a texture from byte array
    /// </summary>
    public static Texture LoadFromBytes(byte[] data)
    {
        using var image = Image.Load<Rgba32>(data);

        // Flip vertically (OpenGL expects bottom-left origin)
        image.Mutate(x => x.Flip(FlipMode.Vertical));

        var texture = new Texture
        {
            Width = image.Width,
            Height = image.Height
        };

        // Convert to byte array
        texture.ImageData = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(texture.ImageData);
        texture.NeedsUpdate = true;

        return texture;
    }

    /// <summary>
    /// Loads a texture from a stream
    /// </summary>
    public static async Task<Texture> LoadFromStreamAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return LoadFromBytes(ms.ToArray());
    }

    /// <summary>
    /// Creates a solid color texture
    /// </summary>
    public static Texture CreateColor(Core.Math.Color color, int width = 1, int height = 1)
    {
        var texture = new Texture
        {
            Width = width,
            Height = height
        };

        texture.ImageData = new byte[width * height * 4];

        byte r = (byte)(color.R * 255);
        byte g = (byte)(color.G * 255);
        byte b = (byte)(color.B * 255);
        byte a = (byte)(color.A * 255);

        for (int i = 0; i < width * height; i++)
        {
            texture.ImageData[i * 4] = r;
            texture.ImageData[i * 4 + 1] = g;
            texture.ImageData[i * 4 + 2] = b;
            texture.ImageData[i * 4 + 3] = a;
        }

        texture.NeedsUpdate = true;
        return texture;
    }

    /// <summary>
    /// Creates a checkerboard pattern texture
    /// </summary>
    public static Texture CreateCheckerboard(int size = 256, int checkSize = 32,
                                            Core.Math.Color color1 = default,
                                            Core.Math.Color color2 = default)
    {
        if (color1.Equals(default(Core.Math.Color)))
            color1 = Core.Math.Color.White;
        if (color2.Equals(default(Core.Math.Color)))
            color2 = Core.Math.Color.Black;

        var texture = new Texture
        {
            Width = size,
            Height = size
        };

        texture.ImageData = new byte[size * size * 4];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool check = ((x / checkSize) + (y / checkSize)) % 2 == 0;
                var color = check ? color1 : color2;

                int idx = (y * size + x) * 4;
                texture.ImageData[idx] = (byte)(color.R * 255);
                texture.ImageData[idx + 1] = (byte)(color.G * 255);
                texture.ImageData[idx + 2] = (byte)(color.B * 255);
                texture.ImageData[idx + 3] = (byte)(color.A * 255);
            }
        }

        texture.NeedsUpdate = true;
        return texture;
    }
}
