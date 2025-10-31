using System.Numerics;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Spot light with cone-shaped illumination
/// </summary>
public class SpotLight : Light
{
    private Vector3 _direction = new(0, -1, 0);

    /// <summary>
    /// Direction of the spotlight
    /// </summary>
    public Vector3 Direction
    {
        get => _direction;
        set => _direction = Vector3.Normalize(value);
    }

    /// <summary>
    /// Maximum distance of light influence (0 = infinite)
    /// </summary>
    public float Distance { get; set; } = 0;

    /// <summary>
    /// Cone angle in radians
    /// </summary>
    public float Angle { get; set; } = MathF.PI / 6; // 30 degrees

    /// <summary>
    /// Softness of the spotlight edge (0-1)
    /// </summary>
    public float Penumbra { get; set; } = 0.0f;

    /// <summary>
    /// Light decay/attenuation
    /// </summary>
    public float Decay { get; set; } = 2.0f;

    /// <summary>
    /// Whether this light casts shadows
    /// </summary>
    public bool CastShadow { get; set; } = false;

    /// <summary>
    /// Shadow configuration
    /// </summary>
    public SpotLightShadow Shadow { get; set; }

    public SpotLight()
    {
        Name = "SpotLight";
        Shadow = new SpotLightShadow();
        Shadow.SetLight(this);
    }

    public SpotLight(Math.Color color, float intensity = 1.0f)
    {
        Name = "SpotLight";
        Color = color;
        Intensity = intensity;
        Shadow = new SpotLightShadow();
        Shadow.SetLight(this);
    }
}
