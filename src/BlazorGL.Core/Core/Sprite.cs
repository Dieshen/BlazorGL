using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Core;

/// <summary>
/// A 2D sprite that always faces the camera (billboard)
/// </summary>
public class Sprite : Object3D
{
    /// <summary>
    /// The material defining sprite appearance (should be SpriteMaterial)
    /// </summary>
    public Material Material { get; set; } = null!;

    /// <summary>
    /// Sprite center point (0,0 = center, -0.5,-0.5 = bottom-left, 0.5,0.5 = top-right)
    /// </summary>
    public Vector2 Center { get; set; } = new Vector2(0.5f, 0.5f);

    /// <summary>
    /// Rendering order (lower values render first)
    /// </summary>
    public int RenderOrder { get; set; } = 0;

    /// <summary>
    /// Internal geometry for sprite quad
    /// </summary>
    internal Geometry Geometry { get; private set; }

    public Sprite()
    {
        Name = "Sprite";
        CreateGeometry();
    }

    public Sprite(Material material)
    {
        Name = "Sprite";
        Material = material;
        CreateGeometry();
    }

    private void CreateGeometry()
    {
        // Create a simple quad geometry for the sprite
        Geometry = new Geometry
        {
            Vertices = new float[]
            {
                -0.5f, -0.5f, 0,
                 0.5f, -0.5f, 0,
                 0.5f,  0.5f, 0,
                -0.5f,  0.5f, 0
            },
            UVs = new float[]
            {
                0, 0,
                1, 0,
                1, 1,
                0, 1
            },
            Indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3
            }
        };
    }
}
