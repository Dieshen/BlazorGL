namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// SMAA Neighborhood Blending Shader (Pass 3)
/// Blends pixels with neighbors using computed weights
/// </summary>
public static class SMAABlendShader
{
    public const string VertexShader = @"
attribute vec3 position;
attribute vec2 uv;

varying vec2 vUv;
varying vec4 vOffset;

uniform vec2 resolution;

void main() {
    vUv = uv;

    vec2 pixelSize = 1.0 / resolution;
    vOffset = uv.xyxy + pixelSize.xyxy * vec4(1.0, 0.0, 0.0, 1.0);

    gl_Position = vec4(position.xy, 0.0, 1.0);
}
";

    public const string FragmentShader = @"
precision highp float;

uniform sampler2D tDiffuse;
uniform sampler2D tWeights;

uniform vec2 resolution;

varying vec2 vUv;
varying vec4 vOffset;

void main() {
    vec4 color = texture2D(tDiffuse, vUv);
    vec4 weights = texture2D(tWeights, vUv);

    // Early exit if no blending needed
    if (dot(weights, vec4(1.0)) < 0.0001) {
        gl_FragColor = color;
        return;
    }

    vec2 pixelSize = 1.0 / resolution;

    // Blend with horizontal neighbors
    vec4 h = vec4(0.0);
    h.x = weights.r;
    h.y = weights.g;
    if (h.x > 0.0) {
        vec3 colorLeft = texture2D(tDiffuse, vUv - vec2(pixelSize.x, 0.0)).rgb;
        color.rgb = mix(color.rgb, colorLeft, h.x);
    }
    if (h.y > 0.0) {
        vec3 colorRight = texture2D(tDiffuse, vUv + vec2(pixelSize.x, 0.0)).rgb;
        color.rgb = mix(color.rgb, colorRight, h.y);
    }

    // Blend with vertical neighbors
    vec4 v = vec4(0.0);
    v.x = weights.b;
    v.y = weights.a;
    if (v.x > 0.0) {
        vec3 colorTop = texture2D(tDiffuse, vUv - vec2(0.0, pixelSize.y)).rgb;
        color.rgb = mix(color.rgb, colorTop, v.x);
    }
    if (v.y > 0.0) {
        vec3 colorBottom = texture2D(tDiffuse, vUv + vec2(0.0, pixelSize.y)).rgb;
        color.rgb = mix(color.rgb, colorBottom, v.y);
    }

    gl_FragColor = color;
}
";
}
