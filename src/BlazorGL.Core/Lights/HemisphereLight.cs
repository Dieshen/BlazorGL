namespace BlazorGL.Core.Lights;

/// <summary>
/// Light with two colors - one from above (sky) and one from below (ground)
/// Creates natural outdoor lighting
/// </summary>
public class HemisphereLight : Light
{
    /// <summary>
    /// Sky color (light from above)
    /// </summary>
    public Math.Color SkyColor { get; set; }

    /// <summary>
    /// Ground color (light from below)
    /// </summary>
    public Math.Color GroundColor { get; set; }

    public HemisphereLight(Math.Color skyColor, Math.Color groundColor, float intensity = 1.0f)
    {
        Name = "HemisphereLight";
        SkyColor = skyColor;
        GroundColor = groundColor;
        Intensity = intensity;
    }
}
