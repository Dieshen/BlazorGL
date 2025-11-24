using BlazorGL.Core.Cameras;
using System.Numerics;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Shadow for directional lights using orthographic projection
/// </summary>
public class DirectionalLightShadow : LightShadow
{
    /// <summary>
    /// Size of the orthographic shadow camera frustum
    /// </summary>
    public float CameraSize { get; set; } = 50f;

    private DirectionalLight? _light;

    public DirectionalLightShadow()
    {
        Camera = new OrthographicCamera(-CameraSize, CameraSize, CameraSize, -CameraSize, Near, Far);
    }

    public void SetLight(DirectionalLight light)
    {
        _light = light;
    }

    public override void UpdateShadowCamera()
    {
        if (_light == null || Camera is not OrthographicCamera orthoCamera)
            return;

        // Update orthographic camera bounds
        orthoCamera.Left = -CameraSize;
        orthoCamera.Right = CameraSize;
        orthoCamera.Top = CameraSize;
        orthoCamera.Bottom = -CameraSize;
        orthoCamera.Near = Near;
        orthoCamera.Far = Far;
        orthoCamera.UpdateProjectionMatrix();

        // Position camera based on light direction
        // Shadow camera looks in the direction of the light
        var lightDir = _light.Direction;
        var target = Vector3.Zero; // Center of scene
        var position = target - lightDir * (Far / 2);

        Camera.Position = position;
        Camera.LookAt(target);
        Camera.UpdateMatrixWorld();
    }
}
