using BlazorGL.Core.Lights;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core;

/// <summary>
/// Container for all objects, lights, and cameras in a 3D scene
/// </summary>
public class Scene : Object3D
{
    /// <summary>
    /// Background color of the scene
    /// </summary>
    public Math.Color Background { get; set; } = new(0, 0, 0);

    /// <summary>
    /// Environment map for reflections and lighting
    /// </summary>
    public Texture? Environment { get; set; }

    /// <summary>
    /// Fog settings for the scene
    /// </summary>
    public Fog? Fog { get; set; }

    /// <summary>
    /// Active camera for rendering (can be set explicitly or will use first camera found)
    /// </summary>
    public Camera? ActiveCamera { get; set; }

    /// <summary>
    /// Gets all lights in the scene (collected from scene graph)
    /// </summary>
    public List<Light> Lights
    {
        get => GetObjectsOfType<Light>();
    }

    /// <summary>
    /// Gets all cameras in the scene (collected from scene graph)
    /// </summary>
    public List<Camera> Cameras
    {
        get => GetObjectsOfType<Camera>();
    }

    public Scene()
    {
        Name = "Scene";
    }

    /// <summary>
    /// Gets the camera to use for rendering
    /// </summary>
    public Camera? GetRenderCamera()
    {
        if (ActiveCamera != null)
            return ActiveCamera;

        var cameras = Cameras;
        return cameras.Count > 0 ? cameras[0] : null;
    }
}

/// <summary>
/// Fog effect for the scene
/// </summary>
public class Fog
{
    /// <summary>
    /// Fog color
    /// </summary>
    public Math.Color Color { get; set; } = Math.Color.White;

    /// <summary>
    /// Distance where fog starts
    /// </summary>
    public float Near { get; set; } = 1.0f;

    /// <summary>
    /// Distance where fog is at maximum
    /// </summary>
    public float Far { get; set; } = 1000.0f;

    /// <summary>
    /// Fog density (for exponential fog)
    /// </summary>
    public float Density { get; set; } = 0.00025f;

    /// <summary>
    /// Fog type: Linear or Exponential
    /// </summary>
    public FogType Type { get; set; } = FogType.Linear;
}

/// <summary>
/// Type of fog calculation
/// </summary>
public enum FogType
{
    Linear,
    Exponential,
    ExponentialSquared
}
