# BlazorGL Debug UI

The `BlazorGL.Debug` package provides real-time performance monitoring and debugging tools for your BlazorGL applications.

## Installation

Add a reference to the `BlazorGL.Debug` project:

```xml
<ItemGroup>
  <ProjectReference Include="..\BlazorGL.Debug\BlazorGL.Debug.csproj" />
</ItemGroup>
```

## Stats Component

The `Stats` component displays real-time rendering statistics as an overlay on your 3D scene.

### Basic Usage

```razor
@using BlazorGL.Debug
@using BlazorGL.Core.Rendering

<Stats Performance="@renderer.Stats" />
```

### Full Configuration

```razor
<Stats Performance="@renderer.Stats"
       Position="StatsPosition.TopLeft"
       ShowCulling="true"
       Opacity="0.85f" />
```

## Features

### Performance Metrics

The Stats component displays:

Metric | Description | Color Coding
-------|-------------|-------------
**FPS** | Frames per second | Green (55+), Yellow (30-54), Red (<30)
**Frame Time** | Milliseconds per frame | White
**Draw Calls** | Number of GPU draw calls | White
**Triangles** | Total triangles rendered | White (formatted as K/M)
**Objects** | Total renderable objects | White
**Culled** | Objects skipped by frustum culling | Green (with percentage)

### Number Formatting

Large numbers are automatically formatted for readability:

```
123 → "123"
1,234 → "1.2K"
1,234,567 → "1.23M"
```

### Visual Design

The Stats overlay features:
- **Semi-transparent dark background** (default 85% opacity)
- **Blur effect** for modern look (backdrop-filter)
- **Monospace font** for technical data
- **Color-coded values** for quick assessment
- **Non-interactive** (pointer-events: none)
- **Fixed positioning** (stays on screen during scroll)

## Position Options

Position the stats panel in any corner:

```csharp
public enum StatsPosition
{
    TopLeft,     // Default
    TopRight,
    BottomLeft,
    BottomRight
}
```

Example:

```razor
<Stats Performance="@renderer.Stats"
       Position="StatsPosition.BottomRight" />
```

## Properties

### Performance (Required)

```csharp
[Parameter]
public PerformanceStats Performance { get; set; }
```

The `PerformanceStats` object from your renderer:

```csharp
var renderer = new Renderer();
// In your component:
<Stats Performance="@renderer.Stats" />
```

### Position

```csharp
[Parameter]
public StatsPosition Position { get; set; } = StatsPosition.TopLeft;
```

Where to display the overlay (default: top-left).

### ShowCulling

```csharp
[Parameter]
public bool ShowCulling { get; set; } = true;
```

Whether to display frustum culling statistics (default: true).

Set to `false` to hide culling metrics:

```razor
<Stats Performance="@renderer.Stats" ShowCulling="false" />
```

### Opacity

```csharp
[Parameter]
public float Opacity { get; set; } = 0.85f;
```

Background opacity from 0.0 (transparent) to 1.0 (opaque).

Examples:

```razor
<!-- Fully opaque -->
<Stats Performance="@renderer.Stats" Opacity="1.0f" />

<!-- Very transparent -->
<Stats Performance="@renderer.Stats" Opacity="0.5f" />
```

## Complete Example

```razor
@page "/scene"
@using BlazorGL.Core
@using BlazorGL.Core.Rendering
@using BlazorGL.Core.Geometries
@using BlazorGL.Core.Materials
@using BlazorGL.Debug

<div style="width: 100%; height: 100vh; position: relative;">
    <canvas @ref="_canvasRef" style="width: 100%; height: 100%;"></canvas>

    <!-- Stats overlay -->
    <Stats Performance="@_renderer.Stats"
           Position="StatsPosition.TopLeft"
           ShowCulling="true"
           Opacity="0.9f" />
</div>

@code {
    private ElementReference _canvasRef;
    private Renderer _renderer = new();
    private Scene _scene = new();
    private PerspectiveCamera _camera = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _renderer.InitializeAsync(_canvasRef, JSRuntime);
            _camera.Position = new Vector3(0, 0, 5);

            // Add objects to scene...

            // Start render loop
            await RenderLoop();
        }
    }

    private async Task RenderLoop()
    {
        while (true)
        {
            _renderer.Render(_scene, _camera);
            await Task.Delay(16); // ~60 FPS
            StateHasChanged(); // Update Stats display
        }
    }
}
```

## Performance Impact

The Stats component has minimal performance impact:

- **Render overhead**: <0.1ms (UI update)
- **Memory**: ~1KB
- **No GPU impact**: Pure CPU/DOM operations

The component only updates when `StateHasChanged()` is called, so you control the update frequency.

## Styling Customization

The Stats component uses scoped CSS. To customize styling:

### Option 1: CSS Variables (Future Enhancement)

```css
/* Override in your app.css */
.blazorgl-stats {
    --stats-bg-color: rgba(0, 0, 0, 0.9);
    --stats-text-color: #fff;
    --stats-font-family: 'Monaco';
}
```

### Option 2: Global CSS Override

```css
/* app.css */
.blazorgl-stats {
    background: rgba(20, 20, 20, 0.95) !important;
    border: 2px solid #00ff00 !important;
    border-radius: 8px !important;
}

.blazorgl-stats-value {
    font-size: 14px !important;
    color: #00ff00 !important;
}
```

### Option 3: Custom Component

Create your own stats display:

```razor
@code {
    private string GetFpsColor()
    {
        if (Performance.FPS >= 55) return "#0f0";
        if (Performance.FPS >= 30) return "#ff0";
        return "#f00";
    }
}

<div class="my-custom-stats">
    <div style="color: @GetFpsColor()">
        FPS: @Performance.FPS.ToString("F1")
    </div>
    <div>
        Draw Calls: @Performance.DrawCalls
    </div>
    <!-- Custom layout -->
</div>
```

## Best Practices

### 1. Hide in Production

Only show stats during development:

```razor
@if (IsDevelopment)
{
    <Stats Performance="@renderer.Stats" />
}

@code {
    #if DEBUG
    private bool IsDevelopment => true;
    #else
    private bool IsDevelopment => false;
    #endif
}
```

### 2. Update Frequency

Update stats every frame for smooth animation:

```csharp
private async Task RenderLoop()
{
    while (true)
    {
        _renderer.Render(_scene, _camera);
        await InvokeAsync(StateHasChanged); // Update UI
        await Task.Delay(16); // ~60 FPS
    }
}
```

### 3. Multiple Stats Panels

Show multiple stats in different positions:

```razor
<!-- Top-left: FPS -->
<Stats Performance="@renderer.Stats"
       Position="StatsPosition.TopLeft"
       ShowCulling="false" />

<!-- Top-right: Full stats -->
<Stats Performance="@renderer.Stats"
       Position="StatsPosition.TopRight"
       ShowCulling="true" />
```

### 4. Conditional Display

Show/hide based on key press:

```razor
@if (_showStats)
{
    <Stats Performance="@renderer.Stats" />
}

@code {
    private bool _showStats = true;

    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "F3")
            _showStats = !_showStats;
    }
}
```

## Troubleshooting

### Stats Not Updating

**Problem**: Stats display shows 0 or doesn't change.

**Solutions**:

1. Ensure `StateHasChanged()` is called:
   ```csharp
   _renderer.Render(_scene, _camera);
   await InvokeAsync(StateHasChanged);
   ```

2. Check renderer is initialized:
   ```csharp
   await _renderer.InitializeAsync(_canvasRef, JSRuntime);
   ```

3. Verify render loop is running:
   ```csharp
   while (true)
   {
       _renderer.Render(_scene, _camera);
       await Task.Delay(16);
       StateHasChanged();
   }
   ```

### Stats Not Visible

**Problem**: Component renders but not visible.

**Solutions**:

1. Check z-index (should be 10000 by default):
   ```css
   .blazorgl-stats {
       z-index: 10000 !important;
   }
   ```

2. Ensure parent has position:
   ```html
   <div style="position: relative;">
       <canvas />
       <Stats />
   </div>
   ```

3. Check opacity setting:
   ```razor
   <Stats Opacity="0.85f" /> <!-- Not 0.0f -->
   ```

### Performance Degradation

**Problem**: Adding Stats component causes frame drops.

**Solutions**:

1. Reduce update frequency:
   ```csharp
   private int _frameCount = 0;

   if (_frameCount++ % 3 == 0) // Update every 3 frames
   {
       StateHasChanged();
   }
   ```

2. Use `InvokeAsync` to avoid blocking:
   ```csharp
   await InvokeAsync(StateHasChanged);
   ```

## Future Enhancements

Planned features for BlazorGL.Debug:

- **Memory profiler**: Track object allocations and GC
- **GPU profiler**: Shader performance breakdown
- **Render graph visualizer**: Show render pass dependencies
- **Scene hierarchy viewer**: Inspect object tree in real-time
- **Texture inspector**: Preview loaded textures
- **Shader debugger**: Live shader editing
- **Performance history graph**: Line chart of FPS over time
- **Bottleneck detector**: Automatically identify performance issues

## Related Documentation

- [Frustum Culling](./FRUSTUM_CULLING.md) - Detailed culling documentation
- [Performance Optimization](./PERFORMANCE.md) - General optimization guide
- [Rendering Pipeline](./RENDERING.md) - How the renderer works

## Contributing

To add new debug components to BlazorGL.Debug:

1. Create component in `src/BlazorGL.Debug/`
2. Use scoped CSS (`Component.razor.css`)
3. Follow non-interactive overlay pattern
4. Add XML documentation
5. Update this document

Example:

```razor
<!-- MemoryStats.razor -->
@using BlazorGL.Core

<div class="blazorgl-memory">
    <div>Allocations: @Allocations</div>
    <div>GC Count: @GCCount</div>
</div>

@code {
    [Parameter]
    public int Allocations { get; set; }

    [Parameter]
    public int GCCount { get; set; }
}
```
