using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core;

/// <summary>
/// A set of points rendered as a point cloud
/// </summary>
public class Points : Object3D
{
    /// <summary>
    /// The geometry defining the point positions
    /// </summary>
    public Geometry Geometry { get; set; } = null!;

    /// <summary>
    /// The material defining point appearance (should be PointsMaterial)
    /// </summary>
    public Material Material { get; set; } = null!;

    /// <summary>
    /// Rendering order (lower values render first)
    /// </summary>
    public int RenderOrder { get; set; } = 0;

    /// <summary>
    /// Whether to perform frustum culling on these points
    /// </summary>
    public bool FrustumCulled { get; set; } = true;

    public Points()
    {
        Name = "Points";
    }

    public Points(Geometry geometry, Material material)
    {
        Name = "Points";
        Geometry = geometry;
        Material = material;
    }
}
