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
    public bool NeedsCompile { get; protected set; } = true;

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
