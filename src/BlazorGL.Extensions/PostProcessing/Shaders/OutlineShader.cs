namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Edge detection shader for outlining selected objects
/// Uses Sobel operator for edge detection
/// </summary>
public static class OutlineShader
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
uniform sampler2D tDepth;
uniform vec2 resolution;
uniform vec3 outlineColor;
uniform float outlineThickness;

varying vec2 vUv;

void main() {
    vec2 texelSize = 1.0 / resolution;

    // Sobel operator for edge detection
    float depth = texture2D(tDepth, vUv).r;

    // Sample surrounding pixels
    float depthN  = texture2D(tDepth, vUv + vec2(0.0, texelSize.y)).r;
    float depthS  = texture2D(tDepth, vUv - vec2(0.0, texelSize.y)).r;
    float depthE  = texture2D(tDepth, vUv + vec2(texelSize.x, 0.0)).r;
    float depthW  = texture2D(tDepth, vUv - vec2(texelSize.x, 0.0)).r;

    // Calculate gradients
    float depthGradX = abs(depthE - depthW);
    float depthGradY = abs(depthN - depthS);
    float depthGrad = sqrt(depthGradX * depthGradX + depthGradY * depthGradY);

    // Threshold for edge detection
    float edgeStrength = smoothstep(0.01 * outlineThickness, 0.02 * outlineThickness, depthGrad);

    // Get base color
    vec4 baseColor = texture2D(tDiffuse, vUv);

    // Mix outline color with base color
    gl_FragColor = mix(baseColor, vec4(outlineColor, 1.0), edgeStrength);
}
";
}
