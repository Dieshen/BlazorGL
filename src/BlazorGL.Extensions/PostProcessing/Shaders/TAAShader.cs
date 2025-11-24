namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Temporal Anti-Aliasing (TAA) shader
/// Accumulates frames over time with reprojection and history clamping
/// </summary>
public static class TAAShader
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
uniform sampler2D tHistory;
uniform sampler2D tVelocity;

uniform vec2 resolution;
uniform float blendFactor;
uniform float sharpness;
uniform bool useMotionVectors;

varying vec2 vUv;

// RGB to YCoCg color space
vec3 RGBToYCoCg(vec3 rgb) {
    float Y = dot(rgb, vec3(0.25, 0.5, 0.25));
    float Co = dot(rgb, vec3(0.5, 0.0, -0.5));
    float Cg = dot(rgb, vec3(-0.25, 0.5, -0.25));
    return vec3(Y, Co, Cg);
}

// YCoCg to RGB color space
vec3 YCoCgToRGB(vec3 ycocg) {
    float Y = ycocg.x;
    float Co = ycocg.y;
    float Cg = ycocg.z;
    float tmp = Y - Cg;
    float r = tmp + Co;
    float g = Y + Cg;
    float b = tmp - Co;
    return vec3(r, g, b);
}

// 3x3 neighborhood clipping (variance clipping)
void ClipHistory(inout vec3 history, vec3 current, vec2 uv) {
    vec3 minColor = current;
    vec3 maxColor = current;
    vec3 m1 = current;
    vec3 m2 = current * current;

    // Sample 3x3 neighborhood
    vec2 pixelSize = 1.0 / resolution;
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            if (x == 0 && y == 0) continue;

            vec2 offset = vec2(float(x), float(y)) * pixelSize;
            vec3 neighbor = texture2D(tColor, uv + offset).rgb;

            minColor = min(minColor, neighbor);
            maxColor = max(maxColor, neighbor);
            m1 += neighbor;
            m2 += neighbor * neighbor;
        }
    }

    // Calculate variance
    m1 /= 9.0;
    m2 /= 9.0;
    vec3 sigma = sqrt(max(vec3(0.0), m2 - m1 * m1));

    // Clip history to neighborhood min/max with variance expansion
    vec3 boxMin = m1 - sigma * 1.5;
    vec3 boxMax = m1 + sigma * 1.5;
    history = clamp(history, boxMin, boxMax);
}

// Sharpen filter
vec3 Sharpen(vec3 color, vec2 uv) {
    vec2 pixelSize = 1.0 / resolution;

    vec3 neighbors = vec3(0.0);
    neighbors += texture2D(tColor, uv + vec2(-1.0, 0.0) * pixelSize).rgb;
    neighbors += texture2D(tColor, uv + vec2(1.0, 0.0) * pixelSize).rgb;
    neighbors += texture2D(tColor, uv + vec2(0.0, -1.0) * pixelSize).rgb;
    neighbors += texture2D(tColor, uv + vec2(0.0, 1.0) * pixelSize).rgb;
    neighbors *= 0.25;

    return color + (color - neighbors) * sharpness;
}

void main() {
    vec3 current = texture2D(tColor, vUv).rgb;

    // Calculate history UV with motion vectors if available
    vec2 historyUV = vUv;
    if (useMotionVectors) {
        vec2 velocity = texture2D(tVelocity, vUv).xy;
        historyUV = vUv - velocity;
    }

    // Check if history UV is valid
    if (historyUV.x < 0.0 || historyUV.x > 1.0 || historyUV.y < 0.0 || historyUV.y > 1.0) {
        // Outside screen space - use current frame
        gl_FragColor = vec4(current, 1.0);
        return;
    }

    // Sample history
    vec3 history = texture2D(tHistory, historyUV).rgb;

    // Clip history to current neighborhood
    ClipHistory(history, current, vUv);

    // Blend current and history
    vec3 result = mix(history, current, blendFactor);

    // Apply sharpening if enabled
    if (sharpness > 0.0) {
        result = Sharpen(result, vUv);
    }

    gl_FragColor = vec4(result, 1.0);
}
";
}
