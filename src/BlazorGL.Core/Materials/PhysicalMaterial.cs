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

    /// <summary>
    /// Clearcoat intensity map
    /// </summary>
    public Texture? ClearcoatMap { get; set; }

    /// <summary>
    /// Clearcoat roughness map
    /// </summary>
    public Texture? ClearcoatRoughnessMap { get; set; }

    /// <summary>
    /// Clearcoat normal map
    /// </summary>
    public Texture? ClearcoatNormalMap { get; set; }

    /// <summary>
    /// Clearcoat normal map scale
    /// </summary>
    public System.Numerics.Vector2 ClearcoatNormalScale { get; set; } = new System.Numerics.Vector2(1, 1);

    // Transmission (for glass, water)
    /// <summary>
    /// Light transmission (0 = opaque, 1 = fully transparent)
    /// </summary>
    public float Transmission { get; set; } = 0.0f;

    /// <summary>
    /// Thickness for transmission
    /// </summary>
    public float Thickness { get; set; } = 0.0f;

    /// <summary>
    /// Transmission map
    /// </summary>
    public Texture? TransmissionMap { get; set; }

    /// <summary>
    /// Thickness map
    /// </summary>
    public Texture? ThicknessMap { get; set; }

    /// <summary>
    /// Attenuation distance for volumetric absorption
    /// </summary>
    public float AttenuationDistance { get; set; } = float.PositiveInfinity;

    /// <summary>
    /// Attenuation color for Beer's law absorption
    /// </summary>
    public Math.Color AttenuationColor { get; set; } = Math.Color.White;

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

    /// <summary>
    /// Sheen color map
    /// </summary>
    public Texture? SheenColorMap { get; set; }

    /// <summary>
    /// Sheen roughness map
    /// </summary>
    public Texture? SheenRoughnessMap { get; set; }

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
        Uniforms["clearcoatNormalScale"] = ClearcoatNormalScale;
        Uniforms["transmission"] = Transmission;
        Uniforms["thickness"] = Thickness;
        Uniforms["attenuationDistance"] = AttenuationDistance;
        Uniforms["attenuationColor"] = AttenuationColor.ToVector3();
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

        // Clearcoat maps
        Uniforms["useClearcoatMap"] = ClearcoatMap != null;
        Uniforms["useClearcoatRoughnessMap"] = ClearcoatRoughnessMap != null;
        Uniforms["useClearcoatNormalMap"] = ClearcoatNormalMap != null;

        // Transmission maps
        Uniforms["useTransmissionMap"] = TransmissionMap != null;
        Uniforms["useThicknessMap"] = ThicknessMap != null;

        // Sheen maps
        Uniforms["useSheenColorMap"] = SheenColorMap != null;
        Uniforms["useSheenRoughnessMap"] = SheenRoughnessMap != null;

        if (Map != null) Uniforms["map"] = Map;
        if (MetalnessMap != null) Uniforms["metalnessMap"] = MetalnessMap;
        if (RoughnessMap != null) Uniforms["roughnessMap"] = RoughnessMap;
        if (NormalMap != null) Uniforms["normalMap"] = NormalMap;
        if (AoMap != null) Uniforms["aoMap"] = AoMap;

        if (ClearcoatMap != null) Uniforms["clearcoatMap"] = ClearcoatMap;
        if (ClearcoatRoughnessMap != null) Uniforms["clearcoatRoughnessMap"] = ClearcoatRoughnessMap;
        if (ClearcoatNormalMap != null) Uniforms["clearcoatNormalMap"] = ClearcoatNormalMap;

        if (TransmissionMap != null) Uniforms["transmissionMap"] = TransmissionMap;
        if (ThicknessMap != null) Uniforms["thicknessMap"] = ThicknessMap;

        if (SheenColorMap != null) Uniforms["sheenColorMap"] = SheenColorMap;
        if (SheenRoughnessMap != null) Uniforms["sheenRoughnessMap"] = SheenRoughnessMap;
    }
}
