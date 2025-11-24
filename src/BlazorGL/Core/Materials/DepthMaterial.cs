using BlazorGL.Core.Shaders;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material that renders depth values (distance from camera in view space)
/// Useful for depth visualization and debugging
/// </summary>
public class DepthMaterial : Material
{
    /// <summary>
    /// Near clipping plane distance
    /// </summary>
    public float Near { get; set; } = 0.1f;

    /// <summary>
    /// Far clipping plane distance
    /// </summary>
    public float Far { get; set; } = 1000f;

    public DepthMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Depth.VertexShader, ShaderLibrary.Depth.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["opacity"] = Opacity;
        Uniforms["near"] = Near;
        Uniforms["far"] = Far;
    }
}
