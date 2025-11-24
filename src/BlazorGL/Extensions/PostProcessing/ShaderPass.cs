using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Base class for shader-based post-processing passes
/// </summary>
public class ShaderPass : Pass
{
    private Mesh? _fullScreenQuad;
    public ShaderMaterial _material;
    private Camera _camera;

    public ShaderPass(ShaderMaterial material)
    {
        _material = material;

        // Create orthographic camera for full-screen rendering
        _camera = new OrthographicCamera(-1, 1, 1, -1, 0, 1);

        // Create full-screen quad geometry
        var geometry = new PlaneGeometry(2, 2);
        _fullScreenQuad = new Mesh(geometry, material);
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        if (_fullScreenQuad == null) return;

        // Set input texture uniform
        if (input?.Texture != null)
        {
            _material.Uniforms["tDiffuse"] = input.Texture;
        }

        // Render to output or screen
        renderer.SetRenderTarget(output);
        renderer.AutoClear = true;

        // Create simple scene with the quad
        var scene = new Scene();
        scene.Add(_fullScreenQuad);

        renderer.Render(scene, _camera);
    }
}
