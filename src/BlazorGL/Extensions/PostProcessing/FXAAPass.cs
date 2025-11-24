using System.Numerics;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Fast Approximate Anti-Aliasing pass
/// Provides edge-based anti-aliasing in a single pass
/// </summary>
public class FXAAPass : ShaderPass
{
    private readonly int _width;
    private readonly int _height;

    public FXAAPass(int width, int height) : base(new ShaderMaterial(FXAAShader.VertexShader, FXAAShader.FragmentShader))
    {
        _width = width;
        _height = height;
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        // Set resolution uniform
        _material.Uniforms["resolution"] = new Vector2(_width, _height);

        // Call base render
        base.Render(renderer, input, output);
    }
}
