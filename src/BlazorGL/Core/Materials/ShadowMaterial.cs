using BlazorGL.Core.Shaders;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material that only receives shadows (renders transparent but shows shadows)
/// Useful for ground planes in AR/VR or compositing
/// </summary>
public class ShadowMaterial : Material
{
    /// <summary>
    /// Shadow color
    /// </summary>
    public Math.Color Color { get; set; } = new Math.Color(0, 0, 0);

    public ShadowMaterial()
    {
        Transparent = true;
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Shadow.VertexShader, ShaderLibrary.Shadow.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
    }
}
