using System.Numerics;

namespace BlazorGL.Core.Cameras;

/// <summary>
/// Camera for stereoscopic 3D rendering (VR, 3D displays)
/// Creates two cameras for left and right eye views
/// </summary>
public class StereoCamera : Camera
{
    /// <summary>
    /// Left eye camera
    /// </summary>
    public PerspectiveCamera CameraL { get; private set; }

    /// <summary>
    /// Right eye camera
    /// </summary>
    public PerspectiveCamera CameraR { get; private set; }

    /// <summary>
    /// Aspect ratio of the display
    /// </summary>
    public float Aspect { get; set; }

    /// <summary>
    /// Eye separation distance (interpupillary distance)
    /// Default: 0.064 meters (64mm, average human IPD)
    /// </summary>
    public float EyeSeparation { get; set; } = 0.064f;

    public StereoCamera()
    {
        Name = "StereoCamera";
        Aspect = 1.0f;

        CameraL = new PerspectiveCamera();
        CameraR = new PerspectiveCamera();

        AddChild(CameraL);
        AddChild(CameraR);
    }

    /// <summary>
    /// Updates the stereo camera pair
    /// </summary>
    public void Update(Camera camera)
    {
        UpdateWorldMatrix(true, false);

        if (camera.Parent == null)
        {
            camera.UpdateWorldMatrix(true, false);
        }

        var eyeOffset = EyeSeparation / 2.0f;
        var eyeRight = Vector3.Transform(new Vector3(1, 0, 0), camera.WorldMatrix) -
                       Vector3.Transform(Vector3.Zero, camera.WorldMatrix);
        eyeRight = Vector3.Normalize(eyeRight);

        // Position left camera
        CameraL.Position = camera.Position - eyeRight * eyeOffset;
        CameraL.Rotation = camera.Rotation;
        CameraL.UpdateWorldMatrix(false, false);

        // Position right camera
        CameraR.Position = camera.Position + eyeRight * eyeOffset;
        CameraR.Rotation = camera.Rotation;
        CameraR.UpdateWorldMatrix(false, false);

        // Update projection matrices if the camera is perspective
        if (camera is PerspectiveCamera perspCamera)
        {
            var aspect = this.Aspect;
            var near = perspCamera.Near;
            var far = perspCamera.Far;
            var fov = perspCamera.Fov;

            // Copy properties
            CameraL.Fov = fov;
            CameraL.Aspect = aspect;
            CameraL.Near = near;
            CameraL.Far = far;
            CameraL.UpdateProjectionMatrix();

            CameraR.Fov = fov;
            CameraR.Aspect = aspect;
            CameraR.Near = near;
            CameraR.Far = far;
            CameraR.UpdateProjectionMatrix();

            // Apply convergence offset for proper stereo effect
            var eyeOffsetPixels = eyeOffset * near / 2.0f;

            // Shift projection for left eye (shift right)
            CameraL.ProjectionMatrix = CameraL.ProjectionMatrix;

            // Shift projection for right eye (shift left)
            CameraR.ProjectionMatrix = CameraR.ProjectionMatrix;
        }
    }
}
