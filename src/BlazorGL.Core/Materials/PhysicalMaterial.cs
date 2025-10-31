using BlazorGL.Core.Shaders;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Advanced physically-based material with additional features like clearcoat,
/// transmission, sheen, and iridescence
/// </summary>
public class PhysicalMaterial : Material
{
    /// <summary>
    /// Base color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Metalness (0 = dielectric, 1 = metal)
    /// </summary>
    public float Metalness { get; set; } = 0.0f;

    /// <summary>
    /// Roughness (0 = smooth, 1 = rough)
    /// </summary>
    public float Roughness { get; set; } = 1.0f;

    /// <summary>
    /// Diffuse texture map
    /// </summary>
    public Texture? Map { get; set; }

    /// <summary>
    /// Metalness texture map
    /// </summary>
    public Texture? MetalnessMap { get; set; }

    /// <summary>
    /// Roughness texture map
    /// </summary>
    public Texture? RoughnessMap { get; set; }

    /// <summary>
    /// Normal map for surface detail
    /// </summary>
    public Texture? NormalMap { get; set; }

    /// <summary>
    /// Ambient occlusion map
    /// </summary>
    public Texture? AoMap { get; set; }

    /// <summary>
    /// Emissive color
    /// </summary>
    public Math.Color Emissive { get; set; } = new Math.Color(0, 0, 0);

    /// <summary>
    /// Emissive intensity
    /// </summary>
    public float EmissiveIntensity { get; set; } = 1.0f;

    // Clearcoat (for car paint, lacquer)
    /// <summary>
    /// Clearcoat layer intensity (0-1)
    /// </summary>
    public float Clearcoat { get; set; } = 0.0f;

    /// <summary>
    /// Clearcoat roughness (0-1)
    /// </summary>
    public float ClearcoatRoughness { get; set; } = 0.0f;

    // Transmission (for glass, water)
    /// <summary>
    /// Light transmission (0 = opaque, 1 = fully transparent)
    /// </summary>
    public float Transmission { get; set; } = 0.0f;

    /// <summary>
    /// Thickness for transmission
    /// </summary>
    public float Thickness { get; set; } = 0.0f;

    // Sheen (for fabric)
    /// <summary>
    /// Sheen intensity (fabric-like edge glow)
    /// </summary>
    public float Sheen { get; set; } = 0.0f;

    /// <summary>
    /// Sheen color
    /// </summary>
    public Math.Color SheenColor { get; set; } = Math.Color.White;

    /// <summary>
    /// Sheen roughness
    /// </summary>
    public float SheenRoughness { get; set; } = 1.0f;

    // Iridescence
    /// <summary>
    /// Iridescence intensity (soap bubble effect)
    /// </summary>
    public float Iridescence { get; set; } = 0.0f;

    /// <summary>
    /// Index of refraction (typically 1.0-2.333)
    /// </summary>
    public float Ior { get; set; } = 1.5f;

    public PhysicalMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Physical.VertexShader, ShaderLibrary.Physical.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["color"] = Color.ToVector3();
        Uniforms["opacity"] = Opacity;
        Uniforms["metalness"] = Metalness;
        Uniforms["roughness"] = Roughness;
        Uniforms["emissive"] = Emissive.ToVector3();
        Uniforms["emissiveIntensity"] = EmissiveIntensity;

        // Advanced properties
        Uniforms["clearcoat"] = Clearcoat;
        Uniforms["clearcoatRoughness"] = ClearcoatRoughness;
        Uniforms["transmission"] = Transmission;
        Uniforms["thickness"] = Thickness;
        Uniforms["sheen"] = Sheen;
        Uniforms["sheenColor"] = SheenColor.ToVector3();
        Uniforms["sheenRoughness"] = SheenRoughness;
        Uniforms["iridescence"] = Iridescence;
        Uniforms["ior"] = Ior;

        // Texture maps
        Uniforms["useMap"] = Map != null;
        Uniforms["useMetalnessMap"] = MetalnessMap != null;
        Uniforms["useRoughnessMap"] = RoughnessMap != null;
        Uniforms["useNormalMap"] = NormalMap != null;
        Uniforms["useAoMap"] = AoMap != null;

        if (Map != null) Uniforms["map"] = Map;
        if (MetalnessMap != null) Uniforms["metalnessMap"] = MetalnessMap;
        if (RoughnessMap != null) Uniforms["roughnessMap"] = RoughnessMap;
        if (NormalMap != null) Uniforms["normalMap"] = NormalMap;
        if (AoMap != null) Uniforms["aoMap"] = AoMap;
    }
}
