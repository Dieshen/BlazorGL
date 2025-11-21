namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Screen Space Ambient Occlusion shader implementation
/// </summary>
public static class SSAOShader
{
    /// <summary>
    /// Vertex shader for full-screen quad
    /// </summary>
    public const string VertexShader = @"
        attribute vec3 position;
        attribute vec2 uv;

        varying vec2 vUv;

        void main() {
            vUv = uv;
            gl_Position = vec4(position, 1.0);
        }
    ";

    /// <summary>
    /// Fragment shader for SSAO calculation
    /// </summary>
    public const string FragmentShader = @"
        precision highp float;

        uniform sampler2D tDiffuse;
        uniform sampler2D tDepth;
        uniform sampler2D tNoise;
        uniform vec3 kernel[64];
        uniform mat4 projection;
        uniform mat4 projectionInverse;
        uniform vec2 resolution;
        uniform float kernelSize;
        uniform float radius;
        uniform float bias;
        uniform float power;

        varying vec2 vUv;

        const float noiseScale = 4.0;

        // Reconstruct view space position from depth
        vec3 getViewPosition(float depth, vec2 uv) {
            vec4 clipSpacePosition = vec4(uv * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);
            vec4 viewSpacePosition = projectionInverse * clipSpacePosition;
            return viewSpacePosition.xyz / viewSpacePosition.w;
        }

        void main() {
            // Get depth and reconstruct position
            float depth = texture2D(tDepth, vUv).r;

            if (depth >= 1.0) {
                // Sky/background
                gl_FragColor = vec4(1.0);
                return;
            }

            vec3 viewPos = getViewPosition(depth, vUv);

            // Get normal from depth derivatives (simple approximation)
            vec3 dFdxPos = dFdx(viewPos);
            vec3 dFdyPos = dFdy(viewPos);
            vec3 normal = normalize(cross(dFdxPos, dFdyPos));

            // Get noise texture for rotation
            vec2 noiseUv = vUv * resolution / noiseScale;
            vec3 randomVec = texture2D(tNoise, noiseUv).xyz;

            // Create TBN matrix for hemisphere orientation
            vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
            vec3 bitangent = cross(normal, tangent);
            mat3 TBN = mat3(tangent, bitangent, normal);

            // Sample kernel
            float occlusion = 0.0;
            for (int i = 0; i < 64; i++) {
                if (float(i) >= kernelSize) break;

                // Get sample position
                vec3 samplePos = TBN * kernel[i];
                samplePos = viewPos + samplePos * radius;

                // Project to screen space
                vec4 offset = vec4(samplePos, 1.0);
                offset = projection * offset;
                offset.xyz /= offset.w;
                offset.xyz = offset.xyz * 0.5 + 0.5;

                // Get sample depth
                float sampleDepth = texture2D(tDepth, offset.xy).r;
                vec3 sampleViewPos = getViewPosition(sampleDepth, offset.xy);

                // Range check and accumulate
                float rangeCheck = smoothstep(0.0, 1.0, radius / abs(viewPos.z - sampleViewPos.z));
                occlusion += (sampleViewPos.z >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
            }

            occlusion = 1.0 - (occlusion / kernelSize);
            occlusion = pow(occlusion, power);

            gl_FragColor = vec4(vec3(occlusion), 1.0);
        }
    ";

    /// <summary>
    /// Blur shader for SSAO result
    /// </summary>
    public const string BlurFragmentShader = @"
        precision highp float;

        uniform sampler2D tDiffuse;
        uniform vec2 resolution;

        varying vec2 vUv;

        void main() {
            vec2 texelSize = 1.0 / resolution;
            float result = 0.0;

            // Simple box blur
            for (int x = -2; x <= 2; x++) {
                for (int y = -2; y <= 2; y++) {
                    vec2 offset = vec2(float(x), float(y)) * texelSize;
                    result += texture2D(tDiffuse, vUv + offset).r;
                }
            }

            result = result / 25.0;
            gl_FragColor = vec4(vec3(result), 1.0);
        }
    ";

    /// <summary>
    /// Depth rendering shader
    /// </summary>
    public const string DepthVertexShader = @"
        attribute vec3 position;

        uniform mat4 modelViewMatrix;
        uniform mat4 projectionMatrix;

        varying float vDepth;

        void main() {
            vec4 mvPosition = modelViewMatrix * vec4(position, 1.0);
            vDepth = -mvPosition.z;
            gl_Position = projectionMatrix * mvPosition;
        }
    ";

    public const string DepthFragmentShader = @"
        precision highp float;

        uniform float cameraNear;
        uniform float cameraFar;

        varying float vDepth;

        // Linear depth to normalized [0,1]
        float linearizeDepth(float depth) {
            return (depth - cameraNear) / (cameraFar - cameraNear);
        }

        void main() {
            float depth = linearizeDepth(vDepth);
            gl_FragColor = vec4(vec3(depth), 1.0);
        }
    ";
}
