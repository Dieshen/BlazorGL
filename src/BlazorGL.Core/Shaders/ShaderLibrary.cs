namespace BlazorGL.Core.Shaders;

/// <summary>
/// Library of built-in shaders
/// </summary>
public static class ShaderLibrary
{
    public static class Basic
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;

void main() {
    vUv = uv;
    vNormal = mat3(normalMatrix) * normal;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;
uniform sampler2D map;
uniform bool useMap;

in vec2 vUv;
in vec3 vNormal;

out vec4 fragColor;

void main() {
    vec4 baseColor = vec4(color, opacity);

    if (useMap) {
        vec4 texColor = texture(map, vUv);
        baseColor *= texColor;
    }

    fragColor = baseColor;
}";
    }

    public static class Phong
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;
out vec3 vPosition;
out vec3 vWorldPosition;

void main() {
    vUv = uv;
    vNormal = mat3(normalMatrix) * normal;
    vPosition = (modelViewMatrix * vec4(position, 1.0)).xyz;
    vWorldPosition = (modelMatrix * vec4(position, 1.0)).xyz;

    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float distance;
    float decay;
};

struct SpotLight {
    vec3 position;
    vec3 direction;
    vec3 color;
    float intensity;
    float distance;
    float angle;
    float penumbra;
    float decay;
};

uniform vec3 color;
uniform vec3 specular;
uniform float shininess;
uniform float opacity;

uniform sampler2D map;
uniform bool useMap;
uniform sampler2D normalMap;
uniform bool useNormalMap;
uniform sampler2D specularMap;
uniform bool useSpecularMap;

uniform vec3 ambientLightColor;
uniform DirectionalLight directionalLights[4];
uniform int numDirectionalLights;
uniform PointLight pointLights[4];
uniform int numPointLights;
uniform SpotLight spotLights[4];
uniform int numSpotLights;

uniform vec3 cameraPosition;

in vec2 vUv;
in vec3 vNormal;
in vec3 vPosition;
in vec3 vWorldPosition;

out vec4 fragColor;

void main() {
    vec3 normal = normalize(vNormal);

    // Base color
    vec4 baseColor = vec4(color, opacity);
    if (useMap) {
        baseColor *= texture(map, vUv);
    }

    // Ambient
    vec3 ambient = ambientLightColor * baseColor.rgb;

    // Lighting accumulation
    vec3 diffuse = vec3(0.0);
    vec3 specularLight = vec3(0.0);

    vec3 viewDir = normalize(cameraPosition - vWorldPosition);

    // Directional lights
    for (int i = 0; i < numDirectionalLights; i++) {
        vec3 lightDir = normalize(-directionalLights[i].direction);
        float diff = max(dot(normal, lightDir), 0.0);
        diffuse += directionalLights[i].color * directionalLights[i].intensity * diff;

        // Specular
        vec3 reflectDir = reflect(-lightDir, normal);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
        specularLight += directionalLights[i].color * directionalLights[i].intensity * spec * specular;
    }

    // Point lights
    for (int i = 0; i < numPointLights; i++) {
        vec3 lightDir = normalize(pointLights[i].position - vWorldPosition);
        float distance = length(pointLights[i].position - vWorldPosition);

        float attenuation = 1.0;
        if (pointLights[i].distance > 0.0) {
            attenuation = pow(clamp(1.0 - (distance / pointLights[i].distance), 0.0, 1.0), pointLights[i].decay);
        }

        float diff = max(dot(normal, lightDir), 0.0);
        diffuse += pointLights[i].color * pointLights[i].intensity * diff * attenuation;

        // Specular
        vec3 reflectDir = reflect(-lightDir, normal);
        float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
        specularLight += pointLights[i].color * pointLights[i].intensity * spec * specular * attenuation;
    }

    vec3 finalColor = ambient + (diffuse * baseColor.rgb) + specularLight;
    fragColor = vec4(finalColor, baseColor.a);
}";
    }

    public static class Standard
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;
out vec3 vPosition;
out vec3 vWorldPosition;

void main() {
    vUv = uv;
    vNormal = mat3(normalMatrix) * normal;
    vPosition = (modelViewMatrix * vec4(position, 1.0)).xyz;
    vWorldPosition = (modelMatrix * vec4(position, 1.0)).xyz;

    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

const float PI = 3.14159265359;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float distance;
    float decay;
};

uniform vec3 color;
uniform float metalness;
uniform float roughness;
uniform float opacity;

uniform sampler2D map;
uniform bool useMap;
uniform sampler2D metalnessMap;
uniform bool useMetalnessMap;
uniform sampler2D roughnessMap;
uniform bool useRoughnessMap;
uniform sampler2D normalMap;
uniform bool useNormalMap;
uniform vec3 emissive;
uniform float emissiveIntensity;

uniform vec3 ambientLightColor;
uniform DirectionalLight directionalLights[4];
uniform int numDirectionalLights;
uniform PointLight pointLights[4];
uniform int numPointLights;

uniform vec3 cameraPosition;

in vec2 vUv;
in vec3 vNormal;
in vec3 vPosition;
in vec3 vWorldPosition;

out vec4 fragColor;

// PBR functions
float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;

    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness) {
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;

    return nom / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main() {
    vec3 normal = normalize(vNormal);

    // Base color
    vec3 albedo = color;
    if (useMap) {
        albedo *= texture(map, vUv).rgb;
    }

    // Material properties
    float metallic = metalness;
    if (useMetalnessMap) {
        metallic *= texture(metalnessMap, vUv).b;
    }

    float roughnessValue = roughness;
    if (useRoughnessMap) {
        roughnessValue *= texture(roughnessMap, vUv).g;
    }

    vec3 N = normal;
    vec3 V = normalize(cameraPosition - vWorldPosition);

    // Fresnel reflectance at normal incidence
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    // Reflectance equation
    vec3 Lo = vec3(0.0);

    // Directional lights
    for (int i = 0; i < numDirectionalLights; i++) {
        vec3 L = normalize(-directionalLights[i].direction);
        vec3 H = normalize(V + L);
        vec3 radiance = directionalLights[i].color * directionalLights[i].intensity;

        // Cook-Torrance BRDF
        float NDF = DistributionGGX(N, H, roughnessValue);
        float G = GeometrySmith(N, V, L, roughnessValue);
        vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

        vec3 numerator = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        vec3 specular = numerator / denominator;

        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;

        float NdotL = max(dot(N, L), 0.0);
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }

    // Point lights
    for (int i = 0; i < numPointLights; i++) {
        vec3 L = normalize(pointLights[i].position - vWorldPosition);
        vec3 H = normalize(V + L);
        float distance = length(pointLights[i].position - vWorldPosition);

        float attenuation = 1.0;
        if (pointLights[i].distance > 0.0) {
            attenuation = pow(clamp(1.0 - (distance / pointLights[i].distance), 0.0, 1.0), pointLights[i].decay);
        }

        vec3 radiance = pointLights[i].color * pointLights[i].intensity * attenuation;

        // Cook-Torrance BRDF
        float NDF = DistributionGGX(N, H, roughnessValue);
        float G = GeometrySmith(N, V, L, roughnessValue);
        vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

        vec3 numerator = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        vec3 specular = numerator / denominator;

        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;

        float NdotL = max(dot(N, L), 0.0);
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }

    // Ambient
    vec3 ambient = ambientLightColor * albedo * 0.3;

    vec3 finalColor = ambient + Lo;

    // Emissive
    if (emissiveIntensity > 0.0) {
        finalColor += emissive * emissiveIntensity;
    }

    // HDR tonemapping
    finalColor = finalColor / (finalColor + vec3(1.0));

    // Gamma correction
    finalColor = pow(finalColor, vec3(1.0/2.2));

    fragColor = vec4(finalColor, opacity);
}";
    }

    public static class LineBasic
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 color;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform bool useVertexColors;

out vec3 vColor;

void main() {
    vColor = color;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;
uniform bool useVertexColors;

in vec3 vColor;

out vec4 fragColor;

void main() {
    vec3 finalColor = useVertexColors ? vColor : color;
    fragColor = vec4(finalColor, opacity);
}";
    }

    public static class LineDashed
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 color;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform bool useVertexColors;
uniform float scale;

out vec3 vColor;
out float vLineDistance;

void main() {
    vColor = color;

    // Calculate line distance for dashing
    // This is a simplified approach - in real three.js this is computed per-segment
    vLineDistance = position.x * scale;

    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;
uniform bool useVertexColors;
uniform float dashSize;
uniform float gapSize;

in vec3 vColor;
in float vLineDistance;

out vec4 fragColor;

void main() {
    // Dash pattern logic
    float totalSize = dashSize + gapSize;
    float modulo = mod(vLineDistance, totalSize);

    if (modulo > dashSize) {
        discard;
    }

    vec3 finalColor = useVertexColors ? vColor : color;
    fragColor = vec4(finalColor, opacity);
}";
    }

    public static class Points
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 color;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform float size;
uniform bool sizeAttenuation;
uniform bool useVertexColors;

out vec3 vColor;

void main() {
    vColor = color;

    vec4 mvPosition = modelViewMatrix * vec4(position, 1.0);
    gl_Position = projectionMatrix * mvPosition;

    // Point size with optional distance attenuation
    if (sizeAttenuation) {
        gl_PointSize = size * (300.0 / -mvPosition.z);
    } else {
        gl_PointSize = size;
    }
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;
uniform sampler2D map;
uniform bool useMap;
uniform bool useVertexColors;

in vec3 vColor;

out vec4 fragColor;

void main() {
    vec3 finalColor = useVertexColors ? vColor : color;

    // Circular point shape
    vec2 coord = gl_PointCoord - vec2(0.5);
    if (length(coord) > 0.5) {
        discard;
    }

    vec4 baseColor = vec4(finalColor, opacity);

    if (useMap) {
        vec4 texColor = texture(map, gl_PointCoord);
        baseColor *= texColor;
    }

    fragColor = baseColor;
}";
    }

    public static class Normal
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec3 vNormal;

void main() {
    vNormal = normalize(mat3(normalMatrix) * normal);
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform float opacity;

in vec3 vNormal;

out vec4 fragColor;

void main() {
    // Convert normal from [-1,1] to [0,1] for RGB display
    vec3 color = normalize(vNormal) * 0.5 + 0.5;
    fragColor = vec4(color, opacity);
}";
    }

    public static class Depth
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;

out vec4 vViewPosition;

void main() {
    vec4 mvPosition = modelViewMatrix * vec4(position, 1.0);
    vViewPosition = mvPosition;
    gl_Position = projectionMatrix * mvPosition;
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform float opacity;
uniform float near;
uniform float far;

in vec4 vViewPosition;

out vec4 fragColor;

void main() {
    // Linear depth
    float depth = length(vViewPosition.xyz);
    float normalizedDepth = (depth - near) / (far - near);
    normalizedDepth = clamp(normalizedDepth, 0.0, 1.0);

    vec3 color = vec3(normalizedDepth);
    fragColor = vec4(color, opacity);
}";
    }

    public static class Distance
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;

uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;

out vec3 vWorldPosition;

void main() {
    vec4 worldPosition = modelMatrix * vec4(position, 1.0);
    vWorldPosition = worldPosition.xyz;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform float opacity;
uniform vec3 referencePosition;
uniform float near;
uniform float far;

in vec3 vWorldPosition;

out vec4 fragColor;

void main() {
    float dist = length(vWorldPosition - referencePosition);
    float normalizedDist = (dist - near) / (far - near);
    normalizedDist = clamp(normalizedDist, 0.0, 1.0);

    vec3 color = vec3(normalizedDist);
    fragColor = vec4(color, opacity);
}";
    }

    public static class Lambert
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;
out vec3 vWorldPosition;

void main() {
    vUv = uv;
    vNormal = mat3(normalMatrix) * normal;
    vWorldPosition = (modelMatrix * vec4(position, 1.0)).xyz;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float distance;
    float decay;
};

uniform vec3 color;
uniform float opacity;
uniform vec3 emissive;
uniform float emissiveIntensity;
uniform sampler2D map;
uniform sampler2D emissiveMap;
uniform bool useMap;
uniform bool useEmissiveMap;

uniform vec3 ambientLightColor;
uniform DirectionalLight directionalLights[4];
uniform PointLight pointLights[4];
uniform int numDirectionalLights;
uniform int numPointLights;

in vec2 vUv;
in vec3 vNormal;
in vec3 vWorldPosition;

out vec4 fragColor;

void main() {
    vec4 baseColor = vec4(color, opacity);

    if (useMap) {
        baseColor *= texture(map, vUv);
    }

    vec3 normal = normalize(vNormal);
    vec3 diffuse = ambientLightColor;

    // Directional lights (Lambertian)
    for (int i = 0; i < numDirectionalLights && i < 4; i++) {
        float diff = max(dot(normal, directionalLights[i].direction), 0.0);
        diffuse += directionalLights[i].color * directionalLights[i].intensity * diff;
    }

    // Point lights (Lambertian with attenuation)
    for (int i = 0; i < numPointLights && i < 4; i++) {
        vec3 lightDir = normalize(pointLights[i].position - vWorldPosition);
        float diff = max(dot(normal, lightDir), 0.0);

        float distance = length(pointLights[i].position - vWorldPosition);
        float attenuation = 1.0 / (1.0 + pointLights[i].decay * distance);

        diffuse += pointLights[i].color * pointLights[i].intensity * diff * attenuation;
    }

    vec3 finalColor = baseColor.rgb * diffuse;

    // Emissive
    vec3 emissiveColor = emissive * emissiveIntensity;
    if (useEmissiveMap) {
        emissiveColor *= texture(emissiveMap, vUv).rgb;
    }
    finalColor += emissiveColor;

    fragColor = vec4(finalColor, baseColor.a);
}";
    }

    public static class Toon
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;
out vec3 vWorldPosition;

void main() {
    vUv = uv;
    vNormal = mat3(normalMatrix) * normal;
    vWorldPosition = (modelMatrix * vec4(position, 1.0)).xyz;
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

uniform vec3 color;
uniform float opacity;
uniform vec3 emissive;
uniform float emissiveIntensity;
uniform sampler2D map;
uniform sampler2D gradientMap;
uniform bool useMap;
uniform bool useGradientMap;

uniform vec3 ambientLightColor;
uniform DirectionalLight directionalLights[4];
uniform int numDirectionalLights;

in vec2 vUv;
in vec3 vNormal;
in vec3 vWorldPosition;

out vec4 fragColor;

void main() {
    vec4 baseColor = vec4(color, opacity);

    if (useMap) {
        baseColor *= texture(map, vUv);
    }

    vec3 normal = normalize(vNormal);
    float totalIntensity = 0.0;

    // Calculate lighting intensity
    for (int i = 0; i < numDirectionalLights && i < 4; i++) {
        float intensity = max(dot(normal, directionalLights[i].direction), 0.0);
        totalIntensity += intensity * directionalLights[i].intensity;
    }

    totalIntensity = clamp(totalIntensity, 0.0, 1.0);

    // Apply gradient map for toon steps
    vec3 toonColor;
    if (useGradientMap) {
        toonColor = texture(gradientMap, vec2(totalIntensity, 0.5)).rgb;
    } else {
        // Default 3-step toon shading
        if (totalIntensity > 0.95) {
            toonColor = vec3(1.0);
        } else if (totalIntensity > 0.5) {
            toonColor = vec3(0.7);
        } else if (totalIntensity > 0.25) {
            toonColor = vec3(0.4);
        } else {
            toonColor = vec3(0.2);
        }
    }

    vec3 finalColor = baseColor.rgb * toonColor;
    finalColor += emissive * emissiveIntensity;

    fragColor = vec4(finalColor, baseColor.a);
}";
    }

    public static class Matcap
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;

void main() {
    vUv = uv;
    vNormal = normalize(mat3(normalMatrix) * normal);
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;
uniform sampler2D matcap;
uniform sampler2D map;
uniform bool useMatcap;
uniform bool useMap;

in vec2 vUv;
in vec3 vNormal;

out vec4 fragColor;

void main() {
    vec4 baseColor = vec4(color, opacity);

    if (useMap) {
        baseColor *= texture(map, vUv);
    }

    vec3 finalColor = baseColor.rgb;

    if (useMatcap) {
        // Convert view-space normal to UV coordinates
        vec3 viewNormal = normalize(vNormal);
        vec2 matcapUV = viewNormal.xy * 0.5 + 0.5;
        vec3 matcapColor = texture(matcap, matcapUV).rgb;
        finalColor *= matcapColor;
    }

    fragColor = vec4(finalColor, baseColor.a);
}";
    }

    public static class Physical
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec3 normal;
in vec2 uv;

uniform mat4 modelMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform mat4 normalMatrix;

out vec2 vUv;
out vec3 vNormal;
out vec3 vWorldPosition;
out vec3 vViewPosition;

void main() {
    vUv = uv;
    vNormal = mat3(normalMatrix) * normal;

    vec4 worldPos = modelMatrix * vec4(position, 1.0);
    vWorldPosition = worldPos.xyz;

    vec4 viewPos = modelViewMatrix * vec4(position, 1.0);
    vViewPosition = viewPos.xyz;

    gl_Position = projectionMatrix * viewPos;
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

const float PI = 3.14159265359;
const float RECIPROCAL_PI = 0.31830988618;
const float EPSILON = 1e-6;

struct DirectionalLight {
    vec3 direction;
    vec3 color;
    float intensity;
};

struct PointLight {
    vec3 position;
    vec3 color;
    float intensity;
    float distance;
    float decay;
};

// Material properties
uniform vec3 color;
uniform float opacity;
uniform float metalness;
uniform float roughness;
uniform vec3 emissive;
uniform float emissiveIntensity;

// Advanced properties
uniform float clearcoat;
uniform float clearcoatRoughness;
uniform vec2 clearcoatNormalScale;
uniform float transmission;
uniform float thickness;
uniform float attenuationDistance;
uniform vec3 attenuationColor;
uniform float sheen;
uniform vec3 sheenColor;
uniform float sheenRoughness;
uniform float iridescence;
uniform float ior;

// Base texture maps
uniform sampler2D map;
uniform bool useMap;
uniform sampler2D metalnessMap;
uniform bool useMetalnessMap;
uniform sampler2D roughnessMap;
uniform bool useRoughnessMap;
uniform sampler2D normalMap;
uniform bool useNormalMap;
uniform sampler2D aoMap;
uniform bool useAoMap;

// Clearcoat maps
uniform sampler2D clearcoatMap;
uniform bool useClearcoatMap;
uniform sampler2D clearcoatRoughnessMap;
uniform bool useClearcoatRoughnessMap;
uniform sampler2D clearcoatNormalMap;
uniform bool useClearcoatNormalMap;

// Transmission maps
uniform sampler2D transmissionMap;
uniform bool useTransmissionMap;
uniform sampler2D thicknessMap;
uniform bool useThicknessMap;

// Sheen maps
uniform sampler2D sheenColorMap;
uniform bool useSheenColorMap;
uniform sampler2D sheenRoughnessMap;
uniform bool useSheenRoughnessMap;

// Lights
uniform vec3 ambientLightColor;
uniform DirectionalLight directionalLights[4];
uniform int numDirectionalLights;
uniform PointLight pointLights[4];
uniform int numPointLights;
uniform vec3 cameraPosition;

in vec2 vUv;
in vec3 vNormal;
in vec3 vWorldPosition;
in vec3 vViewPosition;

out vec4 fragColor;

// =======================
// PBR BRDF Functions
// =======================

float pow2(float x) { return x * x; }
float pow4(float x) { float x2 = x * x; return x2 * x2; }
float pow5(float x) { float x2 = x * x; return x2 * x2 * x; }

// GGX/Trowbridge-Reitz normal distribution function
float D_GGX(float roughness, float NdotH) {
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH2 = NdotH * NdotH;
    float denom = NdotH2 * (a2 - 1.0) + 1.0;
    return a2 / (PI * denom * denom);
}

// Smith GGX geometry function
float G_GGX_Smith(float roughness, float NdotV, float NdotL) {
    float a = roughness * roughness;
    float a2 = a * a;

    float GGXV = NdotL * sqrt(NdotV * NdotV * (1.0 - a2) + a2);
    float GGXL = NdotV * sqrt(NdotL * NdotL * (1.0 - a2) + a2);

    return 0.5 / max(GGXV + GGXL, EPSILON);
}

// Schlick Fresnel approximation
vec3 F_Schlick(vec3 F0, float VdotH) {
    return F0 + (1.0 - F0) * pow5(1.0 - VdotH);
}

// Fresnel for clearcoat (always dielectric, F0 = 0.04)
float F_Schlick_Clearcoat(float VdotH) {
    float F0 = 0.04;
    return F0 + (1.0 - F0) * pow5(1.0 - VdotH);
}

// Charlie sheen distribution (fabric)
float D_Charlie(float roughness, float NdotH) {
    float invAlpha = 1.0 / roughness;
    float cos2h = NdotH * NdotH;
    float sin2h = max(1.0 - cos2h, 0.0078125);
    return (2.0 + invAlpha) * pow(sin2h, invAlpha * 0.5) / (2.0 * PI);
}

// Sheen visibility term
float V_Ashikhmin(float NdotV, float NdotL) {
    return 1.0 / (4.0 * (NdotL + NdotV - NdotL * NdotV));
}

// Compute normal from normal map
vec3 perturbNormal(vec3 N, vec3 V, vec2 uv, sampler2D normalMap, vec2 scale) {
    vec3 mapN = texture(normalMap, uv).xyz * 2.0 - 1.0;
    mapN.xy *= scale;

    vec3 Q1 = dFdx(vWorldPosition);
    vec3 Q2 = dFdy(vWorldPosition);
    vec2 st1 = dFdx(uv);
    vec2 st2 = dFdy(uv);

    vec3 T = normalize(Q1 * st2.t - Q2 * st1.t);
    vec3 B = -normalize(cross(N, T));
    mat3 TBN = mat3(T, B, N);

    return normalize(TBN * mapN);
}

// =======================
// Main PBR Calculation
// =======================

void main() {
    // Sample base color
    vec4 baseColor = vec4(color, opacity);
    if (useMap) {
        baseColor *= texture(map, vUv);
    }
    vec3 albedo = baseColor.rgb;

    // Sample material properties
    float metallic = metalness;
    if (useMetalnessMap) {
        metallic *= texture(metalnessMap, vUv).b;
    }

    float roughnessValue = roughness;
    if (useRoughnessMap) {
        roughnessValue *= texture(roughnessMap, vUv).g;
    }
    roughnessValue = max(roughnessValue, 0.04); // Minimum roughness

    // Ambient occlusion
    float ao = 1.0;
    if (useAoMap) {
        ao = texture(aoMap, vUv).r;
    }

    // Setup geometry
    vec3 N = normalize(vNormal);
    vec3 V = normalize(cameraPosition - vWorldPosition);

    // Apply normal map
    if (useNormalMap) {
        N = perturbNormal(N, V, vUv, normalMap, vec2(1.0));
    }

    float NdotV = abs(dot(N, V)) + EPSILON;

    // F0 for dielectrics and metals
    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    // =======================
    // Main Layer PBR
    // =======================
    vec3 Lo = vec3(0.0);

    // Directional lights
    for (int i = 0; i < numDirectionalLights && i < 4; i++) {
        vec3 L = normalize(-directionalLights[i].direction);
        vec3 H = normalize(V + L);
        vec3 radiance = directionalLights[i].color * directionalLights[i].intensity;

        float NdotL = max(dot(N, L), 0.0);
        float NdotH = max(dot(N, H), 0.0);
        float VdotH = max(dot(V, H), 0.0);

        // Cook-Torrance BRDF
        float D = D_GGX(roughnessValue, NdotH);
        float G = G_GGX_Smith(roughnessValue, NdotV, NdotL);
        vec3 F = F_Schlick(F0, VdotH);

        vec3 specular = D * G * F;

        // Energy conservation
        vec3 kS = F;
        vec3 kD = (vec3(1.0) - kS) * (1.0 - metallic);

        Lo += (kD * albedo * RECIPROCAL_PI + specular) * radiance * NdotL;
    }

    // Point lights
    for (int i = 0; i < numPointLights && i < 4; i++) {
        vec3 L = normalize(pointLights[i].position - vWorldPosition);
        vec3 H = normalize(V + L);
        float distance = length(pointLights[i].position - vWorldPosition);

        float attenuation = 1.0;
        if (pointLights[i].distance > 0.0) {
            attenuation = pow(clamp(1.0 - (distance / pointLights[i].distance), 0.0, 1.0), pointLights[i].decay);
        }

        vec3 radiance = pointLights[i].color * pointLights[i].intensity * attenuation;

        float NdotL = max(dot(N, L), 0.0);
        float NdotH = max(dot(N, H), 0.0);
        float VdotH = max(dot(V, H), 0.0);

        float D = D_GGX(roughnessValue, NdotH);
        float G = G_GGX_Smith(roughnessValue, NdotV, NdotL);
        vec3 F = F_Schlick(F0, VdotH);

        vec3 specular = D * G * F;
        vec3 kS = F;
        vec3 kD = (vec3(1.0) - kS) * (1.0 - metallic);

        Lo += (kD * albedo * RECIPROCAL_PI + specular) * radiance * NdotL;
    }

    // =======================
    // Clearcoat Layer
    // =======================
    vec3 clearcoatRadiance = vec3(0.0);
    float clearcoatStrength = clearcoat;
    if (useClearcoatMap) {
        clearcoatStrength *= texture(clearcoatMap, vUv).r;
    }

    if (clearcoatStrength > 0.0) {
        float clearcoatRough = clearcoatRoughness;
        if (useClearcoatRoughnessMap) {
            clearcoatRough *= texture(clearcoatRoughnessMap, vUv).g;
        }
        clearcoatRough = max(clearcoatRough, 0.04);

        vec3 clearcoatNormal = N;
        if (useClearcoatNormalMap) {
            clearcoatNormal = perturbNormal(N, V, vUv, clearcoatNormalMap, clearcoatNormalScale);
        }

        float clearcoatNdotV = abs(dot(clearcoatNormal, V)) + EPSILON;

        // Directional lights clearcoat contribution
        for (int i = 0; i < numDirectionalLights && i < 4; i++) {
            vec3 L = normalize(-directionalLights[i].direction);
            vec3 H = normalize(V + L);
            vec3 radiance = directionalLights[i].color * directionalLights[i].intensity;

            float NdotL = max(dot(clearcoatNormal, L), 0.0);
            float NdotH = max(dot(clearcoatNormal, H), 0.0);
            float VdotH = max(dot(V, H), 0.0);

            float D = D_GGX(clearcoatRough, NdotH);
            float G = G_GGX_Smith(clearcoatRough, clearcoatNdotV, NdotL);
            float F = F_Schlick_Clearcoat(VdotH);

            clearcoatRadiance += D * G * F * radiance * NdotL;
        }

        // Attenuate base layer by clearcoat
        Lo = Lo * (1.0 - clearcoatStrength * F_Schlick_Clearcoat(NdotV)) +
             clearcoatRadiance * clearcoatStrength;
    }

    // =======================
    // Sheen Layer (for fabric)
    // =======================
    if (sheen > 0.0) {
        vec3 sheenCol = sheenColor;
        if (useSheenColorMap) {
            sheenCol *= texture(sheenColorMap, vUv).rgb;
        }

        float sheenRough = sheenRoughness;
        if (useSheenRoughnessMap) {
            sheenRough *= texture(sheenRoughnessMap, vUv).a;
        }

        for (int i = 0; i < numDirectionalLights && i < 4; i++) {
            vec3 L = normalize(-directionalLights[i].direction);
            vec3 H = normalize(V + L);

            float NdotL = max(dot(N, L), 0.0);
            float NdotH = max(dot(N, H), 0.0);

            float D = D_Charlie(sheenRough, NdotH);
            float V_term = V_Ashikhmin(NdotV, NdotL);

            vec3 sheenRadiance = sheenCol * D * V_term *
                                directionalLights[i].color *
                                directionalLights[i].intensity * NdotL;

            Lo += sheenRadiance * sheen;
        }
    }

    // =======================
    // Transmission (simplified - would need refraction in real implementation)
    // =======================
    float transmissionValue = transmission;
    if (useTransmissionMap) {
        transmissionValue *= texture(transmissionMap, vUv).r;
    }

    if (transmissionValue > 0.0) {
        // Simplified transmission - in real implementation would refract through object
        vec3 transmittedLight = ambientLightColor * albedo;

        // Beer's law absorption
        if (attenuationDistance < 1e10) {
            float thick = thickness;
            if (useThicknessMap) {
                thick *= texture(thicknessMap, vUv).g;
            }
            float absorptionFactor = -thick / attenuationDistance;
            vec3 absorption = exp(absorptionFactor * (vec3(1.0) - attenuationColor));
            transmittedLight *= absorption;
        }

        Lo = mix(Lo, transmittedLight, transmissionValue);
    }

    // =======================
    // Final Output
    // =======================

    // Ambient lighting
    vec3 ambient = ambientLightColor * albedo * ao;
    vec3 finalColor = ambient + Lo;

    // Emissive
    finalColor += emissive * emissiveIntensity;

    // Tone mapping
    finalColor = finalColor / (finalColor + vec3(1.0));

    // Gamma correction
    finalColor = pow(finalColor, vec3(1.0 / 2.2));

    fragColor = vec4(finalColor, baseColor.a);
}";
    }

    public static class Shadow
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;

void main() {
    gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;

out vec4 fragColor;

void main() {
    // Shadow material - would normally sample shadow map
    // For now, just output shadow color with opacity
    fragColor = vec4(color, opacity * 0.5);
}";
    }

    public static class Sprite
    {
        public const string VertexShader = @"#version 300 es
precision highp float;

in vec3 position;
in vec2 uv;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;
uniform float rotation;
uniform bool sizeAttenuation;

out vec2 vUv;

void main() {
    vUv = uv;

    vec2 scale = vec2(1.0);

    // Apply rotation
    vec2 alignedPosition = position.xy;
    if (rotation != 0.0) {
        float c = cos(rotation);
        float s = sin(rotation);
        alignedPosition = vec2(
            alignedPosition.x * c - alignedPosition.y * s,
            alignedPosition.x * s + alignedPosition.y * c
        );
    }

    vec4 mvPosition = modelViewMatrix * vec4(0.0, 0.0, 0.0, 1.0);
    mvPosition.xy += alignedPosition * scale;

    gl_Position = projectionMatrix * mvPosition;
}";

        public const string FragmentShader = @"#version 300 es
precision highp float;

uniform vec3 color;
uniform float opacity;
uniform sampler2D map;
uniform bool useMap;

in vec2 vUv;

out vec4 fragColor;

void main() {
    vec4 baseColor = vec4(color, opacity);

    if (useMap) {
        baseColor *= texture(map, vUv);
    }

    fragColor = baseColor;
}";
    }
}
