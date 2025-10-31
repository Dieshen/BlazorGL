using BlazorGL.Core.Shaders;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material with custom GLSL shaders
/// </summary>
public class ShaderMaterial : Material
{
    /// <summary>
    /// Custom vertex shader source
    /// </summary>
    public string VertexShader { get; set; }

    /// <summary>
    /// Custom fragment shader source
    /// </summary>
    public string FragmentShader { get; set; }

    public ShaderMaterial(string vertexShader, string fragmentShader)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(VertexShader, FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        // Custom uniforms are set directly via the Uniforms dictionary
    }
}
