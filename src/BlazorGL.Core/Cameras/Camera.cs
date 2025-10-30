using System.Numerics;

namespace BlazorGL.Core.Cameras;

/// <summary>
/// Base class for all cameras
/// </summary>
public abstract class Camera : Object3D
{
    private bool _projectionMatrixNeedsUpdate = true;
    protected Matrix4x4 _projectionMatrix = Matrix4x4.Identity;

    /// <summary>
    /// Projection matrix for this camera
    /// </summary>
    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (_projectionMatrixNeedsUpdate)
            {
                UpdateProjectionMatrix();
                _projectionMatrixNeedsUpdate = false;
            }
            return _projectionMatrix;
        }
    }

    /// <summary>
    /// View matrix (inverse of world matrix)
    /// </summary>
    public Matrix4x4 ViewMatrix
    {
        get
        {
            Matrix4x4.Invert(WorldMatrix, out var viewMatrix);
            return viewMatrix;
        }
    }

    /// <summary>
    /// Combined view-projection matrix
    /// </summary>
    public Matrix4x4 ViewProjectionMatrix => ViewMatrix * ProjectionMatrix;

    /// <summary>
    /// Updates the projection matrix (must be implemented by derived classes)
    /// </summary>
    public abstract void UpdateProjectionMatrix();

    /// <summary>
    /// Marks the projection matrix as needing update
    /// </summary>
    protected void InvalidateProjectionMatrix()
    {
        _projectionMatrixNeedsUpdate = true;
    }
}
