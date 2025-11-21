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
        var pointList = points as IList<Vector3> ?? points.ToList();
        if (pointList.Count == 0)
            return new BoundingSphere(Vector3.Zero, 0);

        // Ritter approximation: start with the widest axis pair
        var minX = pointList[0];
        var maxX = pointList[0];
        var minY = pointList[0];
        var maxY = pointList[0];
        var minZ = pointList[0];
        var maxZ = pointList[0];

        foreach (var point in pointList)
        {
            if (point.X < minX.X) minX = point;
            if (point.X > maxX.X) maxX = point;
            if (point.Y < minY.Y) minY = point;
            if (point.Y > maxY.Y) maxY = point;
            if (point.Z < minZ.Z) minZ = point;
            if (point.Z > maxZ.Z) maxZ = point;
        }

        var distX = Vector3.DistanceSquared(minX, maxX);
        var distY = Vector3.DistanceSquared(minY, maxY);
        var distZ = Vector3.DistanceSquared(minZ, maxZ);

        var (pointA, pointB) = distX > distY
            ? (distX > distZ ? (minX, maxX) : (minZ, maxZ))
            : (distY > distZ ? (minY, maxY) : (minZ, maxZ));

        var center = (pointA + pointB) * 0.5f;
        var radius = MathF.Sqrt(Vector3.DistanceSquared(pointA, pointB)) * 0.5f;

        foreach (var point in pointList)
        {
            var offset = point - center;
            var distance = offset.Length();
            if (distance <= radius || distance <= float.Epsilon)
                continue;

            var newRadius = (radius + distance) * 0.5f;
            var direction = offset / distance;
            center += direction * (distance - radius) * 0.5f;
            radius = newRadius;
        }

        return new BoundingSphere(center, radius);
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

    public bool ContainsPoint(Vector3 point) => Contains(point);

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

    /// <summary>
    /// Expands the sphere radius to include a point.
    /// </summary>
    public void ExpandByPoint(Vector3 point)
    {
        var distance = Vector3.Distance(Center, point);
        if (distance > Radius)
        {
            Radius = distance;
        }
    }
}
