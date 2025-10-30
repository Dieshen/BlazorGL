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
}
