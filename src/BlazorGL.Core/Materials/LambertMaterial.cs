using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material for matte, non-shiny surfaces using Lambertian reflectance
/// Less expensive than Phong but no specular highlights
/// </summary>
public class LambertMaterial : Material
{
    /// <summary>
    /// Diffuse color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Diffuse texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Emissive (self-illuminated) color
    /// </summary>
    public Math.Color Emissive { get; set; } = new Math.Color(0, 0, 0);

    /// <summary>
    /// Emissive intensity
    /// </summary>
    public float EmissiveIntensity { get; set; } = 1.0f;

    /// <summary>
    /// Emissive texture map
    /// </summary>
    public Texture? EmissiveMap { get; set; }

    public LambertMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Lambert.VertexShader, ShaderLibrary.Lambert.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["emissive"] = Emissive.ToVector3();
        Uniforms["emissiveIntensity"] = EmissiveIntensity;
        Uniforms["useMap"] = Map != null;
        Uniforms["useEmissiveMap"] = EmissiveMap != null;

        if (Map != null)
            Uniforms["map"] = Map;
        if (EmissiveMap != null)
            Uniforms["emissiveMap"] = EmissiveMap;
    }
}
