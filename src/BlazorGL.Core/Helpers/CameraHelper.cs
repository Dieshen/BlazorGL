using BlazorGL.Core.Cameras;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a camera's frustum
/// </summary>
public class CameraHelper : LineSegments
{
    private Camera _camera;

    public CameraHelper(Camera camera)
    {
        Name = "CameraHelper";
        _camera = camera;

        var geometry = new BufferGeometry();

        // Create frustum wireframe (simplified - 8 corners connected)
        float[] vertices = new float[]
        {
            // Near plane
            -1, -1, -1,  1, -1, -1,
            1, -1, -1,  1, 1, -1,
            1, 1, -1,  -1, 1, -1,
            -1, 1, -1,  -1, -1, -1,
            // Far plane
            -2, -2, -2,  2, -2, -2,
            2, -2, -2,  2, 2, -2,
            2, 2, -2,  -2, 2, -2,
            -2, 2, -2,  -2, -2, -2,
            // Connections
            -1, -1, -1,  -2, -2, -2,
            1, -1, -1,  2, -2, -2,
            1, 1, -1,  2, 2, -2,
            -1, 1, -1,  -2, 2, -2
        };

        geometry.SetAttribute("position", vertices, 3);

        var material = new LineBasicMaterial { Color = new Math.Color(1, 1, 0) };

        Geometry = geometry;
        Material = material;

        Update();
    }

    public void Update()
    {
        // TODO: Update frustum based on camera's projection matrix
    }
}
