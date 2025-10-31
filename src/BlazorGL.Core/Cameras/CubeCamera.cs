using BlazorGL.Core.Textures;
using System.Numerics;

namespace BlazorGL.Core.Cameras;

/// <summary>
/// Camera that captures a scene from 6 directions for cube map rendering
/// Used for environment mapping, reflections, and skyboxes
/// </summary>
public class CubeCamera : Object3D
{
    /// <summary>
    /// Six perspective cameras, one for each cube face
    /// </summary>
    public PerspectiveCamera[] Cameras { get; private set; }

    /// <summary>
    /// Render target for the cube map
    /// </summary>
    public RenderTarget? RenderTarget { get; set; }

    /// <summary>
    /// Near clipping plane
    /// </summary>
    public float Near { get; set; }

    /// <summary>
    /// Far clipping plane
    /// </summary>
    public float Far { get; set; }

    public CubeCamera(float near = 0.1f, float far = 1000f, int resolution = 512)
    {
        Name = "CubeCamera";
        Near = near;
        Far = far;

        Cameras = new PerspectiveCamera[6];

        // Create 6 cameras for cube faces: +X, -X, +Y, -Y, +Z, -Z
        for (int i = 0; i < 6; i++)
        {
            Cameras[i] = new PerspectiveCamera(90, 1, near, far);
        }

        // Set up camera orientations for each cube face
        UpdateCameraOrientations();

        // Create render target for cube map
        RenderTarget = new RenderTarget(resolution, resolution);
    }

    /// <summary>
    /// Updates camera orientations to look at cube faces
    /// </summary>
    private void UpdateCameraOrientations()
    {
        // +X (right)
        Cameras[0].LookAt(new Vector3(1, 0, 0));
        Cameras[0].Up = new Vector3(0, -1, 0);

        // -X (left)
        Cameras[1].LookAt(new Vector3(-1, 0, 0));
        Cameras[1].Up = new Vector3(0, -1, 0);

        // +Y (top)
        Cameras[2].LookAt(new Vector3(0, 1, 0));
        Cameras[2].Up = new Vector3(0, 0, 1);

        // -Y (bottom)
        Cameras[3].LookAt(new Vector3(0, -1, 0));
        Cameras[3].Up = new Vector3(0, 0, -1);

        // +Z (front)
        Cameras[4].LookAt(new Vector3(0, 0, 1));
        Cameras[4].Up = new Vector3(0, -1, 0);

        // -Z (back)
        Cameras[5].LookAt(new Vector3(0, 0, -1));
        Cameras[5].Up = new Vector3(0, -1, 0);
    }

    /// <summary>
    /// Updates the cube camera position and all sub-cameras
    /// </summary>
    public void Update(Rendering.Renderer renderer, Scene scene)
    {
        // Update world matrix
        UpdateWorldMatrix(true, false);

        // Update each camera's position to match cube camera position
        for (int i = 0; i < 6; i++)
        {
            Cameras[i].Position = Position;
            Cameras[i].UpdateWorldMatrix(false, false);
        }
    }
}
