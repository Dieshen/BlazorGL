using System.Numerics;

namespace BlazorGL.Core.Math;

/// <summary>
/// Represents a ray with an origin and direction
/// </summary>
public struct Ray
{
    /// <summary>
    /// Origin point of the ray
    /// </summary>
    public Vector3 Origin { get; set; }

    /// <summary>
    /// Direction of the ray (should be normalized)
    /// </summary>
    public Vector3 Direction { get; set; }

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }

    /// <summary>
    /// Gets a point at distance t along the ray
    /// </summary>
    public Vector3 GetPoint(float t)
    {
        return Origin + Direction * t;
    }

    /// <summary>
    /// Tests intersection with a bounding box
    /// </summary>
    public bool IntersectsBoundingBox(BoundingBox box, out float distance)
    {
        distance = 0;

        float tmin = float.MinValue;
        float tmax = float.MaxValue;

        // X axis
        if (MathF.Abs(Direction.X) < 1e-6f)
        {
            if (Origin.X < box.Min.X || Origin.X > box.Max.X)
                return false;
        }
        else
        {
            float t1 = (box.Min.X - Origin.X) / Direction.X;
            float t2 = (box.Max.X - Origin.X) / Direction.X;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tmin = MathF.Max(tmin, t1);
            tmax = MathF.Min(tmax, t2);
            if (tmin > tmax) return false;
        }

        // Y axis
        if (MathF.Abs(Direction.Y) < 1e-6f)
        {
            if (Origin.Y < box.Min.Y || Origin.Y > box.Max.Y)
                return false;
        }
        else
        {
            float t1 = (box.Min.Y - Origin.Y) / Direction.Y;
            float t2 = (box.Max.Y - Origin.Y) / Direction.Y;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tmin = MathF.Max(tmin, t1);
            tmax = MathF.Min(tmax, t2);
            if (tmin > tmax) return false;
        }

        // Z axis
        if (MathF.Abs(Direction.Z) < 1e-6f)
        {
            if (Origin.Z < box.Min.Z || Origin.Z > box.Max.Z)
                return false;
        }
        else
        {
            float t1 = (box.Min.Z - Origin.Z) / Direction.Z;
            float t2 = (box.Max.Z - Origin.Z) / Direction.Z;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tmin = MathF.Max(tmin, t1);
            tmax = MathF.Min(tmax, t2);
            if (tmin > tmax) return false;
        }

        distance = tmin >= 0 ? tmin : tmax;
        return distance >= 0;
    }

    /// <summary>
    /// Tests intersection with a bounding sphere
    /// </summary>
    public bool IntersectsBoundingSphere(BoundingSphere sphere, out float distance)
    {
        distance = 0;

        Vector3 m = Origin - sphere.Center;
        float b = Vector3.Dot(m, Direction);
        float c = Vector3.Dot(m, m) - sphere.Radius * sphere.Radius;

        if (c > 0 && b > 0)
            return false;

        float discriminant = b * b - c;
        if (discriminant < 0)
            return false;

        distance = -b - MathF.Sqrt(discriminant);
        if (distance < 0)
            distance = 0;

        return true;
    }

    /// <summary>
    /// Tests intersection with a triangle
    /// </summary>
    public bool IntersectsTriangle(Vector3 v0, Vector3 v1, Vector3 v2, out float distance, out Vector3 barycentric)
    {
        distance = 0;
        barycentric = Vector3.Zero;

        // Möller-Trumbore algorithm
        Vector3 edge1 = v1 - v0;
        Vector3 edge2 = v2 - v0;
        Vector3 h = Vector3.Cross(Direction, edge2);
        float a = Vector3.Dot(edge1, h);

        if (MathF.Abs(a) < 1e-6f)
            return false; // Ray is parallel to triangle

        float f = 1.0f / a;
        Vector3 s = Origin - v0;
        float u = f * Vector3.Dot(s, h);

        if (u < 0.0f || u > 1.0f)
            return false;

        Vector3 q = Vector3.Cross(s, edge1);
        float v = f * Vector3.Dot(Direction, q);

        if (v < 0.0f || u + v > 1.0f)
            return false;

        float t = f * Vector3.Dot(edge2, q);

        if (t > 1e-6f)
        {
            distance = t;
            barycentric = new Vector3(1 - u - v, u, v);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Transforms the ray by a matrix
    /// </summary>
    public Ray Transform(Matrix4x4 matrix)
    {
        var newOrigin = Vector3.Transform(Origin, matrix);
        var newDirection = Vector3.TransformNormal(Direction, matrix);
        return new Ray(newOrigin, newDirection);
    }

    public override string ToString() => $"Ray(Origin:{Origin}, Direction:{Direction})";
}
