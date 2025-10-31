using BlazorGL.Core.Shaders;
using System.Numerics;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material for rendering basic solid-color lines
/// </summary>
public class LineBasicMaterial : Material
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
    /// Whether to use vertex colors instead of uniform color
    /// </summary>
    public bool VertexColors { get; set; } = false;

    public LineBasicMaterial()
    {
        CullMode = CullMode.None; // Lines don't need culling
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.LineBasic.VertexShader, ShaderLibrary.LineBasic.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["lineWidth"] = LineWidth;
        Uniforms["useVertexColors"] = VertexColors;
    }
}
