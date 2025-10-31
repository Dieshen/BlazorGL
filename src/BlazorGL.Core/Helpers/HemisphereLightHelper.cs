using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Lights;

namespace BlazorGL.Core.Helpers;

/// <summary>
/// Helper that visualizes a hemisphere light with sky and ground colors
/// </summary>
public class HemisphereLightHelper : Object3D
{
    private HemisphereLight _light;

    public HemisphereLightHelper(HemisphereLight light, float size = 1.0f)
    {
        Name = "HemisphereLightHelper";
        _light = light;

        // Create octahedron to represent hemisphere light
        var geometry = new OctahedronGeometry(size, 0);

        // Create mesh with sky color on top
        var mesh = new Mesh(geometry, new BasicMaterial
        {
            Color = light.SkyColor,
            Wireframe = true
        });

        AddChild(mesh);

        Update();
    }

    public void Update()
    {
        // Sync position with light if it has one
        // Hemisphere lights typically don't have a position, but we can update color
    }
}
