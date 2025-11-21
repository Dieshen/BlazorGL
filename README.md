# BlazorGL

**Version 1.0.0**

A 3D rendering library for Blazor WebAssembly that keeps the API in C# while using a JS-backed WebGL module under the hood.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/) [![WebGL](https://img.shields.io/badge/WebGL-2.0%20%2B%201.0%20fallback-orange)](https://www.khronos.org/webgl/) [![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

## Features

- C#-first rendering API (JS only for WebGL interop glue)
- Scene graph with cameras, lights, meshes, lines, points, sprites
- PBR and classic materials, textures/render targets, shadow support
- Built-in primitives plus animation, raycasting, post-processing, particles
- Asset loading for GLTF/GLB, OBJ/MTL, and STL
- Razor class library static asset for the WebGL module (`_content/BlazorGL/blazorgl.webgl.js`)

## Installation

```bash
# Create new Blazor WASM project
dotnet new blazorwasm -n MyBlazorGLApp
cd MyBlazorGLApp

# Add BlazorGL
dotnet add package BlazorGL
```

## Quick Start

Create a new Razor component (`RotatingCube.razor`):

```csharp
@page "/cube"
@using BlazorGL
@using System.Numerics
@inject IJSRuntime JS

<h3>Rotating Cube</h3>
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
        _renderer = new Renderer();
        await _renderer.InitializeAsync(_canvas, JS); // Requires IJSRuntime for WebGL interop
        _renderer.SetClearColor(new Color(0.1f, 0.1f, 0.1f));

        _scene = new Scene();

        _camera = new PerspectiveCamera(75f, 800f / 600f, 0.1f, 1000f)
        {
            Position = new Vector3(0, 0, 5)
        };

        _scene.Add(new AmbientLight(Color.White, 0.5f));
        _scene.Add(new DirectionalLight
        {
            Color = Color.White,
            Intensity = 0.8f,
            Position = new Vector3(5, 5, 5)
        });

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
            _cube!.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.01f);
            _cube.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.005f);
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

## Build & Test

1. Build the WebGL interop asset (TypeScript → JS) in `src/BlazorGL`:
   ```bash
   cd src/BlazorGL
   npm install
   npm run build:webgl
   ```
   The compiled module lives at `src/BlazorGL/wwwroot/blazorgl.webgl.js` and is served to consumers at `_content/BlazorGL/blazorgl.webgl.js`.
2. Build the solution:
   ```bash
   dotnet build BlazorGL.sln
   ```
3. Run unit tests:
   ```bash
   dotnet test tests/BlazorGL.Tests/BlazorGL.Tests.csproj
   ```
4. Integration tests (Playwright/browser) live under `tests/BlazorGL.IntegrationTests` and may require browser drivers. See docs in `docs/`.

## Project Structure

```
BlazorGL/
├─ src/
│  ├─ BlazorGL.Core/          # Core rendering engine
│  ├─ BlazorGL.Loaders/       # Asset loaders (GLTF, Textures)
│  ├─ BlazorGL.Extensions/    # Advanced features (Animation, Particles, etc.)
│  └─ BlazorGL/               # Main library (aggregates all, serves JS asset)
├─ examples/                  # Example project shells
└─ tests/                     # Unit and integration tests
```

## Documentation

Additional docs previously in the root are now under `docs/`. Key references:
- `docs/BlazorGL_Documentation.md`
- `docs/TESTING.md`
- `docs/INTEGRATION_TESTING.md`

## Browser Support

- WebGL 2.0 preferred; WebGL 1.0 fallback where available
- Tested primarily on modern Chromium/Firefox/Edge; other browsers may require feature fallback paths

## License

MIT License - see `LICENSE`.
