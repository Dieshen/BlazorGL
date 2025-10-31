using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core;

/// <summary>
/// A continuous line that connects back to the first vertex (closed loop)
/// </summary>
public class LineLoop : Object3D
{
    /// <summary>
    /// The geometry defining the line vertices
    /// </summary>
    public Geometry Geometry { get; set; } = null!;

    /// <summary>
    /// The material defining line appearance (should be LineBasicMaterial or LineDashedMaterial)
    /// </summary>
    public Material Material { get; set; } = null!;

    /// <summary>
    /// Rendering order (lower values render first)
    /// </summary>
    public int RenderOrder { get; set; } = 0;

    /// <summary>
    /// Whether to perform frustum culling on this line
    /// </summary>
    public bool FrustumCulled { get; set; } = true;

    public LineLoop()
    {
        Name = "LineLoop";
    }

    public LineLoop(Geometry geometry, Material material)
    {
        Name = "LineLoop";
        Geometry = geometry;
        Material = material;
    }
}
