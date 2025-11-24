namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Gaussian blur shader (separable - horizontal or vertical)
/// </summary>
public static class GaussianBlurShader
{
    public const string VertexShader = @"
attribute vec3 position;
attribute vec2 uv;

varying vec2 vUv;

void main() {
    vUv = uv;
    gl_Position = vec4(position.xy, 0.0, 1.0);
}
";

    public const string FragmentShader = @"
precision mediump float;

uniform sampler2D tDiffuse;
uniform vec2 resolution;
uniform vec2 direction;

varying vec2 vUv;

// 5-tap Gaussian blur
void main() {
    vec2 invSize = 1.0 / resolution;
    vec2 offset = direction * invSize;

    vec4 sum = vec4(0.0);

    // Gaussian kernel weights for 5 samples
    sum += texture2D(tDiffuse, vUv - 2.0 * offset) * 0.0625;
    sum += texture2D(tDiffuse, vUv - offset) * 0.25;
    sum += texture2D(tDiffuse, vUv) * 0.375;
    sum += texture2D(tDiffuse, vUv + offset) * 0.25;
    sum += texture2D(tDiffuse, vUv + 2.0 * offset) * 0.0625;

    gl_FragColor = sum;
}
";
}
