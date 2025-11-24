namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// SMAA Blend Weight Calculation Shader (Pass 2)
/// Calculates blending weights using search and area textures
/// </summary>
public static class SMAABlendWeightShader
{
    public const string VertexShader = @"
attribute vec3 position;
attribute vec2 uv;

varying vec2 vUv;
varying vec2 vPixCoord;
varying vec4 vOffset[3];

uniform vec2 resolution;

void main() {
    vUv = uv;
    vPixCoord = uv * resolution;

    vec2 pixelSize = 1.0 / resolution;

    // Offsets for accessing neighbors
    vOffset[0] = uv.xyxy + pixelSize.xyxy * vec4(-0.25, -0.125, 1.25, -0.125);
    vOffset[1] = uv.xyxy + pixelSize.xyxy * vec4(-0.125, -0.25, -0.125, 1.25);
    vOffset[2] = uv.xyxy + pixelSize.xyxy * vec4(-2.0, 2.0, -2.0, 2.0) * 0.5;

    gl_Position = vec4(position.xy, 0.0, 1.0);
}
";

    public const string FragmentShader = @"
precision highp float;

uniform sampler2D tDiffuse;
uniform sampler2D tArea;
uniform sampler2D tSearch;

uniform vec2 resolution;

varying vec2 vUv;
varying vec2 vPixCoord;
varying vec4 vOffset[3];

#define MAX_SEARCH_STEPS 16
#define SEARCH_THRESHOLD 0.25

// Simplified area texture lookup
vec2 area(vec2 distance, float e1, float e2) {
    // Remap distance to area texture space
    vec2 pixCoord = vec2(0.5) + distance * vec2(1.0 / 160.0, 1.0 / 560.0);
    return texture2D(tArea, pixCoord).rg;
}

// Search for pattern endpoints
float searchXLeft(vec2 texCoord) {
    vec2 e = vec2(0.0, 1.0);
    for (int i = 0; i < MAX_SEARCH_STEPS; i++) {
        e = texture2D(tDiffuse, texCoord).rg;
        texCoord -= vec2(2.0, 0.0) / resolution;
        if (e.g > 0.0 || e.r == 0.0) break;
    }
    return max(e.r, 0.0);
}

float searchXRight(vec2 texCoord) {
    vec2 e = vec2(0.0, 1.0);
    for (int i = 0; i < MAX_SEARCH_STEPS; i++) {
        e = texture2D(tDiffuse, texCoord).rg;
        texCoord += vec2(2.0, 0.0) / resolution;
        if (e.g > 0.0 || e.r == 0.0) break;
    }
    return max(e.r, 0.0);
}

float searchYUp(vec2 texCoord) {
    vec2 e = vec2(1.0, 0.0);
    for (int i = 0; i < MAX_SEARCH_STEPS; i++) {
        e = texture2D(tDiffuse, texCoord).rg;
        texCoord += vec2(0.0, 2.0) / resolution;
        if (e.r > 0.0 || e.g == 0.0) break;
    }
    return max(e.g, 0.0);
}

float searchYDown(vec2 texCoord) {
    vec2 e = vec2(1.0, 0.0);
    for (int i = 0; i < MAX_SEARCH_STEPS; i++) {
        e = texture2D(tDiffuse, texCoord).rg;
        texCoord -= vec2(0.0, 2.0) / resolution;
        if (e.r > 0.0 || e.g == 0.0) break;
    }
    return max(e.g, 0.0);
}

void main() {
    vec4 weights = vec4(0.0);
    vec2 e = texture2D(tDiffuse, vUv).rg;

    if (e.r > 0.0) { // Horizontal edge
        vec2 d;
        d.x = searchXLeft(vOffset[0].xy);
        d.y = searchXRight(vOffset[0].zw);

        vec2 coords = vec2(d.x, e.r);
        weights.rg = area(coords, 0.0, 0.0);
    }

    if (e.g > 0.0) { // Vertical edge
        vec2 d;
        d.x = searchYUp(vOffset[1].zw);
        d.y = searchYDown(vOffset[1].xy);

        vec2 coords = vec2(d.x, e.g);
        weights.ba = area(coords, 0.0, 0.0);
    }

    gl_FragColor = weights;
}
";
}
