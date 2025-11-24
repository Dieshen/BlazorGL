namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Luminosity (brightness) extraction shader for bloom
/// </summary>
public static class LuminosityShader
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
uniform float luminosityThreshold;
uniform float smoothWidth;

varying vec2 vUv;

void main() {
    vec4 texel = texture2D(tDiffuse, vUv);

    // Calculate luminosity using perceptual weights
    float luminosity = dot(texel.rgb, vec3(0.299, 0.587, 0.114));

    // Smooth threshold for gradual falloff
    float threshold = smoothstep(luminosityThreshold, luminosityThreshold + smoothWidth, luminosity);

    gl_FragColor = vec4(texel.rgb * threshold, texel.a);
}
";
}
