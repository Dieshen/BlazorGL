namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Simple copy shader for texture sampling
/// </summary>
public static class CopyShader
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
uniform float opacity;

varying vec2 vUv;

void main() {
    vec4 texel = texture2D(tDiffuse, vUv);
    gl_FragColor = opacity * texel;
}
";
}
