using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;
using System.Numerics;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material for rendering point clouds
/// </summary>
public class PointsMaterial : Material
{
    /// <summary>
    /// Point color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Point size in pixels
    /// </summary>
    public float Size { get; set; } = 1.0f;

    /// <summary>
    /// Whether point size attenuates with distance
    /// </summary>
    public bool SizeAttenuation { get; set; } = true;

    /// <summary>
    /// Optional texture for point sprites
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Whether to use vertex colors instead of uniform color
    /// </summary>
    public bool VertexColors { get; set; } = false;

    public PointsMaterial()
    {
        CullMode = CullMode.None; // Points don't need culling
        Transparent = false;
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Points.VertexShader, ShaderLibrary.Points.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["size"] = Size;
        Uniforms["sizeAttenuation"] = SizeAttenuation;
        Uniforms["useVertexColors"] = VertexColors;
        Uniforms["useMap"] = Map != null;
        if (Map != null)
        {
            Uniforms["map"] = Map;
        }
    }
}
