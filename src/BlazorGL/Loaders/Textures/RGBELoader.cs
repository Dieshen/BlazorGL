using BlazorGL.Core.Textures;
using System.Text;

namespace BlazorGL.Loaders.Textures;

/// <summary>
/// Loader for Radiance HDR (.hdr) files in RGBE format
/// Used for high dynamic range environment maps and lighting
/// </summary>
public class RGBELoader
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Exposure adjustment multiplier (default: 1.0)
    /// </summary>
    public float Exposure { get; set; } = 1.0f;

    /// <summary>
    /// Gamma correction value (default: 2.2 for sRGB)
    /// </summary>
    public float Gamma { get; set; } = 2.2f;

    /// <summary>
    /// Whether to apply tone mapping (reduces dynamic range for display)
    /// </summary>
    public bool ApplyToneMapping { get; set; } = false;

    /// <summary>
    /// Create RGBE loader
    /// </summary>
    public RGBELoader(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Load HDR texture from URL
    /// </summary>
    public async Task<DataTexture> LoadAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        // Download .hdr file
        byte[] data = await _httpClient.GetByteArrayAsync(url);

        // Parse RGBE file
        var rgbeData = ParseRGBE(data);

        // Decode to floating-point RGB
        float[] floatData = DecodeRGBE(rgbeData);

        // Apply exposure and gamma
        ApplyExposureGamma(floatData);

        // Apply tone mapping if requested
        if (ApplyToneMapping)
        {
            ApplyReinhardToneMapping(floatData);
        }

        // Create floating-point texture
        var texture = new DataTexture(floatData, rgbeData.Width, rgbeData.Height)
        {
            DataType = TextureDataType.Float,
            TextureFormat = TextureFormat.RGB,
            Encoding = TextureEncoding.LinearEncoding,
            MinFilter = TextureMinFilter.LinearMipmapLinear,
            MagFilter = TextureMagFilter.Linear,
            GenerateMipmaps = true
        };

        return texture;
    }

    private RGBEData ParseRGBE(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Parse header
        string magic = ReadLine(reader);
        if (!magic.StartsWith("#?RADIANCE") && !magic.StartsWith("#?RGBE"))
            throw new FormatException($"Not a valid RGBE file. Magic: {magic}");

        // Read header lines
        var header = new Dictionary<string, string>();
        while (true)
        {
            string line = ReadLine(reader);
            if (string.IsNullOrEmpty(line))
                break; // Empty line = end of header

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex > 0)
            {
                string key = line.Substring(0, separatorIndex).Trim();
                string value = line.Substring(separatorIndex + 1).Trim();
                header[key] = value;
            }
        }

        // Read resolution line: "-Y height +X width"
        string resLine = ReadLine(reader);
        var resParts = resLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (resParts.Length < 4)
            throw new FormatException($"Invalid resolution line: {resLine}");

        int height = int.Parse(resParts[1]);
        int width = int.Parse(resParts[3]);

        // Read scanlines (RGBE encoded)
        byte[] rgbePixels = new byte[width * height * 4];

        for (int y = 0; y < height; y++)
        {
            ReadScanline(reader, rgbePixels, y * width * 4, width);
        }

        return new RGBEData
        {
            Width = width,
            Height = height,
            Data = rgbePixels,
            Exposure = header.ContainsKey("EXPOSURE") ? float.Parse(header["EXPOSURE"]) : 1.0f
        };
    }

    private string ReadLine(BinaryReader reader)
    {
        var line = new StringBuilder();

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte b = reader.ReadByte();
            if (b == '\n')
                break;
            if (b != '\r') // Skip carriage return
                line.Append((char)b);
        }

        return line.ToString();
    }

    private void ReadScanline(BinaryReader reader, byte[] buffer, int offset, int width)
    {
        // Check for RLE encoding marker
        byte b1 = reader.ReadByte();
        byte b2 = reader.ReadByte();
        byte b3 = reader.ReadByte();
        byte b4 = reader.ReadByte();

        // Check for new RLE format: 0x02 0x02 [width_high] [width_low]
        if (b1 == 2 && b2 == 2 && b3 == ((width >> 8) & 0xFF) && b4 == (width & 0xFF))
        {
            // RLE compressed scanline - decompress each channel separately
            for (int channel = 0; channel < 4; channel++)
            {
                int pos = offset + channel;
                int end = offset + width * 4;

                while (pos < end)
                {
                    byte code = reader.ReadByte();

                    if (code > 128)
                    {
                        // Run length encoding
                        int count = code - 128;
                        byte value = reader.ReadByte();

                        for (int i = 0; i < count && pos < end; i++)
                        {
                            buffer[pos] = value;
                            pos += 4;
                        }
                    }
                    else
                    {
                        // Literal values
                        int count = code;

                        for (int i = 0; i < count && pos < end; i++)
                        {
                            buffer[pos] = reader.ReadByte();
                            pos += 4;
                        }
                    }
                }
            }
        }
        else
        {
            // Uncompressed scanline (old format)
            buffer[offset] = b1;
            buffer[offset + 1] = b2;
            buffer[offset + 2] = b3;
            buffer[offset + 3] = b4;

            for (int i = 1; i < width; i++)
            {
                buffer[offset + i * 4] = reader.ReadByte();
                buffer[offset + i * 4 + 1] = reader.ReadByte();
                buffer[offset + i * 4 + 2] = reader.ReadByte();
                buffer[offset + i * 4 + 3] = reader.ReadByte();
            }
        }
    }

    private float[] DecodeRGBE(RGBEData rgbeData)
    {
        int pixelCount = rgbeData.Width * rgbeData.Height;
        float[] floatData = new float[pixelCount * 3];

        for (int i = 0; i < pixelCount; i++)
        {
            byte r = rgbeData.Data[i * 4];
            byte g = rgbeData.Data[i * 4 + 1];
            byte b = rgbeData.Data[i * 4 + 2];
            byte e = rgbeData.Data[i * 4 + 3];

            if (e == 0)
            {
                // Black pixel
                floatData[i * 3] = 0f;
                floatData[i * 3 + 1] = 0f;
                floatData[i * 3 + 2] = 0f;
            }
            else
            {
                // Decode: RGB * 2^(E - 128) / 256
                // Note: Division by 256 normalizes the mantissa (stored as 0-255)
                float multiplier = MathF.Pow(2f, e - 128f) / 256f;

                floatData[i * 3] = r * multiplier;
                floatData[i * 3 + 1] = g * multiplier;
                floatData[i * 3 + 2] = b * multiplier;
            }
        }

        return floatData;
    }

    private void ApplyExposureGamma(float[] data)
    {
        float invGamma = 1.0f / Gamma;

        for (int i = 0; i < data.Length; i++)
        {
            // Apply exposure
            float value = data[i] * Exposure;

            // Apply gamma correction
            data[i] = MathF.Pow(Math.Max(0f, value), invGamma);
        }
    }

    private void ApplyReinhardToneMapping(float[] data)
    {
        // Reinhard tone mapping: L_out = L_in / (1 + L_in)
        // This compresses HDR values to displayable range [0, 1]

        for (int i = 0; i < data.Length; i++)
        {
            float value = data[i];
            data[i] = value / (1.0f + value);
        }
    }
}

/// <summary>
/// Parsed RGBE file data
/// </summary>
internal class RGBEData
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public float Exposure { get; set; } = 1.0f;
}
