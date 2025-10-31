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
    /// Whether this light casts shadows
    /// </summary>
    public bool CastShadow { get; set; } = false;

    /// <summary>
    /// Shadow configuration
    /// </summary>
    public DirectionalLightShadow Shadow { get; set; }

    public DirectionalLight()
    {
        Name = "DirectionalLight";
        Shadow = new DirectionalLightShadow();
        Shadow.SetLight(this);
    }

    public DirectionalLight(Math.Color color, float intensity = 1.0f)
    {
        Name = "DirectionalLight";
        Color = color;
        Intensity = intensity;
        Shadow = new DirectionalLightShadow();
        Shadow.SetLight(this);
    }
}
