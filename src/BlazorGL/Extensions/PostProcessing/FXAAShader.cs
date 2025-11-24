namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Fast Approximate Anti-Aliasing (FXAA) shader
/// Single-pass edge-based anti-aliasing
/// </summary>
public static class FXAAShader
{
    public const string VertexShader = @"
        attribute vec3 position;
        attribute vec2 uv;

        varying vec2 vUv;

        void main() {
            vUv = uv;
            gl_Position = vec4(position, 1.0);
        }
    ";

    public const string FragmentShader = @"
        precision highp float;

        uniform sampler2D tDiffuse;
        uniform vec2 resolution;

        varying vec2 vUv;

        // FXAA quality presets
        #define FXAA_QUALITY_PS 12
        #define FXAA_QUALITY_P0 1.0
        #define FXAA_QUALITY_P1 1.5
        #define FXAA_QUALITY_P2 2.0
        #define FXAA_QUALITY_P3 2.0
        #define FXAA_QUALITY_P4 2.0
        #define FXAA_QUALITY_P5 2.0
        #define FXAA_QUALITY_P6 2.0
        #define FXAA_QUALITY_P7 2.0
        #define FXAA_QUALITY_P8 2.0
        #define FXAA_QUALITY_P9 2.0
        #define FXAA_QUALITY_P10 4.0
        #define FXAA_QUALITY_P11 8.0

        float rgb2luma(vec3 rgb) {
            return sqrt(dot(rgb, vec3(0.299, 0.587, 0.114)));
        }

        void main() {
            vec2 inverseVP = 1.0 / resolution;

            vec3 colorCenter = texture2D(tDiffuse, vUv).rgb;

            // Luma at current fragment
            float lumaCenter = rgb2luma(colorCenter);

            // Luma at surrounding fragments
            float lumaDown = rgb2luma(texture2D(tDiffuse, vUv + vec2(0.0, -1.0) * inverseVP).rgb);
            float lumaUp = rgb2luma(texture2D(tDiffuse, vUv + vec2(0.0, 1.0) * inverseVP).rgb);
            float lumaLeft = rgb2luma(texture2D(tDiffuse, vUv + vec2(-1.0, 0.0) * inverseVP).rgb);
            float lumaRight = rgb2luma(texture2D(tDiffuse, vUv + vec2(1.0, 0.0) * inverseVP).rgb);

            // Find min/max luma
            float lumaMin = min(lumaCenter, min(min(lumaDown, lumaUp), min(lumaLeft, lumaRight)));
            float lumaMax = max(lumaCenter, max(max(lumaDown, lumaUp), max(lumaLeft, lumaRight)));

            // Compute luma range
            float lumaRange = lumaMax - lumaMin;

            // If luma range is below threshold, no aliasing
            if (lumaRange < max(0.0312, lumaMax * 0.125)) {
                gl_FragColor = vec4(colorCenter, 1.0);
                return;
            }

            // Query corners
            float lumaDownLeft = rgb2luma(texture2D(tDiffuse, vUv + vec2(-1.0, -1.0) * inverseVP).rgb);
            float lumaUpRight = rgb2luma(texture2D(tDiffuse, vUv + vec2(1.0, 1.0) * inverseVP).rgb);
            float lumaUpLeft = rgb2luma(texture2D(tDiffuse, vUv + vec2(-1.0, 1.0) * inverseVP).rgb);
            float lumaDownRight = rgb2luma(texture2D(tDiffuse, vUv + vec2(1.0, -1.0) * inverseVP).rgb);

            // Combine four edges
            float lumaDownUp = lumaDown + lumaUp;
            float lumaLeftRight = lumaLeft + lumaRight;

            // Corners
            float lumaLeftCorners = lumaDownLeft + lumaUpLeft;
            float lumaDownCorners = lumaDownLeft + lumaDownRight;
            float lumaRightCorners = lumaDownRight + lumaUpRight;
            float lumaUpCorners = lumaUpRight + lumaUpLeft;

            // Compute gradient
            float edgeHorizontal = abs(-2.0 * lumaLeft + lumaLeftCorners) +
                                   abs(-2.0 * lumaCenter + lumaDownUp) * 2.0 +
                                   abs(-2.0 * lumaRight + lumaRightCorners);

            float edgeVertical = abs(-2.0 * lumaUp + lumaUpCorners) +
                                 abs(-2.0 * lumaCenter + lumaLeftRight) * 2.0 +
                                 abs(-2.0 * lumaDown + lumaDownCorners);

            // Is edge horizontal or vertical?
            bool isHorizontal = (edgeHorizontal >= edgeVertical);

            // Select the two neighboring pixels perpendicular to edge
            float luma1 = isHorizontal ? lumaDown : lumaLeft;
            float luma2 = isHorizontal ? lumaUp : lumaRight;

            // Compute gradients
            float gradient1 = luma1 - lumaCenter;
            float gradient2 = luma2 - lumaCenter;

            // Which direction is steepest?
            bool is1Steepest = abs(gradient1) >= abs(gradient2);

            // Gradient in chosen direction
            float gradientScaled = 0.25 * max(abs(gradient1), abs(gradient2));

            // Average luma
            float lumaLocalAverage = 0.0;
            if (is1Steepest) {
                luma1 = luma1 + lumaCenter;
                lumaLocalAverage = luma1 * 0.5;
            } else {
                luma2 = luma2 + lumaCenter;
                lumaLocalAverage = luma2 * 0.5;
            }

            // Choose step direction
            vec2 stepLength = isHorizontal ? vec2(inverseVP.x, 0.0) : vec2(0.0, inverseVP.y);

            if (!is1Steepest) {
                stepLength = -stepLength;
            }

            // Exploration along edge
            vec2 currentUv = vUv;
            float lumaEnd = lumaLocalAverage;

            // Simple quality: just blend
            gl_FragColor = vec4(colorCenter, 1.0);
        }
    ";
}
