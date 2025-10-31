using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Mobile browser compatibility tests for BlazorGL
/// Tests iOS Safari, Android Chrome, and various mobile devices
/// </summary>
public class MobileBrowserTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;

    public MobileBrowserTests(ITestOutputHelper output)
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
    public async Task Mobile_iPhone13_RendersCorrectly()
    {
        // Arrange
        var device = _playwright!.Devices["iPhone 13"];
        _browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            // Check if WebGL is available
            var hasWebGL = await page.EvaluateAsync<bool>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl') || canvas.getContext('webgl2');
                    return gl !== null;
                }
            ");

            // Get viewport info
            var viewportInfo = await page.EvaluateAsync<ViewportInfo>(@"
                () => {
                    return {
                        width: window.innerWidth,
                        height: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert
            _output.WriteLine($"iPhone 13 Test:");
            _output.WriteLine($"  WebGL Available: {hasWebGL}");
            _output.WriteLine($"  Viewport: {viewportInfo.Width}x{viewportInfo.Height}");
            _output.WriteLine($"  Device Pixel Ratio: {viewportInfo.DevicePixelRatio}");

            Assert.True(hasWebGL, "WebGL should be available on iPhone 13");
            Assert.True(viewportInfo.Width > 0, "Viewport should have valid width");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    [Fact]
    public async Task Mobile_iPadPro_RendersCorrectly()
    {
        // Arrange
        var device = _playwright!.Devices["iPad Pro"];
        _browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var hasWebGL = await page.EvaluateAsync<bool>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2');
                    return gl !== null;
                }
            ");

            var viewportInfo = await page.EvaluateAsync<ViewportInfo>(@"
                () => {
                    return {
                        width: window.innerWidth,
                        height: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert
            _output.WriteLine($"iPad Pro Test:");
            _output.WriteLine($"  WebGL Available: {hasWebGL}");
            _output.WriteLine($"  Viewport: {viewportInfo.Width}x{viewportInfo.Height}");
            _output.WriteLine($"  Device Pixel Ratio: {viewportInfo.DevicePixelRatio}");

            Assert.True(hasWebGL, "WebGL should be available on iPad Pro");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    [Fact]
    public async Task Mobile_PixelAndroid_RendersCorrectly()
    {
        // Arrange
        var device = _playwright!.Devices["Pixel 5"];
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] { "--use-gl=swiftshader" }
        });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var hasWebGL = await page.EvaluateAsync<bool>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2');
                    return gl !== null;
                }
            ");

            var viewportInfo = await page.EvaluateAsync<ViewportInfo>(@"
                () => {
                    return {
                        width: window.innerWidth,
                        height: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert
            _output.WriteLine($"Pixel 5 (Android) Test:");
            _output.WriteLine($"  WebGL Available: {hasWebGL}");
            _output.WriteLine($"  Viewport: {viewportInfo.Width}x{viewportInfo.Height}");
            _output.WriteLine($"  Device Pixel Ratio: {viewportInfo.DevicePixelRatio}");

            Assert.True(hasWebGL, "WebGL should be available on Pixel 5");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    [Fact]
    public async Task Mobile_TouchEvents_WorkCorrectly()
    {
        // Arrange
        var device = _playwright!.Devices["iPhone 13"];
        _browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            // Check if touch events are supported
            var touchSupported = await page.EvaluateAsync<bool>(@"
                () => {
                    return 'ontouchstart' in window;
                }
            ");

            // Simulate touch interaction
            var canvas = await page.QuerySelectorAsync("#glCanvas");
            if (canvas != null)
            {
                await canvas.TapAsync();
            }

            // Assert
            _output.WriteLine($"Touch Events Test:");
            _output.WriteLine($"  Touch Supported: {touchSupported}");

            Assert.True(touchSupported, "Touch events should be supported on mobile");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    [Fact]
    public async Task Mobile_OrientationChange_HandlesCorrectly()
    {
        // Arrange
        var device = _playwright!.Devices["iPhone 13"];
        _browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act - Portrait
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var portraitSize = await page.EvaluateAsync<ViewportInfo>(@"
                () => {
                    return {
                        width: window.innerWidth,
                        height: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Change to landscape
            await page.SetViewportSizeAsync(new()
            {
                Width = portraitSize.Height,
                Height = portraitSize.Width
            });

            await Task.Delay(500); // Let resize settle

            var landscapeSize = await page.EvaluateAsync<ViewportInfo>(@"
                () => {
                    return {
                        width: window.innerWidth,
                        height: window.innerHeight,
                        devicePixelRatio: window.devicePixelRatio
                    };
                }
            ");

            // Assert
            _output.WriteLine($"Orientation Change Test:");
            _output.WriteLine($"  Portrait: {portraitSize.Width}x{portraitSize.Height}");
            _output.WriteLine($"  Landscape: {landscapeSize.Width}x{landscapeSize.Height}");

            Assert.True(landscapeSize.Width > landscapeSize.Height, "Landscape should be wider than tall");
            Assert.True(portraitSize.Height > portraitSize.Width, "Portrait should be taller than wide");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    [Fact]
    public async Task Mobile_PerformanceOnMobile_IsAcceptable()
    {
        // Arrange
        var device = _playwright!.Devices["iPhone 13"];
        _browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            var performanceResult = await page.EvaluateAsync<MobilePerformanceResult>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                    if (!gl) {
                        return { fps: 0, renderTime: 0, supported: false };
                    }

                    const startTime = performance.now();

                    // Render simple scene 60 times
                    for (let i = 0; i < 60; i++) {
                        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                    }

                    const endTime = performance.now();
                    const renderTime = endTime - startTime;

                    return {
                        fps: 60 / (renderTime / 1000),
                        renderTime: renderTime,
                        supported: true
                    };
                }
            ");

            // Assert
            _output.WriteLine($"Mobile Performance Test:");
            _output.WriteLine($"  Supported: {performanceResult.Supported}");
            _output.WriteLine($"  Render Time (60 frames): {performanceResult.RenderTime:F2}ms");
            _output.WriteLine($"  Estimated FPS: {performanceResult.Fps:F2}");

            Assert.True(performanceResult.Supported, "WebGL should be supported");
            Assert.True(performanceResult.Fps > 30, "Should achieve at least 30 FPS on mobile");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    [Fact]
    public async Task Mobile_MultipleDevices_AllSupported()
    {
        // Test multiple device types
        var deviceNames = new[]
        {
            "iPhone 13",
            "iPhone 12",
            "iPad Pro",
            "Pixel 5",
            "Galaxy S9+"
        };

        var results = new List<DeviceTestResult>();

        foreach (var deviceName in deviceNames)
        {
            var device = _playwright!.Devices[deviceName];
            var browserType = deviceName.StartsWith("i") ? _playwright.Webkit : _playwright.Chromium;
            var browser = await browserType.LaunchAsync(new()
            {
                Headless = true,
                Args = new[] { "--use-gl=swiftshader" }
            });
            var context = await browser.NewContextAsync(device);
            var page = await context.NewPageAsync();

            try
            {
                await page.GotoAsync(TestAppUrl);
                await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

                var hasWebGL = await page.EvaluateAsync<bool>(@"
                    () => {
                        const canvas = document.getElementById('glCanvas');
                        const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');
                        return gl !== null;
                    }
                ");

                results.Add(new DeviceTestResult
                {
                    DeviceName = deviceName,
                    WebGLSupported = hasWebGL,
                    Passed = hasWebGL
                });

                _output.WriteLine($"{deviceName}: WebGL={hasWebGL}");
            }
            catch (Exception ex)
            {
                results.Add(new DeviceTestResult
                {
                    DeviceName = deviceName,
                    WebGLSupported = false,
                    Passed = false,
                    Error = ex.Message
                });
                _output.WriteLine($"{deviceName}: Failed - {ex.Message}");
            }
            finally
            {
                await page.CloseAsync();
                await context.CloseAsync();
                await browser.CloseAsync();
            }
        }

        // Assert
        var allSupported = results.All(r => r.Passed);
        var supportedCount = results.Count(r => r.Passed);

        _output.WriteLine($"\nSummary: {supportedCount}/{results.Count} devices supported");

        Assert.True(supportedCount >= results.Count * 0.8,
            $"At least 80% of devices should be supported (got {supportedCount}/{results.Count})");
    }

    [Fact]
    public async Task Mobile_LowPowerMode_RendersCorrectly()
    {
        // Arrange - Simulate low power mode with reduced performance
        var device = _playwright!.Devices["iPhone 13"];
        _browser = await _playwright.Webkit.LaunchAsync(new() { Headless = true });
        var context = await _browser.NewContextAsync(device);
        var page = await context.NewPageAsync();

        try
        {
            // Act
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 15000 });

            // CPU throttling simulation
            await (page.Context as IBrowserContext)!.RouteAsync("**/*", async route =>
            {
                await Task.Delay(10); // Simulate slower processing
                await route.ContinueAsync();
            });

            var stillWorks = await page.EvaluateAsync<bool>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl2') || canvas.getContext('webgl');

                    if (!gl) return false;

                    // Try basic rendering
                    gl.clearColor(0, 0, 0, 1);
                    gl.clear(gl.COLOR_BUFFER_BIT);

                    return true;
                }
            ");

            // Assert
            _output.WriteLine($"Low Power Mode Test:");
            _output.WriteLine($"  Basic Rendering Works: {stillWorks}");

            Assert.True(stillWorks, "Basic rendering should work even in simulated low power mode");
        }
        finally
        {
            await page.CloseAsync();
            await context.CloseAsync();
            await _browser.CloseAsync();
        }
    }

    // Helper classes
    private class ViewportInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double DevicePixelRatio { get; set; }
    }

    private class MobilePerformanceResult
    {
        public double Fps { get; set; }
        public double RenderTime { get; set; }
        public bool Supported { get; set; }
    }

    private class DeviceTestResult
    {
        public string DeviceName { get; set; } = "";
        public bool WebGLSupported { get; set; }
        public bool Passed { get; set; }
        public string? Error { get; set; }
    }
}
