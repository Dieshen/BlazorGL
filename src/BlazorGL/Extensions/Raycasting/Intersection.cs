using System.Numerics;
using BlazorGL.Core;

namespace BlazorGL.Extensions.Raycasting;

/// <summary>
/// Raycast intersection result
/// </summary>
public class Intersection
{
    public Object3D Object { get; set; } = null!;
    public float Distance { get; set; }
    public Vector3 Point { get; set; }
    public Vector3 Normal { get; set; }
    public Vector2 UV { get; set; }
}
