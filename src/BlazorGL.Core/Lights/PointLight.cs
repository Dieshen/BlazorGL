namespace BlazorGL.Core.Lights;

/// <summary>
/// Point light that emits in all directions from a point
/// </summary>
public class PointLight : Light
{
    /// <summary>
    /// Maximum distance of light influence (0 = infinite)
    /// </summary>
    public float Distance { get; set; } = 0;

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
    public PointLightShadow Shadow { get; set; }

    public PointLight()
    {
        Name = "PointLight";
        Shadow = new PointLightShadow();
        Shadow.SetLight(this);
    }

    public PointLight(Math.Color color, float intensity = 1.0f, float distance = 0, float decay = 2.0f)
    {
        Name = "PointLight";
        Color = color;
        Intensity = intensity;
        Distance = distance;
        Decay = decay;
        Shadow = new PointLightShadow();
        Shadow.SetLight(this);
    }
}
