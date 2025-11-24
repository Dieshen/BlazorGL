using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Phong shading material
/// </summary>
public class PhongMaterial : Material
{
    /// <summary>
    /// Base color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Specular color
    /// </summary>
    public Math.Color Specular { get; set; } = new(0.05f, 0.05f, 0.05f);

    /// <summary>
    /// Shininess (specular exponent)
    /// </summary>
    public float Shininess { get; set; } = 30.0f;

    /// <summary>
    /// Diffuse texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Specular map
    /// </summary>
    public Texture? SpecularMap { get; set; }

    /// <summary>
    /// Normal map
    /// </summary>
    public Texture? NormalMap { get; set; }

    /// <summary>
    /// Bump map
    /// </summary>
    public Texture? BumpMap { get; set; }

    /// <summary>
    /// Bump map scale
    /// </summary>
    public float BumpScale { get; set; } = 1.0f;

    public PhongMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Phong.VertexShader, ShaderLibrary.Phong.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["specular"] = Specular.ToVector3();
        Uniforms["shininess"] = Shininess;
        Uniforms["opacity"] = Opacity;

        Uniforms["useMap"] = Map != null;
        if (Map != null)
            Uniforms["map"] = Map;

        Uniforms["useNormalMap"] = NormalMap != null;
        if (NormalMap != null)
            Uniforms["normalMap"] = NormalMap;

        Uniforms["useSpecularMap"] = SpecularMap != null;
        if (SpecularMap != null)
            Uniforms["specularMap"] = SpecularMap;
    }
}
