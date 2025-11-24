using BlazorGL.Core.Cameras;
using System.Numerics;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Shadow for point lights using cubemap (6 perspectives)
/// </summary>
public class PointLightShadow : LightShadow
{
    /// <summary>
    /// Six cameras for cubemap faces (one per direction)
    /// </summary>
    public PerspectiveCamera[] Cameras { get; private set; } = new PerspectiveCamera[6];

    private PointLight? _light;

    public PointLightShadow()
    {
        // Create 6 perspective cameras for cubemap faces
        for (int i = 0; i < 6; i++)
        {
            Cameras[i] = new PerspectiveCamera(90, 1.0f, Near, Far);
        }
        Camera = Cameras[0]; // Default to first camera
    }

    public void SetLight(PointLight light)
    {
        _light = light;
    }

    public override void UpdateShadowCamera()
    {
        if (_light == null)
            return;

        var position = Vector3.Transform(Vector3.Zero, _light.WorldMatrix);

        // Update all 6 cameras for cubemap faces
        // +X, -X, +Y, -Y, +Z, -Z
        var targets = new Vector3[]
        {
            position + Vector3.UnitX,     // +X
            position - Vector3.UnitX,     // -X
            position + Vector3.UnitY,     // +Y
            position - Vector3.UnitY,     // -Y
            position + Vector3.UnitZ,     // +Z
            position - Vector3.UnitZ      // -Z
        };

        var ups = new Vector3[]
        {
            -Vector3.UnitY,  // +X
            -Vector3.UnitY,  // -X
            Vector3.UnitZ,   // +Y
            -Vector3.UnitZ,  // -Y
            -Vector3.UnitY,  // +Z
            -Vector3.UnitY   // -Z
        };

        for (int i = 0; i < 6; i++)
        {
            Cameras[i].Position = position;
            Cameras[i].Near = Near;
            Cameras[i].Far = Far;
            Cameras[i].UpdateProjectionMatrix();
            Cameras[i].LookAt(targets[i], ups[i]);
            Cameras[i].UpdateMatrixWorld();
        }
    }
}
