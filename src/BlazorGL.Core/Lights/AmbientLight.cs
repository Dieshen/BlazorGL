namespace BlazorGL.Core.Lights;

/// <summary>
/// Ambient light that illuminates all objects equally
/// </summary>
public class AmbientLight : Light
{
    public AmbientLight()
    {
        Name = "AmbientLight";
    }

    public AmbientLight(Math.Color color, float intensity = 1.0f)
    {
        Name = "AmbientLight";
        Color = color;
        Intensity = intensity;
    }
}
