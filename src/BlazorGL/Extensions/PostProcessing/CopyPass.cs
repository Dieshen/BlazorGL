using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing.Shaders;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Simple copy pass for copying one texture to another with optional opacity
/// </summary>
public class CopyPass : Pass
{
    private ShaderPass? _copyShaderPass;

    /// <summary>
    /// Opacity for the copy operation (0-1)
    /// </summary>
    public float Opacity { get; set; } = 1.0f;

    public CopyPass()
    {
        InitializePass();
    }

    private void InitializePass()
    {
        var copyMaterial = new ShaderMaterial(
            CopyShader.VertexShader,
            CopyShader.FragmentShader
        );
        copyMaterial.Uniforms["opacity"] = Opacity;

        _copyShaderPass = new ShaderPass(copyMaterial);
    }

    public override void Render(Renderer renderer, RenderTarget? writeBuffer, RenderTarget? readBuffer)
    {
        if (_copyShaderPass == null)
        {
            return;
        }

        // Update opacity uniform
        _copyShaderPass._material.Uniforms["opacity"] = Opacity;

        // Render copy
        _copyShaderPass.Render(renderer, writeBuffer, readBuffer);
    }
}
