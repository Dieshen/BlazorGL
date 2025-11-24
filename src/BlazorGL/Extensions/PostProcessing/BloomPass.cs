using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing.Shaders;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Bloom post-processing effect (glow for bright areas)
/// Implements a 3-pass bloom:
/// 1. Extract bright pixels above threshold
/// 2. Apply separable Gaussian blur (horizontal + vertical)
/// 3. Additively blend bloom with original
/// </summary>
public class BloomPass : Pass
{
    private ShaderPass? _luminosityPass;
    private ShaderPass? _blurPassH;
    private ShaderPass? _blurPassV;
    private ShaderPass? _blendPass;
    private RenderTarget? _brightTarget;
    private RenderTarget? _blurTarget1;
    private RenderTarget? _blurTarget2;

    /// <summary>
    /// Luminosity threshold for bright pixel extraction (0-1)
    /// </summary>
    public float LuminosityThreshold { get; set; } = 0.8f;

    /// <summary>
    /// Smooth width for threshold falloff
    /// </summary>
    public float SmoothWidth { get; set; } = 0.1f;

    /// <summary>
    /// Bloom intensity/strength
    /// </summary>
    public float BloomStrength { get; set; } = 1.5f;

    /// <summary>
    /// Blur radius (controls blur sample distance)
    /// </summary>
    public float BlurRadius { get; set; } = 1.0f;

    /// <summary>
    /// Resolution divisor for bloom (2 = half resolution, 4 = quarter resolution)
    /// Lower resolution = better performance but less detail
    /// </summary>
    public int ResolutionDivisor { get; set; } = 2;

    private int _width;
    private int _height;

    public BloomPass(int width, int height)
    {
        _width = width;
        _height = height;
        InitializePasses();
    }

    private void InitializePasses()
    {
        int bloomWidth = _width / ResolutionDivisor;
        int bloomHeight = _height / ResolutionDivisor;

        // Create render targets for intermediate steps
        _brightTarget = new RenderTarget(bloomWidth, bloomHeight)
        {
            DepthBuffer = false
        };
        _blurTarget1 = new RenderTarget(bloomWidth, bloomHeight)
        {
            DepthBuffer = false
        };
        _blurTarget2 = new RenderTarget(bloomWidth, bloomHeight)
        {
            DepthBuffer = false
        };

        // Pass 1: Extract bright pixels
        var luminosityMaterial = new ShaderMaterial(
            LuminosityShader.VertexShader,
            LuminosityShader.FragmentShader
        );
        luminosityMaterial.Uniforms["luminosityThreshold"] = LuminosityThreshold;
        luminosityMaterial.Uniforms["smoothWidth"] = SmoothWidth;
        _luminosityPass = new ShaderPass(luminosityMaterial);

        // Pass 2a: Horizontal blur
        var blurMaterialH = new ShaderMaterial(
            GaussianBlurShader.VertexShader,
            GaussianBlurShader.FragmentShader
        );
        blurMaterialH.Uniforms["resolution"] = new System.Numerics.Vector2(bloomWidth, bloomHeight);
        blurMaterialH.Uniforms["direction"] = new System.Numerics.Vector2(BlurRadius, 0f);
        _blurPassH = new ShaderPass(blurMaterialH);

        // Pass 2b: Vertical blur
        var blurMaterialV = new ShaderMaterial(
            GaussianBlurShader.VertexShader,
            GaussianBlurShader.FragmentShader
        );
        blurMaterialV.Uniforms["resolution"] = new System.Numerics.Vector2(bloomWidth, bloomHeight);
        blurMaterialV.Uniforms["direction"] = new System.Numerics.Vector2(0f, BlurRadius);
        _blurPassV = new ShaderPass(blurMaterialV);

        // Pass 3: Additive blend
        var blendMaterial = new ShaderMaterial(
            AdditiveBlendShader.VertexShader,
            AdditiveBlendShader.FragmentShader
        );
        blendMaterial.Uniforms["bloomStrength"] = BloomStrength;
        _blendPass = new ShaderPass(blendMaterial);
    }

    public override void Render(Renderer renderer, RenderTarget? writeBuffer, RenderTarget? readBuffer)
    {
        if (_luminosityPass == null || _blurPassH == null || _blurPassV == null || _blendPass == null)
        {
            return;
        }

        // Update uniforms
        var lumMaterial = (ShaderMaterial)_luminosityPass._material;
        lumMaterial.Uniforms["luminosityThreshold"] = LuminosityThreshold;
        lumMaterial.Uniforms["smoothWidth"] = SmoothWidth;

        var blendMaterial = (ShaderMaterial)_blendPass._material;
        blendMaterial.Uniforms["bloomStrength"] = BloomStrength;

        // Step 1: Extract bright pixels
        _luminosityPass.Render(renderer, _brightTarget, readBuffer);

        // Step 2: Apply separable blur (horizontal then vertical)
        _blurPassH.Render(renderer, _blurTarget1, _brightTarget);
        _blurPassV.Render(renderer, _blurTarget2, _blurTarget1);

        // Step 3: Blend bloom with original
        // Set both textures as uniforms
        blendMaterial.Uniforms["tDiffuse"] = readBuffer?.Texture;
        blendMaterial.Uniforms["tBloom"] = _blurTarget2?.Texture;

        _blendPass.Render(renderer, writeBuffer, readBuffer);
    }

    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;

        // Dispose old targets
        _brightTarget?.Dispose();
        _blurTarget1?.Dispose();
        _blurTarget2?.Dispose();

        // Reinitialize with new size
        InitializePasses();
    }
}
