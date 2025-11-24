using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing.Shaders;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Lookup Table (LUT) color grading pass
/// Applies 3D color lookup tables for color transformation
/// </summary>
public class LUTPass : ShaderPass
{
    private readonly int _width;
    private readonly int _height;
    private Texture? _lutTexture;
    private int _lutSize = 16; // Default 16x16x16 LUT

    /// <summary>
    /// LUT texture (3D LUT stored as 2D texture slices)
    /// </summary>
    public Texture? LUT
    {
        get => _lutTexture;
        set
        {
            _lutTexture = value;
            if (value != null)
            {
                _material.Uniforms["tLUT"] = value;
            }
        }
    }

    /// <summary>
    /// LUT size (typically 16, 32, or 64 for 16³, 32³, 64³ LUTs)
    /// </summary>
    public int LUTSize
    {
        get => _lutSize;
        set
        {
            _lutSize = value;
            _material.Uniforms["lutSize"] = value;
        }
    }

    /// <summary>
    /// Intensity/blend amount (0 = original, 1 = fully graded)
    /// </summary>
    public float Intensity { get; set; } = 1.0f;

    public LUTPass(int width, int height)
        : base(new ShaderMaterial(LUTShader.VertexShader, LUTShader.FragmentShader))
    {
        _width = width;
        _height = height;

        // Initialize with neutral (identity) LUT
        SetNeutralLUT();
    }

    /// <summary>
    /// Sets a neutral identity LUT (no color transformation)
    /// </summary>
    public void SetNeutralLUT()
    {
        _lutTexture = GenerateNeutralLUT(_lutSize);
        _material.Uniforms["tLUT"] = _lutTexture;
        _material.Uniforms["lutSize"] = _lutSize;
    }

    /// <summary>
    /// Generates a neutral (identity) LUT texture
    /// </summary>
    private Texture GenerateNeutralLUT(int size)
    {
        // Create identity LUT where output = input
        // This is a placeholder - actual implementation would generate
        // proper texture data

        var texture = new Texture
        {
            Width = size * size,  // All slices horizontally
            Height = size,
            // Note: Would need to populate actual pixel data here
            // Each slice represents a Z-coordinate value
        };

        return texture;
    }

    /// <summary>
    /// Load LUT from texture
    /// </summary>
    /// <param name="texture">LUT texture (2D representation of 3D LUT)</param>
    /// <param name="size">Size of the LUT cube (16, 32, or 64)</param>
    public void SetLUT(Texture texture, int size)
    {
        _lutTexture = texture;
        _lutSize = size;

        _material.Uniforms["tLUT"] = texture;
        _material.Uniforms["lutSize"] = size;
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        if (_lutTexture == null)
        {
            throw new InvalidOperationException("LUTPass requires a LUT texture");
        }

        // Update intensity uniform
        _material.Uniforms["intensity"] = Intensity;

        // Render
        base.Render(renderer, input, output);
    }
}

/// <summary>
/// LUT loader for .cube file format
/// Common format used by color grading applications
/// </summary>
public static class LUTLoader
{
    /// <summary>
    /// Load LUT from .cube file format
    /// </summary>
    /// <param name="cubeFilePath">Path to .cube file</param>
    /// <returns>LUT texture and size</returns>
    public static (Texture texture, int size) LoadFromCubeFile(string cubeFilePath)
    {
        // Parse .cube file format
        // Format typically contains:
        // - Header with LUT_3D_SIZE
        // - RGB triplets for each LUT entry

        if (!File.Exists(cubeFilePath))
        {
            throw new FileNotFoundException($"LUT file not found: {cubeFilePath}");
        }

        int lutSize = 0;
        var lutData = new List<float[]>();

        var lines = File.ReadAllLines(cubeFilePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#"))
            {
                continue;
            }

            // Parse LUT size
            if (trimmedLine.StartsWith("LUT_3D_SIZE"))
            {
                var parts = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && int.TryParse(parts[1], out var size))
                {
                    lutSize = size;
                }
                continue;
            }

            // Parse RGB data
            var values = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 3)
            {
                if (float.TryParse(values[0], out var r) &&
                    float.TryParse(values[1], out var g) &&
                    float.TryParse(values[2], out var b))
                {
                    lutData.Add(new[] { r, g, b });
                }
            }
        }

        if (lutSize == 0)
        {
            throw new InvalidDataException("Invalid .cube file: LUT_3D_SIZE not found");
        }

        if (lutData.Count != lutSize * lutSize * lutSize)
        {
            throw new InvalidDataException(
                $"Invalid .cube file: Expected {lutSize * lutSize * lutSize} entries, got {lutData.Count}");
        }

        // Convert to texture format
        var texture = ConvertLUTDataToTexture(lutData, lutSize);

        return (texture, lutSize);
    }

    private static Texture ConvertLUTDataToTexture(List<float[]> lutData, int size)
    {
        // Convert 3D LUT data to 2D texture format
        // Arrange as horizontal slices: [slice0][slice1][slice2]...
        // Each slice is size x size

        var texture = new Texture
        {
            Width = size * size,  // All slices horizontally
            Height = size,
            // Note: Would need to convert float data to pixel format here
            // This is a placeholder for the actual implementation
        };

        // TODO: Convert lutData to actual texture pixel data
        // Would typically convert to RGBA format

        return texture;
    }

    /// <summary>
    /// Generate common color grading LUTs programmatically
    /// </summary>
    public static class PresetLUTs
    {
        public static Texture Warm(int size = 16)
        {
            // Generate warm color grading LUT
            return GenerateColorGradedLUT(size, warmShift: 0.1f, coolShift: -0.05f);
        }

        public static Texture Cool(int size = 16)
        {
            // Generate cool color grading LUT
            return GenerateColorGradedLUT(size, warmShift: -0.05f, coolShift: 0.1f);
        }

        public static Texture Sepia(int size = 16)
        {
            // Generate sepia tone LUT
            // This would apply sepia color transformation
            return GenerateColorGradedLUT(size, sepia: true);
        }

        private static Texture GenerateColorGradedLUT(int size, float warmShift = 0, float coolShift = 0, bool sepia = false)
        {
            // Placeholder for procedural LUT generation
            var texture = new Texture
            {
                Width = size * size,
                Height = size,
            };

            // TODO: Generate actual LUT data based on parameters

            return texture;
        }
    }
}
