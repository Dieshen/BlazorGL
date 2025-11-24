# BlazorGL.Controls

Camera and object controls for BlazorGL, providing intuitive interaction with 3D scenes.

## OrbitControls

Allows the camera to orbit around a target point with mouse/touch interaction.

### Features

- Mouse and touch support
- Rotation (left mouse button / single touch)
- Zoom (mouse wheel / pinch gesture)
- Pan (right mouse button / two-finger drag)
- Damping for smooth motion
- Configurable constraints (distance, angles)
- Auto-rotation mode

### Basic Usage

```csharp
@using BlazorGL.Controls
@using BlazorGL.Core.Cameras
@inject IJSRuntime JSRuntime

<canvas id="gl-canvas" width="800" height="600"></canvas>

@code {
    private OrbitControls? controls;
    private PerspectiveCamera? camera;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Create camera
            camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            camera.Position = new Vector3(0, 5, 10);

            // Create controls
            controls = new OrbitControls(camera, JSRuntime, "gl-canvas");
            await controls.InitializeAsync();

            // Configure controls
            controls.EnableDamping = true;
            controls.DampingFactor = 0.05f;
            controls.MinDistance = 5f;
            controls.MaxDistance = 50f;

            // Start render loop
            StartRenderLoop();
        }
    }

    private void StartRenderLoop()
    {
        // Your render loop
        // Call controls.Update(deltaTime) on each frame
    }

    public async ValueTask DisposeAsync()
    {
        if (controls != null)
            await controls.DisposeAsync();
    }
}
```

### Properties

#### Enable/Disable Features
- `Enabled` - Enable or disable controls completely
- `EnableRotate` - Enable/disable camera rotation
- `EnableZoom` - Enable/disable zoom
- `EnablePan` - Enable/disable panning

#### Constraints
- `MinDistance` / `MaxDistance` - Limit zoom distance
- `MinPolarAngle` / `MaxPolarAngle` - Restrict vertical rotation (0 to PI)
- `MinAzimuthAngle` / `MaxAzimuthAngle` - Restrict horizontal rotation

#### Damping
- `EnableDamping` - Enable inertia/damping
- `DampingFactor` - Damping factor (0.0 to 1.0, default 0.05)

#### Speeds
- `RotateSpeed` - Rotation sensitivity (default 1.0)
- `ZoomSpeed` - Zoom sensitivity (default 1.0)
- `PanSpeed` - Pan sensitivity (default 1.0)

#### Auto-Rotation
- `AutoRotate` - Automatically rotate camera
- `AutoRotateSpeed` - Auto-rotation speed (default 2.0)

#### Target
- `Target` - Point the camera orbits around (default Vector3.Zero)

### Methods

- `Update(float deltaTime)` - Must be called every frame in render loop
- `Reset()` - Reset controls to initial state
- `InitializeAsync()` - Initialize JavaScript interop (call once)
- `DisposeAsync()` - Clean up resources

### Example with Animation Loop

```csharp
private float lastTime = 0f;

private async Task RenderLoop()
{
    while (!disposed)
    {
        float currentTime = (float)DateTime.Now.TimeOfDay.TotalSeconds;
        float deltaTime = currentTime - lastTime;
        lastTime = currentTime;

        // Update controls
        controls.Update(deltaTime);

        // Render scene
        renderer.Render(scene, camera);

        await Task.Delay(16); // ~60 FPS
    }
}
```

### Mouse/Touch Input

**Mouse:**
- Left button: Rotate
- Right button: Pan
- Wheel: Zoom

**Touch:**
- One finger: Rotate
- Two fingers: Zoom + Pan

## TrackballControls

Free 360° camera rotation using quaternion-based math (no gimbal lock).

### Features

- Quaternion-based rotation (no gimbal lock)
- Virtual trackball projection
- Dynamic or static movement modes
- Optional roll constraint
- Distance constraints
- Screen-space panning
- Touch and mouse support

### Basic Usage

```csharp
@using BlazorGL.Controls
@using BlazorGL.Core.Cameras
@inject IJSRuntime JSRuntime

<canvas id="gl-canvas" width="800" height="600"></canvas>

@code {
    private TrackballControls? controls;
    private PerspectiveCamera? camera;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            camera.Position = new Vector3(0, 0, 5);

            controls = new TrackballControls(camera, JSRuntime, "gl-canvas");
            await controls.InitializeAsync();

            // Configure controls
            controls.RotateSpeed = 1.0f;
            controls.StaticMoving = false; // Enable momentum
            controls.DynamicDampingFactor = 0.2f;
            controls.NoRoll = false; // Allow free rotation
            controls.Screen = new Vector2(800, 600);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (controls != null)
            await controls.DisposeAsync();
    }
}
```

## TransformControls

Interactive 3D gizmos for translating, rotating, and scaling objects.

### Features

- Three transform modes: Translate, Rotate, Scale
- World space and local space modes
- Axis constraints (X, Y, Z, XY, YZ, XZ, XYZ)
- Snap to grid (translation)
- Snap to angle (rotation)
- Visual gizmo feedback
- Interactive dragging
- Comprehensive events

### Basic Usage

```csharp
@using BlazorGL.Controls
@using BlazorGL.Core
@using BlazorGL.Core.Cameras
@using BlazorGL.Core.Rendering
@inject IJSRuntime JSRuntime

<canvas id="gl-canvas" width="800" height="600"></canvas>

@code {
    private TransformControls? controls;
    private PerspectiveCamera? camera;
    private Renderer? renderer;
    private Object3D? selectedObject;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            renderer = new Renderer();

            controls = new TransformControls(camera, renderer, JSRuntime, "gl-canvas");
            await controls.InitializeAsync();

            // Configure controls
            controls.Mode = TransformMode.Translate;
            controls.Space = TransformSpace.World;
            controls.TranslationSnap = 0.5f; // Snap to 0.5 unit grid

            // Attach to object
            selectedObject = new Object3D();
            controls.Attach(selectedObject);

            // Subscribe to events
            controls.DraggingChanged += (s, dragging) =>
            {
                // Disable orbit controls during transform
                orbitControls.Enabled = !dragging;
            };
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (controls != null)
            await controls.DisposeAsync();
    }
}
```

## DragControls

Click-and-drag object manipulation using raycasting.

### Features

- Raycaster-based object picking
- Drag along camera-facing plane
- Multiple object support
- Recursive child picking
- Hover detection
- Comprehensive drag events

### Basic Usage

```csharp
@using BlazorGL.Controls
@using BlazorGL.Core
@using BlazorGL.Core.Cameras
@using BlazorGL.Core.Rendering
@inject IJSRuntime JSRuntime

<canvas id="gl-canvas" width="800" height="600"></canvas>

@code {
    private DragControls? controls;
    private PerspectiveCamera? camera;
    private Renderer? renderer;
    private List<Object3D> draggableObjects = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            camera = new PerspectiveCamera(75, 800f / 600f, 0.1f, 1000f);
            renderer = new Renderer();

            // Add draggable objects
            draggableObjects.Add(new Object3D { Position = new Vector3(-2, 0, 0) });
            draggableObjects.Add(new Object3D { Position = new Vector3(0, 0, 0) });
            draggableObjects.Add(new Object3D { Position = new Vector3(2, 0, 0) });

            controls = new DragControls(camera, draggableObjects, renderer, JSRuntime, "gl-canvas");
            await controls.InitializeAsync();

            // Subscribe to events
            controls.DragStart += (s, e) => Console.WriteLine($"Dragging: {e.Object}");
            controls.DragEnd += (s, e) => Console.WriteLine($"Released: {e.Object}");
            controls.HoverOn += (s, e) => e.Object.Scale *= 1.1f;
            controls.HoverOff += (s, e) => e.Object.Scale /= 1.1f;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (controls != null)
            await controls.DisposeAsync();
    }
}
```

## Complete Documentation

For comprehensive documentation including API reference, performance considerations, and advanced usage patterns, see:

- [CONTROLS_GUIDE.md](../../docs/CONTROLS_GUIDE.md) - Complete guide with examples and best practices

## Requirements

- BlazorGL.Core >= 1.0.0
- Microsoft.JSInterop >= 9.0.0
- .NET 9.0 or later

## Installation

```bash
dotnet add package BlazorGL.Controls
```

## License

MIT License - see LICENSE file for details.
