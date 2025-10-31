namespace BlazorGL.Core.Cameras;

/// <summary>
/// Camera that holds an array of cameras for multi-view rendering
/// Used for multi-screen setups, portals, picture-in-picture, etc.
/// </summary>
public class ArrayCamera : PerspectiveCamera
{
    /// <summary>
    /// Array of sub-cameras, each with its own viewport
    /// </summary>
    public Camera[] Cameras { get; set; }

    /// <summary>
    /// Whether this is an array camera (used by renderer)
    /// </summary>
    public bool IsArrayCamera { get; } = true;

    public ArrayCamera(Camera[] cameras)
    {
        Name = "ArrayCamera";
        Cameras = cameras;

        // Add all cameras as children
        foreach (var camera in cameras)
        {
            AddChild(camera);
        }
    }

    public ArrayCamera() : this(Array.Empty<Camera>())
    {
    }
}

/// <summary>
/// Defines a viewport region for a camera in an ArrayCamera
/// </summary>
public class CameraViewport
{
    /// <summary>
    /// X position of viewport (0-1, normalized)
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Y position of viewport (0-1, normalized)
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Width of viewport (0-1, normalized)
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Height of viewport (0-1, normalized)
    /// </summary>
    public float Height { get; set; }

    public CameraViewport(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
