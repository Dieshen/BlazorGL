# Advanced Post-Processing System

BlazorGL provides a comprehensive post-processing pipeline with multiple effects that can be combined to create stunning visual results.

## Overview

The post-processing system is built around the `EffectComposer` class, which manages a chain of rendering passes. Each pass processes the output of the previous pass, allowing for complex multi-stage effects.

## Architecture

```
Scene → Renderer → Pass 1 → Pass 2 → Pass N → Screen
```

## Core Components

### EffectComposer

The main class that orchestrates post-processing passes.

```csharp
var composer = new EffectComposer(renderer, width, height);
composer.AddPass(ssaoPass);
composer.AddPass(fxaaPass);
composer.Render(scene, camera);
```

### Pass

Base class for all post-processing effects. Each pass has:
- `Enabled` property to toggle the effect
- `Render()` method that processes input and writes to output

### ShaderPass

Convenient base class for shader-based effects that render a full-screen quad.

## Available Effects

### 1. SSAO (Screen Space Ambient Occlusion)

Adds realistic contact shadows by approximating ambient occlusion in screen space.

**Algorithm:**
1. Render scene depth to texture
2. Generate hemisphere sample kernel (default 32 samples)
3. For each pixel, sample depth at kernel positions
4. Count occlusions based on depth comparison
5. Blur the result to reduce noise
6. Multiply with scene color

**Usage:**

```csharp
var ssaoPass = new SSAOPass(renderer, camera, width, height)
{
    KernelSize = 32,    // Number of samples (8-64)
    Radius = 0.5f,      // Sample radius in world space
    Bias = 0.01f,       // Depth comparison bias
    Power = 1.5f        // Occlusion intensity
};
composer.AddPass(ssaoPass);
```

**Parameters:**

- **KernelSize**: Number of samples per pixel. Higher = better quality but slower.
  - Low quality: 8-16 samples
  - Medium quality: 32 samples (default)
  - High quality: 64 samples

- **Radius**: How far to sample around each pixel in world space.
  - Small scenes: 0.1-0.3
  - Medium scenes: 0.5-1.0 (default: 0.5)
  - Large scenes: 1.0-2.0

- **Bias**: Prevents self-occlusion artifacts.
  - Default: 0.01
  - Increase if you see banding artifacts
  - Decrease if occlusion is too weak

- **Power**: Amplifies the occlusion effect.
  - Subtle: 1.0-1.5 (default: 1.5)
  - Strong: 2.0-3.0

**Performance:**
- ~2-5ms per frame at 1080p (depends on KernelSize)
- Uses two render targets (depth + SSAO)
- Requires depth rendering pass

### 2. FXAA (Fast Approximate Anti-Aliasing)

Single-pass edge-based anti-aliasing that smooths jagged edges without expensive multi-sampling.

**Algorithm:**
1. Calculate luminance for each pixel
2. Detect edges based on luminance gradients
3. Blur edges perpendicular to edge direction

**Usage:**

```csharp
var fxaaPass = new FXAAPass(width, height);
composer.AddPass(fxaaPass);
```

**Parameters:**
- Width and height of the render target

**Performance:**
- ~1-2ms per frame at 1080p
- Single render target
- No additional textures needed

**Quality:**
- Best for reducing aliasing on edges
- May slightly blur textures
- Cheaper than MSAA or SMAA

### 3. Color Correction

Comprehensive color grading with multiple adjustments.

**Usage:**

```csharp
var colorPass = new ColorCorrectionPass()
{
    Brightness = 0.1f,     // -1 to 1 (default: 0)
    Contrast = 1.2f,       // 0 to 2 (default: 1)
    Saturation = 0.9f,     // 0 to 2 (default: 1)
    Hue = 0.05f,           // 0 to 1 (default: 0)
    Exposure = 1.1f,       // 0 to 2 (default: 1)
    Gamma = 2.2f           // 0.5 to 3 (default: 2.2)
};
composer.AddPass(colorPass);
```

**Parameters:**

- **Brightness**: Adds/subtracts from all color channels
  - Negative: darker
  - Positive: brighter

- **Contrast**: Amplifies color differences
  - Less than 1: washed out
  - Greater than 1: more dramatic

- **Saturation**: Color intensity
  - 0: grayscale
  - 1: normal (default)
  - Greater than 1: vibrant

- **Hue**: Shifts colors around color wheel
  - 0-1 wraps around (0 = red, 0.33 = green, 0.67 = blue)

- **Exposure**: Simulates camera exposure
  - Less than 1: underexposed
  - Greater than 1: overexposed

- **Gamma**: Non-linear brightness correction
  - Standard: 2.2 (sRGB)
  - Lighter: 1.8-2.0
  - Darker: 2.4-2.8

**Performance:**
- ~0.5-1ms per frame at 1080p
- Single render target
- RGB↔HSL conversion per pixel

## Complete Example

```csharp
// Initialize renderer
var renderer = new Renderer();
await renderer.InitializeAsync(canvas, jsRuntime);

// Create camera
var camera = new PerspectiveCamera(45, 16f/9f, 0.1f, 100f);

// Setup post-processing
var composer = new EffectComposer(renderer, 1920, 1080);

// Add SSAO for realistic shadows
var ssaoPass = new SSAOPass(renderer, camera, 1920, 1080)
{
    KernelSize = 32,
    Radius = 0.5f,
    Power = 1.5f
};
composer.AddPass(ssaoPass);

// Add FXAA for smooth edges
var fxaaPass = new FXAAPass(1920, 1080);
composer.AddPass(fxaaPass);

// Add color correction for mood
var colorPass = new ColorCorrectionPass()
{
    Contrast = 1.1f,
    Saturation = 0.9f
};
composer.AddPass(colorPass);

// Render loop
while (true)
{
    composer.Render(scene, camera);
    await Task.Delay(16);
}
```

## Performance Guidelines

### Optimization Tips

1. **SSAO Optimization:**
   - Use lower kernel sizes for real-time (16-32)
   - Render SSAO at half resolution and upscale
   - Cache depth buffer between frames if scene is static

2. **Pass Ordering:**
   - SSAO should come first (needs clean depth)
   - FXAA should come last (works on final image)
   - Color correction can go anywhere

3. **Conditional Passes:**
   ```csharp
   ssaoPass.Enabled = qualitySettings.EnableSSAO;
   fxaaPass.Enabled = !msaaEnabled; // Only if MSAA is off
   ```

4. **Resolution Scaling:**
   ```csharp
   // Render effects at lower resolution
   var effectWidth = width / 2;
   var effectHeight = height / 2;
   var ssaoPass = new SSAOPass(renderer, camera, effectWidth, effectHeight);
   ```

### Performance Budget (1080p, 60 FPS target)

| Effect | Budget | Notes |
|--------|--------|-------|
| SSAO (32 samples) | 2-3ms | Major impact |
| FXAA | 1ms | Minimal impact |
| Color Correction | 0.5ms | Negligible |
| **Total** | **3.5-4.5ms** | ~27% of 16.67ms frame budget |

## Technical Details

### Depth Rendering

SSAO requires a depth pre-pass:

```csharp
renderer.RenderDepth(scene, camera, depthTarget);
```

This renders the scene with a depth material that outputs normalized depth values.

### Shader Uniforms

All post-processing shaders receive:
- `tDiffuse`: Input texture from previous pass
- Resolution-specific uniforms (width, height)

Effect-specific uniforms are documented in each pass.

### Render Target Management

The `EffectComposer` manages two render targets and ping-pongs between them:
- Write Buffer: Current pass output
- Read Buffer: Next pass input

The final pass renders to the screen (null target).

## Troubleshooting

### SSAO looks noisy
- Increase blur radius
- Increase kernel size
- Check that depth buffer is properly configured

### FXAA makes image blurry
- This is expected behavior
- Try reducing effect strength (not currently exposed)
- Consider SMAA for better quality (future feature)

### Color correction artifacts
- Check gamma value (should be ~2.2 for sRGB)
- Ensure input colors are in correct color space
- Clamp extreme values

### Performance issues
- Reduce SSAO kernel size
- Lower render target resolution
- Disable effects selectively based on hardware
- Profile using browser DevTools

## Future Enhancements

Planned features:
- Bloom effect
- Depth of Field (Bokeh)
- Motion Blur
- Screen Space Reflections (SSR)
- Temporal Anti-Aliasing (TAA)
- Tone Mapping operators
- Vignette effect

## References

- [SSAO Tutorial by John Chapman](http://john-chapman-graphics.blogspot.com/2013/01/ssao-tutorial.html)
- [FXAA Whitepaper by NVIDIA](https://developer.download.nvidia.com/assets/gamedev/files/sdk/11/FXAA_WhitePaper.pdf)
- [Color Grading in Games](https://www.gamedeveloper.com/programming/color-grading-in-games)
