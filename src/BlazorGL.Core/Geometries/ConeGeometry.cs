namespace BlazorGL.Core.Geometries;

/// <summary>
/// Cone geometry - a cylinder with radiusTop of 0
/// </summary>
public class ConeGeometry : CylinderGeometry
{
    /// <summary>
    /// Creates a cone geometry
    /// </summary>
    /// <param name="radius">Radius of the base</param>
    /// <param name="height">Height of the cone</param>
    /// <param name="radialSegments">Number of segments around the circumference</param>
    /// <param name="heightSegments">Number of segments along the height</param>
    /// <param name="openEnded">Whether the bottom is open or capped</param>
    public ConeGeometry(float radius = 1, float height = 1, int radialSegments = 32,
                       int heightSegments = 1, bool openEnded = false)
        : base(0, radius, height, radialSegments, heightSegments, openEnded)
    {
    }
}
