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

    // Advanced blending state
    public BlendEquation CurrentBlendEquation { get; set; } = BlendEquation.Add;
    public BlendEquation CurrentBlendEquationAlpha { get; set; } = BlendEquation.Add;
    public BlendFactor CurrentBlendSrc { get; set; } = BlendFactor.SrcAlpha;
    public BlendFactor CurrentBlendDst { get; set; } = BlendFactor.OneMinusSrcAlpha;
    public BlendFactor CurrentBlendSrcAlpha { get; set; } = BlendFactor.One;
    public BlendFactor CurrentBlendDstAlpha { get; set; } = BlendFactor.OneMinusSrcAlpha;

    // Polygon offset state
    public bool PolygonOffset { get; set; } = false;
    public float PolygonOffsetFactor { get; set; } = 0.0f;
    public float PolygonOffsetUnits { get; set; } = 0.0f;

    // Stencil state
    public bool StencilTest { get; set; } = false;
    public StencilFunc StencilFunc { get; set; } = StencilFunc.Always;
    public int StencilRef { get; set; } = 0;
    public uint StencilMask { get; set; } = 0xFFFFFFFF;
    public StencilOp StencilFail { get; set; } = StencilOp.Keep;
    public StencilOp StencilZFail { get; set; } = StencilOp.Keep;
    public StencilOp StencilZPass { get; set; } = StencilOp.Keep;

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

        CurrentBlendEquation = BlendEquation.Add;
        CurrentBlendEquationAlpha = BlendEquation.Add;
        CurrentBlendSrc = BlendFactor.SrcAlpha;
        CurrentBlendDst = BlendFactor.OneMinusSrcAlpha;
        CurrentBlendSrcAlpha = BlendFactor.One;
        CurrentBlendDstAlpha = BlendFactor.OneMinusSrcAlpha;

        PolygonOffset = false;
        PolygonOffsetFactor = 0.0f;
        PolygonOffsetUnits = 0.0f;

        StencilTest = false;
        StencilFunc = StencilFunc.Always;
        StencilRef = 0;
        StencilMask = 0xFFFFFFFF;
        StencilFail = StencilOp.Keep;
        StencilZFail = StencilOp.Keep;
        StencilZPass = StencilOp.Keep;
    }
}
