using System.Numerics;

namespace BlazorGL.Core.Math;

/// <summary>
/// View frustum for frustum culling optimization
/// Contains 6 planes: left, right, top, bottom, near, far
/// </summary>
public class Frustum
{
    /// <summary>
    /// The 6 frustum planes in order: left, right, top, bottom, near, far
    /// </summary>
    public Plane[] Planes { get; } = new Plane[6];

    public Frustum()
    {
        for (int i = 0; i < 6; i++)
        {
            Planes[i] = new Plane(Vector3.UnitZ, 0);
        }
    }

    public Frustum(Plane[] planes)
    {
        if (planes.Length != 6)
            throw new ArgumentException("Frustum must have exactly 6 planes", nameof(planes));

        for (int i = 0; i < 6; i++)
        {
            Planes[i] = planes[i];
        }
    }

    /// <summary>
    /// Extracts frustum planes from a view-projection matrix using the Gribb-Hartmann method
    /// This is the standard, fast method used in modern graphics engines
    /// </summary>
    public Frustum SetFromProjectionMatrix(Matrix4x4 viewProjectionMatrix)
    {
        var m = viewProjectionMatrix;

        // Left plane: m14 + m11, m24 + m21, m34 + m31, m44 + m41
        Planes[0] = new Plane(
            new Vector3(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31),
            m.M44 + m.M41
        ).Normalized();

        // Right plane: m14 - m11, m24 - m21, m34 - m31, m44 - m41
        Planes[1] = new Plane(
            new Vector3(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31),
            m.M44 - m.M41
        ).Normalized();

        // Top plane: m14 - m12, m24 - m22, m34 - m32, m44 - m42
        Planes[2] = new Plane(
            new Vector3(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32),
            m.M44 - m.M42
        ).Normalized();

        // Bottom plane: m14 + m12, m24 + m22, m34 + m32, m44 + m42
        Planes[3] = new Plane(
            new Vector3(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32),
            m.M44 + m.M42
        ).Normalized();

        // Near plane: m14 + m13, m24 + m23, m34 + m33, m44 + m43
        Planes[4] = new Plane(
            new Vector3(m.M14 + m.M13, m.M24 + m.M23, m.M34 + m.M33),
            m.M44 + m.M43
        ).Normalized();

        // Far plane: m14 - m13, m24 - m23, m34 - m33, m44 - m43
        Planes[5] = new Plane(
            new Vector3(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33),
            m.M44 - m.M43
        ).Normalized();

        return this;
    }

    /// <summary>
    /// Tests if a bounding sphere intersects the frustum
    /// Returns true if the sphere is at least partially inside the frustum
    /// </summary>
    public bool IntersectsSphere(BoundingSphere sphere)
    {
        // Test against all 6 planes
        for (int i = 0; i < 6; i++)
        {
            var distance = Planes[i].DistanceToPoint(sphere.Center);

            // If the sphere is completely outside any plane, it's not visible
            if (distance < -sphere.Radius)
            {
                return false;
            }
        }

        // Sphere is at least partially inside frustum
        return true;
    }

    /// <summary>
    /// Tests if a point is inside the frustum
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        for (int i = 0; i < 6; i++)
        {
            if (Planes[i].DistanceToPoint(point) < 0)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Tests if a bounding box intersects the frustum
    /// </summary>
    public bool IntersectsBox(BoundingBox box)
    {
        // For each plane, find the p-vertex (most positive vertex)
        for (int i = 0; i < 6; i++)
        {
            var plane = Planes[i];

            // Compute p-vertex (farthest point in the direction of the normal)
            var pVertex = new Vector3(
                plane.Normal.X >= 0 ? box.Max.X : box.Min.X,
                plane.Normal.Y >= 0 ? box.Max.Y : box.Min.Y,
                plane.Normal.Z >= 0 ? box.Max.Z : box.Min.Z
            );

            // If p-vertex is outside this plane, box is outside frustum
            if (plane.DistanceToPoint(pVertex) < 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the left frustum plane
    /// </summary>
    public Plane Left => Planes[0];

    /// <summary>
    /// Gets the right frustum plane
    /// </summary>
    public Plane Right => Planes[1];

    /// <summary>
    /// Gets the top frustum plane
    /// </summary>
    public Plane Top => Planes[2];

    /// <summary>
    /// Gets the bottom frustum plane
    /// </summary>
    public Plane Bottom => Planes[3];

    /// <summary>
    /// Gets the near frustum plane
    /// </summary>
    public Plane Near => Planes[4];

    /// <summary>
    /// Gets the far frustum plane
    /// </summary>
    public Plane Far => Planes[5];

    public override string ToString() => $"Frustum(6 planes)";
}
