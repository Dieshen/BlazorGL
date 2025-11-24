namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Bokeh depth-of-field shader
/// Simulates camera lens focus with depth-based blur
/// </summary>
public static class BokehShader
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

uniform sampler2D tColor;
uniform sampler2D tDepth;

uniform float focus;
uniform float aperture;
uniform float maxBlur;
uniform int samples;
uniform int rings;

uniform vec2 resolution;
uniform float cameraNear;
uniform float cameraFar;

varying vec2 vUv;

const float PI = 3.14159265359;

// Linearize depth from depth buffer
float getLinearDepth(float depth) {
    float z = depth * 2.0 - 1.0;
    return (2.0 * cameraNear * cameraFar) / (cameraFar + cameraNear - z * (cameraFar - cameraNear));
}

// Calculate Circle of Confusion
float getCoC(float depth) {
    float linearDepth = getLinearDepth(depth);
    float coc = abs(linearDepth - focus) * aperture / (linearDepth * (focus - aperture + 0.001));
    return clamp(coc, 0.0, maxBlur);
}

void main() {
    float depth = texture2D(tDepth, vUv).x;
    float centerCoC = getCoC(depth);

    vec3 color = texture2D(tColor, vUv).rgb;
    float totalWeight = 1.0;

    // Bokeh-shaped sampling pattern (spiral)
    float goldenAngle = 2.39996323; // Golden angle in radians
    int totalSamples = samples;

    for (int i = 1; i <= totalSamples; i++) {
        float angle = float(i) * goldenAngle;
        float radius = sqrt(float(i) / float(totalSamples));

        vec2 offset = vec2(cos(angle), sin(angle)) * radius * centerCoC;
        vec2 sampleUV = vUv + offset / resolution;

        // Sample color and depth
        vec3 sampleColor = texture2D(tColor, sampleUV).rgb;
        float sampleDepth = texture2D(tDepth, sampleUV).x;
        float sampleCoC = getCoC(sampleDepth);

        // Weight based on CoC comparison (foreground has priority)
        float weight = sampleCoC >= centerCoC ? 1.0 : sampleCoC / centerCoC;

        color += sampleColor * weight;
        totalWeight += weight;
    }

    color /= totalWeight;

    gl_FragColor = vec4(color, 1.0);
}
";
}
