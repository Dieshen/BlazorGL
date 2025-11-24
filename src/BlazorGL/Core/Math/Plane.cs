using System.Numerics;

namespace BlazorGL.Core.Math;

/// <summary>
/// Represents a plane in 3D space defined by a normal and a distance from origin
/// Used primarily for frustum culling
/// </summary>
public struct Plane
{
    /// <summary>
    /// Normal vector of the plane (should be normalized)
    /// </summary>
    public Vector3 Normal { get; set; }

    /// <summary>
    /// Distance from origin (constant in plane equation: ax + by + cz + d = 0)
    /// </summary>
    public float Constant { get; set; }

    public Plane(Vector3 normal, float constant)
    {
        Normal = Vector3.Normalize(normal);
        Constant = constant;
    }

    /// <summary>
    /// Creates a plane from a normal and a point on the plane
    /// </summary>
    public static Plane FromNormalAndCoplanarPoint(Vector3 normal, Vector3 point)
    {
        var normalizedNormal = Vector3.Normalize(normal);
        var constant = -Vector3.Dot(normalizedNormal, point);
        return new Plane(normalizedNormal, constant);
    }

    /// <summary>
    /// Sets the plane from a normal and a point on the plane
    /// </summary>
    public void SetFromNormalAndCoplanarPoint(Vector3 normal, Vector3 point)
    {
        Normal = Vector3.Normalize(normal);
        Constant = -Vector3.Dot(Normal, point);
    }

    /// <summary>
    /// Calculates the signed distance from a point to the plane
    /// Positive = in front, Negative = behind, Zero = on plane
    /// </summary>
    public float DistanceToPoint(Vector3 point)
    {
        return Vector3.Dot(Normal, point) + Constant;
    }

    /// <summary>
    /// Normalizes the plane equation
    /// </summary>
    public void Normalize()
    {
        var length = Normal.Length();
        if (length > float.Epsilon)
        {
            var invLength = 1.0f / length;
            Normal *= invLength;
            Constant *= invLength;
        }
    }

    /// <summary>
    /// Returns a normalized copy of this plane
    /// </summary>
    public Plane Normalized()
    {
        var length = Normal.Length();
        if (length > float.Epsilon)
        {
            var invLength = 1.0f / length;
            return new Plane(Normal * invLength, Constant * invLength);
        }
        return this;
    }

    /// <summary>
    /// Tests if a sphere intersects with the plane
    /// </summary>
    public bool IntersectsSphere(BoundingSphere sphere)
    {
        return MathF.Abs(DistanceToPoint(sphere.Center)) <= sphere.Radius;
    }

    /// <summary>
    /// Tests if a point is in front of the plane (positive side)
    /// </summary>
    public bool IsPointInFront(Vector3 point)
    {
        return DistanceToPoint(point) > 0;
    }

    public override string ToString() => $"Plane(Normal:{Normal}, Constant:{Constant:F2})";
}
