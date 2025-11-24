using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Basic material with no lighting calculations
/// </summary>
public class BasicMaterial : Material
{
    /// <summary>
    /// Base color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Diffuse texture map
    /// </summary>
    public Texture? Map { get; set; }

    public BasicMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Basic.VertexShader, ShaderLibrary.Basic.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["useMap"] = Map != null;
        if (Map != null)
        {
            Uniforms["map"] = Map;
        }
    }
}
