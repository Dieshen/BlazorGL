namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// SMAA Edge Detection Shader (Pass 1)
/// Detects edges using luminance discontinuities
/// </summary>
public static class SMAAEdgeDetectionShader
{
    public const string VertexShader = @"
attribute vec3 position;
attribute vec2 uv;

varying vec2 vUv;
varying vec4 vOffset[3];

uniform vec2 resolution;

void main() {
    vUv = uv;

    vec2 pixelSize = 1.0 / resolution;

    // Calculate texture coordinates for neighbor samples
    vOffset[0] = uv.xyxy + pixelSize.xyxy * vec4(-1.0, 0.0, 0.0, -1.0);
    vOffset[1] = uv.xyxy + pixelSize.xyxy * vec4(1.0, 0.0, 0.0, 1.0);
    vOffset[2] = uv.xyxy + pixelSize.xyxy * vec4(-2.0, 0.0, 0.0, -2.0);

    gl_Position = vec4(position.xy, 0.0, 1.0);
}
";

    public const string FragmentShader = @"
precision highp float;

uniform sampler2D tDiffuse;
uniform float threshold;

varying vec2 vUv;
varying vec4 vOffset[3];

// Luminance calculation
float luminance(vec3 color) {
    return dot(color, vec3(0.299, 0.587, 0.114));
}

void main() {
    // Sample center and neighbors
    float L = luminance(texture2D(tDiffuse, vUv).rgb);
    float Lleft = luminance(texture2D(tDiffuse, vOffset[0].xy).rgb);
    float Ltop = luminance(texture2D(tDiffuse, vOffset[0].zw).rgb);
    float Lright = luminance(texture2D(tDiffuse, vOffset[1].xy).rgb);
    float Lbottom = luminance(texture2D(tDiffuse, vOffset[1].zw).rgb);

    // Calculate deltas
    vec4 delta;
    delta.x = abs(L - Lleft);
    delta.y = abs(L - Ltop);
    delta.z = abs(L - Lright);
    delta.w = abs(L - Lbottom);

    // Maximum delta determines edge strength
    vec2 edges = step(threshold, delta.xy);

    // Early exit if no edges
    if (dot(edges, vec2(1.0)) == 0.0) {
        discard;
    }

    gl_FragColor = vec4(edges, 0.0, 1.0);
}
";
}
