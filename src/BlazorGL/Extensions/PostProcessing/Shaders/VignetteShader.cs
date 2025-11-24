namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Vignette shader - radial darkening effect
/// </summary>
public static class VignetteShader
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
precision highp float;

uniform sampler2D tDiffuse;
uniform float offset;
uniform float darkness;
uniform float smoothness;

varying vec2 vUv;

void main() {
    vec4 texel = texture2D(tDiffuse, vUv);

    // Calculate distance from center
    vec2 uv = vUv - 0.5;
    float dist = length(uv);

    // Calculate vignette using smoothstep for smooth falloff
    float vignette = smoothstep(offset, offset - smoothness, dist);

    // Apply vignette darkening
    vec3 color = texel.rgb * mix(1.0 - darkness, 1.0, vignette);

    gl_FragColor = vec4(color, texel.a);
}
";
}
