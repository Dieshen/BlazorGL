using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Effect composer for post-processing
/// </summary>
public class EffectComposer
{
    private List<Pass> _passes = new();
    private RenderTarget? _writeBuffer;
    private RenderTarget? _readBuffer;
    private Renderer _renderer;

    public EffectComposer(Renderer renderer, int width, int height)
    {
        _renderer = renderer;
        _writeBuffer = new RenderTarget(width, height);
        _readBuffer = new RenderTarget(width, height);
    }

    public void AddPass(Pass pass)
    {
        _passes.Add(pass);
    }

    public void Render(Scene scene, Camera camera)
    {
        if (_passes.Count == 0 || _writeBuffer == null || _readBuffer == null)
        {
            // No passes, render directly
            _renderer.Render(scene, camera);
            return;
        }

        // Render scene to first buffer
        _renderer.SetRenderTarget(_writeBuffer);
        _renderer.Render(scene, camera);

        // Apply each pass
        for (int i = 0; i < _passes.Count; i++)
        {
            var pass = _passes[i];
            if (!pass.Enabled) continue;

            bool isLastPass = (i == _passes.Count - 1);
            var output = isLastPass ? null : _readBuffer;

            pass.Render(_renderer, _writeBuffer, output);

            // Swap buffers for next pass
            if (!isLastPass)
            {
                var temp = _writeBuffer;
                _writeBuffer = _readBuffer;
                _readBuffer = temp;
            }
        }

        // Final render is already on screen from last pass
        _renderer.SetRenderTarget(null);
    }
}

public abstract class Pass
{
    public bool Enabled { get; set; } = true;
    public abstract void Render(Renderer renderer, RenderTarget input, RenderTarget output);
}
