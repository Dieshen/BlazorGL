using BlazorGL.Core.Shaders;
using BlazorGL.Core.Rendering;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Base class for all materials
/// </summary>
public abstract class Material : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Material name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this material is transparent
    /// </summary>
    public bool Transparent { get; set; } = false;

    /// <summary>
    /// Opacity (0 = fully transparent, 1 = fully opaque)
    /// </summary>
    public float Opacity { get; set; } = 1.0f;

    /// <summary>
    /// Blending mode
    /// </summary>
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;

    /// <summary>
    /// Face culling mode
    /// </summary>
    public CullMode CullMode { get; set; } = CullMode.Back;

    /// <summary>
    /// Enable depth testing
    /// </summary>
    public bool DepthTest { get; set; } = true;

    /// <summary>
    /// Enable depth writing
    /// </summary>
    public bool DepthWrite { get; set; } = true;

    /// <summary>
    /// Polygon rendering mode
    /// </summary>
    public PolygonMode PolygonMode { get; set; } = PolygonMode.Fill;

    /// <summary>
    /// Whether to render as wireframe
    /// </summary>
    public bool Wireframe
    {
        get => PolygonMode == PolygonMode.Line;
        set => PolygonMode = value ? PolygonMode.Line : PolygonMode.Fill;
    }

    /// <summary>
    /// Blending equation (how to combine source and destination)
    /// </summary>
    public BlendEquation BlendEquation { get; set; } = BlendEquation.Add;

    /// <summary>
    /// Blending equation for alpha channel
    /// </summary>
    public BlendEquation BlendEquationAlpha { get; set; } = BlendEquation.Add;

    /// <summary>
    /// Source blend factor
    /// </summary>
    public BlendFactor BlendSrc { get; set; } = BlendFactor.SrcAlpha;

    /// <summary>
    /// Destination blend factor
    /// </summary>
    public BlendFactor BlendDst { get; set; } = BlendFactor.OneMinusSrcAlpha;

    /// <summary>
    /// Source alpha blend factor
    /// </summary>
    public BlendFactor BlendSrcAlpha { get; set; } = BlendFactor.One;

    /// <summary>
    /// Destination alpha blend factor
    /// </summary>
    public BlendFactor BlendDstAlpha { get; set; } = BlendFactor.OneMinusSrcAlpha;

    /// <summary>
    /// Alpha test threshold (fragments with alpha below this are discarded)
    /// </summary>
    public float AlphaTest { get; set; } = 0.0f;

    /// <summary>
    /// Enable polygon offset (useful for decals, outlines, etc.)
    /// </summary>
    public bool PolygonOffset { get; set; } = false;

    /// <summary>
    /// Polygon offset factor
    /// </summary>
    public float PolygonOffsetFactor { get; set; } = 0.0f;

    /// <summary>
    /// Polygon offset units
    /// </summary>
    public float PolygonOffsetUnits { get; set; } = 0.0f;

    /// <summary>
    /// Enable stencil testing
    /// </summary>
    public bool StencilTest { get; set; } = false;

    /// <summary>
    /// Stencil function
    /// </summary>
    public StencilFunc StencilFunc { get; set; } = StencilFunc.Always;

    /// <summary>
    /// Stencil reference value
    /// </summary>
    public int StencilRef { get; set; } = 0;

    /// <summary>
    /// Stencil mask
    /// </summary>
    public uint StencilMask { get; set; } = 0xFFFFFFFF;

    /// <summary>
    /// Stencil operation when test fails
    /// </summary>
    public StencilOp StencilFail { get; set; } = StencilOp.Keep;

    /// <summary>
    /// Stencil operation when depth test fails
    /// </summary>
    public StencilOp StencilZFail { get; set; } = StencilOp.Keep;

    /// <summary>
    /// Stencil operation when both tests pass
    /// </summary>
    public StencilOp StencilZPass { get; set; } = StencilOp.Keep;

    /// <summary>
    /// Shader program for this material
    /// </summary>
    public Shader Shader { get; protected set; } = null!;

    /// <summary>
    /// Uniform values for this material
    /// </summary>
    public Dictionary<string, object> Uniforms { get; } = new();

    /// <summary>
    /// Whether the shader needs to be compiled
    /// </summary>
    public bool NeedsCompile { get; set; } = true;

    /// <summary>
    /// Initializes the material's shader
    /// </summary>
    public abstract void InitializeShader();

    /// <summary>
    /// Called before the material is compiled (allows shader customization)
    /// </summary>
    public virtual void OnBeforeCompile(Shader shader)
    {
    }

    /// <summary>
    /// Updates material uniforms before rendering
    /// </summary>
    public virtual void UpdateUniforms()
    {
    }

    /// <summary>
    /// Applies this material to the rendering context
    /// </summary>
    public virtual void Apply(RenderContext context)
    {
        // Subclasses should override to set specific uniforms
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Shader?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
