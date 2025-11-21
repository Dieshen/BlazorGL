using System.Numerics;

namespace BlazorGL.Core.Cameras;

/// <summary>
/// Perspective camera with field of view
/// </summary>
public class PerspectiveCamera : Camera
{
    private float _fov = 50f;
    private float _aspect = 1.0f;
    private float _near = 0.1f;
    private float _far = 2000f;

    /// <summary>
    /// Field of view in degrees
    /// </summary>
    public float Fov
    {
        get => _fov;
        set
        {
            _fov = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Aspect ratio (width / height)
    /// </summary>
    public float Aspect
    {
        get => _aspect;
        set
        {
            _aspect = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Near clipping plane distance
    /// </summary>
    public float Near
    {
        get => _near;
        set
        {
            _near = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Far clipping plane distance
    /// </summary>
    public float Far
    {
        get => _far;
        set
        {
            _far = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Creates a new perspective camera with default configuration.
    /// </summary>
    public PerspectiveCamera() : this(50f, 1f, 0.1f, 2000f)
    {
    }

    /// <summary>
    /// Creates a new perspective camera
    /// </summary>
    /// <param name="fov">Field of view in degrees</param>
    /// <param name="aspect">Aspect ratio (width / height)</param>
    /// <param name="near">Near clipping plane</param>
    /// <param name="far">Far clipping plane</param>
    public PerspectiveCamera(float fov, float aspect, float near, float far)
    {
        _fov = fov;
        _aspect = aspect;
        _near = near;
        _far = far;
        Name = "PerspectiveCamera";
        UpdateProjectionMatrix();
    }

    public override void UpdateProjectionMatrix()
    {
        float fovRadians = _fov * MathF.PI / 180f;
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, _aspect, _near, _far);
    }

    /// <summary>
    /// Updates aspect ratio based on viewport size
    /// </summary>
    public void UpdateAspectRatio(int width, int height)
    {
        Aspect = (float)width / height;
    }
}
