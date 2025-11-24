namespace BlazorGL.Core.Shaders.ShaderChunks;

/// <summary>
/// GLSL shader chunks for advanced shadow mapping techniques
/// Based on Three.js shadowmap implementation with PCF, VSM, and CSM support
/// </summary>
public static class ShadowMapChunks
{
    /// <summary>
    /// Poisson disk samples for PCF filtering
    /// Provides better distribution than regular grid sampling
    /// </summary>
    public const string PoissonDiskSamples = @"
// Poisson disk sampling pattern for PCF
const vec2 poissonDisk16[16] = vec2[](
    vec2(-0.94201624, -0.39906216),
    vec2(0.94558609, -0.76890725),
    vec2(-0.094184101, -0.92938870),
    vec2(0.34495938, 0.29387760),
    vec2(-0.91588581, 0.45771432),
    vec2(-0.81544232, -0.87912464),
    vec2(-0.38277543, 0.27676845),
    vec2(0.97484398, 0.75648379),
    vec2(0.44323325, -0.97511554),
    vec2(0.53742981, -0.47373420),
    vec2(-0.26496911, -0.41893023),
    vec2(0.79197514, 0.19090188),
    vec2(-0.24188840, 0.99706507),
    vec2(-0.81409955, 0.91437590),
    vec2(0.19984126, 0.78641367),
    vec2(0.14383161, -0.14100790)
);

const vec2 poissonDisk25[25] = vec2[](
    vec2(-0.978698, -0.0884121),
    vec2(-0.841121, 0.521165),
    vec2(-0.71746, -0.50322),
    vec2(-0.702933, 0.903134),
    vec2(-0.663198, 0.15482),
    vec2(-0.495102, -0.232887),
    vec2(-0.364238, -0.961791),
    vec2(-0.345866, -0.564379),
    vec2(-0.325663, 0.64037),
    vec2(-0.182714, 0.321329),
    vec2(-0.142613, -0.0227363),
    vec2(-0.0564287, -0.36729),
    vec2(-0.0185858, 0.918882),
    vec2(0.0381787, -0.728996),
    vec2(0.16599, 0.093112),
    vec2(0.253639, 0.719535),
    vec2(0.369549, -0.655019),
    vec2(0.423627, 0.429975),
    vec2(0.530747, -0.364971),
    vec2(0.566027, -0.940489),
    vec2(0.639332, 0.0284127),
    vec2(0.652089, 0.669668),
    vec2(0.773797, 0.345012),
    vec2(0.968871, 0.840449),
    vec2(0.991882, -0.657338)
);

const vec2 poissonDisk64[64] = vec2[](
    vec2(-0.613392, 0.617481),
    vec2(0.170019, -0.040254),
    vec2(-0.299417, 0.791925),
    vec2(0.645680, 0.493210),
    vec2(-0.651784, 0.717887),
    vec2(0.421003, 0.027070),
    vec2(-0.817194, -0.271096),
    vec2(-0.705374, -0.668203),
    vec2(0.977050, -0.108615),
    vec2(0.063326, 0.142369),
    vec2(0.203528, 0.214331),
    vec2(-0.667531, 0.326090),
    vec2(-0.098422, -0.295755),
    vec2(-0.885922, 0.215369),
    vec2(0.566637, 0.605213),
    vec2(0.039766, -0.396100),
    vec2(0.751946, 0.453352),
    vec2(0.078707, -0.715323),
    vec2(-0.075838, -0.529344),
    vec2(0.724479, -0.580798),
    vec2(0.222999, -0.215125),
    vec2(-0.467574, -0.405438),
    vec2(-0.248268, -0.814753),
    vec2(0.354411, -0.887570),
    vec2(0.175817, 0.382366),
    vec2(0.487472, -0.063082),
    vec2(-0.084078, 0.898312),
    vec2(0.488876, -0.783441),
    vec2(0.470016, 0.217933),
    vec2(-0.696890, -0.549791),
    vec2(-0.149693, 0.605762),
    vec2(0.034211, 0.979980),
    vec2(0.503098, -0.308878),
    vec2(-0.016205, -0.872921),
    vec2(0.385784, -0.393902),
    vec2(-0.146886, -0.859249),
    vec2(0.643361, 0.164098),
    vec2(0.634388, -0.049471),
    vec2(-0.688894, 0.007843),
    vec2(0.464034, -0.188818),
    vec2(-0.440840, 0.137486),
    vec2(0.364483, 0.511704),
    vec2(0.034028, 0.325968),
    vec2(0.099094, -0.308023),
    vec2(0.693960, -0.366253),
    vec2(0.678884, -0.204688),
    vec2(0.001801, 0.780328),
    vec2(0.145177, -0.898984),
    vec2(0.062655, -0.611866),
    vec2(0.315226, -0.604297),
    vec2(-0.780145, 0.486251),
    vec2(-0.371868, 0.882138),
    vec2(0.200476, 0.494430),
    vec2(-0.494552, -0.711051),
    vec2(0.612476, 0.705252),
    vec2(-0.578845, -0.768792),
    vec2(-0.772454, -0.090976),
    vec2(0.504440, 0.372295),
    vec2(0.155736, 0.065157),
    vec2(0.391522, 0.849605),
    vec2(-0.620106, -0.328104),
    vec2(0.789239, -0.419965),
    vec2(-0.545396, 0.538133),
    vec2(-0.178564, -0.596057)
);
";

    /// <summary>
    /// Basic shadow map sampling (no filtering)
    /// </summary>
    public const string BasicShadowMap = @"
float getShadowBasic(sampler2D shadowMap, vec4 shadowCoord, float shadowBias) {
    vec3 shadowCoordNDC = shadowCoord.xyz / shadowCoord.w;
    shadowCoordNDC = shadowCoordNDC * 0.5 + 0.5;

    if (shadowCoordNDC.x < 0.0 || shadowCoordNDC.x > 1.0 ||
        shadowCoordNDC.y < 0.0 || shadowCoordNDC.y > 1.0 ||
        shadowCoordNDC.z < 0.0 || shadowCoordNDC.z > 1.0) {
        return 1.0; // Outside shadow map bounds
    }

    float closestDepth = texture(shadowMap, shadowCoordNDC.xy).r;
    float currentDepth = shadowCoordNDC.z;

    return (currentDepth - shadowBias > closestDepth) ? 0.0 : 1.0;
}
";

    /// <summary>
    /// PCF (Percentage Closer Filtering) shadow sampling
    /// Samples shadow map in a pattern around the fragment for soft edges
    /// </summary>
    public const string PCFShadowMap = @"
float getShadowPCF(sampler2D shadowMap, vec4 shadowCoord, float shadowBias, float shadowRadius, vec2 shadowMapSize, int numSamples) {
    vec3 shadowCoordNDC = shadowCoord.xyz / shadowCoord.w;
    shadowCoordNDC = shadowCoordNDC * 0.5 + 0.5;

    if (shadowCoordNDC.x < 0.0 || shadowCoordNDC.x > 1.0 ||
        shadowCoordNDC.y < 0.0 || shadowCoordNDC.y > 1.0 ||
        shadowCoordNDC.z < 0.0 || shadowCoordNDC.z > 1.0) {
        return 1.0;
    }

    float shadow = 0.0;
    vec2 texelSize = 1.0 / shadowMapSize;
    float currentDepth = shadowCoordNDC.z;

    // Use appropriate Poisson disk based on sample count
    if (numSamples <= 16) {
        for (int i = 0; i < numSamples && i < 16; i++) {
            vec2 offset = poissonDisk16[i] * shadowRadius;
            float depth = texture(shadowMap, shadowCoordNDC.xy + offset * texelSize).r;
            shadow += (currentDepth - shadowBias > depth) ? 0.0 : 1.0;
        }
    } else if (numSamples <= 25) {
        for (int i = 0; i < numSamples && i < 25; i++) {
            vec2 offset = poissonDisk25[i] * shadowRadius;
            float depth = texture(shadowMap, shadowCoordNDC.xy + offset * texelSize).r;
            shadow += (currentDepth - shadowBias > depth) ? 0.0 : 1.0;
        }
    } else {
        for (int i = 0; i < numSamples && i < 64; i++) {
            vec2 offset = poissonDisk64[i] * shadowRadius;
            float depth = texture(shadowMap, shadowCoordNDC.xy + offset * texelSize).r;
            shadow += (currentDepth - shadowBias > depth) ? 0.0 : 1.0;
        }
    }

    return shadow / float(numSamples);
}
";

    /// <summary>
    /// PCSS (Percentage Closer Soft Shadows) with variable penumbra
    /// More expensive but produces realistic soft shadows
    /// </summary>
    public const string PCSSShadowMap = @"
// Find average blocker depth for PCSS
float findBlockerDepth(sampler2D shadowMap, vec2 uv, float receiverDepth, float searchRadius, vec2 shadowMapSize) {
    float blockerSum = 0.0;
    int numBlockers = 0;
    vec2 texelSize = 1.0 / shadowMapSize;

    // Use 16 samples for blocker search
    for (int i = 0; i < 16; i++) {
        vec2 offset = poissonDisk16[i] * searchRadius;
        float depth = texture(shadowMap, uv + offset * texelSize).r;
        if (depth < receiverDepth) {
            blockerSum += depth;
            numBlockers++;
        }
    }

    return numBlockers > 0 ? blockerSum / float(numBlockers) : -1.0;
}

// Calculate penumbra size based on blocker distance
float getPenumbraSize(float receiverDepth, float blockerDepth, float lightSize) {
    return lightSize * (receiverDepth - blockerDepth) / blockerDepth;
}

float getShadowPCSS(sampler2D shadowMap, vec4 shadowCoord, float shadowBias, float lightSize, vec2 shadowMapSize) {
    vec3 shadowCoordNDC = shadowCoord.xyz / shadowCoord.w;
    shadowCoordNDC = shadowCoordNDC * 0.5 + 0.5;

    if (shadowCoordNDC.x < 0.0 || shadowCoordNDC.x > 1.0 ||
        shadowCoordNDC.y < 0.0 || shadowCoordNDC.y > 1.0 ||
        shadowCoordNDC.z < 0.0 || shadowCoordNDC.z > 1.0) {
        return 1.0;
    }

    float currentDepth = shadowCoordNDC.z;

    // Step 1: Blocker search
    float searchRadius = lightSize * 0.1; // Search radius based on light size
    float blockerDepth = findBlockerDepth(shadowMap, shadowCoordNDC.xy, currentDepth, searchRadius, shadowMapSize);

    // No blockers found - fully lit
    if (blockerDepth < 0.0) {
        return 1.0;
    }

    // Step 2: Calculate penumbra size
    float penumbra = getPenumbraSize(currentDepth, blockerDepth, lightSize);
    penumbra = clamp(penumbra, 0.0, 5.0); // Clamp for stability

    // Step 3: PCF with variable kernel size
    float shadow = 0.0;
    vec2 texelSize = 1.0 / shadowMapSize;
    int numSamples = 25; // Use 25 samples for final filtering

    for (int i = 0; i < numSamples; i++) {
        vec2 offset = poissonDisk25[i] * penumbra;
        float depth = texture(shadowMap, shadowCoordNDC.xy + offset * texelSize).r;
        shadow += (currentDepth - shadowBias > depth) ? 0.0 : 1.0;
    }

    return shadow / float(numSamples);
}
";

    /// <summary>
    /// VSM (Variance Shadow Maps) shadow sampling
    /// Uses statistical filtering for smooth shadows without sampling
    /// </summary>
    public const string VSMShadowMap = @"
float linstep(float low, float high, float v) {
    return clamp((v - low) / (high - low), 0.0, 1.0);
}

float getShadowVSM(sampler2D shadowMap, vec4 shadowCoord, float minVariance, float lightBleedingReduction) {
    vec3 shadowCoordNDC = shadowCoord.xyz / shadowCoord.w;
    shadowCoordNDC = shadowCoordNDC * 0.5 + 0.5;

    if (shadowCoordNDC.x < 0.0 || shadowCoordNDC.x > 1.0 ||
        shadowCoordNDC.y < 0.0 || shadowCoordNDC.y > 1.0 ||
        shadowCoordNDC.z < 0.0 || shadowCoordNDC.z > 1.0) {
        return 1.0;
    }

    // Sample moments from shadow map (RG channels contain depth and depth^2)
    vec2 moments = texture(shadowMap, shadowCoordNDC.xy).rg;
    float depth = shadowCoordNDC.z;

    // Standard shadow test
    if (depth <= moments.x) {
        return 1.0;
    }

    // Variance shadow computation
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, minVariance); // Prevent division by zero

    float d = depth - moments.x;
    float pMax = variance / (variance + d * d);

    // Light bleeding reduction
    pMax = linstep(lightBleedingReduction, 1.0, pMax);

    return pMax;
}
";

    /// <summary>
    /// CSM (Cascaded Shadow Maps) cascade selection
    /// </summary>
    public const string CSMShadowMap = @"
int selectCascade(float viewZ, float cascadeSplits[4], int numCascades) {
    float absViewZ = abs(viewZ);
    for (int i = 0; i < numCascades - 1; i++) {
        if (absViewZ < cascadeSplits[i]) {
            return i;
        }
    }
    return numCascades - 1;
}

float getShadowCSM(
    sampler2D cascadeShadowMaps[4],
    mat4 cascadeMatrices[4],
    float cascadeSplits[4],
    int numCascades,
    vec3 worldPosition,
    float viewZ,
    float shadowBias,
    float shadowRadius,
    vec2 shadowMapSize,
    int numSamples
) {
    // Select appropriate cascade
    int cascadeIndex = selectCascade(viewZ, cascadeSplits, numCascades);

    // Project into cascade shadow space
    vec4 shadowCoord = cascadeMatrices[cascadeIndex] * vec4(worldPosition, 1.0);

    // Use PCF sampling for this cascade
    return getShadowPCF(cascadeShadowMaps[cascadeIndex], shadowCoord, shadowBias, shadowRadius, shadowMapSize, numSamples);
}

// CSM with cascade blending for smooth transitions
float getShadowCSMBlended(
    sampler2D cascadeShadowMaps[4],
    mat4 cascadeMatrices[4],
    float cascadeSplits[4],
    int numCascades,
    vec3 worldPosition,
    float viewZ,
    float shadowBias,
    float shadowRadius,
    vec2 shadowMapSize,
    int numSamples
) {
    int cascadeIndex = selectCascade(viewZ, cascadeSplits, numCascades);
    float absViewZ = abs(viewZ);

    // Sample current cascade
    vec4 shadowCoord = cascadeMatrices[cascadeIndex] * vec4(worldPosition, 1.0);
    float shadow = getShadowPCF(cascadeShadowMaps[cascadeIndex], shadowCoord, shadowBias, shadowRadius, shadowMapSize, numSamples);

    // Blend with next cascade near transition
    if (cascadeIndex < numCascades - 1) {
        float cascadeEnd = cascadeSplits[cascadeIndex];
        float blendStart = cascadeEnd * 0.9;
        float blendFactor = smoothstep(blendStart, cascadeEnd, absViewZ);

        if (blendFactor > 0.0) {
            vec4 nextShadowCoord = cascadeMatrices[cascadeIndex + 1] * vec4(worldPosition, 1.0);
            float nextShadow = getShadowPCF(cascadeShadowMaps[cascadeIndex + 1], nextShadowCoord, shadowBias, shadowRadius, shadowMapSize, numSamples);
            shadow = mix(shadow, nextShadow, blendFactor);
        }
    }

    return shadow;
}
";

    /// <summary>
    /// Depth packing utilities for VSM
    /// </summary>
    public const string VSMDepthPacking = @"
// Pack depth for VSM (stores depth and depth^2)
vec2 packDepthToVSM(float depth) {
    float moment1 = depth;
    float moment2 = depth * depth;

    // Add small bias based on depth derivatives to reduce precision issues
    float dx = dFdx(depth);
    float dy = dFdy(depth);
    moment2 += 0.25 * (dx * dx + dy * dy);

    return vec2(moment1, moment2);
}
";

    /// <summary>
    /// Complete shadow map fragment shader functions
    /// Includes all techniques in one comprehensive shader
    /// </summary>
    public const string CompleteShadowFunctions = PoissonDiskSamples + BasicShadowMap + PCFShadowMap + PCSSShadowMap + VSMShadowMap + CSMShadowMap;

    /// <summary>
    /// Vertex shader for depth-only rendering (shadow map generation)
    /// </summary>
    public const string DepthVertexShader = @"#version 300 es
precision highp float;

in vec3 position;

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

void main() {
    gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(position, 1.0);
}
";

    /// <summary>
    /// Fragment shader for basic depth rendering
    /// </summary>
    public const string DepthFragmentShader = @"#version 300 es
precision highp float;

out vec4 fragColor;

void main() {
    // Depth is automatically written to depth buffer
    // For standard shadow maps, we don't need to output anything
    fragColor = vec4(1.0);
}
";

    /// <summary>
    /// Fragment shader for VSM depth rendering (outputs depth and depth^2)
    /// </summary>
    public const string VSMDepthFragmentShader = @"#version 300 es
precision highp float;

out vec4 fragColor;

" + VSMDepthPacking + @"

void main() {
    float depth = gl_FragCoord.z;
    vec2 moments = packDepthToVSM(depth);
    fragColor = vec4(moments.x, moments.y, 0.0, 1.0);
}
";
}
