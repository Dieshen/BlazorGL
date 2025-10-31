using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Performance benchmark tests for BlazorGL
/// Measures FPS, memory usage, draw call timing, and rendering performance
/// </summary>
public class PerformanceBenchmarkTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;

    public PerformanceBenchmarkTests(ITestOutputHelper output)
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
                "--disable-gpu-sandbox"
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
    public async Task Performance_SimpleScene_RendersFast()
    {
        // Arrange
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");
        await Task.Delay(1000);

        // Act - Measure render time for 100 frames
        var result = await _page.EvaluateAsync<PerformanceResult>(@"
            async () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const startTime = performance.now();
                const startMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                // Render 100 frames
                for (let i = 0; i < 100; i++) {
                    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                }

                const endTime = performance.now();
                const endMemory = performance.memory ? performance.memory.usedJSHeapSize : 0;

                return {
                    totalTime: endTime - startTime,
                    avgFrameTime: (endTime - startTime) / 100,
                    fps: 1000 / ((endTime - startTime) / 100),
                    memoryUsed: endMemory - startMemory
                };
            }
        ");

        // Assert
        _output.WriteLine($"Performance Results:");
        _output.WriteLine($"  Total Time: {result.TotalTime:F2}ms");
        _output.WriteLine($"  Avg Frame Time: {result.AvgFrameTime:F2}ms");
        _output.WriteLine($"  FPS: {result.Fps:F2}");
        _output.WriteLine($"  Memory Used: {result.MemoryUsed} bytes");

        Assert.True(result.TotalTime < 2000, "100 frames should render in under 2 seconds");
        Assert.True(result.AvgFrameTime < 20, "Average frame time should be under 20ms (50+ FPS)");
    }

    [Fact]
    public async Task Performance_DrawCalls_AreEfficient()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        // Measure draw call performance
        var result = await _page.EvaluateAsync<DrawCallPerformance>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create simple shader program
                const vsSource = `#version 300 es
                    in vec3 position;
                    void main() { gl_Position = vec4(position, 1.0); }
                `;
                const fsSource = `#version 300 es
                    precision highp float;
                    out vec4 fragColor;
                    void main() { fragColor = vec4(1.0); }
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

                // Create buffer
                const vertices = new Float32Array([
                    0.0, 0.5, 0.0,
                    -0.5, -0.5, 0.0,
                    0.5, -0.5, 0.0
                ]);

                const vao = gl.createVertexArray();
                gl.bindVertexArray(vao);

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                const posLoc = gl.getAttribLocation(program, 'position');
                gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(posLoc);

                // Benchmark draw calls
                const iterations = 1000;
                const startTime = performance.now();

                for (let i = 0; i < iterations; i++) {
                    gl.drawArrays(gl.TRIANGLES, 0, 3);
                }

                const endTime = performance.now();

                // Cleanup
                gl.deleteBuffer(buffer);
                gl.deleteVertexArray(vao);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return {
                    totalTime: endTime - startTime,
                    avgDrawCallTime: (endTime - startTime) / iterations,
                    drawCallsPerSecond: iterations / ((endTime - startTime) / 1000)
                };
            }
        ");

        // Assert
        _output.WriteLine($"Draw Call Performance:");
        _output.WriteLine($"  Total Time (1000 calls): {result.TotalTime:F2}ms");
        _output.WriteLine($"  Avg Draw Call Time: {result.AvgDrawCallTime:F4}ms");
        _output.WriteLine($"  Draw Calls/Second: {result.DrawCallsPerSecond:F0}");

        Assert.True(result.TotalTime < 1000, "1000 draw calls should complete in under 1 second");
        Assert.True(result.AvgDrawCallTime < 1, "Average draw call should be under 1ms");
    }

    [Fact]
    public async Task Performance_BufferUploads_AreFast()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<BufferPerformance>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const bufferSizes = [1024, 10240, 102400, 1024000]; // 1KB to 1MB
                const results = [];

                for (const size of bufferSizes) {
                    const data = new Float32Array(size / 4); // 4 bytes per float

                    const startTime = performance.now();

                    const buffer = gl.createBuffer();
                    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                    gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);

                    const endTime = performance.now();

                    gl.deleteBuffer(buffer);

                    results.push({
                        size: size,
                        uploadTime: endTime - startTime
                    });
                }

                return {
                    small: results[0].uploadTime,
                    medium: results[1].uploadTime,
                    large: results[2].uploadTime,
                    veryLarge: results[3].uploadTime
                };
            }
        ");

        // Assert
        _output.WriteLine($"Buffer Upload Performance:");
        _output.WriteLine($"  1KB: {result.Small:F4}ms");
        _output.WriteLine($"  10KB: {result.Medium:F4}ms");
        _output.WriteLine($"  100KB: {result.Large:F4}ms");
        _output.WriteLine($"  1MB: {result.VeryLarge:F4}ms");

        Assert.True(result.Small < 10, "1KB buffer upload should be under 10ms");
        Assert.True(result.Medium < 50, "10KB buffer upload should be under 50ms");
        Assert.True(result.Large < 100, "100KB buffer upload should be under 100ms");
        Assert.True(result.VeryLarge < 500, "1MB buffer upload should be under 500ms");
    }

    [Fact]
    public async Task Performance_TextureUploads_AreFast()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<TexturePerformance>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const textureSizes = [
                    { width: 64, height: 64 },
                    { width: 256, height: 256 },
                    { width: 512, height: 512 },
                    { width: 1024, height: 1024 }
                ];

                const results = [];

                for (const size of textureSizes) {
                    const data = new Uint8Array(size.width * size.height * 4);

                    const startTime = performance.now();

                    const texture = gl.createTexture();
                    gl.bindTexture(gl.TEXTURE_2D, texture);
                    gl.texImage2D(
                        gl.TEXTURE_2D, 0, gl.RGBA,
                        size.width, size.height, 0,
                        gl.RGBA, gl.UNSIGNED_BYTE, data
                    );

                    const endTime = performance.now();

                    gl.deleteTexture(texture);

                    results.push({
                        size: `${size.width}x${size.height}`,
                        uploadTime: endTime - startTime
                    });
                }

                return {
                    small: results[0].uploadTime,
                    medium: results[1].uploadTime,
                    large: results[2].uploadTime,
                    veryLarge: results[3].uploadTime
                };
            }
        ");

        // Assert
        _output.WriteLine($"Texture Upload Performance:");
        _output.WriteLine($"  64x64: {result.Small:F4}ms");
        _output.WriteLine($"  256x256: {result.Medium:F4}ms");
        _output.WriteLine($"  512x512: {result.Large:F4}ms");
        _output.WriteLine($"  1024x1024: {result.VeryLarge:F4}ms");

        Assert.True(result.Small < 10, "64x64 texture upload should be under 10ms");
        Assert.True(result.Medium < 50, "256x256 texture upload should be under 50ms");
        Assert.True(result.Large < 100, "512x512 texture upload should be under 100ms");
        Assert.True(result.VeryLarge < 200, "1024x1024 texture upload should be under 200ms");
    }

    [Fact]
    public async Task Performance_ShaderCompilation_IsReasonablyFast()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<ShaderPerformance>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const iterations = 10;
                const compileTimes = [];

                for (let i = 0; i < iterations; i++) {
                    const vsSource = `#version 300 es
                        in vec3 position;
                        in vec3 normal;
                        uniform mat4 modelViewMatrix;
                        uniform mat4 projectionMatrix;
                        out vec3 vNormal;
                        void main() {
                            vNormal = normal;
                            gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
                        }
                    `;

                    const fsSource = `#version 300 es
                        precision highp float;
                        in vec3 vNormal;
                        out vec4 fragColor;
                        uniform vec3 lightDirection;
                        uniform vec3 color;
                        void main() {
                            float diff = max(dot(normalize(vNormal), lightDirection), 0.0);
                            fragColor = vec4(color * diff, 1.0);
                        }
                    `;

                    const startTime = performance.now();

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

                    const endTime = performance.now();

                    compileTimes.push(endTime - startTime);

                    gl.deleteProgram(program);
                    gl.deleteShader(vs);
                    gl.deleteShader(fs);
                }

                const avg = compileTimes.reduce((a, b) => a + b, 0) / iterations;
                const min = Math.min(...compileTimes);
                const max = Math.max(...compileTimes);

                return {
                    avgCompileTime: avg,
                    minCompileTime: min,
                    maxCompileTime: max
                };
            }
        ");

        // Assert
        _output.WriteLine($"Shader Compilation Performance (10 iterations):");
        _output.WriteLine($"  Average: {result.AvgCompileTime:F4}ms");
        _output.WriteLine($"  Min: {result.MinCompileTime:F4}ms");
        _output.WriteLine($"  Max: {result.MaxCompileTime:F4}ms");

        Assert.True(result.AvgCompileTime < 50, "Average shader compilation should be under 50ms");
        Assert.True(result.MaxCompileTime < 100, "Max shader compilation should be under 100ms");
    }

    [Fact]
    public async Task Performance_MemoryLeaks_AreNotPresent()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var result = await _page.EvaluateAsync<MemoryLeakTest>(@"
            async () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Force garbage collection if available
                if (window.gc) window.gc();

                const initialMemory = performance.memory ?
                    performance.memory.usedJSHeapSize : 0;

                // Create and destroy resources 100 times
                for (let i = 0; i < 100; i++) {
                    // Create buffers
                    const buffer = gl.createBuffer();
                    gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(1000), gl.STATIC_DRAW);
                    gl.deleteBuffer(buffer);

                    // Create textures
                    const texture = gl.createTexture();
                    gl.bindTexture(gl.TEXTURE_2D, texture);
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 64, 64, 0, gl.RGBA, gl.UNSIGNED_BYTE,
                        new Uint8Array(64 * 64 * 4));
                    gl.deleteTexture(texture);

                    // Create shaders
                    const vs = gl.createShader(gl.VERTEX_SHADER);
                    const fs = gl.createShader(gl.FRAGMENT_SHADER);
                    gl.deleteShader(vs);
                    gl.deleteShader(fs);
                }

                // Force GC again
                if (window.gc) window.gc();

                await new Promise(resolve => setTimeout(resolve, 1000));

                const finalMemory = performance.memory ?
                    performance.memory.usedJSHeapSize : 0;

                return {
                    initialMemory: initialMemory,
                    finalMemory: finalMemory,
                    memoryGrowth: finalMemory - initialMemory,
                    hasMemoryAPI: performance.memory !== undefined
                };
            }
        ");

        // Assert
        _output.WriteLine($"Memory Leak Test:");
        _output.WriteLine($"  Initial Memory: {result.InitialMemory / 1024 / 1024:F2}MB");
        _output.WriteLine($"  Final Memory: {result.FinalMemory / 1024 / 1024:F2}MB");
        _output.WriteLine($"  Memory Growth: {result.MemoryGrowth / 1024:F2}KB");
        _output.WriteLine($"  Has Memory API: {result.HasMemoryAPI}");

        if (result.HasMemoryAPI)
        {
            // Allow up to 5MB growth for 100 iterations
            Assert.True(result.MemoryGrowth < 5 * 1024 * 1024,
                "Memory growth should be minimal (< 5MB for 100 create/delete cycles)");
        }
    }

    [Fact]
    public async Task Performance_Benchmark_Summary()
    {
        // This test runs a comprehensive benchmark and outputs a summary

        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        // Wait for all tests to complete
        await _page.WaitForFunctionAsync(@"
            () => document.querySelectorAll('#testResults li').length >= 8
        ", new() { Timeout = 30000 });

        var summary = await _page.EvaluateAsync<BenchmarkSummary>(@"
            () => {
                const tests = document.querySelectorAll('#testResults li');
                const passed = document.querySelectorAll('#testResults li.passed').length;
                const failed = document.querySelectorAll('#testResults li.failed').length;

                return {
                    totalTests: tests.length,
                    passed: passed,
                    failed: failed,
                    successRate: (passed / tests.length) * 100
                };
            }
        ");

        _output.WriteLine($"\n╔══════════════════════════════════════════════╗");
        _output.WriteLine($"║     BLAZORGL PERFORMANCE BENCHMARK SUMMARY    ║");
        _output.WriteLine($"╚══════════════════════════════════════════════╝");
        _output.WriteLine($"");
        _output.WriteLine($"  Integration Tests: {summary.TotalTests}");
        _output.WriteLine($"  Passed: {summary.Passed}");
        _output.WriteLine($"  Failed: {summary.Failed}");
        _output.WriteLine($"  Success Rate: {summary.SuccessRate:F1}%");
        _output.WriteLine($"");

        Assert.True(summary.SuccessRate >= 80, "At least 80% of integration tests should pass");
    }

    // Helper classes for type-safe result handling
    private class PerformanceResult
    {
        public double TotalTime { get; set; }
        public double AvgFrameTime { get; set; }
        public double Fps { get; set; }
        public long MemoryUsed { get; set; }
    }

    private class DrawCallPerformance
    {
        public double TotalTime { get; set; }
        public double AvgDrawCallTime { get; set; }
        public double DrawCallsPerSecond { get; set; }
    }

    private class BufferPerformance
    {
        public double Small { get; set; }
        public double Medium { get; set; }
        public double Large { get; set; }
        public double VeryLarge { get; set; }
    }

    private class TexturePerformance
    {
        public double Small { get; set; }
        public double Medium { get; set; }
        public double Large { get; set; }
        public double VeryLarge { get; set; }
    }

    private class ShaderPerformance
    {
        public double AvgCompileTime { get; set; }
        public double MinCompileTime { get; set; }
        public double MaxCompileTime { get; set; }
    }

    private class MemoryLeakTest
    {
        public long InitialMemory { get; set; }
        public long FinalMemory { get; set; }
        public long MemoryGrowth { get; set; }
        public bool HasMemoryAPI { get; set; }
    }

    private class BenchmarkSummary
    {
        public int TotalTests { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public double SuccessRate { get; set; }
    }
}
