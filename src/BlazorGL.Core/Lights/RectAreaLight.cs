namespace BlazorGL.Core.Lights;

/// <summary>
/// Rectangular area light - emits light uniformly across a planar surface
/// Useful for architectural visualization (windows, panels, etc.)
/// </summary>
public class RectAreaLight : Light
{
    /// <summary>
    /// Width of the light rectangle
    /// </summary>
    public float Width { get; set; } = 10f;

    /// <summary>
    /// Height of the light rectangle
    /// </summary>
    public float Height { get; set; } = 10f;

    public RectAreaLight(Math.Color color, float intensity = 1.0f, float width = 10f, float height = 10f)
    {
        Name = "RectAreaLight";
        Color = color;
        Intensity = intensity;
        Width = width;
        Height = height;
    }
}
