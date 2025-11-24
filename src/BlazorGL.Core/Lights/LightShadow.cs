using BlazorGL.Core.Cameras;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Shadow map filtering type
/// </summary>
public enum ShadowMapType
{
    /// <summary>
    /// Basic hard shadows (no filtering)
    /// </summary>
    Basic,

    /// <summary>
    /// Percentage Closer Filtering (PCF) - smooth shadows
    /// </summary>
    PCF,

    /// <summary>
    /// PCF with larger kernel for softer shadows
    /// </summary>
    PCFSoft,

    /// <summary>
    /// Variance Shadow Maps - statistical filtering
    /// </summary>
    VSM
}

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
    /// Shadow map filtering type
    /// </summary>
    public ShadowMapType Type { get; set; } = ShadowMapType.PCF;

    /// <summary>
    /// Number of PCF samples (9 = 3x3, 25 = 5x5, 64 = 8x8)
    /// </summary>
    public int PCFSamples { get; set; } = 9;

    /// <summary>
    /// Shadow softness factor (for PCFSoft and PCSS)
    /// </summary>
    public float ShadowSoftness { get; set; } = 1.0f;

    /// <summary>
    /// Light size for PCSS (affects penumbra calculation)
    /// </summary>
    public float LightSize { get; set; } = 1.0f;

    /// <summary>
    /// VSM: Minimum variance to prevent precision issues
    /// </summary>
    public float MinVariance { get; set; } = 0.00001f;

    /// <summary>
    /// VSM: Light bleeding reduction factor (0-1)
    /// </summary>
    public float LightBleedingReduction { get; set; } = 0.1f;

    /// <summary>
    /// VSM: Gaussian blur size for shadow map
    /// </summary>
    public int BlurSize { get; set; } = 3;

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
