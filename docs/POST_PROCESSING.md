# BlazorGL Post-Processing Effects

Complete guide to post-processing effects in BlazorGL, including usage, performance considerations, and best practices.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Effect Composer](#effect-composer)
- [Available Effects](#available-effects)
  - [Anti-Aliasing](#anti-aliasing)
  - [Depth of Field](#depth-of-field)
  - [Color Grading](#color-grading)
  - [Screen Space Effects](#screen-space-effects)
  - [Lighting Effects](#lighting-effects)
- [Performance Guide](#performance-guide)
- [Best Practices](#best-practices)

## Overview

Post-processing effects are shader-based image filters applied after the main scene rendering. BlazorGL provides a comprehensive suite of effects that can be combined to achieve cinematic visual quality.

### Supported Effects

| Effect | Type | Quality | Performance | Use Case |
|--------|------|---------|-------------|----------|
| **FXAA** | Anti-Aliasing | Good | Very Fast | General purpose AA |
| **SMAA** | Anti-Aliasing | Excellent | Fast | High quality AA |
| **TAA** | Anti-Aliasing | Best | Medium | Temporal AA with history |
| **SSAO** | Lighting | Excellent | Medium | Ambient occlusion |
| **Bloom** | Lighting | Good | Medium | HDR glow effects |
| **Bokeh** | Depth | Excellent | Slow | Depth of field/focus |
| **Outline** | Edge | Good | Fast | Object highlighting |
| **LUT** | Color | Excellent | Very Fast | Color grading |
| **Vignette** | Screen | Good | Very Fast | Edge darkening |
| **Color Correction** | Color | Good | Very Fast | Basic color adjustments |

## Getting Started

### Basic Setup

```csharp
// Create renderer
var renderer = new Renderer(1920, 1080);
await renderer.InitializeAsync("canvas-id");

// Create scene and camera
var scene = new Scene();
var camera = new PerspectiveCamera(75, 16f/9f, 0.1f, 1000f);

// Create effect composer
var composer = new EffectComposer(renderer);

// Add render pass (required first pass)
composer.AddPass(new RenderPass(scene, camera));

// Add post-processing effects
composer.AddPass(new BloomPass(1920, 1080));
composer.AddPass(new FXAAPass(1920, 1080));

// Render
composer.Render();
```

## Effect Composer

The `EffectComposer` manages the post-processing pipeline, rendering effects in sequence.

### Usage

```csharp
var composer = new EffectComposer(renderer);

// Passes are executed in order added
composer.AddPass(new RenderPass(scene, camera));  // Always first
composer.AddPass(new SSAOPass(width, height));    // Ambient occlusion
composer.AddPass(new BloomPass(width, height));   // HDR bloom
composer.AddPass(new SMAAPass(width, height));    // Anti-aliasing (last)

// Enable/disable individual passes
composer.Passes[1].Enabled = false;

// Render with all enabled passes
composer.Render();
```

### Pass Order Guidelines

1. **RenderPass** - Always first (renders the scene)
2. **SSAO** - Apply before lighting effects
3. **Bloom** - After lighting, before AA
4. **Bokeh/DOF** - After all lighting effects
5. **Color Correction/LUT** - Near end of pipeline
6. **Anti-Aliasing** - Usually last (smooths final output)
7. **Vignette** - Very last (frame effect)

## Available Effects

### Anti-Aliasing

#### FXAA (Fast Approximate Anti-Aliasing)

Single-pass edge-based anti-aliasing. Fast and effective for most scenes.

```csharp
var fxaaPass = new FXAAPass(width, height);
composer.AddPass(fxaaPass);
```

**Pros:**
- Very fast (single pass)
- Good quality for performance
- Works on all hardware

**Cons:**
- Slight blur on textures
- Less effective on thin lines

**Best for:** Real-time games, mobile, VR

#### SMAA (Subpixel Morphological Anti-Aliasing)

Three-pass anti-aliasing with superior quality and minimal blur.

```csharp
var smaaPass = new SMAAPass(width, height);
smaaPass.SetQuality(SMAAQuality.High);
smaaPass.EdgeDetectionThreshold = 0.05f;
composer.AddPass(smaaPass);
```

**Quality Presets:**
- `SMAAQuality.Low` - Threshold: 0.15 (fastest)
- `SMAAQuality.Medium` - Threshold: 0.1 (balanced)
- `SMAAQuality.High` - Threshold: 0.05 (recommended)
- `SMAAQuality.Ultra` - Threshold: 0.025 (best quality)

**Pros:**
- Excellent quality with minimal blur
- Better than FXAA on geometric edges
- Three quality presets

**Cons:**
- Three render passes (slower than FXAA)
- Requires search/area lookup textures

**Best for:** Desktop applications, high-quality rendering

#### TAA (Temporal Anti-Aliasing)

Multi-frame accumulation with camera jitter for the highest quality AA.

```csharp
var taaPass = new TAARenderPass(scene, camera, width, height);
taaPass.SampleCount = 8;              // 8-16 frames
taaPass.Sharpness = 0.5f;             // Reduce temporal blur
taaPass.BlendFactor = 0.1f;           // History weight
taaPass.EnableJitter = true;          // Camera sub-pixel offset
composer.AddPass(taaPass);

// Apply jitter to camera before rendering scene
var jitter = taaPass.GetCurrentJitterOffset();
// Apply jitter to camera projection...
```

**Configuration:**
- `SampleCount` - Number of frames to accumulate (4-16)
- `Sharpness` - Sharpening to reduce blur (0-1)
- `BlendFactor` - Blend weight (lower = more history)
- `EnableJitter` - Camera jitter on/off
- `UseMotionVectors` - Handle moving objects

**Pros:**
- Best quality anti-aliasing
- Removes temporal aliasing (shimmering)
- Works on sub-pixel details

**Cons:**
- Requires stable framerate
- Ghosting on fast motion
- Must implement camera jitter
- Requires history buffer

**Best for:** Cinematic rendering, photo mode, slow cameras

**Important:** Call `ResetHistory()` on camera cuts or scene changes!

### Depth of Field

#### BokehPass

Simulates camera lens focus with depth-based blur.

```csharp
var bokehPass = new BokehPass(scene, camera, width, height);
bokehPass.Focus = 5.0f;           // Focus distance (world units)
bokehPass.Aperture = 0.025f;      // Blur amount (f-stop)
bokehPass.MaxBlur = 1.0f;         // Maximum blur radius
bokehPass.Samples = 64;           // Quality (16-128)
composer.AddPass(bokehPass);
```

**Parameters:**
- `Focus` - Distance where scene is sharp (0.1-100)
- `Aperture` - Blur amount (0.001-0.1, higher = more blur)
- `MaxBlur` - Maximum blur radius (0.1-3.0)
- `Samples` - Sample count (16=fast, 128=quality)
- `ShowFocus` - Debug visualization of focus plane

**Algorithm:**
- Calculates Circle of Confusion (CoC) from depth
- Spiral sampling pattern (golden angle)
- Foreground/background blur separation

**Pros:**
- Realistic camera focus simulation
- Adjustable focus distance
- High-quality bokeh blur

**Cons:**
- Expensive (many texture samples)
- Requires depth texture
- Performance scales with sample count

**Best for:** Portrait mode, cinematic shots, focus effects

**Performance Tips:**
- Use 32-64 samples for real-time
- Use 128+ samples for photo mode
- Lower MaxBlur for better performance

### Color Grading

#### LUTPass (Lookup Table)

Apply 3D color lookup tables for professional color grading.

```csharp
var lutPass = new LUTPass(width, height);

// Use preset LUTs
lutPass.LUT = LUTLoader.PresetLUTs.Warm();
lutPass.LUT = LUTLoader.PresetLUTs.Cool();
lutPass.LUT = LUTLoader.PresetLUTs.Sepia();

// Or load custom .cube file
var (texture, size) = LUTLoader.LoadFromCubeFile("path/to/lut.cube");
lutPass.SetLUT(texture, size);

// Adjust intensity
lutPass.Intensity = 0.8f;  // 0 = original, 1 = fully graded

composer.AddPass(lutPass);
```

**LUT Sizes:**
- 16³ (4096 entries) - Fast, good for most uses
- 32³ (32,768 entries) - Balanced quality
- 64³ (262,144 entries) - High precision

**File Format Support:**
- `.cube` - Adobe Cube format (industry standard)
- Supports exports from Photoshop, Resolve, After Effects

**Pros:**
- Professional color grading
- Single texture lookup (very fast)
- Load custom LUTs from grading software
- Consistent look across devices

**Cons:**
- Requires LUT texture memory
- Limited to color transformations
- Larger LUTs use more memory

**Best for:** Cinematic color, film look, mood/atmosphere

**Workflow:**
1. Export screenshot from your app
2. Grade in Photoshop/Resolve
3. Export LUT (.cube)
4. Load LUT in app

#### ColorCorrectionPass

Basic color adjustments without LUTs.

```csharp
var colorPass = new ColorCorrectionPass(width, height);
colorPass.Brightness = 0.1f;      // -1 to 1
colorPass.Contrast = 0.2f;        // -1 to 1
colorPass.Saturation = 0.15f;     // -1 to 1
colorPass.Hue = 0.05f;            // -1 to 1
composer.AddPass(colorPass);
```

**Best for:** Simple adjustments, runtime tweaking

### Screen Space Effects

#### VignettePass

Radial darkening from edges to center.

```csharp
var vignettePass = new VignettePass(width, height);

// Use preset
vignettePass.SetPreset(VignettePreset.Cinematic);

// Or configure manually
vignettePass.Offset = 1.0f;       // Size of clear area (0-2)
vignettePass.Darkness = 0.7f;     // Intensity (0-1)
vignettePass.Smoothness = 0.6f;   // Falloff gradient (0-1)

composer.AddPass(vignettePass);
```

**Presets:**
- `Subtle` - Barely noticeable (offset: 1.2, darkness: 0.3)
- `Medium` - Standard look (offset: 1.0, darkness: 0.6)
- `Strong` - Clear effect (offset: 0.8, darkness: 0.9)
- `Dramatic` - Heavy impact (offset: 0.6, darkness: 1.0)
- `Cinematic` - Film-like (offset: 1.1, darkness: 0.7)

**Pros:**
- Very fast (simple calculation)
- Focuses attention on center
- Adds cinematic feel

**Cons:**
- Can feel overused
- May darken important UI elements

**Best for:** Portrait mode, focus attention, cinematic look

### Lighting Effects

#### BloomPass

HDR glow effect for bright areas.

```csharp
var bloomPass = new BloomPass(width, height);
bloomPass.Threshold = 0.8f;       // Brightness threshold
bloomPass.Intensity = 1.5f;       // Bloom strength
bloomPass.Radius = 0.5f;          // Blur radius
composer.AddPass(bloomPass);
```

**Best for:** HDR rendering, light sources, sci-fi effects

#### SSAOPass (Screen Space Ambient Occlusion)

Contact shadows in crevices and corners.

```csharp
var ssaoPass = new SSAOPass(scene, camera, width, height);
ssaoPass.Radius = 0.5f;           // Effect radius
ssaoPass.Intensity = 1.0f;        // Shadow strength
ssaoPass.Bias = 0.01f;            // Depth bias
ssaoPass.Samples = 32;            // Quality
composer.AddPass(ssaoPass);
```

**Best for:** Enhanced depth perception, realistic lighting

#### OutlinePass

Highlight objects with colored outlines.

```csharp
var outlinePass = new OutlinePass(scene, camera, width, height);
outlinePass.EdgeColor = 0xff0000;     // Red outline
outlinePass.EdgeThickness = 2.0f;     // Line width
outlinePass.EdgeGlow = 0.5f;          // Glow intensity
composer.AddPass(outlinePass);
```

**Best for:** Object selection, highlighting, toon shading

## Performance Guide

### Performance Comparison

| Effect | GPU Cost | Resolution Scaling | Mobile-Friendly |
|--------|----------|-------------------|-----------------|
| **FXAA** | Low | Linear | Yes |
| **SMAA** | Medium | Linear | Yes |
| **TAA** | Medium | Linear | Conditional |
| **SSAO** | High | Quadratic | No |
| **Bloom** | Medium | Linear | Yes |
| **Bokeh** | Very High | Quadratic | No |
| **LUT** | Very Low | Linear | Yes |
| **Vignette** | Very Low | Linear | Yes |
| **ColorCorrection** | Low | Linear | Yes |

### Optimization Tips

#### 1. Resolution Scaling

Render expensive effects at lower resolution:

```csharp
// Render scene at full resolution
var renderPass = new RenderPass(scene, camera);
composer.AddPass(renderPass);

// Expensive effects at half resolution
var halfWidth = width / 2;
var halfHeight = height / 2;
var ssaoPass = new SSAOPass(scene, camera, halfWidth, halfHeight);
composer.AddPass(ssaoPass);

// Cheap effects at full resolution
var fxaaPass = new FXAAPass(width, height);
composer.AddPass(fxaaPass);
```

#### 2. Quality vs Performance

**High Performance (Mobile, VR):**
```csharp
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new FXAAPass(width, height));
composer.AddPass(new VignettePass(width, height));
// Total: ~2ms @ 1080p
```

**Balanced (Desktop):**
```csharp
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new BloomPass(width, height));
composer.AddPass(new SMAAPass(width, height));
composer.AddPass(new ColorCorrectionPass(width, height));
// Total: ~4-6ms @ 1080p
```

**Cinematic (Photo Mode):**
```csharp
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new SSAOPass(scene, camera, width, height));
composer.AddPass(new BloomPass(width, height));
composer.AddPass(new BokehPass(scene, camera, width, height) { Samples = 128 });
composer.AddPass(new TAARenderPass(scene, camera, width, height));
composer.AddPass(new LUTPass(width, height));
composer.AddPass(new VignettePass(width, height));
// Total: ~15-25ms @ 1080p (30-60 FPS acceptable)
```

#### 3. Conditional Effects

Enable expensive effects only when needed:

```csharp
// Enable TAA only for photo mode
taaPass.Enabled = isPhotoMode;

// Enable SSAO only on high settings
ssaoPass.Enabled = graphicsQuality == GraphicsQuality.High;

// Disable Bokeh during gameplay
bokehPass.Enabled = isInCutscene;
```

#### 4. Sample Count Tuning

Reduce samples for real-time:

```csharp
// Real-time
bokehPass.Samples = 32;
ssaoPass.Samples = 16;
taaPass.SampleCount = 4;

// Photo mode
bokehPass.Samples = 128;
ssaoPass.Samples = 64;
taaPass.SampleCount = 16;
```

### Platform-Specific Recommendations

#### Desktop (High-End GPU)
- Full resolution rendering
- SMAA or TAA for anti-aliasing
- SSAO + Bloom + Bokeh
- LUT color grading
- Target: 60+ FPS @ 1080p

#### Desktop (Mid-Range GPU)
- Full resolution rendering
- FXAA or SMAA Medium
- Bloom + ColorCorrection
- Vignette
- Target: 60 FPS @ 1080p

#### Mobile/Tablet
- Reduced resolution (720p)
- FXAA only
- Simple effects (Vignette, ColorCorrection)
- Avoid SSAO, Bokeh, TAA
- Target: 30 FPS @ 720p

#### WebGL
- Similar to Desktop Mid-Range
- Test browser performance
- Consider WebGL 1.0 fallbacks
- Avoid 3D textures (use 2D LUT format)

## Best Practices

### 1. Always Start Simple

Begin with basic setup and add effects incrementally:

```csharp
// Start here
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new FXAAPass(width, height));

// Add gradually
composer.AddPass(new BloomPass(width, height));
composer.AddPass(new VignettePass(width, height));

// Profile performance before adding more
```

### 2. Profile Before Optimizing

Measure actual performance impact:

```csharp
var stopwatch = Stopwatch.StartNew();
composer.Render();
stopwatch.Stop();
Console.WriteLine($"Frame time: {stopwatch.ElapsedMilliseconds}ms");
```

### 3. Use Presets When Available

Presets are tuned for good results:

```csharp
smaaPass.SetQuality(SMAAQuality.High);
vignettePass.SetPreset(VignettePreset.Cinematic);
```

### 4. Layer Effects Carefully

Order matters for visual quality:

```csharp
// Good: AA smooths final result
composer.AddPass(new BloomPass(width, height));
composer.AddPass(new FXAAPass(width, height));

// Bad: Bloom amplifies AA artifacts
composer.AddPass(new FXAAPass(width, height));
composer.AddPass(new BloomPass(width, height));
```

### 5. Reset State on Scene Changes

Some effects maintain history:

```csharp
// On camera cut or scene load
taaPass.ResetHistory();

// On resolution change
composer = new EffectComposer(renderer);
// Re-add all passes...
```

### 6. Use Appropriate AA for Use Case

| Use Case | Recommended AA | Why |
|----------|---------------|-----|
| Real-time game | FXAA | Fast, works everywhere |
| Architectural viz | SMAA High | Clean edges, minimal blur |
| Photo mode | TAA 16 samples | Best quality |
| VR | FXAA + low settings | Performance critical |
| Mobile | FXAA or none | Limited GPU |

### 7. Test on Target Hardware

Effects perform differently across GPUs:

```csharp
// Detect and adapt
if (isLowEndDevice)
{
    // Disable expensive effects
    ssaoPass.Enabled = false;
    bokehPass.Enabled = false;

    // Use fast alternatives
    composer.AddPass(new FXAAPass(width, height));
}
else
{
    composer.AddPass(new SMAAPass(width, height));
}
```

## Common Recipes

### Cinematic Look

```csharp
// Film-like rendering
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new BloomPass(width, height) { Intensity = 1.2f });
composer.AddPass(new LUTPass(width, height));  // Film emulation LUT
composer.AddPass(new SMAAPass(width, height));
composer.AddPass(new VignettePass(width, height));
```

### Performance Mode

```csharp
// Maximum FPS
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new FXAAPass(width, height));
// That's it!
```

### Quality Mode

```csharp
// Best visual quality
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new SSAOPass(scene, camera, width, height));
composer.AddPass(new BloomPass(width, height));
composer.AddPass(new TAARenderPass(scene, camera, width, height));
composer.AddPass(new LUTPass(width, height));
composer.AddPass(new VignettePass(width, height));
```

### Portrait Mode

```csharp
// Depth of field focus
composer.AddPass(new RenderPass(scene, camera));
composer.AddPass(new BokehPass(scene, camera, width, height)
{
    Focus = 5.0f,
    Aperture = 0.05f,
    Samples = 64
});
composer.AddPass(new VignettePass(width, height));
composer.AddPass(new FXAAPass(width, height));
```

## Troubleshooting

### Effects Not Visible

1. Check pass is enabled: `pass.Enabled = true`
2. Ensure RenderPass is first
3. Verify resolution matches renderer
4. Check effect intensity/strength settings

### Performance Issues

1. Profile individual passes
2. Reduce resolution of expensive effects
3. Lower sample counts
4. Use simpler alternatives (FXAA vs SMAA)

### Visual Artifacts

1. Check pass order (AA should be last)
2. Adjust thresholds/bias
3. Reset history on scene changes (TAA)
4. Verify depth texture is available (Bokeh, SSAO)

### TAA Ghosting

1. Increase `BlendFactor` (0.15-0.2)
2. Enable motion vectors for moving objects
3. Reset history on fast camera movement
4. Reduce sample count

## API Reference

See individual class documentation for detailed API:

- `EffectComposer` - Pipeline management
- `Pass` - Base class for all effects
- `RenderPass` - Scene rendering
- Anti-aliasing: `FXAAPass`, `SMAAPass`, `TAARenderPass`
- Depth: `BokehPass`, `SSAOPass`
- Color: `LUTPass`, `ColorCorrectionPass`
- Screen: `VignettePass`, `BloomPass`, `OutlinePass`

## Further Reading

- [Three.js Post-Processing](https://threejs.org/docs/#manual/en/introduction/How-to-use-post-processing)
- [SMAA Paper](http://www.iryoku.com/smaa/)
- [TAA in Unreal Engine](https://docs.unrealengine.com/en-US/RenderingAndGraphics/PostProcessEffects/TemporalAntiAliasing/)
- [LUT Color Grading](https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@10.2/manual/Post-Processing-Color-Grading.html)

---

**Next Steps:**
1. Try the [example scenes](../examples/PostProcessing/)
2. Experiment with effect combinations
3. Profile on your target hardware
4. Build your custom post-processing pipeline
