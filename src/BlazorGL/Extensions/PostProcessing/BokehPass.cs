using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing.Shaders;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Bokeh depth-of-field post-processing pass
/// Simulates camera focus with depth-based blur
/// </summary>
public class BokehPass : ShaderPass
{
    private readonly Scene _scene;
    private readonly Camera _camera;
    private readonly int _width;
    private readonly int _height;

    /// <summary>
    /// Focus distance in world units (where the camera is focused)
    /// </summary>
    public float Focus { get; set; } = 1.0f;

    /// <summary>
    /// Aperture/f-stop control - larger values create more blur
    /// </summary>
    public float Aperture { get; set; } = 0.025f;

    /// <summary>
    /// Maximum blur size (clamps circle of confusion)
    /// </summary>
    public float MaxBlur { get; set; } = 1.0f;

    /// <summary>
    /// Number of samples for bokeh blur (more = better quality, slower)
    /// </summary>
    public int Samples { get; set; } = 64;

    /// <summary>
    /// Number of sampling rings (legacy parameter, maintained for compatibility)
    /// </summary>
    public int Rings { get; set; } = 3;

    /// <summary>
    /// Show focus plane for debugging
    /// </summary>
    public bool ShowFocus { get; set; } = false;

    public BokehPass(Scene scene, Camera camera, int width, int height)
        : base(new ShaderMaterial(BokehShader.VertexShader, BokehShader.FragmentShader))
    {
        _scene = scene;
        _camera = camera;
        _width = width;
        _height = height;

        // Initialize uniforms
        UpdateUniforms();
    }

    private void UpdateUniforms()
    {
        _material.Uniforms["focus"] = Focus;
        _material.Uniforms["aperture"] = Aperture;
        _material.Uniforms["maxBlur"] = MaxBlur;
        _material.Uniforms["samples"] = Samples;
        _material.Uniforms["rings"] = Rings;
        _material.Uniforms["resolution"] = new Vector2(_width, _height);

        // Set camera parameters for depth linearization
        if (_camera is PerspectiveCamera perspectiveCamera)
        {
            _material.Uniforms["cameraNear"] = perspectiveCamera.Near;
            _material.Uniforms["cameraFar"] = perspectiveCamera.Far;
        }
        else if (_camera is OrthographicCamera orthoCamera)
        {
            _material.Uniforms["cameraNear"] = orthoCamera.Near;
            _material.Uniforms["cameraFar"] = orthoCamera.Far;
        }
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        if (input?.Texture == null)
        {
            throw new InvalidOperationException("BokehPass requires input texture");
        }

        // Note: Depth texture support requires RenderTarget enhancement
        // For now, we use a placeholder depth texture
        // TODO: Add depth texture rendering support to RenderTarget

        // Update uniforms with latest values
        UpdateUniforms();

        // Set textures
        _material.Uniforms["tColor"] = input.Texture;
        // _material.Uniforms["tDepth"] = depthTexture; // TODO: Set when available

        // Render
        base.Render(renderer, input, output);
    }

    /// <summary>
    /// Sets focus distance based on screen coordinates
    /// </summary>
    /// <param name="screenX">Screen X coordinate (0-1)</param>
    /// <param name="screenY">Screen Y coordinate (0-1)</param>
    /// <param name="depthTexture">Depth texture to sample from</param>
    public void SetFocusFromScreen(float screenX, float screenY, Texture depthTexture)
    {
        // Sample depth at screen position
        // Note: This would require reading back from GPU, which is expensive
        // In practice, this should be done on the JavaScript side
        // For now, this is a placeholder API
        throw new NotImplementedException("SetFocusFromScreen requires GPU readback - implement on JS side");
    }
}
