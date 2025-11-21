using System.Numerics;

namespace BlazorGL.Core.Cameras;

/// <summary>
/// Orthographic camera with no perspective distortion
/// </summary>
public class OrthographicCamera : Camera
{
    private float _left;
    private float _right;
    private float _top;
    private float _bottom;
    private float _near;
    private float _far;

    /// <summary>
    /// Left edge of the view frustum
    /// </summary>
    public float Left
    {
        get => _left;
        set
        {
            _left = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Right edge of the view frustum
    /// </summary>
    public float Right
    {
        get => _right;
        set
        {
            _right = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Top edge of the view frustum
    /// </summary>
    public float Top
    {
        get => _top;
        set
        {
            _top = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Bottom edge of the view frustum
    /// </summary>
    public float Bottom
    {
        get => _bottom;
        set
        {
            _bottom = value;
            InvalidateProjectionMatrix();
        }
    }

    /// <summary>
    /// Near clipping plane
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
    /// Far clipping plane
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
    /// Creates a new orthographic camera
    /// </summary>
    public OrthographicCamera(float left, float right, float top, float bottom)
        : this(left, right, top, bottom, -1f, 1f)
    {
    }

    public OrthographicCamera(float left, float right, float top, float bottom, float near, float far)
    {
        _left = left;
        _right = right;
        _top = top;
        _bottom = bottom;
        _near = near;
        _far = far;
        Name = "OrthographicCamera";
        UpdateProjectionMatrix();
    }

    public override void UpdateProjectionMatrix()
    {
        _projectionMatrix = Matrix4x4.CreateOrthographic(
            _right - _left,
            _top - _bottom,
            _near,
            _far
        );
    }
}
