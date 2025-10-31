using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Physically-based rendering (PBR) material
/// </summary>
public class StandardMaterial : Material
{
    /// <summary>
    /// Base color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Diffuse/albedo texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Metalness (0 = dielectric, 1 = metal)
    /// </summary>
    public float Metalness { get; set; } = 0.0f;

    /// <summary>
    /// Roughness (0 = smooth, 1 = rough)
    /// </summary>
    public float Roughness { get; set; } = 0.5f;

    /// <summary>
    /// Metalness map
    /// </summary>
    public Texture? MetalnessMap { get; set; }

    /// <summary>
    /// Roughness map
    /// </summary>
    public Texture? RoughnessMap { get; set; }

    /// <summary>
    /// Normal map
    /// </summary>
    public Texture? NormalMap { get; set; }

    /// <summary>
    /// Normal map scale
    /// </summary>
    public float NormalScale { get; set; } = 1.0f;

    /// <summary>
    /// Ambient occlusion map
    /// </summary>
    public Texture? AOMap { get; set; }

    /// <summary>
    /// AO map intensity
    /// </summary>
    public float AOMapIntensity { get; set; } = 1.0f;

    /// <summary>
    /// Emissive map
    /// </summary>
    public Texture? EmissiveMap { get; set; }

    /// <summary>
    /// Emissive color
    /// </summary>
    public Math.Color Emissive { get; set; } = Math.Color.Black;

    /// <summary>
    /// Emissive intensity
    /// </summary>
    public float EmissiveIntensity { get; set; } = 0.0f;

    /// <summary>
    /// Environment map for reflections
    /// </summary>
    public Texture? EnvMap { get; set; }

    /// <summary>
    /// Environment map intensity
    /// </summary>
    public float EnvMapIntensity { get; set; } = 1.0f;

    public StandardMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Standard.VertexShader, ShaderLibrary.Standard.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["metalness"] = Metalness;
        Uniforms["roughness"] = Roughness;
        Uniforms["opacity"] = Opacity;

        Uniforms["useMap"] = Map != null;
        if (Map != null)
            Uniforms["map"] = Map;

        Uniforms["useMetalnessMap"] = MetalnessMap != null;
        if (MetalnessMap != null)
            Uniforms["metalnessMap"] = MetalnessMap;

        Uniforms["useRoughnessMap"] = RoughnessMap != null;
        if (RoughnessMap != null)
            Uniforms["roughnessMap"] = RoughnessMap;

        Uniforms["useNormalMap"] = NormalMap != null;
        if (NormalMap != null)
            Uniforms["normalMap"] = NormalMap;

        Uniforms["emissive"] = Emissive.ToVector3();
        Uniforms["emissiveIntensity"] = EmissiveIntensity;
    }
}
