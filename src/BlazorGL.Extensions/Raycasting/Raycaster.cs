using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Math;

namespace BlazorGL.Extensions.Raycasting;

/// <summary>
/// Raycasting for object picking and intersection tests
/// </summary>
public class Raycaster
{
    public Ray Ray { get; set; }

    public Raycaster()
    {
        Ray = new Ray(Vector3.Zero, Vector3.UnitZ);
    }

    public void SetFromCamera(Vector2 ndc, Camera camera)
    {
        // Convert NDC to world space ray
        var nearPoint = new Vector3(ndc.X, ndc.Y, -1);
        var farPoint = new Vector3(ndc.X, ndc.Y, 1);

        Matrix4x4.Invert(camera.ViewProjectionMatrix, out var invViewProj);

        var nearWorld = Vector3.Transform(nearPoint, invViewProj);
        var farWorld = Vector3.Transform(farPoint, invViewProj);

        var origin = nearWorld;
        var direction = Vector3.Normalize(farWorld - nearWorld);

        Ray = new Ray(origin, direction);
    }

    public List<Intersection> IntersectObjects(List<Object3D> objects, bool recursive = false)
    {
        var intersections = new List<Intersection>();

        foreach (var obj in objects)
        {
            if (obj is Mesh mesh)
            {
                var intersection = IntersectMesh(mesh);
                if (intersection != null)
                    intersections.Add(intersection);
            }

            if (recursive)
            {
                intersections.AddRange(IntersectObjects(obj.Children, true));
            }
        }

        return intersections.OrderBy(i => i.Distance).ToList();
    }

    private Intersection? IntersectMesh(Mesh mesh)
    {
        // Transform ray to local space
        Matrix4x4.Invert(mesh.WorldMatrix, out var invWorld);
        var localRay = Ray.Transform(invWorld);

        // Test bounding sphere first
        var boundingSphere = mesh.Geometry.BoundingSphere.Transform(mesh.WorldMatrix);
        if (!localRay.IntersectsBoundingSphere(mesh.Geometry.BoundingSphere, out float distance))
            return null;

        return new Intersection
        {
            Object = mesh,
            Distance = distance,
            Point = Ray.GetPoint(distance)
        };
    }
}
