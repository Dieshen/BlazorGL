namespace BlazorGL.Extensions.PostProcessing.Shaders;

/// <summary>
/// Lookup Table (LUT) color grading shader
/// Applies 3D LUT for color transformation
/// </summary>
public static class LUTShader
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
uniform sampler2D tLUT;  // 3D LUT stored as 2D texture
uniform float intensity;
uniform int lutSize;

varying vec2 vUv;

vec3 ApplyLUT(vec3 color) {
    // Clamp color to valid range
    color = clamp(color, 0.0, 1.0);

    // 3D LUT is typically stored as horizontal slices in a 2D texture
    // For a 16x16x16 LUT, it's stored as 16 horizontal 16x16 slices
    // Total texture size would be 256x16 pixels

    float lutSizeFloat = float(lutSize);
    float scale = (lutSizeFloat - 1.0) / lutSizeFloat;
    float offset = 0.5 / lutSizeFloat;

    // Calculate 3D lookup coordinates
    vec3 lutCoord = color * scale + offset;

    // Calculate slice positions for trilinear interpolation
    float slice = lutCoord.z * lutSizeFloat;
    float sliceIndex = floor(slice);
    float sliceFraction = slice - sliceIndex;

    // Calculate 2D texture coordinates for two adjacent slices
    vec2 slice0UV = vec2(
        (lutCoord.x + sliceIndex) / lutSizeFloat,
        lutCoord.y
    );

    vec2 slice1UV = vec2(
        (lutCoord.x + sliceIndex + 1.0) / lutSizeFloat,
        lutCoord.y
    );

    // Sample both slices
    vec3 color0 = texture2D(tLUT, slice0UV).rgb;
    vec3 color1 = texture2D(tLUT, slice1UV).rgb;

    // Interpolate between slices
    vec3 gradedColor = mix(color0, color1, sliceFraction);

    return gradedColor;
}

void main() {
    vec4 texel = texture2D(tDiffuse, vUv);
    vec3 color = texel.rgb;

    // Apply LUT
    vec3 gradedColor = ApplyLUT(color);

    // Blend with original based on intensity
    vec3 result = mix(color, gradedColor, intensity);

    gl_FragColor = vec4(result, texel.a);
}
";

    // Alternative shader for true 3D texture (WebGL 2.0)
    public const string FragmentShader3D = @"
#version 300 es
precision highp float;

uniform sampler2D tDiffuse;
uniform sampler3D tLUT3D;
uniform float intensity;

in vec2 vUv;
out vec4 FragColor;

void main() {
    vec4 texel = texture(tDiffuse, vUv);
    vec3 color = clamp(texel.rgb, 0.0, 1.0);

    // Sample 3D LUT directly
    vec3 gradedColor = texture(tLUT3D, color).rgb;

    // Blend with original based on intensity
    vec3 result = mix(color, gradedColor, intensity);

    FragColor = vec4(result, texel.a);
}
";
}
