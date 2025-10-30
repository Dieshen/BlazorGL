using System.Numerics;

namespace BlazorGL.Core.Math;

/// <summary>
/// Bounding sphere for collision detection and culling
/// </summary>
public struct BoundingSphere
{
    /// <summary>
    /// Center of the sphere
    /// </summary>
    public Vector3 Center { get; set; }

    /// <summary>
    /// Radius of the sphere
    /// </summary>
    public float Radius { get; set; }

    public BoundingSphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// Creates a bounding sphere from a set of points
    /// </summary>
    public static BoundingSphere FromPoints(IEnumerable<Vector3> points)
    {
        var pointList = points.ToList();
        if (pointList.Count == 0)
            return new BoundingSphere(Vector3.Zero, 0);

        // Simple approach: use centroid and maximum distance
        var center = Vector3.Zero;
        foreach (var point in pointList)
            center += point;
        center /= pointList.Count;

        float maxDistanceSq = 0;
        foreach (var point in pointList)
        {
            var distSq = Vector3.DistanceSquared(center, point);
            if (distSq > maxDistanceSq)
                maxDistanceSq = distSq;
        }

        return new BoundingSphere(center, MathF.Sqrt(maxDistanceSq));
    }

    /// <summary>
    /// Creates a bounding sphere from a bounding box
    /// </summary>
    public static BoundingSphere FromBoundingBox(BoundingBox box)
    {
        var center = box.Center;
        var radius = Vector3.Distance(center, box.Max);
        return new BoundingSphere(center, radius);
    }

    /// <summary>
    /// Checks if a point is inside the sphere
    /// </summary>
    public bool Contains(Vector3 point)
    {
        return Vector3.DistanceSquared(Center, point) <= Radius * Radius;
    }

    /// <summary>
    /// Checks if this sphere intersects with another
    /// </summary>
    public bool Intersects(BoundingSphere other)
    {
        var distance = Vector3.Distance(Center, other.Center);
        return distance <= (Radius + other.Radius);
    }

    /// <summary>
    /// Transforms the bounding sphere by a matrix
    /// </summary>
    public BoundingSphere Transform(Matrix4x4 matrix)
    {
        var newCenter = Vector3.Transform(Center, matrix);

        // Extract scale from matrix (approximate)
        var scaleX = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
        var scaleY = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
        var scaleZ = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();
        var maxScale = MathF.Max(scaleX, MathF.Max(scaleY, scaleZ));

        return new BoundingSphere(newCenter, Radius * maxScale);
    }

    public override string ToString() => $"BoundingSphere(Center:{Center}, Radius:{Radius:F2})";
}
