using System.Numerics;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Directional light with parallel rays (like sunlight)
/// </summary>
public class DirectionalLight : Light
{
    private Vector3 _direction = new(0, -1, 0);
    private DirectionalLightCSM? _csm;

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

    /// <summary>
    /// Whether to use Cascaded Shadow Maps
    /// CSM eliminates perspective aliasing for large scenes
    /// </summary>
    public bool UseCSM { get; set; } = false;

    /// <summary>
    /// Cascaded Shadow Maps configuration (only used if UseCSM is true)
    /// </summary>
    public DirectionalLightCSM? CSM
    {
        get => _csm;
        set => _csm = value;
    }

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

    /// <summary>
    /// Enable Cascaded Shadow Maps for this light
    /// </summary>
    public void EnableCSM(Cameras.Camera camera, int cascadeCount = 3, float maxDistance = 1000f)
    {
        UseCSM = true;
        _csm = new DirectionalLightCSM(this, camera)
        {
            CascadeCount = cascadeCount,
            MaxDistance = maxDistance
        };
    }

    /// <summary>
    /// Disable Cascaded Shadow Maps
    /// </summary>
    public void DisableCSM()
    {
        UseCSM = false;
        _csm?.Dispose();
        _csm = null;
    }
}
