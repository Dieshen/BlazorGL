using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core;

/// <summary>
/// A visible 3D object with geometry and material
/// </summary>
public class Mesh : Object3D
{
    /// <summary>
    /// The geometry defining the shape
    /// </summary>
    public Geometry Geometry { get; set; } = null!;

    /// <summary>
    /// The material defining appearance
    /// </summary>
    public Material Material { get; set; } = null!;

    /// <summary>
    /// Whether this mesh casts shadows
    /// </summary>
    public bool CastShadow { get; set; } = false;

    /// <summary>
    /// Whether this mesh receives shadows
    /// </summary>
    public bool ReceiveShadow { get; set; } = false;

    /// <summary>
    /// Whether to perform frustum culling on this mesh
    /// </summary>
    public bool FrustumCulled { get; set; } = true;

    /// <summary>
    /// Rendering order (lower values render first)
    /// </summary>
    public int RenderOrder { get; set; } = 0;

    public Mesh()
    {
        Name = "Mesh";
        Type = "Mesh";
    }

    public Mesh(Geometry geometry, Material material)
    {
        Name = "Mesh";
        Type = "Mesh";
        Geometry = geometry;
        Material = material;
    }
}
