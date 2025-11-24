using System.Numerics;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Shaders;
using BlazorGL.Core.Shaders.ShaderChunks;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Variance Shadow Map implementation
/// Uses statistical filtering (depth and depth^2) for smooth shadows without multiple samples
/// </summary>
public class VSMShadowMap
{
    /// <summary>
    /// Shadow map render target (stores depth and depth^2 in RG channels)
    /// </summary>
    public RenderTarget ShadowMapTarget { get; private set; }

    /// <summary>
    /// Horizontal blur intermediate target
    /// </summary>
    public RenderTarget HorizontalBlurTarget { get; private set; }

    /// <summary>
    /// Final blurred shadow map target
    /// </summary>
    public RenderTarget BlurredShadowMapTarget { get; private set; }

    /// <summary>
    /// Width of shadow map
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Height of shadow map
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Minimum variance to prevent precision issues
    /// </summary>
    public float MinVariance { get; set; } = 0.00001f;

    /// <summary>
    /// Light bleeding reduction factor (0-1)
    /// Higher values reduce light bleeding but may darken shadows
    /// </summary>
    public float LightBleedingReduction { get; set; } = 0.1f;

    /// <summary>
    /// Gaussian blur size (number of samples per direction)
    /// </summary>
    public int BlurSize { get; set; } = 5;

    /// <summary>
    /// Gaussian blur sigma (controls blur spread)
    /// </summary>
    public float BlurSigma { get; set; } = 2.0f;

    public VSMShadowMap(int width, int height)
    {
        Width = width;
        Height = height;

        // Create render targets
        // Main shadow map - stores depth moments (RG channels)
        ShadowMapTarget = new RenderTarget(width, height)
        {
            DepthBuffer = true,
            StencilBuffer = false
        };

        // Horizontal blur target
        HorizontalBlurTarget = new RenderTarget(width, height)
        {
            DepthBuffer = false,
            StencilBuffer = false
        };

        // Final blurred target
        BlurredShadowMapTarget = new RenderTarget(width, height)
        {
            DepthBuffer = false,
            StencilBuffer = false
        };
    }

    /// <summary>
    /// Get VSM depth shader code for creating custom depth material
    /// </summary>
    public static (string vertexShader, string fragmentShader) GetDepthShaders()
    {
        return (ShadowMapChunks.DepthVertexShader, ShadowMapChunks.VSMDepthFragmentShader);
    }

    /// <summary>
    /// Get Gaussian blur shader code for creating custom blur material
    /// </summary>
    public static (string vertexShader, string fragmentShader) GetBlurShaders(bool horizontal)
    {
        string fragmentShader = @"#version 300 es
precision highp float;

uniform sampler2D tDiffuse;
uniform vec2 resolution;
uniform int blurSize;
uniform float sigma;

in vec2 vUv;
out vec4 fragColor;

// Gaussian weight calculation
float gaussian(float x, float sigma) {
    return exp(-(x * x) / (2.0 * sigma * sigma));
}

void main() {
    vec2 texelSize = 1.0 / resolution;
    vec4 result = vec4(0.0);
    float weightSum = 0.0;

    // Direction vector
    vec2 direction = " + (horizontal ? "vec2(1.0, 0.0)" : "vec2(0.0, 1.0)") + @";

    // Gaussian blur
    for (int i = -blurSize; i <= blurSize; i++) {
        float weight = gaussian(float(i), sigma);
        vec2 offset = direction * float(i) * texelSize;
        result += texture(tDiffuse, vUv + offset) * weight;
        weightSum += weight;
    }

    fragColor = result / weightSum;
}";

        string vertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec2 uv;

uniform mat4 projectionMatrix;

out vec2 vUv;

void main() {
    vUv = uv;
    gl_Position = projectionMatrix * vec4(position, 1.0);
}";

        return (vertexShader, fragmentShader);
    }

    /// <summary>
    /// Render VSM shadow map from light's perspective
    /// NOTE: This method provides the structure for integration with the renderer.
    /// Full implementation requires renderer-level VSM support.
    /// </summary>
    public void RenderShadowMap(Renderer renderer, Scene scene, Light light)
    {
        if (light is not DirectionalLight dirLight)
            return;

        // Update shadow camera
        dirLight.Shadow.UpdateShadowCamera();
        Camera shadowCamera = dirLight.Shadow.Camera;

        // Render depth moments to shadow map
        renderer.SetRenderTarget(ShadowMapTarget);
        renderer.Context.Clear(false, true, false);

        // Render scene with VSM depth material
        // Implementation requires:
        // 1. Create material with GetDepthShaders()
        // 2. Render scene objects with depth material
        // 3. Material should output depth and depth^2 to RG channels
    }

    /// <summary>
    /// Apply Gaussian blur to shadow map
    /// Uses separable filtering (horizontal then vertical) for performance
    /// NOTE: This method provides the structure for integration with the renderer.
    /// Full implementation requires full-screen quad rendering utility.
    /// </summary>
    public void BlurShadowMap(Renderer renderer)
    {
        // Horizontal blur pass
        renderer.SetRenderTarget(HorizontalBlurTarget);
        // Implementation requires:
        // 1. Create material with GetBlurShaders(horizontal: true)
        // 2. Set uniforms: resolution, blurSize, sigma, tDiffuse (ShadowMapTarget.Texture)
        // 3. Render full-screen quad

        // Vertical blur pass
        renderer.SetRenderTarget(BlurredShadowMapTarget);
        // Implementation requires:
        // 1. Create material with GetBlurShaders(horizontal: false)
        // 2. Set uniforms: resolution, blurSize, sigma, tDiffuse (HorizontalBlurTarget.Texture)
        // 3. Render full-screen quad

        // Restore default framebuffer
        renderer.SetRenderTarget(null);
    }

    /// <summary>
    /// Calculate Gaussian kernel weights
    /// </summary>
    public static float[] CalculateGaussianWeights(int size, float sigma)
    {
        int kernelSize = size * 2 + 1;
        float[] weights = new float[kernelSize];
        float sum = 0.0f;

        for (int i = 0; i < kernelSize; i++)
        {
            float x = i - size;
            weights[i] = MathF.Exp(-(x * x) / (2.0f * sigma * sigma));
            sum += weights[i];
        }

        // Normalize
        for (int i = 0; i < kernelSize; i++)
        {
            weights[i] /= sum;
        }

        return weights;
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        ShadowMapTarget?.Dispose();
        HorizontalBlurTarget?.Dispose();
        BlurredShadowMapTarget?.Dispose();
    }
}
