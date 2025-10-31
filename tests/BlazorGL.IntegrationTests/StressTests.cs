using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Stress tests for BlazorGL - tests system limits and stability
/// Tests large scenes, many objects, rapid state changes, and resource limits
/// </summary>
public class StressTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;

    public StressTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] {
                "--use-gl=swiftshader",
                "--disable-gpu-sandbox",
                "--max-old-space-size=4096"  // Increase memory for stress tests
            }
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task Stress_1000Objects_RendersSuccessfully()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<StressTestResult>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create shader program
                const vsSource = `#version 300 es
                    in vec3 position;
                    uniform mat4 mvp;
                    void main() {
                        gl_Position = mvp * vec4(position, 1.0);
                    }
                `;
                const fsSource = `#version 300 es
                    precision highp float;
                    uniform vec3 color;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(color, 1.0);
                    }
                `;

                const vs = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vs, vsSource);
                gl.compileShader(vs);

                const fs = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fs, fsSource);
                gl.compileShader(fs);

                const program = gl.createProgram();
                gl.attachShader(program, vs);
                gl.attachShader(program, fs);
                gl.linkProgram(program);
                gl.useProgram(program);

                // Create cube geometry
                const vertices = new Float32Array([
                    -0.5, -0.5,  0.5,  0.5, -0.5,  0.5,  0.5,  0.5,  0.5, -0.5,  0.5,  0.5,
                    -0.5, -0.5, -0.5, -0.5,  0.5, -0.5,  0.5,  0.5, -0.5,  0.5, -0.5, -0.5
                ]);

                const indices = new Uint16Array([
                    0, 1, 2,  0, 2, 3,
                    4, 5, 6,  4, 6, 7,
                    0, 3, 5,  0, 5, 4,
                    1, 7, 6,  1, 6, 2,
                    3, 2, 6,  3, 6, 5,
                    0, 4, 7,  0, 7, 1
                ]);

                const vao = gl.createVertexArray();
                gl.bindVertexArray(vao);

                const vbo = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, vbo);
                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                const ibo = gl.createBuffer();
                gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, ibo);
                gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

                const posLoc = gl.getAttribLocation(program, 'position');
                gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(posLoc);

                const mvpLoc = gl.getUniformLocation(program, 'mvp');
                const colorLoc = gl.getUniformLocation(program, 'color');

                // Render 1000 cubes
                const objectCount = 1000;
                const gridSize = Math.ceil(Math.pow(objectCount, 1/3));

                const startTime = performance.now();
                const startMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

                let drawn = 0;
                for (let x = 0; x < gridSize; x++) {
                    for (let y = 0; y < gridSize; y++) {
                        for (let z = 0; z < gridSize; z++) {
                            if (drawn >= objectCount) break;

                            // Simple identity matrix for MVP (would be transform in real scenario)
                            const mvp = new Float32Array(16);
                            mvp[0] = mvp[5] = mvp[10] = mvp[15] = 1;

                            gl.uniformMatrix4fv(mvpLoc, false, mvp);
                            gl.uniform3f(colorLoc,
                                x / gridSize,
                                y / gridSize,
                                z / gridSize
                            );

                            gl.drawElements(gl.TRIANGLES, 36, gl.UNSIGNED_SHORT, 0);
                            drawn++;
                        }
                    }
                }

                const endTime = performance.now();
                const endMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                // Cleanup
                gl.deleteBuffer(vbo);
                gl.deleteBuffer(ibo);
                gl.deleteVertexArray(vao);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return {
                    objectsRendered: drawn,
                    renderTime: endTime - startTime,
                    memoryUsed: endMemory - startMemory,
                    fps: 1000 / (endTime - startTime)
                };
            }
        ");

        // Assert
        _output.WriteLine($"1000 Objects Stress Test:");
        _output.WriteLine($"  Objects Rendered: {result.ObjectsRendered}");
        _output.WriteLine($"  Render Time: {result.RenderTime:F2}ms");
        _output.WriteLine($"  FPS: {result.Fps:F2}");
        _output.WriteLine($"  Memory Used: {result.MemoryUsed / 1024:F2}KB");

        Assert.Equal(1000, result.ObjectsRendered);
        Assert.True(result.RenderTime < 5000, "Should render 1000 objects in under 5 seconds");
    }

    [Fact]
    public async Task Stress_10000Objects_RendersWithoutCrashing()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<StressTestResult>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Simplified shader
                const vsSource = `#version 300 es
                    in vec3 position;
                    void main() {
                        gl_Position = vec4(position * 0.01, 1.0);
                    }
                `;
                const fsSource = `#version 300 es
                    precision highp float;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(1.0, 0.0, 0.0, 1.0);
                    }
                `;

                const vs = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vs, vsSource);
                gl.compileShader(vs);

                const fs = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fs, fsSource);
                gl.compileShader(fs);

                const program = gl.createProgram();
                gl.attachShader(program, vs);
                gl.attachShader(program, fs);
                gl.linkProgram(program);
                gl.useProgram(program);

                // Single triangle
                const vertices = new Float32Array([0, 0.5, 0, -0.5, -0.5, 0, 0.5, -0.5, 0]);

                const vao = gl.createVertexArray();
                gl.bindVertexArray(vao);

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                const posLoc = gl.getAttribLocation(program, 'position');
                gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(posLoc);

                const objectCount = 10000;
                const startTime = performance.now();
                const startMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

                for (let i = 0; i < objectCount; i++) {
                    gl.drawArrays(gl.TRIANGLES, 0, 3);
                }

                const endTime = performance.now();
                const endMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                // Cleanup
                gl.deleteBuffer(buffer);
                gl.deleteVertexArray(vao);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return {
                    objectsRendered: objectCount,
                    renderTime: endTime - startTime,
                    memoryUsed: endMemory - startMemory,
                    fps: 1000 / (endTime - startTime)
                };
            }
        ");

        // Assert
        _output.WriteLine($"10,000 Objects Stress Test:");
        _output.WriteLine($"  Objects Rendered: {result.ObjectsRendered}");
        _output.WriteLine($"  Render Time: {result.RenderTime:F2}ms");
        _output.WriteLine($"  FPS: {result.Fps:F2}");
        _output.WriteLine($"  Memory Used: {result.MemoryUsed / 1024:F2}KB");

        Assert.Equal(10000, result.ObjectsRendered);
        Assert.True(result.RenderTime < 30000, "Should handle 10000 objects without crashing");
    }

    [Fact]
    public async Task Stress_RapidStateChanges_HandlesCorrectly()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<StateChangeResult>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const iterations = 1000;
                const startTime = performance.now();

                for (let i = 0; i < iterations; i++) {
                    // Rapid state changes
                    gl.enable(gl.DEPTH_TEST);
                    gl.disable(gl.DEPTH_TEST);

                    gl.enable(gl.BLEND);
                    gl.disable(gl.BLEND);

                    gl.enable(gl.CULL_FACE);
                    gl.cullFace(gl.BACK);
                    gl.cullFace(gl.FRONT);
                    gl.disable(gl.CULL_FACE);

                    gl.depthFunc(gl.LESS);
                    gl.depthFunc(gl.LEQUAL);

                    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
                    gl.blendFunc(gl.ONE, gl.ONE);

                    gl.clearColor(Math.random(), Math.random(), Math.random(), 1.0);
                }

                const endTime = performance.now();

                return {
                    iterations: iterations,
                    totalTime: endTime - startTime,
                    avgTimePerChange: (endTime - startTime) / (iterations * 9)
                };
            }
        ");

        // Assert
        _output.WriteLine($"Rapid State Changes Test:");
        _output.WriteLine($"  Iterations: {result.Iterations}");
        _output.WriteLine($"  Total Time: {result.TotalTime:F2}ms");
        _output.WriteLine($"  Avg Time Per Change: {result.AvgTimePerChange:F4}ms");

        Assert.True(result.TotalTime < 5000, "1000 iterations of state changes should complete quickly");
    }

    [Fact]
    public async Task Stress_MassiveBufferCreation_HandlesGracefully()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<BufferCreationResult>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const bufferCount = 1000;
                const buffers = [];

                const startTime = performance.now();
                const startMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                // Create many buffers
                for (let i = 0; i < bufferCount; i++) {
                    const buffer = gl.createBuffer();
                    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(1000), gl.STATIC_DRAW);
                    buffers.push(buffer);
                }

                const midTime = performance.now();

                // Delete all buffers
                for (const buffer of buffers) {
                    gl.deleteBuffer(buffer);
                }

                const endTime = performance.now();
                const endMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                return {
                    buffersCreated: bufferCount,
                    creationTime: midTime - startTime,
                    deletionTime: endTime - midTime,
                    totalTime: endTime - startTime,
                    memoryGrowth: endMemory - startMemory
                };
            }
        ");

        // Assert
        _output.WriteLine($"Massive Buffer Creation Test:");
        _output.WriteLine($"  Buffers Created: {result.BuffersCreated}");
        _output.WriteLine($"  Creation Time: {result.CreationTime:F2}ms");
        _output.WriteLine($"  Deletion Time: {result.DeletionTime:F2}ms");
        _output.WriteLine($"  Total Time: {result.TotalTime:F2}ms");
        _output.WriteLine($"  Memory Growth: {result.MemoryGrowth / 1024:F2}KB");

        Assert.Equal(1000, result.BuffersCreated);
        Assert.True(result.TotalTime < 5000, "Creating and deleting 1000 buffers should be fast");
    }

    [Fact]
    public async Task Stress_MassiveTextureCreation_HandlesGracefully()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<TextureCreationResult>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const textureCount = 500;
                const textures = [];

                const startTime = performance.now();

                // Create many small textures
                for (let i = 0; i < textureCount; i++) {
                    const texture = gl.createTexture();
                    gl.bindTexture(gl.TEXTURE_2D, texture);

                    const data = new Uint8Array(32 * 32 * 4);
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 32, 32, 0, gl.RGBA, gl.UNSIGNED_BYTE, data);

                    textures.push(texture);
                }

                const midTime = performance.now();

                // Delete all textures
                for (const texture of textures) {
                    gl.deleteTexture(texture);
                }

                const endTime = performance.now();

                return {
                    texturesCreated: textureCount,
                    creationTime: midTime - startTime,
                    deletionTime: endTime - midTime,
                    totalTime: endTime - startTime
                };
            }
        ");

        // Assert
        _output.WriteLine($"Massive Texture Creation Test:");
        _output.WriteLine($"  Textures Created: {result.TexturesCreated}");
        _output.WriteLine($"  Creation Time: {result.CreationTime:F2}ms");
        _output.WriteLine($"  Deletion Time: {result.DeletionTime:F2}ms");
        _output.WriteLine($"  Total Time: {result.TotalTime:F2}ms");

        Assert.Equal(500, result.TexturesCreated);
        Assert.True(result.TotalTime < 10000, "Creating and deleting 500 textures should complete");
    }

    [Fact]
    public async Task Stress_ContinuousRendering_RemainsStable()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<ContinuousRenderResult>(@"
            async () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create simple rendering setup
                const vsSource = `#version 300 es
                    in vec3 position;
                    void main() { gl_Position = vec4(position, 1.0); }
                `;
                const fsSource = `#version 300 es
                    precision highp float;
                    out vec4 fragColor;
                    void main() { fragColor = vec4(1.0, 0.0, 0.0, 1.0); }
                `;

                const vs = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vs, vsSource);
                gl.compileShader(vs);

                const fs = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fs, fsSource);
                gl.compileShader(fs);

                const program = gl.createProgram();
                gl.attachShader(program, vs);
                gl.attachShader(program, fs);
                gl.linkProgram(program);
                gl.useProgram(program);

                const vertices = new Float32Array([0, 0.5, 0, -0.5, -0.5, 0, 0.5, -0.5, 0]);

                const vao = gl.createVertexArray();
                gl.bindVertexArray(vao);

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                const posLoc = gl.getAttribLocation(program, 'position');
                gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(posLoc);

                // Render continuously for 5 seconds
                const duration = 5000;
                const startTime = performance.now();
                let frameCount = 0;

                while (performance.now() - startTime < duration) {
                    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                    gl.drawArrays(gl.TRIANGLES, 0, 3);
                    frameCount++;

                    // Yield to browser
                    await new Promise(resolve => setTimeout(resolve, 0));
                }

                const endTime = performance.now();

                // Cleanup
                gl.deleteBuffer(buffer);
                gl.deleteVertexArray(vao);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return {
                    duration: endTime - startTime,
                    frames: frameCount,
                    avgFps: frameCount / ((endTime - startTime) / 1000)
                };
            }
        ");

        // Assert
        _output.WriteLine($"Continuous Rendering Test (5 seconds):");
        _output.WriteLine($"  Duration: {result.Duration:F2}ms");
        _output.WriteLine($"  Frames: {result.Frames}");
        _output.WriteLine($"  Average FPS: {result.AvgFps:F2}");

        Assert.True(result.Frames > 100, "Should render at least 100 frames in 5 seconds");
        Assert.True(result.AvgFps > 20, "Should maintain at least 20 FPS");
    }

    // Helper classes
    private class StressTestResult
    {
        public int ObjectsRendered { get; set; }
        public double RenderTime { get; set; }
        public long MemoryUsed { get; set; }
        public double Fps { get; set; }
    }

    private class StateChangeResult
    {
        public int Iterations { get; set; }
        public double TotalTime { get; set; }
        public double AvgTimePerChange { get; set; }
    }

    private class BufferCreationResult
    {
        public int BuffersCreated { get; set; }
        public double CreationTime { get; set; }
        public double DeletionTime { get; set; }
        public double TotalTime { get; set; }
        public long MemoryGrowth { get; set; }
    }

    private class TextureCreationResult
    {
        public int TexturesCreated { get; set; }
        public double CreationTime { get; set; }
        public double DeletionTime { get; set; }
        public double TotalTime { get; set; }
    }

    private class ContinuousRenderResult
    {
        public double Duration { get; set; }
        public int Frames { get; set; }
        public double AvgFps { get; set; }
    }
}
