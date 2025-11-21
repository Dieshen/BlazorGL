namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Color correction shader for post-processing
/// Supports brightness, contrast, saturation, hue shift
/// </summary>
public static class ColorCorrectionShader
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
        uniform float brightness;
        uniform float contrast;
        uniform float saturation;
        uniform float hue;
        uniform float exposure;
        uniform float gamma;

        varying vec2 vUv;

        // RGB to HSL conversion
        vec3 rgb2hsl(vec3 color) {
            float maxC = max(max(color.r, color.g), color.b);
            float minC = min(min(color.r, color.g), color.b);
            float delta = maxC - minC;

            float h = 0.0;
            float s = 0.0;
            float l = (maxC + minC) / 2.0;

            if (delta > 0.0) {
                s = l < 0.5 ? delta / (maxC + minC) : delta / (2.0 - maxC - minC);

                if (color.r == maxC) {
                    h = (color.g - color.b) / delta + (color.g < color.b ? 6.0 : 0.0);
                } else if (color.g == maxC) {
                    h = (color.b - color.r) / delta + 2.0;
                } else {
                    h = (color.r - color.g) / delta + 4.0;
                }
                h /= 6.0;
            }

            return vec3(h, s, l);
        }

        // HSL to RGB conversion
        vec3 hsl2rgb(vec3 hsl) {
            float h = hsl.x;
            float s = hsl.y;
            float l = hsl.z;

            float c = (1.0 - abs(2.0 * l - 1.0)) * s;
            float x = c * (1.0 - abs(mod(h * 6.0, 2.0) - 1.0));
            float m = l - c / 2.0;

            vec3 rgb;
            if (h < 1.0 / 6.0) {
                rgb = vec3(c, x, 0.0);
            } else if (h < 2.0 / 6.0) {
                rgb = vec3(x, c, 0.0);
            } else if (h < 3.0 / 6.0) {
                rgb = vec3(0.0, c, x);
            } else if (h < 4.0 / 6.0) {
                rgb = vec3(0.0, x, c);
            } else if (h < 5.0 / 6.0) {
                rgb = vec3(x, 0.0, c);
            } else {
                rgb = vec3(c, 0.0, x);
            }

            return rgb + m;
        }

        void main() {
            vec4 color = texture2D(tDiffuse, vUv);

            // Apply exposure
            color.rgb *= exposure;

            // Apply brightness
            color.rgb += brightness;

            // Apply contrast
            color.rgb = (color.rgb - 0.5) * contrast + 0.5;

            // Convert to HSL
            vec3 hsl = rgb2hsl(color.rgb);

            // Apply hue shift
            hsl.x = mod(hsl.x + hue, 1.0);

            // Apply saturation
            hsl.y *= saturation;

            // Convert back to RGB
            color.rgb = hsl2rgb(hsl);

            // Apply gamma correction
            color.rgb = pow(color.rgb, vec3(1.0 / gamma));

            // Clamp
            color.rgb = clamp(color.rgb, 0.0, 1.0);

            gl_FragColor = color;
        }
    ";
}
