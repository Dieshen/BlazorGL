using BlazorGL.Core.Shaders;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Custom shader material without any built-in uniforms or attributes
/// Provides complete control over the shader pipeline
/// </summary>
public class RawShaderMaterial : Material
{
    /// <summary>
    /// Custom vertex shader source
    /// </summary>
    public string VertexShader { get; set; } = string.Empty;

    /// <summary>
    /// Custom fragment shader source
    /// </summary>
    public string FragmentShader { get; set; } = string.Empty;

    public RawShaderMaterial(string vertexShader, string fragmentShader)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
        InitializeShader();
    }

    public override void InitializeShader()
    {
        if (!string.IsNullOrEmpty(VertexShader) && !string.IsNullOrEmpty(FragmentShader))
        {
            Shader = new Shader(VertexShader, FragmentShader);
            NeedsCompile = true;
        }
    }

    public override void UpdateUniforms()
    {
        // User provides all uniforms manually
        // No automatic uniforms added
    }
}
