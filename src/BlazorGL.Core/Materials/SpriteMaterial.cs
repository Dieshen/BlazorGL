using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;
using System.Numerics;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material for rendering 2D sprites that always face the camera
/// </summary>
public class SpriteMaterial : Material
{
    /// <summary>
    /// Sprite color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Sprite texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Rotation of the sprite in radians
    /// </summary>
    public float Rotation { get; set; } = 0;

    /// <summary>
    /// Whether the sprite size is affected by distance from camera
    /// </summary>
    public bool SizeAttenuation { get; set; } = true;

    public SpriteMaterial()
    {
        Transparent = true;
        DepthWrite = false;
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Sprite.VertexShader, ShaderLibrary.Sprite.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["rotation"] = Rotation;
        Uniforms["sizeAttenuation"] = SizeAttenuation;
        Uniforms["useMap"] = Map != null;

        if (Map != null)
            Uniforms["map"] = Map;
    }
}
