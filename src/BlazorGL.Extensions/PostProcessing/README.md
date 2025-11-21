# BlazorGL Post-Processing System

Enterprise-grade post-processing pipeline for BlazorGL with multiple high-quality effects.

## Quick Start

```csharp
// Setup
var composer = new EffectComposer(renderer, 1920, 1080);

// Add effects
composer.AddPass(new SSAOPass(renderer, camera, 1920, 1080));
composer.AddPass(new FXAAPass(1920, 1080));
composer.AddPass(new ColorCorrectionPass());

// Render with effects
composer.Render(scene, camera);
```

## Available Effects

### SSAO (Screen Space Ambient Occlusion)
Adds realistic contact shadows and depth perception.
- **Performance**: 2-5ms at 1080p
- **Quality**: Adjustable kernel size (8-64 samples)
- **Best for**: Architectural visualization, character models, complex scenes

### FXAA (Fast Approximate Anti-Aliasing)
Single-pass edge smoothing without multi-sampling overhead.
- **Performance**: 1-2ms at 1080p
- **Quality**: Good edge smoothing, minimal blur
- **Best for**: All scenes where MSAA is not available

### Color Correction
Comprehensive color grading with HSL manipulation.
- **Performance**: 0.5-1ms at 1080p
- **Features**: Brightness, contrast, saturation, hue, exposure, gamma
- **Best for**: Mood setting, color grading, tone mapping

## Architecture

```
┌─────────┐     ┌──────────┐     ┌────────┐     ┌────────┐     ┌────────┐
│  Scene  │ --> │ Renderer │ --> │ Pass 1 │ --> │ Pass 2 │ --> │ Screen │
└─────────┘     └──────────┘     └────────┘     └────────┘     └────────┘
                                      │              │
                                      └──── Ping-Pong Buffers
```

## Files

### Core Classes
- **EffectComposer.cs**: Manages post-processing pipeline
- **ShaderPass.cs**: Base class for full-screen shader effects

### SSAO Effect
- **SSAOPass.cs**: Main SSAO implementation
- **SSAOShader.cs**: GLSL shaders for depth, SSAO calculation, and blur

### FXAA Effect
- **FXAAPass.cs**: Anti-aliasing pass
- **FXAAShader.cs**: GLSL shader for edge detection and smoothing

### Color Correction
- **ColorCorrectionPass.cs**: Color grading pass
- **ColorCorrectionShader.cs**: GLSL shader with RGB↔HSL conversion

## Performance Guidelines

### Recommended Settings

**High Quality (Desktop)**
```csharp
var ssao = new SSAOPass(renderer, camera, 1920, 1080)
{
    KernelSize = 64,
    Radius = 0.5f,
    Power = 1.5f
};
```

**Medium Quality (Desktop/Mobile)**
```csharp
var ssao = new SSAOPass(renderer, camera, 1920, 1080)
{
    KernelSize = 32,
    Radius = 0.5f,
    Power = 1.5f
};
```

**Low Quality (Mobile)**
```csharp
var ssao = new SSAOPass(renderer, camera, 1280, 720)
{
    KernelSize = 16,
    Radius = 0.3f,
    Power = 1.2f
};
```

### Performance Budget (1080p, 60fps)

| Effect | Budget | Priority |
|--------|--------|----------|
| SSAO   | 2-3ms  | High     |
| FXAA   | 1ms    | Medium   |
| Color  | 0.5ms  | Low      |

## Examples

See `examples/BlazorGL.Examples/Pages/PostProcessing.razor` for a complete interactive demo.

## Tests

Run tests:
```bash
dotnet test --filter "FullyQualifiedName~PostProcessing"
```

18 tests covering:
- Effect composer creation
- Pass management
- SSAO parameters
- FXAA functionality
- Color correction
- Shader uniforms
- Render target management

## Documentation

See `docs/POST_PROCESSING.md` for comprehensive documentation including:
- Detailed algorithm explanations
- Parameter tuning guides
- Performance optimization
- Troubleshooting
- Technical implementation details

## Future Enhancements

Planned features:
- Bloom effect
- Depth of Field (Bokeh)
- Motion Blur
- Screen Space Reflections
- Temporal Anti-Aliasing
- Tone Mapping
- Vignette

## License

Part of BlazorGL - see repository root for license information.
