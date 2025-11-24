namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Additive blending shader for combining bloom with original scene
/// </summary>
public static class AdditiveBlendShader
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
uniform sampler2D tBloom;
uniform float bloomStrength;

varying vec2 vUv;

void main() {
    vec4 baseColor = texture2D(tDiffuse, vUv);
    vec4 bloomColor = texture2D(tBloom, vUv);

    // Additive blend with strength control
    gl_FragColor = baseColor + bloomColor * bloomStrength;
}
";
}
