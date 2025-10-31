using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Lights;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a point light as a wireframe sphere
/// </summary>
public class PointLightHelper : Mesh
{
    private PointLight _light;

    public PointLightHelper(PointLight light, float size = 1.0f, Math.Color? color = null)
    {
        Name = "PointLightHelper";
        _light = light;

        var col = color ?? light.Color;

        // Create small sphere to represent point light
        var geometry = new IcosahedronGeometry(size, 0);
        var material = new LineBasicMaterial { Color = col };

        Geometry = geometry;
        Material = material;

        Update();
    }

    public void Update()
    {
        // Sync position with light
        Position = _light.Position;
    }
}
