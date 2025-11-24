# BlazorGL Shadow Mapping Guide

Complete guide to shadow mapping techniques in BlazorGL, from basic hard shadows to advanced soft shadow filtering.

## Table of Contents

1. [Overview](#overview)
2. [Shadow Map Types](#shadow-map-types)
3. [Basic Setup](#basic-setup)
4. [PCF (Percentage Closer Filtering)](#pcf-percentage-closer-filtering)
5. [PCFSoft (Soft Shadows)](#pcfsoft-soft-shadows)
6. [VSM (Variance Shadow Maps)](#vsm-variance-shadow-maps)
7. [CSM (Cascaded Shadow Maps)](#csm-cascaded-shadow-maps)
8. [Quality vs Performance](#quality-vs-performance)
9. [Troubleshooting](#troubleshooting)
10. [Best Practices](#best-practices)

## Overview

Shadow mapping is a technique for rendering realistic shadows by rendering the scene from the light's perspective to determine which areas are occluded from light.

BlazorGL supports four shadow map filtering techniques:

- **Basic**: Hard shadows, fastest but aliased
- **PCF**: Smooth shadows with configurable sample count
- **PCFSoft/PCSS**: Very soft shadows with variable penumbra
- **VSM**: Statistical filtering for smooth shadows without heavy sampling

Additionally, **CSM (Cascaded Shadow Maps)** eliminates perspective aliasing for large scenes.

## Shadow Map Types

### Basic Shadows

Hard shadows with no filtering. Fastest but shows aliasing artifacts.

**When to use:**
- Low-end devices
- Performance-critical applications
- Stylized visuals where hard shadows fit the aesthetic

**Characteristics:**
- Single depth comparison
- No extra samples
- Sharp shadow edges
- Visible aliasing on curved surfaces

### PCF (Percentage Closer Filtering)

Samples the shadow map multiple times around each fragment and averages the results.

**When to use:**
- General purpose smooth shadows
- Good balance of quality and performance
- Mobile devices (with lower sample counts)

**Characteristics:**
- Configurable sample count (9, 16, 25, or 64)
- Poisson disk sampling for good distribution
- Soft shadow edges
- Linear performance cost with sample count

### PCFSoft / PCSS

Enhanced PCF with variable penumbra size based on distance from shadow caster.

**When to use:**
- High-quality realistic shadows
- Desktop applications
- Scenes requiring physically accurate shadow softness

**Characteristics:**
- Variable shadow softness (contact-hardened shadows)
- Blocker search + adaptive filtering
- More expensive than PCF
- Realistic penumbra effects

### VSM (Variance Shadow Maps)

Statistical filtering using depth and depth² moments.

**When to use:**
- Large blur radii without performance cost
- When you can accept light bleeding artifacts
- Scenes with smooth surfaces

**Characteristics:**
- Can be blurred like regular textures
- No sample count limitation
- Potential light bleeding artifacts
- Requires two-channel render target

## Basic Setup

### Enable Shadows on a Light

```csharp
var directionalLight = new DirectionalLight(new Color(1, 1, 1), 1.0f)
{
    Direction = new Vector3(-1, -3, -2),
    CastShadow = true
};

// Configure shadow properties
directionalLight.Shadow.Width = 2048;
directionalLight.Shadow.Height = 2048;
directionalLight.Shadow.Near = 0.5f;
directionalLight.Shadow.Far = 500f;
directionalLight.Shadow.Bias = 0.001f;

scene.Add(directionalLight);
```

### Enable Shadow Receiving on Objects

```csharp
var mesh = new Mesh(geometry, material)
{
    CastShadow = true,
    ReceiveShadow = true
};

scene.Add(mesh);
```

## PCF (Percentage Closer Filtering)

### Basic PCF Setup

```csharp
var light = new DirectionalLight(new Color(1, 1, 1), 1.0f)
{
    CastShadow = true
};

// Configure PCF
light.Shadow.Type = ShadowMapType.PCF;
light.Shadow.PCFSamples = 16;  // 3x3=9, 16, 5x5=25, or 8x8=64
light.Shadow.Radius = 2.0f;    // Shadow softness
light.Shadow.Bias = 0.001f;    // Prevent shadow acne
```

### PCF Sample Counts

| Sample Count | Pattern | Quality | Performance |
|--------------|---------|---------|-------------|
| 9 | 3x3 | Low | Fast |
| 16 | Poisson | Medium | Good |
| 25 | 5x5 | High | Moderate |
| 64 | 8x8 | Very High | Slow |

**Recommendation:**
- Mobile: 9-16 samples
- Desktop: 16-25 samples
- High-end: 25-64 samples

### Adjusting Shadow Softness

```csharp
// Harder shadows (sharper edges)
light.Shadow.Radius = 1.0f;

// Softer shadows (blurrier edges)
light.Shadow.Radius = 3.0f;

// Very soft shadows
light.Shadow.Radius = 5.0f;
```

## PCFSoft (Soft Shadows)

### PCSS Setup

```csharp
var light = new DirectionalLight(new Color(1, 1, 1), 1.0f)
{
    CastShadow = true
};

// Configure PCSS (Percentage Closer Soft Shadows)
light.Shadow.Type = ShadowMapType.PCFSoft;
light.Shadow.PCFSamples = 25;          // Higher quality
light.Shadow.ShadowSoftness = 2.0f;    // Overall softness multiplier
light.Shadow.LightSize = 5.0f;         // Physical light size (affects penumbra)
light.Shadow.Bias = 0.002f;            // May need higher bias
```

### Understanding Light Size

The `LightSize` parameter simulates the physical size of the light source:

```csharp
// Small light source (sun-like)
light.Shadow.LightSize = 1.0f;  // Sharp shadows with small penumbra

// Medium light source (area light)
light.Shadow.LightSize = 5.0f;  // Moderate penumbra

// Large light source (large area light)
light.Shadow.LightSize = 10.0f; // Large, soft penumbra
```

### Variable Penumbra

PCSS automatically creates contact-hardened shadows:
- Shadows are sharp near the contact point
- Shadows become softer with distance from the caster

This is physically accurate and creates realistic shadows.

## VSM (Variance Shadow Maps)

### VSM Setup

```csharp
var light = new DirectionalLight(new Color(1, 1, 1), 1.0f)
{
    CastShadow = true
};

// Configure VSM
light.Shadow.Type = ShadowMapType.VSM;
light.Shadow.MinVariance = 0.00001f;           // Precision control
light.Shadow.LightBleedingReduction = 0.2f;    // Reduce artifacts (0-1)
light.Shadow.BlurSize = 5;                     // Gaussian blur size
```

### VSM Parameters

#### MinVariance
Controls precision and prevents division by zero:
```csharp
// More precision (may show artifacts)
light.Shadow.MinVariance = 0.000001f;

// Less precision (smoother but may be too soft)
light.Shadow.MinVariance = 0.0001f;
```

#### Light Bleeding Reduction
Reduces the light bleeding artifact:
```csharp
// No reduction (more artifacts, lighter shadows)
light.Shadow.LightBleedingReduction = 0.0f;

// Medium reduction (balanced)
light.Shadow.LightBleedingReduction = 0.2f;

// Strong reduction (darker shadows, less bleeding)
light.Shadow.LightBleedingReduction = 0.5f;
```

#### Blur Size
Controls shadow softness:
```csharp
// Sharp shadows
light.Shadow.BlurSize = 1;

// Soft shadows
light.Shadow.BlurSize = 5;

// Very soft shadows
light.Shadow.BlurSize = 9;
```

### VSM Advantages
- Very soft shadows with minimal performance cost
- Blur size doesn't affect performance significantly
- Can pre-blur shadow maps
- Good for ambient shadows

### VSM Limitations
- Light bleeding artifacts on overlapping geometry
- Requires higher precision render targets (RG16F or RG32F)
- Not suitable for all scenes

## CSM (Cascaded Shadow Maps)

CSM solves perspective aliasing in large scenes by using multiple shadow maps at different distances.

### Basic CSM Setup

```csharp
var camera = new PerspectiveCamera(45, aspectRatio, 0.1f, 1000f);
var light = new DirectionalLight(new Color(1, 1, 1), 1.0f)
{
    CastShadow = true
};

// Enable CSM
light.EnableCSM(camera, cascadeCount: 3, maxDistance: 500f);

// Configure cascade properties
light.CSM.Lambda = 0.5f;                   // Split distribution (0=uniform, 1=logarithmic)
light.CSM.CascadeResolution = 1024;        // Resolution per cascade
light.CSM.EnableCascadeBlending = true;    // Smooth transitions
light.CSM.BlendRange = 0.1f;               // Blend zone size

scene.Add(light);
```

### Cascade Count

| Cascades | Use Case | Memory | Quality |
|----------|----------|--------|---------|
| 2 | Simple scenes | Low | Basic |
| 3 | General purpose | Medium | Good |
| 4 | Large open worlds | High | Excellent |

### Lambda Parameter

Controls how cascades are distributed:

```csharp
// Uniform distribution (equal spacing)
light.CSM.Lambda = 0.0f;  // Good for even depth distribution

// Balanced (PSSM - Practical Split Scheme Method)
light.CSM.Lambda = 0.5f;  // Recommended default

// Logarithmic distribution (more detail near camera)
light.CSM.Lambda = 1.0f;  // Good for first-person views
```

### Cascade Blending

Smooth transitions between cascades:

```csharp
// Enable blending
light.CSM.EnableCascadeBlending = true;
light.CSM.BlendRange = 0.1f;  // 10% blend zone

// Wider blend zone (smoother but may show double shadows)
light.CSM.BlendRange = 0.2f;

// Disable blending (visible seams but no double shadows)
light.CSM.EnableCascadeBlending = false;
```

### CSM with Other Techniques

You can combine CSM with other filtering:

```csharp
light.EnableCSM(camera, 3, 500f);
light.Shadow.Type = ShadowMapType.PCF;  // PCF filtering in each cascade
light.Shadow.PCFSamples = 16;
light.Shadow.Radius = 2.0f;
```

## Quality vs Performance

### Performance Comparison

| Technique | Relative Cost | Quality | Mobile Suitable |
|-----------|--------------|---------|-----------------|
| Basic | 1x | Low | Yes |
| PCF (9 samples) | 2-3x | Medium | Yes |
| PCF (25 samples) | 5-6x | High | Maybe |
| PCF (64 samples) | 10-12x | Very High | No |
| PCSS | 8-15x | Excellent | No |
| VSM | 3-4x | High | Yes |
| CSM (3 cascades) | 3x | Excellent | Yes |

### Recommended Settings by Platform

#### Mobile (Low-End)
```csharp
light.Shadow.Width = 1024;
light.Shadow.Height = 1024;
light.Shadow.Type = ShadowMapType.PCF;
light.Shadow.PCFSamples = 9;
light.Shadow.Radius = 1.5f;
```

#### Mobile (High-End)
```csharp
light.Shadow.Width = 2048;
light.Shadow.Height = 2048;
light.Shadow.Type = ShadowMapType.PCF;
light.Shadow.PCFSamples = 16;
light.Shadow.Radius = 2.0f;
```

#### Desktop (Medium Quality)
```csharp
light.Shadow.Width = 2048;
light.Shadow.Height = 2048;
light.Shadow.Type = ShadowMapType.PCF;
light.Shadow.PCFSamples = 25;
light.Shadow.Radius = 2.5f;
```

#### Desktop (High Quality)
```csharp
light.Shadow.Width = 4096;
light.Shadow.Height = 4096;
light.Shadow.Type = ShadowMapType.PCFSoft;
light.Shadow.PCFSamples = 25;
light.Shadow.LightSize = 5.0f;
light.Shadow.ShadowSoftness = 2.0f;
```

#### Desktop (Ultra Quality with CSM)
```csharp
light.Shadow.Width = 4096;
light.Shadow.Height = 4096;
light.EnableCSM(camera, 4, 1000f);
light.CSM.CascadeResolution = 2048;
light.Shadow.Type = ShadowMapType.PCFSoft;
light.Shadow.PCFSamples = 25;
```

## Troubleshooting

### Shadow Acne (Self-Shadowing Artifacts)

**Problem:** Surfaces incorrectly shadow themselves, creating a striped pattern.

**Solution:**
```csharp
// Increase shadow bias
light.Shadow.Bias = 0.002f;  // Try values from 0.001 to 0.01

// Use normal bias (offsets along surface normal)
light.Shadow.NormalBias = 0.05f;

// Increase shadow camera near plane
light.Shadow.Near = 1.0f;
```

### Peter Panning

**Problem:** Shadows appear detached from objects (floating).

**Solution:**
```csharp
// Decrease shadow bias
light.Shadow.Bias = 0.0005f;

// Reduce normal bias
light.Shadow.NormalBias = 0.01f;
```

### Aliased/Pixelated Shadows

**Problem:** Shadows have visible pixels or jagged edges.

**Solution:**
```csharp
// Increase shadow map resolution
light.Shadow.Width = 4096;
light.Shadow.Height = 4096;

// Use PCF filtering
light.Shadow.Type = ShadowMapType.PCF;
light.Shadow.PCFSamples = 25;

// For large scenes, use CSM
light.EnableCSM(camera, 3, 500f);
```

### Shadows Cut Off or Missing

**Problem:** Shadows disappear at certain distances or angles.

**Solution:**
```csharp
// Increase shadow camera far plane
light.Shadow.Far = 1000f;

// For directional lights, ensure camera encompasses the entire scene

// For CSM, increase max distance
light.CSM.MaxDistance = 2000f;
```

### Light Bleeding (VSM Only)

**Problem:** Light leaks through geometry, especially at corners.

**Solution:**
```csharp
// Increase light bleeding reduction
light.Shadow.LightBleedingReduction = 0.3f;

// Increase minimum variance
light.Shadow.MinVariance = 0.0001f;

// Or switch to PCF
light.Shadow.Type = ShadowMapType.PCF;
```

### Performance Issues

**Problem:** Shadows cause frame rate drops.

**Solution:**
```csharp
// Reduce shadow map resolution
light.Shadow.Width = 1024;
light.Shadow.Height = 1024;

// Use fewer PCF samples
light.Shadow.PCFSamples = 9;

// Use Basic shadows
light.Shadow.Type = ShadowMapType.Basic;

// Reduce cascade count
light.CSM.CascadeCount = 2;
```

## Best Practices

### 1. Choose the Right Shadow Map Resolution

```csharp
// For close-up objects
light.Shadow.Width = 4096;
light.Shadow.Height = 4096;

// For medium-distance objects
light.Shadow.Width = 2048;
light.Shadow.Height = 2048;

// For distant objects
light.Shadow.Width = 1024;
light.Shadow.Height = 1024;
```

### 2. Fit Shadow Camera Tightly

For directional lights, ensure the shadow camera bounds fit your scene tightly:

```csharp
// In DirectionalLightShadow implementation
shadowCamera.Left = sceneMin.X;
shadowCamera.Right = sceneMax.X;
shadowCamera.Bottom = sceneMin.Y;
shadowCamera.Top = sceneMax.Y;
```

### 3. Use CSM for Large Outdoor Scenes

```csharp
// Large outdoor scene (0-1000 units)
light.EnableCSM(camera, 4, 1000f);
light.CSM.Lambda = 0.5f;  // Balanced distribution
```

### 4. Adjust Bias Per-Scene

Different scenes require different bias values:

```csharp
// Indoor scenes (small scale)
light.Shadow.Bias = 0.0001f;

// Outdoor scenes (large scale)
light.Shadow.Bias = 0.005f;

// Curved surfaces
light.Shadow.NormalBias = 0.05f;
```

### 5. Use VSM for Ambient Shadows

VSM works well for contact shadows and ambient occlusion:

```csharp
var aoLight = new DirectionalLight(new Color(0.5f, 0.5f, 0.5f), 0.5f)
{
    CastShadow = true
};
aoLight.Shadow.Type = ShadowMapType.VSM;
aoLight.Shadow.BlurSize = 7;  // Large blur for soft AO-like shadows
```

### 6. Combine Techniques

Use different techniques for different lights:

```csharp
// Main sun: CSM with PCF
sunLight.EnableCSM(camera, 3, 500f);
sunLight.Shadow.Type = ShadowMapType.PCF;
sunLight.Shadow.PCFSamples = 16;

// Secondary light: Basic VSM
fillLight.Shadow.Type = ShadowMapType.VSM;
fillLight.Shadow.BlurSize = 5;

// Spot light: PCSS
spotLight.Shadow.Type = ShadowMapType.PCFSoft;
spotLight.Shadow.LightSize = 3.0f;
```

### 7. Profile and Optimize

Always measure performance:

```csharp
// Start with high quality
light.Shadow.Type = ShadowMapType.PCFSoft;
light.Shadow.PCFSamples = 64;

// If FPS < target, reduce:
// 1. Sample count: 64 -> 25 -> 16 -> 9
// 2. Shadow map size: 4096 -> 2048 -> 1024
// 3. Shadow type: PCFSoft -> PCF -> Basic
// 4. Cascade count: 4 -> 3 -> 2
```

### 8. Consider Shadow Updates

Not all shadows need to update every frame:

```csharp
// Static shadows for static objects
if (objectIsStatic && shadowsRenderedOnce)
{
    // Skip shadow map rendering
}

// Dynamic shadows only when camera or light moves
if (cameraMovedSignificantly || lightMovedSignificantly)
{
    RenderShadowMaps();
}
```

## Conclusion

BlazorGL provides a complete shadow mapping solution with techniques ranging from basic hard shadows to advanced CSM with soft filtering. Choose the technique that best balances quality and performance for your specific use case.

For most applications, **PCF with 16-25 samples** provides excellent quality-performance balance. For large outdoor scenes, **CSM with PCF filtering** eliminates aliasing. For very soft shadows on capable hardware, use **PCSS**. For unique artistic effects or very soft ambient shadows, try **VSM**.

Remember to always profile your application and adjust settings based on target hardware capabilities.
