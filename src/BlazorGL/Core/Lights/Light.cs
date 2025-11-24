namespace BlazorGL.Core.Lights;

/// <summary>
/// Base class for all lights
/// </summary>
public abstract class Light : Object3D
{
    /// <summary>
    /// Light color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Light intensity
    /// </summary>
    public float Intensity { get; set; } = 1.0f;

    /// <summary>
    /// Whether this light casts shadows
    /// </summary>
    public bool CastShadow { get; set; } = false;
}
