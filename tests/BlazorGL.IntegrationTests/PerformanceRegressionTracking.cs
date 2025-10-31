using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using System.Text.Json;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Automated performance regression tracking
/// Compares current performance against historical baselines
/// Alerts when performance degrades beyond acceptable thresholds
/// </summary>
public class PerformanceRegressionTracking : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;
    private const string HistoryFile = "performance-history.json";
    private const string BaselineFile = "performance-baseline.json";
    private const double RegressionThreshold = 0.10; // 10% degradation allowed

    public PerformanceRegressionTracking(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright!.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--use-gl=swiftshader", "--disable-gpu-sandbox" }
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }

    [Fact]
    public async Task Regression_DrawCallPerformance_NoSignificantDegradation()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            // Run current performance test
            var current = await MeasureDrawCallPerformance(page);

            // Load baseline
            var baseline = LoadBaseline("DrawCallPerformance");

            // Save current measurement to history
            SaveToHistory("DrawCallPerformance", current);

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  Performance Regression: Draw Call Performance            ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Current:  {current.Value:F2} {current.Unit}");

            if (baseline != null)
            {
                _output.WriteLine($"  Baseline: {baseline.Value:F2} {baseline.Unit}");

                var change = ((current.Value - baseline.Value) / baseline.Value);
                var changePercent = change * 100;

                _output.WriteLine($"  Change:   {changePercent:+F2}%");

                if (change < -RegressionThreshold)
                {
                    _output.WriteLine($"  ⚠ WARNING: Performance degradation detected!");
                    Assert.Fail($"Performance degraded by {-changePercent:F1}% (threshold: {RegressionThreshold * 100}%)");
                }
                else if (change > 0.05)
                {
                    _output.WriteLine($"  ✓ Performance improvement!");
                }
                else
                {
                    _output.WriteLine($"  ✓ Performance within acceptable range");
                }
            }
            else
            {
                _output.WriteLine($"  Baseline: Not set");
                _output.WriteLine($"  ℹ Setting current as baseline...");
                SaveBaseline("DrawCallPerformance", current);
            }

            // Trend analysis
            var history = LoadHistory("DrawCallPerformance");
            if (history.Count >= 5)
            {
                AnalyzeTrend("DrawCallPerformance", history);
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Regression_RenderingThroughput_NoSignificantDegradation()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var current = await MeasureRenderingThroughput(page);
            var baseline = LoadBaseline("RenderingThroughput");

            SaveToHistory("RenderingThroughput", current);

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  Performance Regression: Rendering Throughput              ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Current:  {current.Value:F2} {current.Unit}");

            if (baseline != null)
            {
                _output.WriteLine($"  Baseline: {baseline.Value:F2} {baseline.Unit}");

                var change = ((current.Value - baseline.Value) / baseline.Value);
                var changePercent = change * 100;

                _output.WriteLine($"  Change:   {changePercent:+F2}%");

                if (change < -RegressionThreshold)
                {
                    Assert.Fail($"Rendering throughput degraded by {-changePercent:F1}%");
                }
            }
            else
            {
                SaveBaseline("RenderingThroughput", current);
                _output.WriteLine($"  Baseline set");
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Regression_BufferUploadSpeed_NoSignificantDegradation()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var current = await MeasureBufferUploadSpeed(page);
            var baseline = LoadBaseline("BufferUploadSpeed");

            SaveToHistory("BufferUploadSpeed", current);

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  Performance Regression: Buffer Upload Speed              ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Current:  {current.Value:F2} {current.Unit}");

            if (baseline != null)
            {
                _output.WriteLine($"  Baseline: {baseline.Value:F2} {baseline.Unit}");

                var change = ((current.Value - baseline.Value) / baseline.Value);
                var changePercent = change * 100;

                _output.WriteLine($"  Change:   {changePercent:+F2}%");

                if (change < -RegressionThreshold)
                {
                    Assert.Fail($"Buffer upload speed degraded by {-changePercent:F1}%");
                }
            }
            else
            {
                SaveBaseline("BufferUploadSpeed", current);
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Regression_ShaderCompilationTime_NoSignificantDegradation()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var current = await MeasureShaderCompilationTime(page);
            var baseline = LoadBaseline("ShaderCompilationTime");

            SaveToHistory("ShaderCompilationTime", current);

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  Performance Regression: Shader Compilation               ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Current:  {current.Value:F2} {current.Unit}");

            if (baseline != null)
            {
                _output.WriteLine($"  Baseline: {baseline.Value:F2} {baseline.Unit}");

                var change = ((current.Value - baseline.Value) / baseline.Value);
                var changePercent = change * 100;

                _output.WriteLine($"  Change:   {changePercent:+F2}%");

                // For compilation time, lower is better, so reverse the check
                if (change > RegressionThreshold)
                {
                    Assert.Fail($"Shader compilation time increased by {changePercent:F1}%");
                }
            }
            else
            {
                SaveBaseline("ShaderCompilationTime", current);
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Regression_MemoryUsage_NoSignificantIncrease()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var current = await MeasureMemoryUsage(page);
            var baseline = LoadBaseline("MemoryUsage");

            SaveToHistory("MemoryUsage", current);

            _output.WriteLine($"╔═══════════════════════════════════════════════════════════╗");
            _output.WriteLine($"║  Performance Regression: Memory Usage                      ║");
            _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝");
            _output.WriteLine($"  Current:  {current.Value:F2} {current.Unit}");

            if (baseline != null)
            {
                _output.WriteLine($"  Baseline: {baseline.Value:F2} {baseline.Unit}");

                var change = ((current.Value - baseline.Value) / baseline.Value);
                var changePercent = change * 100;

                _output.WriteLine($"  Change:   {changePercent:+F2}%");

                // Memory increase is bad
                if (change > RegressionThreshold)
                {
                    Assert.Fail($"Memory usage increased by {changePercent:F1}%");
                }
            }
            else
            {
                SaveBaseline("MemoryUsage", current);
            }
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public void Regression_GeneratePerformanceReport()
    {
        _output.WriteLine($"\n╔═══════════════════════════════════════════════════════════╗");
        _output.WriteLine($"║  Performance Regression: Historical Report                 ║");
        _output.WriteLine($"╚═══════════════════════════════════════════════════════════╝\n");

        var metrics = new[] {
            "DrawCallPerformance",
            "RenderingThroughput",
            "BufferUploadSpeed",
            "ShaderCompilationTime",
            "MemoryUsage"
        };

        foreach (var metric in metrics)
        {
            var history = LoadHistory(metric);
            var baseline = LoadBaseline(metric);

            if (history.Count == 0) continue;

            _output.WriteLine($"\n{metric}:");
            _output.WriteLine($"  Baseline: {baseline?.Value:F2} {baseline?.Unit ?? ""}");
            _output.WriteLine($"  Current:  {history.Last().Value:F2} {history.Last().Unit}");
            _output.WriteLine($"  Samples:  {history.Count}");

            if (baseline != null && history.Count > 0)
            {
                var current = history.Last();
                var change = ((current.Value - baseline.Value) / baseline.Value) * 100;
                _output.WriteLine($"  Change:   {change:+F2}%");

                if (Math.Abs(change) > RegressionThreshold * 100)
                {
                    _output.WriteLine($"  Status:   ⚠ {(change > 0 ? "REGRESSION" : "IMPROVEMENT")}");
                }
                else
                {
                    _output.WriteLine($"  Status:   ✓ Stable");
                }
            }

            if (history.Count >= 5)
            {
                var trend = CalculateTrend(history.TakeLast(5).Select(h => h.Value).ToList());
                _output.WriteLine($"  Trend:    {(trend > 0 ? "↑" : trend < 0 ? "↓" : "→")} {Math.Abs(trend):F2}%");
            }
        }
    }

    // Measurement methods
    private async Task<PerformanceMetric> MeasureDrawCallPerformance(IPage page)
    {
        var value = await page.EvaluateAsync<double>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                const vs = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vs, 'attribute vec3 p; void main() { gl_Position = vec4(p, 1.0); }');
                gl.compileShader(vs);

                const fs = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fs, 'precision highp float; void main() { gl_FragColor = vec4(1.0); }');
                gl.compileShader(fs);

                const prog = gl.createProgram();
                gl.attachShader(prog, vs);
                gl.attachShader(prog, fs);
                gl.linkProgram(prog);
                gl.useProgram(prog);

                const buf = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buf);
                gl.bufferData(gl.ARRAY_BUFFER, new Float32Array([0,0.5,0,-0.5,-0.5,0,0.5,-0.5,0]), gl.STATIC_DRAW);

                const loc = gl.getAttribLocation(prog, 'p');
                gl.vertexAttribPointer(loc, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(loc);

                const iterations = 5000;
                const start = performance.now();

                for (let i = 0; i < iterations; i++) {
                    gl.drawArrays(gl.TRIANGLES, 0, 3);
                }

                gl.finish();
                const elapsed = performance.now() - start;

                gl.deleteBuffer(buf);
                gl.deleteProgram(prog);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return iterations / (elapsed / 1000);
            }
        ");

        return new PerformanceMetric
        {
            Name = "DrawCallPerformance",
            Value = value,
            Unit = "calls/sec",
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<PerformanceMetric> MeasureRenderingThroughput(IPage page)
    {
        var value = await page.EvaluateAsync<double>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                const iterations = 100;
                const start = performance.now();

                for (let i = 0; i < iterations; i++) {
                    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                }

                gl.finish();
                const elapsed = performance.now() - start;

                return 1000 / (elapsed / iterations);
            }
        ");

        return new PerformanceMetric
        {
            Name = "RenderingThroughput",
            Value = value,
            Unit = "FPS",
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<PerformanceMetric> MeasureBufferUploadSpeed(IPage page)
    {
        var value = await page.EvaluateAsync<double>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                const size = 1024 * 1024; // 1MB
                const data = new Float32Array(size / 4);

                const start = performance.now();

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                gl.bufferData(gl.ARRAY_BUFFER, data, gl.STATIC_DRAW);
                gl.finish();

                const elapsed = performance.now() - start;

                gl.deleteBuffer(buffer);

                return size / (elapsed / 1000) / (1024 * 1024); // MB/sec
            }
        ");

        return new PerformanceMetric
        {
            Name = "BufferUploadSpeed",
            Value = value,
            Unit = "MB/sec",
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<PerformanceMetric> MeasureShaderCompilationTime(IPage page)
    {
        var value = await page.EvaluateAsync<double>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                const iterations = 10;
                const start = performance.now();

                for (let i = 0; i < iterations; i++) {
                    const vs = gl.createShader(gl.VERTEX_SHADER);
                    gl.shaderSource(vs, 'attribute vec3 position; void main() { gl_Position = vec4(position, 1.0); }');
                    gl.compileShader(vs);

                    const fs = gl.createShader(gl.FRAGMENT_SHADER);
                    gl.shaderSource(fs, 'precision highp float; void main() { gl_FragColor = vec4(1.0); }');
                    gl.compileShader(fs);

                    const prog = gl.createProgram();
                    gl.attachShader(prog, vs);
                    gl.attachShader(prog, fs);
                    gl.linkProgram(prog);

                    gl.deleteProgram(prog);
                    gl.deleteShader(vs);
                    gl.deleteShader(fs);
                }

                const elapsed = performance.now() - start;

                return elapsed / iterations;
            }
        ");

        return new PerformanceMetric
        {
            Name = "ShaderCompilationTime",
            Value = value,
            Unit = "ms",
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<PerformanceMetric> MeasureMemoryUsage(IPage page)
    {
        var value = await page.EvaluateAsync<double>(@"
            () => {
                return performance.memory ? performance.memory.usedJSHeapSize / (1024 * 1024) : 0;
            }
        ");

        return new PerformanceMetric
        {
            Name = "MemoryUsage",
            Value = value,
            Unit = "MB",
            Timestamp = DateTime.UtcNow
        };
    }

    // Persistence methods
    private void SaveBaseline(string metricName, PerformanceMetric metric)
    {
        var baselines = File.Exists(BaselineFile)
            ? JsonSerializer.Deserialize<Dictionary<string, PerformanceMetric>>(File.ReadAllText(BaselineFile))
            : new Dictionary<string, PerformanceMetric>();

        baselines ??= new Dictionary<string, PerformanceMetric>();
        baselines[metricName] = metric;

        File.WriteAllText(BaselineFile, JsonSerializer.Serialize(baselines, new JsonSerializerOptions { WriteIndented = true }));
    }

    private PerformanceMetric? LoadBaseline(string metricName)
    {
        if (!File.Exists(BaselineFile)) return null;

        var baselines = JsonSerializer.Deserialize<Dictionary<string, PerformanceMetric>>(File.ReadAllText(BaselineFile));

        return baselines?.GetValueOrDefault(metricName);
    }

    private void SaveToHistory(string metricName, PerformanceMetric metric)
    {
        var allHistory = File.Exists(HistoryFile)
            ? JsonSerializer.Deserialize<Dictionary<string, List<PerformanceMetric>>>(File.ReadAllText(HistoryFile))
            : new Dictionary<string, List<PerformanceMetric>>();

        allHistory ??= new Dictionary<string, List<PerformanceMetric>>();

        if (!allHistory.ContainsKey(metricName))
        {
            allHistory[metricName] = new List<PerformanceMetric>();
        }

        allHistory[metricName].Add(metric);

        // Keep last 100 entries
        if (allHistory[metricName].Count > 100)
        {
            allHistory[metricName] = allHistory[metricName].TakeLast(100).ToList();
        }

        File.WriteAllText(HistoryFile, JsonSerializer.Serialize(allHistory, new JsonSerializerOptions { WriteIndented = true }));
    }

    private List<PerformanceMetric> LoadHistory(string metricName)
    {
        if (!File.Exists(HistoryFile)) return new List<PerformanceMetric>();

        var allHistory = JsonSerializer.Deserialize<Dictionary<string, List<PerformanceMetric>>>(File.ReadAllText(HistoryFile));

        return allHistory?.GetValueOrDefault(metricName) ?? new List<PerformanceMetric>();
    }

    private void AnalyzeTrend(string metricName, List<PerformanceMetric> history)
    {
        if (history.Count < 5) return;

        var recent = history.TakeLast(5).Select(h => h.Value).ToList();
        var trend = CalculateTrend(recent);

        _output.WriteLine($"  5-run trend: {(trend > 0 ? "↑" : trend < 0 ? "↓" : "→")} {Math.Abs(trend):F2}%");

        if (Math.Abs(trend) > 5)
        {
            _output.WriteLine($"  ⚠ Trend shows {(trend > 0 ? "improving" : "degrading")} performance");
        }
    }

    private double CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return 0;

        var first = values.Take(values.Count / 2).Average();
        var last = values.Skip(values.Count / 2).Average();

        return ((last - first) / first) * 100;
    }

    public class PerformanceMetric
    {
        public string Name { get; set; } = "";
        public double Value { get; set; }
        public string Unit { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
