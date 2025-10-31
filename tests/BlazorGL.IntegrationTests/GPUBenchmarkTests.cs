using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// GPU benchmark comparison tests
/// Compares performance across different GPU configurations and browsers
/// Tracks performance metrics over time for regression detection
/// </summary>
public class GPUBenchmarkTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;
    private const string BenchmarkResultsFile = "benchmark-results.json";

    public GPUBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        _playwright?.Dispose();
    }

    [Fact]
    public async Task GPUBenchmark_ChromiumSwiftShader_RecordsBaseline()
    {
        // Arrange
        var browser = await _playwright!.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--use-gl=swiftshader", "--disable-gpu-sandbox" }
        });
        var page = await browser.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var benchmark = await RunGPUBenchmark(page, "Chromium-SwiftShader");

            // Assert & Log
            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  GPU Benchmark: Chromium + SwiftShader                    ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Renderer: {benchmark.Renderer}");
            _output.WriteLine($"  Vendor: {benchmark.Vendor}");
            _output.WriteLine($"  Draw Calls/sec: {benchmark.DrawCallsPerSecond:F0}");
            _output.WriteLine($"  Triangle Throughput: {benchmark.TriangleThroughput:F0}/sec");
            _output.WriteLine($"  Fill Rate: {benchmark.FillRate:F2} Mpixels/sec");
            _output.WriteLine($"  Texture Bandwidth: {benchmark.TextureBandwidth:F2} MB/sec");
            _output.WriteLine($"  Overall Score: {benchmark.OverallScore:F0}");

            SaveBenchmarkResult(benchmark);

            Assert.True(benchmark.DrawCallsPerSecond > 1000, "Should achieve > 1000 draw calls/sec");
        }
        finally
        {
            await page.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Fact]
    public async Task GPUBenchmark_ChromiumHardware_ComparesWithSwiftShader()
    {
        // This test attempts hardware acceleration if available
        var browser = await _playwright!.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--disable-gpu-sandbox" } // Try hardware
        });
        var page = await browser.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var benchmark = await RunGPUBenchmark(page, "Chromium-Hardware");

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  GPU Benchmark: Chromium + Hardware Acceleration          ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Renderer: {benchmark.Renderer}");
            _output.WriteLine($"  Vendor: {benchmark.Vendor}");
            _output.WriteLine($"  Draw Calls/sec: {benchmark.DrawCallsPerSecond:F0}");
            _output.WriteLine($"  Triangle Throughput: {benchmark.TriangleThroughput:F0}/sec");
            _output.WriteLine($"  Fill Rate: {benchmark.FillRate:F2} Mpixels/sec");
            _output.WriteLine($"  Overall Score: {benchmark.OverallScore:F0}");

            SaveBenchmarkResult(benchmark);

            // Compare with SwiftShader baseline
            var swiftShaderResults = LoadBenchmarkResults()
                .FirstOrDefault(r => r.Configuration == "Chromium-SwiftShader");

            if (swiftShaderResults != null)
            {
                var improvement = ((benchmark.OverallScore - swiftShaderResults.OverallScore)
                    / swiftShaderResults.OverallScore) * 100;

                _output.WriteLine($"\n  Comparison to SwiftShader:");
                _output.WriteLine($"  Performance Difference: {improvement:+F1}%");

                if (improvement > 0)
                {
                    _output.WriteLine($"  ✓ Hardware acceleration is faster");
                }
            }

            Assert.True(benchmark.DrawCallsPerSecond > 500, "Should achieve > 500 draw calls/sec");
        }
        finally
        {
            await page.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Fact]
    public async Task GPUBenchmark_Firefox_ComparesPerformance()
    {
        var browser = await _playwright!.Firefox.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var benchmark = await RunGPUBenchmark(page, "Firefox");

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  GPU Benchmark: Firefox                                   ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Renderer: {benchmark.Renderer}");
            _output.WriteLine($"  Vendor: {benchmark.Vendor}");
            _output.WriteLine($"  Draw Calls/sec: {benchmark.DrawCallsPerSecond:F0}");
            _output.WriteLine($"  Overall Score: {benchmark.OverallScore:F0}");

            SaveBenchmarkResult(benchmark);

            Assert.True(benchmark.DrawCallsPerSecond > 500, "Firefox should achieve > 500 draw calls/sec");
        }
        finally
        {
            await page.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Fact]
    public async Task GPUBenchmark_WebKit_ComparesPerformance()
    {
        var browser = await _playwright!.Webkit.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var benchmark = await RunGPUBenchmark(page, "WebKit");

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  GPU Benchmark: WebKit                                    ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Renderer: {benchmark.Renderer}");
            _output.WriteLine($"  Vendor: {benchmark.Vendor}");
            _output.WriteLine($"  Draw Calls/sec: {benchmark.DrawCallsPerSecond:F0}");
            _output.WriteLine($"  Overall Score: {benchmark.OverallScore:F0}");

            SaveBenchmarkResult(benchmark);

            Assert.True(benchmark.DrawCallsPerSecond > 500, "WebKit should achieve > 500 draw calls/sec");
        }
        finally
        {
            await page.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Fact]
    public async Task GPUBenchmark_ComparativeAnalysis_AllBrowsers()
    {
        // Run benchmarks on all browsers and compare
        var results = new List<GPUBenchmarkResult>();

        // Chromium
        var chromium = await _playwright!.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--use-gl=swiftshader" }
        });
        var chromiumPage = await chromium.NewPageAsync();
        await chromiumPage.GotoAsync(TestAppUrl);
        await chromiumPage.WaitForSelectorAsync("#glCanvas");
        results.Add(await RunGPUBenchmark(chromiumPage, "Chromium"));
        await chromiumPage.CloseAsync();
        await chromium.CloseAsync();

        // Firefox
        var firefox = await _playwright.Firefox.LaunchAsync(new() { Headless = true });
        var firefoxPage = await firefox.NewPageAsync();
        await firefoxPage.GotoAsync(TestAppUrl);
        await firefoxPage.WaitForSelectorAsync("#glCanvas");
        results.Add(await RunGPUBenchmark(firefoxPage, "Firefox"));
        await firefoxPage.CloseAsync();
        await firefox.CloseAsync();

        // WebKit
        var webkit = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var webkitPage = await webkit.NewPageAsync();
        await webkitPage.GotoAsync(TestAppUrl);
        await webkitPage.WaitForSelectorAsync("#glCanvas");
        results.Add(await RunGPUBenchmark(webkitPage, "WebKit"));
        await webkitPage.CloseAsync();
        await webkit.CloseAsync();

        // Comparative Analysis
        _output.WriteLine($"\n╔═══════════════════════════════════════════════════════════╗");
        _output.WriteLine($"║  GPU Benchmark: Comparative Analysis                      ║");
        _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝\n");

        _output.WriteLine($"{"Browser",-15} {"Draw Calls/s",15} {"Triangles/s",15} {"Overall Score",15}");
        _output.WriteLine(new string('─', 60));

        foreach (var result in results.OrderByDescending(r => r.OverallScore))
        {
            _output.WriteLine($"{result.Configuration,-15} {result.DrawCallsPerSecond,15:F0} " +
                $"{result.TriangleThroughput,15:F0} {result.OverallScore,15:F0}");
        }

        var fastest = results.MaxBy(r => r.OverallScore);
        var slowest = results.MinBy(r => r.OverallScore);

        _output.WriteLine($"\n  Fastest: {fastest!.Configuration} ({fastest.OverallScore:F0})");
        _output.WriteLine($"  Slowest: {slowest!.Configuration} ({slowest.OverallScore:F0})");

        var variance = ((fastest.OverallScore - slowest.OverallScore) / slowest.OverallScore) * 100;
        _output.WriteLine($"  Performance Variance: {variance:F1}%");

        Assert.True(results.All(r => r.DrawCallsPerSecond > 300),
            "All browsers should achieve > 300 draw calls/sec");
    }

    private async Task<GPUBenchmarkResult> RunGPUBenchmark(IPage page, string configuration)
    {
        var result = await page.EvaluateAsync<GPUBenchmarkResult>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                if (!gl) {
                    return {
                        configuration: '',
                        renderer: 'No WebGL',
                        vendor: 'Unknown',
                        drawCallsPerSecond: 0,
                        triangleThroughput: 0,
                        fillRate: 0,
                        textureBandwidth: 0,
                        overallScore: 0,
                        timestamp: new Date().toISOString()
                    };
                }

                const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
                const renderer = debugInfo ? gl.getParameter(debugInfo.UNMASKED_RENDERER_WEBGL) : gl.getParameter(gl.RENDERER);
                const vendor = debugInfo ? gl.getParameter(debugInfo.UNMASKED_VENDOR_WEBGL) : gl.getParameter(gl.VENDOR);

                // Benchmark 1: Draw calls
                const vsSource = `
                    attribute vec3 position;
                    void main() { gl_Position = vec4(position, 1.0); }
                `;
                const fsSource = `
                    precision highp float;
                    void main() { gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0); }
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
                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                const posLoc = gl.getAttribLocation(program, 'position');
                gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(posLoc);

                // Draw call benchmark
                const drawCallIterations = 10000;
                const drawCallStart = performance.now();
                for (let i = 0; i < drawCallIterations; i++) {
                    gl.drawArrays(gl.TRIANGLES, 0, 3);
                }
                gl.finish();
                const drawCallEnd = performance.now();
                const drawCallTime = (drawCallEnd - drawCallStart) / 1000;
                const drawCallsPerSecond = drawCallIterations / drawCallTime;

                // Triangle throughput
                const trianglesPerCall = 1;
                const triangleThroughput = drawCallsPerSecond * trianglesPerCall;

                // Fill rate test
                const fillRateIterations = 100;
                const fillRateStart = performance.now();
                for (let i = 0; i < fillRateIterations; i++) {
                    gl.clear(gl.COLOR_BUFFER_BIT);
                }
                gl.finish();
                const fillRateEnd = performance.now();
                const fillRateTime = (fillRateEnd - fillRateStart) / 1000;
                const pixelsPerClear = canvas.width * canvas.height;
                const fillRate = (fillRateIterations * pixelsPerClear) / fillRateTime / 1000000; // Mpixels/sec

                // Texture bandwidth test
                const textureSize = 512;
                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);

                const texIterations = 10;
                const texData = new Uint8Array(textureSize * textureSize * 4);
                const texStart = performance.now();
                for (let i = 0; i < texIterations; i++) {
                    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, textureSize, textureSize, 0, gl.RGBA, gl.UNSIGNED_BYTE, texData);
                }
                gl.finish();
                const texEnd = performance.now();
                const texTime = (texEnd - texStart) / 1000;
                const bytesUploaded = texIterations * textureSize * textureSize * 4;
                const textureBandwidth = bytesUploaded / texTime / (1024 * 1024); // MB/sec

                // Cleanup
                gl.deleteBuffer(buffer);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);
                gl.deleteTexture(texture);

                // Calculate overall score (weighted combination)
                const overallScore =
                    (drawCallsPerSecond * 0.4) +
                    (triangleThroughput * 0.3) +
                    (fillRate * 10 * 0.2) +
                    (textureBandwidth * 0.1);

                return {
                    configuration: '',
                    renderer: renderer,
                    vendor: vendor,
                    drawCallsPerSecond: drawCallsPerSecond,
                    triangleThroughput: triangleThroughput,
                    fillRate: fillRate,
                    textureBandwidth: textureBandwidth,
                    overallScore: overallScore,
                    timestamp: new Date().toISOString()
                };
            }
        ");

        result.Configuration = configuration;
        return result;
    }

    private void SaveBenchmarkResult(GPUBenchmarkResult result)
    {
        var results = LoadBenchmarkResults();
        results.Add(result);

        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(BenchmarkResultsFile, json);
    }

    private List<GPUBenchmarkResult> LoadBenchmarkResults()
    {
        if (!File.Exists(BenchmarkResultsFile))
        {
            return new List<GPUBenchmarkResult>();
        }

        var json = File.ReadAllText(BenchmarkResultsFile);
        return JsonSerializer.Deserialize<List<GPUBenchmarkResult>>(json)
            ?? new List<GPUBenchmarkResult>();
    }

    public class GPUBenchmarkResult
    {
        public string Configuration { get; set; } = "";
        public string Renderer { get; set; } = "";
        public string Vendor { get; set; } = "";
        public double DrawCallsPerSecond { get; set; }
        public double TriangleThroughput { get; set; }
        public double FillRate { get; set; }
        public double TextureBandwidth { get; set; }
        public double OverallScore { get; set; }
        public string Timestamp { get; set; } = "";
    }
}
