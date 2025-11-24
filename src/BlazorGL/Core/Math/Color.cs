using System.Numerics;

namespace BlazorGL.Core.Math;

/// <summary>
/// Represents an RGBA color with floating-point components (0-1 range)
/// </summary>
public struct Color
{
    /// <summary>
    /// Red component (0-1)
    /// </summary>
    public float R { get; set; }

    /// <summary>
    /// Green component (0-1)
    /// </summary>
    public float G { get; set; }

    /// <summary>
    /// Blue component (0-1)
    /// </summary>
    public float B { get; set; }

    /// <summary>
    /// Alpha component (0-1)
    /// </summary>
    public float A { get; set; }

    /// <summary>
    /// Creates a new color with the specified RGB components and alpha=1
    /// </summary>
    public Color(float r, float g, float b) : this(r, g, b, 1.0f) { }

    /// <summary>
    /// Creates a new color with the specified RGBA components
    /// </summary>
    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Creates a color from a hexadecimal value (0xRRGGBB or 0xAARRGGBB)
    /// </summary>
    public static Color FromHex(uint hex, bool hasAlpha = false)
    {
        if (hasAlpha)
        {
            return new Color(
                ((hex >> 16) & 0xFF) / 255f,
                ((hex >> 8) & 0xFF) / 255f,
                (hex & 0xFF) / 255f,
                ((hex >> 24) & 0xFF) / 255f
            );
        }
        else
        {
            return new Color(
                ((hex >> 16) & 0xFF) / 255f,
                ((hex >> 8) & 0xFF) / 255f,
                (hex & 0xFF) / 255f,
                1.0f
            );
        }
    }

    /// <summary>
    /// Converts the color to a hexadecimal value
    /// </summary>
    public uint ToHex(bool includeAlpha = false)
    {
        uint r = (uint)(R * 255);
        uint g = (uint)(G * 255);
        uint b = (uint)(B * 255);
        uint a = (uint)(A * 255);

        if (includeAlpha)
            return (a << 24) | (r << 16) | (g << 8) | b;
        else
            return (r << 16) | (g << 8) | b;
    }

    /// <summary>
    /// Converts to Vector3 (RGB only)
    /// </summary>
    public Vector3 ToVector3() => new(R, G, B);

    /// <summary>
    /// Converts to Vector4 (RGBA)
    /// </summary>
    public Vector4 ToVector4() => new(R, G, B, A);

    /// <summary>
    /// Linearly interpolates between two colors
    /// </summary>
    public static Color Lerp(Color a, Color b, float t)
    {
        return new Color(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            a.A + (b.A - a.A) * t
        );
    }

    /// <summary>
    /// Multiplies two colors component-wise
    /// </summary>
    public static Color operator *(Color a, Color b)
    {
        return new Color(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
    }

    /// <summary>
    /// Multiplies a color by a scalar
    /// </summary>
    public static Color operator *(Color color, float scalar)
    {
        return new Color(color.R * scalar, color.G * scalar, color.B * scalar, color.A * scalar);
    }

    /// <summary>
    /// Adds two colors component-wise
    /// </summary>
    public static Color operator +(Color a, Color b)
    {
        return new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
    }

    // Common colors
    public static readonly Color White = new(1, 1, 1);
    public static readonly Color Black = new(0, 0, 0);
    public static readonly Color Red = new(1, 0, 0);
    public static readonly Color Green = new(0, 1, 0);
    public static readonly Color Blue = new(0, 0, 1);
    public static readonly Color Yellow = new(1, 1, 0);
    public static readonly Color Cyan = new(0, 1, 1);
    public static readonly Color Magenta = new(1, 0, 1);
    public static readonly Color Transparent = new(0, 0, 0, 0);

    public override string ToString() => $"Color(R:{R:F2}, G:{G:F2}, B:{B:F2}, A:{A:F2})";
}
