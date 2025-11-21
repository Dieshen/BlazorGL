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

## Future Controls

### TrackballControls
Full 360° rotation without up-vector constraint (coming soon).

### TransformControls
Interactive gizmos for translating, rotating, and scaling objects (coming soon).

### DragControls
Click and drag objects with raycasting (coming soon).

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
