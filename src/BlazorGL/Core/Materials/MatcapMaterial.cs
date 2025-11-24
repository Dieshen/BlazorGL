using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material that uses a matcap (material capture) texture for fast stylized rendering
/// Matcap encodes lighting and material properties in a single spherical texture
/// </summary>
public class MatcapMaterial : Material
{
    /// <summary>
    /// Base color to multiply with matcap
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Matcap texture (sphere-mapped material capture)
    /// </summary>
    public Texture? Matcap { get; set; }

    /// <summary>
    /// Optional diffuse texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Normal map for additional surface detail
    /// </summary>
    public Texture? NormalMap { get; set; }

    /// <summary>
    /// Normal map intensity
    /// </summary>
    public float NormalScale { get; set; } = 1.0f;

    public MatcapMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Matcap.VertexShader, ShaderLibrary.Matcap.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["normalScale"] = NormalScale;
        Uniforms["useMatcap"] = Matcap != null;
        Uniforms["useMap"] = Map != null;
        Uniforms["useNormalMap"] = NormalMap != null;

        if (Matcap != null)
            Uniforms["matcap"] = Matcap;
        if (Map != null)
            Uniforms["map"] = Map;
        if (NormalMap != null)
            Uniforms["normalMap"] = NormalMap;
    }
}
