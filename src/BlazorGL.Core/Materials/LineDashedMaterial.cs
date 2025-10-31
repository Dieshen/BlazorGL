using BlazorGL.Core.Shaders;
using System.Numerics;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material for rendering dashed lines with customizable dash pattern
/// </summary>
public class LineDashedMaterial : Material
{
    /// <summary>
    /// Line color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Line width (note: lineWidth > 1 is not supported on all platforms)
    /// </summary>
    public float LineWidth { get; set; } = 1.0f;

    /// <summary>
    /// Scale of the dash pattern
    /// </summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>
    /// Size of the dash (solid portion)
    /// </summary>
    public float DashSize { get; set; } = 3.0f;

    /// <summary>
    /// Size of the gap (transparent portion)
    /// </summary>
    public float GapSize { get; set; } = 1.0f;

    /// <summary>
    /// Whether to use vertex colors instead of uniform color
    /// </summary>
    public bool VertexColors { get; set; } = false;

    public LineDashedMaterial()
    {
        CullMode = CullMode.None; // Lines don't need culling
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.LineDashed.VertexShader, ShaderLibrary.LineDashed.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["lineWidth"] = LineWidth;
        Uniforms["scale"] = Scale;
        Uniforms["dashSize"] = DashSize;
        Uniforms["gapSize"] = GapSize;
        Uniforms["useVertexColors"] = VertexColors;
    }
}
