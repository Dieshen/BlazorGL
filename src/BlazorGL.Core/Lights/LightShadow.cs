using BlazorGL.Core.Cameras;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Base class for light shadows
/// </summary>
public abstract class LightShadow
{
    /// <summary>
    /// Shadow camera used to render the shadow map
    /// </summary>
    public Camera Camera { get; protected set; } = null!;

    /// <summary>
    /// Shadow map render target
    /// </summary>
    public RenderTarget? Map { get; set; }

    /// <summary>
    /// Shadow map width
    /// </summary>
    public int Width { get; set; } = 512;

    /// <summary>
    /// Shadow map height
    /// </summary>
    public int Height { get; set; } = 512;

    /// <summary>
    /// Shadow bias to prevent shadow acne
    /// </summary>
    public float Bias { get; set; } = 0.0f;

    /// <summary>
    /// Normal bias
    /// </summary>
    public float NormalBias { get; set; } = 0.0f;

    /// <summary>
    /// Shadow map radius for PCF filtering
    /// </summary>
    public float Radius { get; set; } = 1.0f;

    /// <summary>
    /// Near clipping plane for shadow camera
    /// </summary>
    public float Near { get; set; } = 0.5f;

    /// <summary>
    /// Far clipping plane for shadow camera
    /// </summary>
    public float Far { get; set; } = 500f;

    /// <summary>
    /// Initialize the shadow map render target
    /// </summary>
    public virtual void Initialize()
    {
        if (Map == null)
        {
            Map = new RenderTarget(Width, Height)
            {
                DepthBuffer = true,
                StencilBuffer = false
            };
        }
    }

    /// <summary>
    /// Update the shadow camera based on light position/direction
    /// </summary>
    public abstract void UpdateShadowCamera();
}
