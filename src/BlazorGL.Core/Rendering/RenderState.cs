using BlazorGL.Core.Materials;
using BlazorGL.Core.Shaders;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Core.Rendering;

/// <summary>
/// Tracks current rendering state to minimize state changes
/// </summary>
internal class RenderState
{
    public Shader? CurrentShader { get; set; }
    public Material? CurrentMaterial { get; set; }
    public Geometry? CurrentGeometry { get; set; }
    public BlendMode CurrentBlendMode { get; set; } = BlendMode.Normal;
    public CullMode CurrentCullMode { get; set; } = CullMode.Back;
    public bool DepthTest { get; set; } = true;
    public bool DepthWrite { get; set; } = true;
    public uint CurrentVAO { get; set; }

    public void Reset()
    {
        CurrentShader = null;
        CurrentMaterial = null;
        CurrentGeometry = null;
        CurrentBlendMode = BlendMode.Normal;
        CurrentCullMode = CullMode.Back;
        DepthTest = true;
        DepthWrite = true;
        CurrentVAO = 0;
    }
}
