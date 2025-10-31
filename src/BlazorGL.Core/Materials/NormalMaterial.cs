using BlazorGL.Core.Shaders;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material that visualizes surface normals as RGB colors
/// Useful for debugging normal maps and geometry
/// </summary>
public class NormalMaterial : Material
{
    /// <summary>
    /// Whether to use flat shading (face normals instead of vertex normals)
    /// </summary>
    public bool FlatShading { get; set; } = false;

    public NormalMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Normal.VertexShader, ShaderLibrary.Normal.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["opacity"] = Opacity;
    }
}
