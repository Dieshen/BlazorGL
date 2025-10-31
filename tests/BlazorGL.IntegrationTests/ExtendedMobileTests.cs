using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Extended mobile device profiles testing
/// Tests a comprehensive matrix of mobile devices across different manufacturers,
/// screen sizes, and operating system versions
/// </summary>
public class ExtendedMobileTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;

    public ExtendedMobileTests(ITestOutputHelper output)
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

    [Theory]
    [InlineData("iPhone 13", "Webkit")]
    [InlineData("iPhone 13 Pro", "Webkit")]
    [InlineData("iPhone 13 Pro Max", "Webkit")]
    [InlineData("iPhone 13 Mini", "Webkit")]
    [InlineData("iPhone 12", "Webkit")]
    [InlineData("iPhone 12 Pro", "Webkit")]
    [InlineData("iPhone SE", "Webkit")]
    [InlineData("iPhone 11", "Webkit")]
    [InlineData("iPhone 11 Pro", "Webkit")]
    [InlineData("iPhone XR", "Webkit")]
    public async Task ExtendedMobile_iOSDevices_AllSupported(string deviceName, string engine)
    {
        // Arrange
        var device = _playwright!.Devices[deviceName];
        var browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var result = await page.EvaluateAsync<DeviceTestResult>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                    return {
                        hasWebGL: gl !== null,
                        webglVersion: gl ? (gl.getParameter(gl.VERSION).includes('2.0') ? 2 : 1) : 0,
                        renderer: gl ? gl.getParameter(gl.RENDERER) : 'None',
                        maxTextureSize: gl ? gl.getParameter(gl.MAX_TEXTURE_SIZE) : 0,
                        viewportWidth: window.innerWidth,
                        viewportHeight: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert & Log
            _output.WriteLine($"{deviceName}:");
            _output.WriteLine($"  WebGL: {result.HasWebGL} (v{result.WebglVersion})");
            _output.WriteLine($"  Renderer: {result.Renderer}");
            _output.WriteLine($"  Max Texture: {result.MaxTextureSize}");
            _output.WriteLine($"  Viewport: {result.ViewportWidth}x{result.ViewportHeight}");
            _output.WriteLine($"  DPR: {result.DevicePixelRatio}");

            Assert.True(result.HasWebGL, $"{deviceName} should support WebGL");
            Assert.True(result.MaxTextureSize >= 2048, $"{deviceName} should support at least 2048px textures");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Theory]
    [InlineData("Pixel 5", "Chromium")]
    [InlineData("Pixel 4", "Chromium")]
    [InlineData("Pixel 3", "Chromium")]
    [InlineData("Galaxy S9+", "Chromium")]
    [InlineData("Galaxy S8", "Chromium")]
    [InlineData("Galaxy Tab S4", "Chromium")]
    [InlineData("Nexus 7", "Chromium")]
    public async Task ExtendedMobile_AndroidDevices_AllSupported(string deviceName, string engine)
    {
        // Arrange
        var device = _playwright!.Devices[deviceName];
        var browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--use-gl=swiftshader" }
        });
        var context = await browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var result = await page.EvaluateAsync<DeviceTestResult>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                    return {
                        hasWebGL: gl !== null,
                        webglVersion: gl ? (gl.getParameter(gl.VERSION).includes('2.0') ? 2 : 1) : 0,
                        renderer: gl ? gl.getParameter(gl.RENDERER) : 'None',
                        maxTextureSize: gl ? gl.getParameter(gl.MAX_TEXTURE_SIZE) : 0,
                        viewportWidth: window.innerWidth,
                        viewportHeight: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert & Log
            _output.WriteLine($"{deviceName}:");
            _output.WriteLine($"  WebGL: {result.HasWebGL} (v{result.WebglVersion})");
            _output.WriteLine($"  Renderer: {result.Renderer}");
            _output.WriteLine($"  Max Texture: {result.MaxTextureSize}");
            _output.WriteLine($"  Viewport: {result.ViewportWidth}x{result.ViewportHeight}");
            _output.WriteLine($"  DPR: {result.DevicePixelRatio}");

            Assert.True(result.HasWebGL, $"{deviceName} should support WebGL");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Theory]
    [InlineData("iPad Pro", "Webkit")]
    [InlineData("iPad (gen 7)", "Webkit")]
    [InlineData("iPad Mini", "Webkit")]
    public async Task ExtendedMobile_iPadDevices_AllSupported(string deviceName, string engine)
    {
        // Arrange
        var device = _playwright!.Devices[deviceName];
        var browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var result = await page.EvaluateAsync<DeviceTestResult>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                    return {
                        hasWebGL: gl !== null,
                        webglVersion: gl ? (gl.getParameter(gl.VERSION).includes('2.0') ? 2 : 1) : 0,
                        renderer: gl ? gl.getParameter(gl.RENDERER) : 'None',
                        maxTextureSize: gl ? gl.getParameter(gl.MAX_TEXTURE_SIZE) : 0,
                        viewportWidth: window.innerWidth,
                        viewportHeight: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert & Log
            _output.WriteLine($"{deviceName}:");
            _output.WriteLine($"  WebGL: {result.HasWebGL} (v{result.WebglVersion})");
            _output.WriteLine($"  Max Texture: {result.MaxTextureSize}");
            _output.WriteLine($"  Viewport: {result.ViewportWidth}x{result.ViewportHeight}");

            Assert.True(result.HasWebGL, $"{deviceName} should support WebGL");
            Assert.True(result.MaxTextureSize >= 4096, $"{deviceName} (iPad) should support at least 4096px textures");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await browser.CloseAsync();
        }
    }

    [Fact]
    public async Task ExtendedMobile_ComprehensiveDeviceMatrix_GeneratesReport()
    {
        // Test all available devices and generate compatibility matrix
        var devices = new[]
        {
            // iOS Phones
            ("iPhone 13", _playwright!.Webkit),
            ("iPhone 12", _playwright.Webkit),
            ("iPhone SE", _playwright.Webkit),
            ("iPhone 11", _playwright.Webkit),
            ("iPhone XR", _playwright.Webkit),

            // Android Phones
            ("Pixel 5", _playwright.Chromium),
            ("Pixel 4", _playwright.Chromium),
            ("Galaxy S9+", _playwright.Chromium),

            // Tablets
            ("iPad Pro", _playwright.Webkit),
            ("iPad (gen 7)", _playwright.Webkit),
            ("Galaxy Tab S4", _playwright.Chromium),
        };

        var results = new List<DeviceMatrixResult>();

        foreach (var (deviceName, browserType) in devices)
        {
            try
            {
                var device = _playwright.Devices[deviceName];
                var launchOptions = deviceName.Contains("Pixel") || deviceName.Contains("Galaxy")
                    ? new BrowserTypeLaunchOptions { Headless = true, Args = new[] { "--use-gl=swiftshader" } }
                    : new BrowserTypeLaunchOptions { Headless = true };

                var browser = await browserType.LaunchAsync(launchOptions);
                var context = await browser.NewContextAsync(device);
                var page = await context.NewPageAsync();

                await page.GotoAsync(TestAppUrl);
                await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

                var testResult = await page.EvaluateAsync<DeviceTestResult>(@"
                    () => {
                        const canvas = document.getElementById('glCanvas');
                        const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                        // Performance test
                        const start = performance.now();
                        for (let i = 0; i < 100; i++) {
                            if (gl) gl.clear(gl.COLOR_BUFFER_BIT);
                        }
                        const renderTime = performance.now() - start;

                        return {
                            hasWebGL: gl !== null,
                            webglVersion: gl ? (gl.getParameter(gl.VERSION).includes('2.0') ? 2 : 1) : 0,
                            renderer: gl ? gl.getParameter(gl.RENDERER) : 'None',
                            maxTextureSize: gl ? gl.getParameter(gl.MAX_TEXTURE_SIZE) : 0,
                            viewportWidth: window.innerWidth,
                            viewportHeight: window.innerHeight,
                            devicePixelRatio: window.devicePixelRatio,
                            performanceScore: renderTime > 0 ? 100 / renderTime : 0
                        };
                    }
                ");

                results.Add(new DeviceMatrixResult
                {
                    DeviceName = deviceName,
                    Supported = testResult.HasWebGL,
                    WebGLVersion = testResult.WebglVersion,
                    MaxTextureSize = testResult.MaxTextureSize,
                    PerformanceScore = testResult.PerformanceScore,
                    ViewportSize = $"{testResult.ViewportWidth}x{testResult.ViewportHeight}",
                    DevicePixelRatio = testResult.DevicePixelRatio
                });

                await page.CloseAsync();
                await context.CloseAsync();
                await browser.CloseAsync();
            }
            catch (Exception ex)
            {
                results.Add(new DeviceMatrixResult
                {
                    DeviceName = deviceName,
                    Supported = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        // Generate Report
        _output.WriteLine($"\n╔═══════════════════════════════════════════════════════════════════╗");
        _output.WriteLine($"║          MOBILE DEVICE COMPATIBILITY MATRIX                        ║");
        _output.WriteLine($"╚═══════════════════════════════════════════════════════════════════╝\n");

        _output.WriteLine($"{"Device",-20} {"WebGL",8} {"Version",8} {"Max Tex",10} {"Viewport",15} {"DPR",5} {"Perf",8}");
        _output.WriteLine(new string('─', 85));

        foreach (var result in results)
        {
            var supported = result.Supported ? "✓" : "✗";
            var perfScore = result.PerformanceScore > 0 ? $"{result.PerformanceScore:F1}" : "N/A";

            _output.WriteLine($"{result.DeviceName,-20} {supported,8} {result.WebGLVersion,8} " +
                $"{result.MaxTextureSize,10} {result.ViewportSize,15} {result.DevicePixelRatio,5:F1} {perfScore,8}");

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                _output.WriteLine($"  Error: {result.ErrorMessage}");
            }
        }

        var supportedCount = results.Count(r => r.Supported);
        var supportRate = (double)supportedCount / results.Count * 100;

        _output.WriteLine($"\n  Total Devices Tested: {results.Count}");
        _output.WriteLine($"  Supported: {supportedCount} ({supportRate:F1}%)");
        _output.WriteLine($"  Unsupported: {results.Count - supportedCount}");

        // Category breakdown
        var iosDevices = results.Where(r => r.DeviceName.Contains("iPhone") || r.DeviceName.Contains("iPad")).ToList();
        var androidDevices = results.Where(r => r.DeviceName.Contains("Pixel") || r.DeviceName.Contains("Galaxy")).ToList();

        _output.WriteLine($"\n  iOS Devices: {iosDevices.Count(r => r.Supported)}/{iosDevices.Count} supported");
        _output.WriteLine($"  Android Devices: {androidDevices.Count(r => r.Supported)}/{androidDevices.Count} supported");

        Assert.True(supportRate >= 80, $"At least 80% of devices should be supported (got {supportRate:F1}%)");
    }

    [Fact]
    public async Task ExtendedMobile_ScreenSizeCategories_AllResolutionsWork()
    {
        // Test different screen size categories
        var screenSizes = new[]
        {
            (name: "Small Phone (iPhone SE)", width: 375, height: 667, dpr: 2.0),
            (name: "Medium Phone (iPhone 12)", width: 390, height: 844, dpr: 3.0),
            (name: "Large Phone (iPhone 13 Pro Max)", width: 428, height: 926, dpr: 3.0),
            (name: "Small Tablet (iPad Mini)", width: 768, height: 1024, dpr: 2.0),
            (name: "Large Tablet (iPad Pro 12.9)", width: 1024, height: 1366, dpr: 2.0),
        };

        foreach (var screen in screenSizes)
        {
            var browser = await _playwright!.Webkit.LaunchAsync(new() { Headless = true });
            var context = await browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = screen.width, Height = screen.height },
                DeviceScaleFactor = screen.dpr,
                IsMobile = true,
                HasTouch = true
            });
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(TestAppUrl);
                await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

                var result = await page.EvaluateAsync<bool>(@"
                    () => {
                        const canvas = document.getElementById('glCanvas');
                        const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
                        return gl !== null;
                    }
                ");

                _output.WriteLine($"{screen.name}: {(result ? "✓" : "✗")} ({screen.width}x{screen.height} @{screen.dpr}x)");

                Assert.True(result, $"{screen.name} should support WebGL");
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
                await browser.CloseAsync();
            }
        }
    }

    // Helper classes
    private class DeviceTestResult
    {
        public bool HasWebGL { get; set; }
        public int WebglVersion { get; set; }
        public string Renderer { get; set; } = "";
        public int MaxTextureSize { get; set; }
        public int ViewportWidth { get; set; }
        public int ViewportHeight { get; set; }
        public double DevicePixelRatio { get; set; }
        public double PerformanceScore { get; set; }
    }

    private class DeviceMatrixResult
    {
        public string DeviceName { get; set; } = "";
        public bool Supported { get; set; }
        public int WebGLVersion { get; set; }
        public int MaxTextureSize { get; set; }
        public double PerformanceScore { get; set; }
        public string ViewportSize { get; set; } = "";
        public double DevicePixelRatio { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
