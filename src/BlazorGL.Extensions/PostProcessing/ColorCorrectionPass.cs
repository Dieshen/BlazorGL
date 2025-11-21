using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Color correction pass for adjusting brightness, contrast, saturation, hue, exposure, and gamma
/// </summary>
public class ColorCorrectionPass : ShaderPass
{
    /// <summary>
    /// Brightness adjustment (-1 to 1, default 0)
    /// </summary>
    public float Brightness { get; set; } = 0.0f;

    /// <summary>
    /// Contrast adjustment (0 to 2, default 1)
    /// </summary>
    public float Contrast { get; set; } = 1.0f;

    /// <summary>
    /// Saturation adjustment (0 to 2, default 1)
    /// </summary>
    public float Saturation { get; set; } = 1.0f;

    /// <summary>
    /// Hue shift (0 to 1, default 0)
    /// </summary>
    public float Hue { get; set; } = 0.0f;

    /// <summary>
    /// Exposure adjustment (0 to 2, default 1)
    /// </summary>
    public float Exposure { get; set; } = 1.0f;

    /// <summary>
    /// Gamma correction (0.5 to 3, default 2.2)
    /// </summary>
    public float Gamma { get; set; } = 2.2f;

    public ColorCorrectionPass() : base(new ShaderMaterial(
        ColorCorrectionShader.VertexShader,
        ColorCorrectionShader.FragmentShader))
    {
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        // Update uniforms
        _material.Uniforms["brightness"] = Brightness;
        _material.Uniforms["contrast"] = Contrast;
        _material.Uniforms["saturation"] = Saturation;
        _material.Uniforms["hue"] = Hue;
        _material.Uniforms["exposure"] = Exposure;
        _material.Uniforms["gamma"] = Gamma;

        // Call base render
        base.Render(renderer, input, output);
    }
}
