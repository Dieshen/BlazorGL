using System.Numerics;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Directional light with parallel rays (like sunlight)
/// </summary>
public class DirectionalLight : Light
{
    private Vector3 _direction = new(0, -1, 0);

    /// <summary>
    /// Direction of the light (will be normalized)
    /// </summary>
    public Vector3 Direction
    {
        get => _direction;
        set => _direction = Vector3.Normalize(value);
    }

    /// <summary>
    /// Shadow map settings
    /// </summary>
    public ShadowMapSettings? Shadow { get; set; }

    public DirectionalLight()
    {
        Name = "DirectionalLight";
    }

    public DirectionalLight(Math.Color color, float intensity = 1.0f)
    {
        Name = "DirectionalLight";
        Color = color;
        Intensity = intensity;
    }
}

/// <summary>
/// Shadow map configuration
/// </summary>
public class ShadowMapSettings
{
    public int MapWidth { get; set; } = 1024;
    public int MapHeight { get; set; } = 1024;
    public float Near { get; set; } = 0.1f;
    public float Far { get; set; } = 500f;
    public float Bias { get; set; } = 0.001f;
}
