using System.Numerics;

namespace BlazorGL.Core.Math;

/// <summary>
/// Axis-aligned bounding box
/// </summary>
public struct BoundingBox
{
    /// <summary>
    /// Minimum point of the box
    /// </summary>
    public Vector3 Min { get; set; }

    /// <summary>
    /// Maximum point of the box
    /// </summary>
    public Vector3 Max { get; set; }

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Center of the bounding box
    /// </summary>
    public Vector3 Center => (Min + Max) * 0.5f;

    /// <summary>
    /// Size of the bounding box
    /// </summary>
    public Vector3 Size => Max - Min;

    /// <summary>
    /// Creates a bounding box that encompasses all given points
    /// </summary>
    public static BoundingBox FromPoints(IEnumerable<Vector3> points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var point in points)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Expands the bounding box to include a point
    /// </summary>
    public void ExpandToInclude(Vector3 point)
    {
        Min = Vector3.Min(Min, point);
        Max = Vector3.Max(Max, point);
    }

    /// <summary>
    /// Backwards-compatible alias for ExpandToInclude.
    /// </summary>
    public void ExpandByPoint(Vector3 point) => ExpandToInclude(point);

    /// <summary>
    /// Expands the bounding box to include another bounding box
    /// </summary>
    public void ExpandToInclude(BoundingBox other)
    {
        Min = Vector3.Min(Min, other.Min);
        Max = Vector3.Max(Max, other.Max);
    }

    /// <summary>
    /// Checks if a point is inside the bounding box
    /// </summary>
    public bool Contains(Vector3 point)
    {
        return point.X >= Min.X && point.X <= Max.X &&
               point.Y >= Min.Y && point.Y <= Max.Y &&
               point.Z >= Min.Z && point.Z <= Max.Z;
    }

    /// <summary>
    /// Backwards-compatible alias for Contains.
    /// </summary>
    public bool ContainsPoint(Vector3 point) => Contains(point);

    /// <summary>
    /// Checks if this bounding box intersects with another
    /// </summary>
    public bool Intersects(BoundingBox other)
    {
        return Min.X <= other.Max.X && Max.X >= other.Min.X &&
               Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
               Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
    }

    /// <summary>
    /// Transforms the bounding box by a matrix
    /// </summary>
    public BoundingBox Transform(Matrix4x4 matrix)
    {
        // Transform all 8 corners and create new bounding box
        var corners = new[]
        {
            new Vector3(Min.X, Min.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Min.X, Max.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Max.Z),
        };

        var transformedCorners = corners.Select(c => Vector3.Transform(c, matrix));
        return FromPoints(transformedCorners);
    }

    public override string ToString() => $"BoundingBox(Min:{Min}, Max:{Max})";
}
