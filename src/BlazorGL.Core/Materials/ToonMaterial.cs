using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material for cel-shading/cartoon-style non-photorealistic rendering
/// </summary>
public class ToonMaterial : Material
{
    /// <summary>
    /// Base color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Diffuse texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Gradient map for toon shading (defines light intensity steps)
    /// </summary>
    public Texture? GradientMap { get; set; }

    /// <summary>
    /// Emissive (self-illuminated) color
    /// </summary>
    public Math.Color Emissive { get; set; } = new Math.Color(0, 0, 0);

    /// <summary>
    /// Emissive intensity
    /// </summary>
    public float EmissiveIntensity { get; set; } = 1.0f;

    public ToonMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Toon.VertexShader, ShaderLibrary.Toon.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["emissive"] = Emissive.ToVector3();
        Uniforms["emissiveIntensity"] = EmissiveIntensity;
        Uniforms["useMap"] = Map != null;
        Uniforms["useGradientMap"] = GradientMap != null;

        if (Map != null)
            Uniforms["map"] = Map;
        if (GradientMap != null)
            Uniforms["gradientMap"] = GradientMap;
    }
}
