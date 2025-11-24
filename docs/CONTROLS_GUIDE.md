# BlazorGL Controls Guide

Complete guide to camera controls and object manipulation in BlazorGL.

## Table of Contents

- [Overview](#overview)
- [OrbitControls](#orbitcontrols)
- [TrackballControls](#trackballcontrols)
- [TransformControls](#transformcontrols)
- [DragControls](#dragcontrols)
- [Choosing the Right Control](#choosing-the-right-control)
- [Best Practices](#best-practices)
- [Performance Considerations](#performance-considerations)

## Overview

BlazorGL provides four primary control types for camera manipulation and object interaction:

1. **OrbitControls** - Constrained camera rotation around a target point
2. **TrackballControls** - Free 360° camera rotation without constraints
3. **TransformControls** - Interactive 3D gizmos for object manipulation
4. **DragControls** - Click-and-drag object positioning

All controls follow the Three.js API conventions for easy migration and familiarity.

## OrbitControls

OrbitControls allows the camera to orbit around a target point with constraints on rotation angles and distance.

### Features

- Spherical coordinate-based rotation
- Polar and azimuthal angle constraints
- Distance constraints (min/max zoom)
- Damping/inertia support
- Auto-rotation
- Touch support (1 finger rotate, 2 finger zoom/pan)
- Mouse support (left-click rotate, right-click pan, wheel zoom)

### Basic Usage

```csharp
@page "/orbit-demo"
@using BlazorGL.Controls
@using BlazorGL.Core.Cameras
@inject IJSRuntime JSRuntime

<canvas id="glCanvas" width="800" height="600"></canvas>

@code {
    private OrbitControls? _controls;
    private PerspectiveCamera? _camera;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            _camera.Position = new Vector3(0, 0, 5);

            _controls = new OrbitControls(_camera, JSRuntime, "glCanvas");
            await _controls.InitializeAsync();

            // Configure controls
            _controls.EnableDamping = true;
            _controls.DampingFactor = 0.05f;
            _controls.MinDistance = 1f;
            _controls.MaxDistance = 50f;
            _controls.MinPolarAngle = 0f;
            _controls.MaxPolarAngle = MathF.PI;
        }
    }

    private void RenderLoop(float deltaTime)
    {
        _controls?.Update(deltaTime);
        // Render scene...
    }

    public async ValueTask DisposeAsync()
    {
        if (_controls != null)
            await _controls.DisposeAsync();
    }
}
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Target` | `Vector3` | `(0,0,0)` | Point to orbit around |
| `EnableDamping` | `bool` | `false` | Enable inertia/momentum |
| `DampingFactor` | `float` | `0.05` | Damping strength (0-1) |
| `MinDistance` | `float` | `0` | Minimum zoom distance |
| `MaxDistance` | `float` | `∞` | Maximum zoom distance |
| `MinPolarAngle` | `float` | `0` | Minimum vertical angle |
| `MaxPolarAngle` | `float` | `π` | Maximum vertical angle |
| `MinAzimuthAngle` | `float` | `-∞` | Minimum horizontal angle |
| `MaxAzimuthAngle` | `float` | `∞` | Maximum horizontal angle |
| `EnableRotate` | `bool` | `true` | Allow camera rotation |
| `EnableZoom` | `bool` | `true` | Allow camera zoom |
| `EnablePan` | `bool` | `true` | Allow camera panning |
| `RotateSpeed` | `float` | `1.0` | Rotation sensitivity |
| `ZoomSpeed` | `float` | `1.0` | Zoom sensitivity |
| `PanSpeed` | `float` | `1.0` | Pan sensitivity |
| `AutoRotate` | `bool` | `false` | Automatic rotation |
| `AutoRotateSpeed` | `float` | `2.0` | Auto-rotation speed |

### Use Cases

- Product viewers (constrained viewing angles)
- Architectural visualization (prevent upside-down views)
- Map navigation (restrict to ground level)
- Most 3D applications requiring intuitive camera control

## TrackballControls

TrackballControls provides free 360° camera rotation using quaternion-based math, eliminating gimbal lock.

### Features

- Quaternion-based rotation (no gimbal lock)
- Free 360° rotation in all directions
- Virtual trackball projection
- Dynamic or static movement modes
- Optional roll constraint
- Distance constraints
- Screen-space panning
- Touch and mouse support

### Basic Usage

```csharp
@page "/trackball-demo"
@using BlazorGL.Controls
@using BlazorGL.Core.Cameras
@inject IJSRuntime JSRuntime

<canvas id="glCanvas" width="800" height="600"></canvas>

@code {
    private TrackballControls? _controls;
    private PerspectiveCamera? _camera;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            _camera.Position = new Vector3(0, 0, 5);

            _controls = new TrackballControls(_camera, JSRuntime, "glCanvas");
            await _controls.InitializeAsync();

            // Configure controls
            _controls.RotateSpeed = 1.0f;
            _controls.ZoomSpeed = 1.2f;
            _controls.PanSpeed = 0.3f;
            _controls.StaticMoving = false; // Enable momentum
            _controls.DynamicDampingFactor = 0.2f;
            _controls.NoRoll = false; // Allow free rotation
            _controls.Screen = new Vector2(800, 600);
        }
    }

    private void RenderLoop(float deltaTime)
    {
        _controls?.Update(deltaTime);
        // Render scene...
    }

    public async ValueTask DisposeAsync()
    {
        if (_controls != null)
            await _controls.DisposeAsync();
    }
}
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Target` | `Vector3` | `(0,0,0)` | Point to rotate around |
| `RotateSpeed` | `float` | `1.0` | Rotation sensitivity |
| `ZoomSpeed` | `float` | `1.2` | Zoom sensitivity |
| `PanSpeed` | `float` | `0.3` | Pan sensitivity |
| `StaticMoving` | `bool` | `false` | Disable momentum |
| `DynamicDampingFactor` | `float` | `0.2` | Damping strength |
| `NoRoll` | `bool` | `false` | Prevent camera roll |
| `MinDistance` | `float` | `0` | Minimum zoom distance |
| `MaxDistance` | `float` | `∞` | Maximum zoom distance |
| `Screen` | `Vector2` | `(1920,1080)` | Canvas dimensions |

### Use Cases

- CAD/CAM applications (free rotation needed)
- Scientific visualization (examine data from any angle)
- Space simulations (no "up" direction)
- Medical imaging (view anatomy from any perspective)

## TransformControls

TransformControls provides interactive 3D gizmos for translating, rotating, and scaling objects.

### Features

- Three transform modes: Translate, Rotate, Scale
- World space and local space modes
- Axis constraints (X, Y, Z, XY, YZ, XZ, XYZ)
- Snap to grid (translation)
- Snap to angle (rotation)
- Snap to increment (scale)
- Visual gizmo feedback
- Interactive dragging
- Comprehensive events

### Basic Usage

```csharp
@page "/transform-demo"
@using BlazorGL.Controls
@using BlazorGL.Core
@using BlazorGL.Core.Cameras
@using BlazorGL.Core.Rendering
@inject IJSRuntime JSRuntime

<canvas id="glCanvas" width="800" height="600"></canvas>

@code {
    private TransformControls? _controls;
    private PerspectiveCamera? _camera;
    private Renderer? _renderer;
    private Object3D? _selectedObject;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            _camera.Position = new Vector3(0, 5, 10);
            _renderer = new Renderer();

            _controls = new TransformControls(_camera, _renderer, JSRuntime, "glCanvas");
            await _controls.InitializeAsync();

            // Configure controls
            _controls.Mode = TransformMode.Translate;
            _controls.Space = TransformSpace.World;
            _controls.TranslationSnap = 0.5f; // Snap to 0.5 unit grid
            _controls.RotationSnap = MathF.PI / 12; // Snap to 15 degrees
            _controls.Size = 1.0f;

            // Attach to an object
            _selectedObject = new Object3D();
            _controls.Attach(_selectedObject);

            // Subscribe to events
            _controls.DraggingChanged += (s, dragging) =>
            {
                Console.WriteLine($"Dragging: {dragging}");
            };
            _controls.ObjectChanged += (s, e) =>
            {
                Console.WriteLine("Object transform completed");
            };
        }
    }

    private void SwitchMode(TransformMode mode)
    {
        if (_controls != null)
            _controls.Mode = mode;
    }

    private void RenderLoop(float deltaTime)
    {
        _controls?.Update();
        // Render scene...
    }

    public async ValueTask DisposeAsync()
    {
        if (_controls != null)
            await _controls.DisposeAsync();
    }
}
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Mode` | `TransformMode` | `Translate` | Transform mode |
| `Space` | `TransformSpace` | `World` | Coordinate space |
| `ShowX` | `bool` | `true` | Show X axis gizmo |
| `ShowY` | `bool` | `true` | Show Y axis gizmo |
| `ShowZ` | `bool` | `true` | Show Z axis gizmo |
| `TranslationSnap` | `float?` | `null` | Grid snap size |
| `RotationSnap` | `float?` | `null` | Angle snap (radians) |
| `ScaleSnap` | `float?` | `null` | Scale snap increment |
| `Size` | `float` | `1.0` | Gizmo size |

### Events

- `DraggingChanged` - Fired when drag state changes (useful for disabling orbit controls)
- `Change` - Fired continuously during drag
- `ObjectChanged` - Fired when drag completes
- `MouseDown` - Fired when gizmo is clicked
- `MouseUp` - Fired when mouse is released

### Use Cases

- Level editors (precise object placement)
- 3D modeling tools (transform primitives)
- Scene composition (arrange objects)
- CAD applications (precise positioning)

## DragControls

DragControls enables click-and-drag object manipulation using raycasting.

### Features

- Raycaster-based object picking
- Drag along camera-facing plane
- Multiple object support
- Recursive child picking
- Hover detection
- Comprehensive drag events
- Parent-space transformations

### Basic Usage

```csharp
@page "/drag-demo"
@using BlazorGL.Controls
@using BlazorGL.Core
@using BlazorGL.Core.Cameras
@using BlazorGL.Core.Rendering
@inject IJSRuntime JSRuntime

<canvas id="glCanvas" width="800" height="600"></canvas>

@code {
    private DragControls? _controls;
    private PerspectiveCamera? _camera;
    private Renderer? _renderer;
    private List<Object3D> _draggableObjects = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            _camera.Position = new Vector3(0, 0, 10);
            _renderer = new Renderer();

            // Create draggable objects
            _draggableObjects.Add(new Object3D { Position = new Vector3(-2, 0, 0) });
            _draggableObjects.Add(new Object3D { Position = new Vector3(0, 0, 0) });
            _draggableObjects.Add(new Object3D { Position = new Vector3(2, 0, 0) });

            _controls = new DragControls(_camera, _draggableObjects, _renderer, JSRuntime, "glCanvas");
            await _controls.InitializeAsync();

            // Configure controls
            _controls.Recursive = true;

            // Subscribe to events
            _controls.DragStart += (s, e) =>
            {
                Console.WriteLine($"Started dragging: {e.Object}");
            };
            _controls.Drag += (s, e) =>
            {
                Console.WriteLine($"Dragging at: {e.Point}");
            };
            _controls.DragEnd += (s, e) =>
            {
                Console.WriteLine($"Finished dragging: {e.Object}");
            };
            _controls.HoverOn += (s, e) =>
            {
                Console.WriteLine($"Hovering: {e.Object}");
            };
            _controls.HoverOff += (s, e) =>
            {
                Console.WriteLine($"Hover ended: {e.Object}");
            };
        }
    }

    private void RenderLoop(float deltaTime)
    {
        _controls?.Update();
        // Render scene...
    }

    public async ValueTask DisposeAsync()
    {
        if (_controls != null)
            await _controls.DisposeAsync();
    }
}
```

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | `bool` | `true` | Enable/disable controls |
| `Recursive` | `bool` | `true` | Pick child objects |
| `TransformMode` | `TransformMode` | `Translate` | Transform mode |

### Events

- `DragStart` - Object picking started
- `Drag` - Object being dragged
- `DragEnd` - Object released
- `HoverOn` - Mouse entered object
- `HoverOff` - Mouse left object

### Use Cases

- UI builders (drag-and-drop components)
- Game editors (place objects in scene)
- Interactive visualizations (move data points)
- Puzzle games (manipulate game pieces)

## Choosing the Right Control

| Scenario | Recommended Control | Reason |
|----------|-------------------|---------|
| Product viewer | OrbitControls | Constrained viewing prevents disorientation |
| CAD application | TrackballControls | Free rotation needed for complex models |
| Level editor | TransformControls | Precise positioning with visual feedback |
| Interactive scene | DragControls | Natural drag-and-drop interaction |
| Map navigation | OrbitControls | Constraints prevent invalid views |
| Medical imaging | TrackballControls | View from any angle without restrictions |
| Architecture viz | OrbitControls | Realistic ground-level viewing |
| Space simulation | TrackballControls | No "up" direction in space |

## Best Practices

### Performance

1. **Update Frequency**
   ```csharp
   // Call Update() once per frame
   private void RenderLoop(float deltaTime)
   {
       _controls?.Update(deltaTime);
       _renderer?.Render(_scene, _camera);
   }
   ```

2. **Damping vs Static**
   - Use damping for smooth, polished feel (higher CPU usage)
   - Use static movement for immediate response (lower CPU usage)

3. **Object Count for DragControls**
   - Limit draggable objects to < 100 for optimal performance
   - Use recursive picking sparingly (increases raycast cost)

### User Experience

1. **Disable Conflicting Controls**
   ```csharp
   _transformControls.DraggingChanged += (s, dragging) =>
   {
       _orbitControls.Enabled = !dragging;
   };
   ```

2. **Provide Visual Feedback**
   ```csharp
   _dragControls.HoverOn += (s, e) =>
   {
       // Highlight hovered object
       e.Object.Material.Color = Color.Yellow;
   };
   ```

3. **Use Snapping for Precision**
   ```csharp
   _transformControls.TranslationSnap = 0.25f; // Quarter-unit grid
   _transformControls.RotationSnap = MathF.PI / 8; // 22.5 degree snaps
   ```

### Memory Management

1. **Always Dispose**
   ```csharp
   public async ValueTask DisposeAsync()
   {
       await _orbitControls?.DisposeAsync();
       await _trackballControls?.DisposeAsync();
       await _transformControls?.DisposeAsync();
       await _dragControls?.DisposeAsync();
   }
   ```

2. **Event Cleanup**
   - Event handlers are automatically cleaned up on dispose
   - Unsubscribe manually if disposing controls before component

### Touch Support

All controls support touch gestures:
- **1 finger** - Rotate/drag
- **2 fingers** - Zoom and pan
- **Pinch** - Zoom

## Performance Considerations

### OrbitControls

- **CPU**: Very light (~0.1ms per frame)
- **Memory**: Minimal (< 1KB)
- **Best Practice**: Enable damping only when needed

### TrackballControls

- **CPU**: Light (~0.15ms per frame)
- **Memory**: Minimal (< 1KB)
- **Best Practice**: Use StaticMoving for real-time applications

### TransformControls

- **CPU**: Light (~0.2ms per frame) + raycasting overhead
- **Memory**: Moderate (gizmo geometry ~10KB)
- **Best Practice**: Hide gizmos when not in use

### DragControls

- **CPU**: Variable (depends on object count and raycasting)
- **Memory**: Minimal (< 1KB + raycaster)
- **Best Practice**: Limit draggable objects, disable when not needed

### Optimization Tips

1. **Conditional Updates**
   ```csharp
   if (_controls.Enabled && _camera.HasChanged)
   {
       _controls.Update(deltaTime);
   }
   ```

2. **Distance-Based Enabling**
   ```csharp
   var distance = Vector3.Distance(_camera.Position, _object.Position);
   _transformControls.Enabled = distance < 50f;
   ```

3. **Batch Operations**
   ```csharp
   // Update all controls in one pass
   _orbitControls?.Update(deltaTime);
   _transformControls?.Update();
   _dragControls?.Update();
   ```

## Additional Resources

- [OrbitControls API Reference](./api/OrbitControls.md)
- [TrackballControls API Reference](./api/TrackballControls.md)
- [TransformControls API Reference](./api/TransformControls.md)
- [DragControls API Reference](./api/DragControls.md)
- [Three.js Controls Documentation](https://threejs.org/docs/#examples/en/controls/OrbitControls)

## Examples

See the `examples/Controls/` directory for complete working examples:
- `OrbitExample.razor` - Product viewer
- `TrackballExample.razor` - CAD-style viewer
- `TransformExample.razor` - Scene editor
- `DragExample.razor` - Interactive arrangement

## Troubleshooting

### Controls Not Working

1. Verify `InitializeAsync()` was called
2. Check that `Update()` is called every frame
3. Ensure canvas element ID matches constructor parameter
4. Verify JavaScript module is loaded correctly

### Jumpy Camera Movement

1. Enable damping: `controls.EnableDamping = true`
2. Reduce damping factor: `controls.DampingFactor = 0.05f`
3. Check frame timing consistency

### Performance Issues

1. Disable unused controls
2. Reduce raycasting frequency for DragControls
3. Use static movement mode for TrackballControls
4. Limit draggable object count

### Gizmo Not Visible

1. Verify object is attached: `controls.Attach(obj)`
2. Check gizmo size: `controls.Size = 2.0f`
3. Ensure camera can see object position
4. Verify controls are enabled: `controls.Enabled = true`
