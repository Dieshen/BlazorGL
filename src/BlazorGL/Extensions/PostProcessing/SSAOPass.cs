using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Screen Space Ambient Occlusion pass
/// </summary>
public class SSAOPass : Pass
{
    private readonly Renderer _renderer;
    private readonly Camera _camera;
    private RenderTarget? _depthTarget;
    private RenderTarget? _ssaoTarget;
    private ShaderPass? _ssaoPass;
    private ShaderPass? _blurPass;

    // SSAO parameters
    public int KernelSize { get; set; } = 32;
    public float Radius { get; set; } = 0.5f;
    public float Bias { get; set; } = 0.01f;
    public float Power { get; set; } = 1.5f;

    private Vector3[] _kernel = Array.Empty<Vector3>();
    private Texture? _noiseTexture;

    public SSAOPass(Renderer renderer, Camera camera, int width, int height)
    {
        _renderer = renderer;
        _camera = camera;

        // Initialize render targets
        _depthTarget = new RenderTarget(width, height) { DepthBuffer = true };
        _ssaoTarget = new RenderTarget(width, height) { DepthBuffer = false };

        // Generate SSAO kernel
        GenerateKernel();

        // Generate noise texture
        GenerateNoiseTexture();

        // Create SSAO material and pass
        var ssaoMaterial = new ShaderMaterial(SSAOShader.VertexShader, SSAOShader.FragmentShader);
        _ssaoPass = new ShaderPass(ssaoMaterial);

        // Create blur material and pass
        var blurMaterial = new ShaderMaterial(SSAOShader.VertexShader, SSAOShader.BlurFragmentShader);
        _blurPass = new ShaderPass(blurMaterial);
    }

    /// <summary>
    /// Generates hemisphere sample kernel for SSAO
    /// </summary>
    private void GenerateKernel()
    {
        _kernel = new Vector3[64];
        var random = new Random(42); // Fixed seed for consistency

        for (int i = 0; i < 64; i++)
        {
            // Generate random point in hemisphere
            var sample = new Vector3(
                (float)(random.NextDouble() * 2.0 - 1.0),
                (float)(random.NextDouble() * 2.0 - 1.0),
                (float)random.NextDouble()
            );

            sample = Vector3.Normalize(sample);

            // Scale samples s.t. they're more aligned to center of kernel
            float scale = (float)i / 64.0f;
            scale = Lerp(0.1f, 1.0f, scale * scale);
            sample *= scale;

            _kernel[i] = sample;
        }
    }

    /// <summary>
    /// Generates random noise texture for sample rotation
    /// </summary>
    private void GenerateNoiseTexture()
    {
        const int noiseSize = 4;
        var random = new Random(42);
        var noiseData = new byte[noiseSize * noiseSize * 4];

        for (int i = 0; i < noiseSize * noiseSize; i++)
        {
            // Random rotation vector (tangent space)
            var noise = new Vector3(
                (float)(random.NextDouble() * 2.0 - 1.0),
                (float)(random.NextDouble() * 2.0 - 1.0),
                0.0f
            );
            noise = Vector3.Normalize(noise);

            int offset = i * 4;
            noiseData[offset + 0] = (byte)((noise.X * 0.5f + 0.5f) * 255);
            noiseData[offset + 1] = (byte)((noise.Y * 0.5f + 0.5f) * 255);
            noiseData[offset + 2] = (byte)((noise.Z * 0.5f + 0.5f) * 255);
            noiseData[offset + 3] = 255;
        }

        _noiseTexture = new Texture
        {
            Width = noiseSize,
            Height = noiseSize,
            ImageData = noiseData,
            MinFilter = TextureMinFilter.Nearest,
            MagFilter = TextureMagFilter.Nearest,
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.Repeat,
            GenerateMipmaps = false,
            NeedsUpdate = true
        };
    }

    public override void Render(Renderer renderer, RenderTarget? input, RenderTarget? output)
    {
        if (_depthTarget == null || _ssaoTarget == null || _ssaoPass == null || _blurPass == null)
            return;

        // Step 1: Render depth to texture
        RenderDepth(renderer);

        // Step 2: Calculate SSAO
        UpdateSSAOUniforms();
        _ssaoPass.Render(renderer, input, _ssaoTarget);

        // Step 3: Blur SSAO
        UpdateBlurUniforms();
        _blurPass.Render(renderer, _ssaoTarget, output);
    }

    /// <summary>
    /// Renders scene depth to depth target
    /// </summary>
    private void RenderDepth(Renderer renderer)
    {
        // This would require scene reference - for now, we'll use the depth buffer
        // In a real implementation, we'd render the scene with a depth material
        // For now, this is a placeholder showing the intent
    }

    /// <summary>
    /// Updates SSAO shader uniforms
    /// </summary>
    private void UpdateSSAOUniforms()
    {
        if (_ssaoPass == null || _depthTarget == null || _noiseTexture == null)
            return;

        var material = _ssaoPass._material;

        // Set textures
        material.Uniforms["tDepth"] = _depthTarget.Texture;
        material.Uniforms["tNoise"] = _noiseTexture;

        // Set kernel samples
        for (int i = 0; i < _kernel.Length; i++)
        {
            material.Uniforms[$"kernel[{i}]"] = _kernel[i];
        }

        // Set matrices
        if (_camera is PerspectiveCamera perspCamera)
        {
            material.Uniforms["projection"] = perspCamera.ProjectionMatrix;
            var projectionInverse = Matrix4x4.Invert(perspCamera.ProjectionMatrix, out var inverted)
                ? inverted
                : Matrix4x4.Identity;
            material.Uniforms["projectionInverse"] = projectionInverse;
        }

        // Set parameters
        material.Uniforms["kernelSize"] = (float)KernelSize;
        material.Uniforms["radius"] = Radius;
        material.Uniforms["bias"] = Bias;
        material.Uniforms["power"] = Power;
        material.Uniforms["resolution"] = new Vector2(_ssaoTarget?.Width ?? 1024, _ssaoTarget?.Height ?? 768);
    }

    /// <summary>
    /// Updates blur shader uniforms
    /// </summary>
    private void UpdateBlurUniforms()
    {
        if (_blurPass == null || _ssaoTarget == null)
            return;

        var material = _blurPass._material;
        material.Uniforms["resolution"] = new Vector2(_ssaoTarget.Width, _ssaoTarget.Height);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}
