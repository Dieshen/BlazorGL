using BlazorGL.Core.Cameras;
using System.Numerics;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Shadow for spot lights using perspective projection
/// </summary>
public class SpotLightShadow : LightShadow
{
    private SpotLight? _light;

    public SpotLightShadow()
    {
        Camera = new PerspectiveCamera(50, 1.0f, Near, Far);
    }

    public void SetLight(SpotLight light)
    {
        _light = light;
    }

    public override void UpdateShadowCamera()
    {
        if (_light == null || Camera is not PerspectiveCamera perspCamera)
            return;

        var position = Vector3.Transform(Vector3.Zero, _light.WorldMatrix);
        var target = position + _light.Direction;

        // Update perspective camera FOV based on spotlight angle
        perspCamera.Fov = _light.Angle * 2 * (180f / MathF.PI);
        perspCamera.Near = Near;
        perspCamera.Far = Far;
        perspCamera.UpdateProjectionMatrix();

        Camera.Position = position;
        Camera.LookAt(target);
        Camera.UpdateMatrixWorld();
    }
}
