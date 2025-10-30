# BlazorGL

**Version 1.0.0**

A comprehensive 3D rendering library for Blazor WebAssembly using pure C#. No JavaScript required!

## 🚀 Features

- **Pure C# API** - No JavaScript interop needed
- **Scene Graph Architecture** - Hierarchical object management with parent-child relationships
- **PBR Materials** - Physically-based rendering support with metalness/roughness workflow
- **Multiple Light Types** - Ambient, directional, point, and spot lights
- **Built-in Geometries** - Box, Sphere, Plane, Cylinder, Torus, and custom geometry support
- **Asset Loading** - GLTF model loading and texture support
- **Animation System** - Keyframe animation with interpolation
- **Post-Processing** - Effects pipeline (extensible)
- **Raycasting** - Object picking and intersection tests
- **Particle Systems** - Basic particle simulation
- **Type-Safe** - Leverage C#'s strong typing and modern language features

## 📦 Installation

```bash
# Create new Blazor WASM project
dotnet new blazorwasm -n MyBlazorGLApp
cd MyBlazorGLApp

# Add BlazorGL
dotnet add package BlazorGL
```

## 🎯 Quick Start

Create a new Razor component (`RotatingCube.razor`):

```csharp
@page "/cube"
@using BlazorGL
@using System.Numerics
@inject IJSRuntime JS

<h3>Rotating Cube Example</h3>
<canvas @ref="_canvas" style="width: 800px; height: 600px; border: 1px solid black;"></canvas>

@code {
    private ElementReference _canvas;
    private Renderer? _renderer;
    private Scene? _scene;
    private PerspectiveCamera? _camera;
    private Mesh? _cube;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeScene();
            await RenderLoop();
        }
    }

    private async Task InitializeScene()
    {
        // Create renderer
        _renderer = new Renderer();
        await _renderer.InitializeAsync(_canvas, JS);
        _renderer.SetClearColor(new Color(0.1f, 0.1f, 0.1f));

        // Create scene
        _scene = new Scene();

        // Create camera
        _camera = new PerspectiveCamera(75f, 800f / 600f, 0.1f, 1000f)
        {
            Position = new Vector3(0, 0, 5)
        };

        // Add lights
        _scene.Add(new AmbientLight(Color.White, 0.5f));
        _scene.Add(new DirectionalLight
        {
            Color = Color.White,
            Intensity = 0.8f,
            Position = new Vector3(5, 5, 5)
        });

        // Create cube
        _cube = new Mesh
        {
            Geometry = new BoxGeometry(1, 1, 1),
            Material = new StandardMaterial
            {
                Color = new Color(0.3f, 0.6f, 1.0f),
                Metalness = 0.5f,
                Roughness = 0.3f
            }
        };
        _scene.Add(_cube);
    }

    private async Task RenderLoop()
    {
        while (true)
        {
            // Rotate cube
            _cube!.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.01f);
            _cube.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.005f);

            // Render
            _renderer!.Render(_scene!, _camera!);

            await Task.Delay(16); // ~60 FPS
        }
    }

    public void Dispose()
    {
        _renderer?.Dispose();
    }
}
```

## 📚 Core Concepts

### Scene Graph

BlazorGL uses a hierarchical scene graph where objects can have parent-child relationships:

```csharp
var parent = new Group { Position = new Vector3(0, 1, 0) };
var child = new Mesh
{
    Geometry = new SphereGeometry(0.5f),
    Material = new BasicMaterial(),
    Position = new Vector3(2, 0, 0) // Relative to parent
};
parent.Add(child);
scene.Add(parent);
```

### Coordinate System

- **+X**: Right
- **+Y**: Up
- **+Z**: Toward viewer (out of screen)

### Available Geometries

```csharp
// Basic shapes
var box = new BoxGeometry(width, height, depth);
var sphere = new SphereGeometry(radius, widthSegments, heightSegments);
var plane = new PlaneGeometry(width, height);
var cylinder = new CylinderGeometry(radiusTop, radiusBottom, height);
var torus = new TorusGeometry(radius, tube);
```

### Materials

```csharp
// Basic material (no lighting)
var basic = new BasicMaterial { Color = Color.Red };

// Phong material (classic lighting)
var phong = new PhongMaterial
{
    Color = Color.White,
    Specular = new Color(0.1f, 0.1f, 0.1f),
    Shininess = 30f
};

// PBR Standard material
var standard = new StandardMaterial
{
    Color = Color.White,
    Metalness = 0.7f,
    Roughness = 0.3f
};

// Custom shader
var custom = new ShaderMaterial(vertexShaderSource, fragmentShaderSource);
```

### Lights

```csharp
// Ambient light
scene.Add(new AmbientLight(Color.White, 0.3f));

// Directional light (sun-like)
scene.Add(new DirectionalLight
{
    Color = Color.White,
    Intensity = 1.0f,
    Position = new Vector3(10, 10, 10)
});

// Point light
scene.Add(new PointLight
{
    Color = Color.White,
    Intensity = 2.0f,
    Position = new Vector3(0, 5, 0),
    Distance = 50f
});

// Spot light
scene.Add(new SpotLight
{
    Color = Color.White,
    Intensity = 1.5f,
    Angle = MathF.PI / 6,
    Penumbra = 0.1f
});
```

## 🎨 Loading Assets

### Textures

```csharp
var texture = await TextureLoader.LoadAsync("textures/diffuse.jpg");
material.Map = texture;

// Or create procedural textures
var checkerboard = TextureLoader.CreateCheckerboard(256, 32);
var solidColor = TextureLoader.CreateColor(Color.Red);
```

### GLTF Models

```csharp
var loader = new GLTFLoader();
var gltfScene = await loader.LoadAsync("models/robot.glb");
scene.Add(gltfScene.Scene);
```

## 🎬 Animation

```csharp
var clip = new AnimationClip
{
    Name = "Bounce",
    Duration = 2.0f,
    Tracks = new List<KeyframeTrack>
    {
        new KeyframeTrack
        {
            TargetProperty = "position",
            Times = new[] { 0f, 1f, 2f },
            Values = new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 2, 0),
                new Vector3(0, 0, 0)
            }
        }
    }
};

var mixer = new AnimationMixer(mesh);
mixer.Play(clip);

// In update loop
mixer.Update(deltaTime);
```

## 🎯 Raycasting

```csharp
private async Task OnCanvasClick(MouseEventArgs e)
{
    // Convert mouse to NDC
    var ndc = new Vector2(
        (2f * e.ClientX / _renderer.Width) - 1f,
        1f - (2f * e.ClientY / _renderer.Height)
    );

    var raycaster = new Raycaster();
    raycaster.SetFromCamera(ndc, _camera);

    var intersections = raycaster.IntersectObjects(_scene.Children);

    if (intersections.Count > 0)
    {
        var hit = intersections[0];
        Console.WriteLine($"Clicked on: {hit.Object.Name}");
    }
}
```

## 🏗️ Project Structure

```
BlazorGL/
├── src/
│   ├── BlazorGL.Core/          # Core rendering engine
│   ├── BlazorGL.Loaders/       # Asset loaders (GLTF, Textures)
│   ├── BlazorGL.Extensions/    # Advanced features (Animation, Particles, etc.)
│   └── BlazorGL/               # Main library (aggregates all)
├── examples/
│   └── BlazorGL.Examples/      # Example projects
└── tests/
    └── BlazorGL.Tests/         # Unit tests
```

## 📖 Documentation

Full documentation is available in `BlazorGL_Documentation.md`.

## 🌐 Browser Support

- Chrome 90+
- Firefox 88+
- Safari 15+
- Edge 90+

Requires WebGL 2.0 support.

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

MIT License - see LICENSE file for details

## 🦊 About

Made with 🦊 by Narcoleptic Fox LLC

---

**Happy Rendering!** 🎨✨
