using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing.Shaders;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Vignette post-processing pass
/// Applies radial darkening from edges towards center
/// </summary>
public class VignettePass : ShaderPass
{
    private readonly int _width;
    private readonly int _height;

    /// <summary>
    /// Size of the clear (unaffected) area in center (0-2, default 1.0)
    /// Larger values = smaller vignette effect
    /// </summary>
    public float Offset { get; set; } = 1.0f;

    /// <summary>
    /// Intensity of the darkening (0-1, default 1.0)
    /// 0 = no darkening, 1 = complete black at edges
    /// </summary>
    public float Darkness { get; set; } = 1.0f;

    /// <summary>
    /// Smoothness of the falloff gradient (0-1, default 0.5)
    /// Lower = sharper transition, Higher = smoother transition
    /// </summary>
    public float Smoothness { get; set; } = 0.5f;

    public VignettePass(int width, int height)
        : base(new ShaderMaterial(VignetteShader.VertexShader, VignetteShader.FragmentShader))
    {
        _width = width;
        _height = height;
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        // Update uniforms
        _material.Uniforms["offset"] = Offset;
        _material.Uniforms["darkness"] = Darkness;
        _material.Uniforms["smoothness"] = Smoothness;

        // Render
        base.Render(renderer, input, output);
    }

    /// <summary>
    /// Sets preset vignette styles
    /// </summary>
    public void SetPreset(VignettePreset preset)
    {
        switch (preset)
        {
            case VignettePreset.Subtle:
                Offset = 1.2f;
                Darkness = 0.3f;
                Smoothness = 0.8f;
                break;

            case VignettePreset.Medium:
                Offset = 1.0f;
                Darkness = 0.6f;
                Smoothness = 0.5f;
                break;

            case VignettePreset.Strong:
                Offset = 0.8f;
                Darkness = 0.9f;
                Smoothness = 0.3f;
                break;

            case VignettePreset.Dramatic:
                Offset = 0.6f;
                Darkness = 1.0f;
                Smoothness = 0.2f;
                break;

            case VignettePreset.Cinematic:
                Offset = 1.1f;
                Darkness = 0.7f;
                Smoothness = 0.6f;
                break;
        }
    }
}

/// <summary>
/// Preset vignette styles
/// </summary>
public enum VignettePreset
{
    Subtle,
    Medium,
    Strong,
    Dramatic,
    Cinematic
}
