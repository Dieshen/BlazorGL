using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core;

/// <summary>
/// A series of disconnected line segments (pairs of vertices)
/// </summary>
public class LineSegments : Object3D
{
    /// <summary>
    /// The geometry defining the line vertices (every 2 vertices form a segment)
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

    public LineSegments()
    {
        Name = "LineSegments";
    }

    public LineSegments(Geometry geometry, Material material)
    {
        Name = "LineSegments";
        Geometry = geometry;
        Material = material;
    }
}
