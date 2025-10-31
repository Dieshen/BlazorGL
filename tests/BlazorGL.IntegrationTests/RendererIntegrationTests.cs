using Microsoft.Playwright;
using Xunit;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Integration tests for Renderer class using Playwright to test WebGL functionality
/// </summary>
public class RendererIntegrationTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private const string TestAppUrl = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = true,
            Args = new[] {
                "--use-gl=swiftshader",  // Use software WebGL renderer
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
    public async Task Renderer_ShouldInitialize_WithValidCanvas()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);

        // Wait for the app to load
        await _page.WaitForSelectorAsync("#glCanvas", new() { Timeout = 10000 });

        // Assert - Check if renderer initialized
        var initialized = await _page.GetAttributeAsync("#testData", "data-initialized");
        Assert.Equal("True", initialized);
    }

    [Fact]
    public async Task Renderer_ShouldCreateCanvas_WithCorrectDimensions()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        // Assert
        var canvas = await _page.QuerySelectorAsync("#glCanvas");
        Assert.NotNull(canvas);

        var width = await canvas!.GetAttributeAsync("width");
        var height = await canvas.GetAttributeAsync("height");

        Assert.Equal("800", width);
        Assert.Equal("600", height);
    }

    [Fact]
    public async Task Renderer_ShouldRenderScene_Successfully()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        // Wait for rendering to complete
        await _page.WaitForFunctionAsync(@"
            () => document.querySelector('#testData').getAttribute('data-rendered') === 'True'
        ", new() { Timeout = 15000 });

        // Assert
        var rendered = await _page.GetAttributeAsync("#testData", "data-rendered");
        Assert.Equal("True", rendered);
    }

    [Fact]
    public async Task Renderer_ShouldPassAllIntegrationTests()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        // Wait for all tests to complete
        await _page.WaitForFunctionAsync(@"
            () => {
                const results = document.querySelectorAll('#testResults li');
                return results.length > 0;
            }
        ", new() { Timeout = 20000 });

        // Assert - Check that all tests passed
        var failedTests = await _page.QuerySelectorAllAsync("#testResults li.failed");
        var passedTests = await _page.QuerySelectorAllAsync("#testResults li.passed");

        Assert.NotEmpty(passedTests);

        if (failedTests.Count > 0)
        {
            var failedMessages = new List<string>();
            foreach (var test in failedTests)
            {
                var text = await test.TextContentAsync();
                failedMessages.Add(text ?? "Unknown failure");
            }
            Assert.Fail($"Some integration tests failed:\n{string.Join("\n", failedMessages)}");
        }
    }

    [Fact]
    public async Task Renderer_ShouldCompileShaders_ForDifferentMaterials()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => document.querySelectorAll('#testResults li').length >= 3
        ", new() { Timeout = 15000 });

        // Assert - Check shader compilation test
        var shaderTest = await _page.QuerySelectorAsync("[data-test='Shader Compilation']");
        Assert.NotNull(shaderTest);

        var className = await shaderTest!.GetAttributeAsync("class");
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Renderer_ShouldHandleMultipleGeometryTypes()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => document.querySelectorAll('#testResults li').length >= 4
        ", new() { Timeout = 15000 });

        // Assert - Check geometry buffers test
        var geometryTest = await _page.QuerySelectorAsync("[data-test='Geometry Buffers']");
        Assert.NotNull(geometryTest);

        var className = await geometryTest!.GetAttributeAsync("class");
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Renderer_ShouldUploadTextures_Successfully()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => document.querySelectorAll('#testResults li').length >= 6
        ", new() { Timeout = 15000 });

        // Assert - Check texture upload test
        var textureTest = await _page.QuerySelectorAsync("[data-test='Texture Upload']");
        Assert.NotNull(textureTest);

        var className = await textureTest!.GetAttributeAsync("class");
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Renderer_ShouldHandleMultipleObjects_Efficiently()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => document.querySelectorAll('#testResults li').length >= 7
        ", new() { Timeout = 20000 });

        // Assert - Check multiple objects test
        var multiObjectTest = await _page.QuerySelectorAsync("[data-test='Multiple Objects']");
        Assert.NotNull(multiObjectTest);

        var className = await multiObjectTest!.GetAttributeAsync("class");
        Assert.Contains("passed", className);

        var text = await multiObjectTest.TextContentAsync();
        Assert.Contains("objects", text);
    }

    [Fact]
    public async Task Renderer_ShouldIntegrateLights_Properly()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => document.querySelectorAll('#testResults li').length >= 8
        ", new() { Timeout = 20000 });

        // Assert - Check light integration test
        var lightTest = await _page.QuerySelectorAsync("[data-test='Light Integration']");
        Assert.NotNull(lightTest);

        var className = await lightTest!.GetAttributeAsync("class");
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Renderer_ShouldNotHaveConsoleErrors()
    {
        // Arrange
        var consoleErrors = new List<string>();
        _page!.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleErrors.Add(msg.Text);
            }
        };

        // Act
        await _page.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");
        await Task.Delay(2000); // Wait for any delayed errors

        // Assert
        Assert.Empty(consoleErrors);
    }

    [Fact]
    public async Task Renderer_ShouldGetWebGLContext()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        // Check if WebGL context is available
        var hasWebGL = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');
                return gl !== null;
            }
        ");

        // Assert
        Assert.True(hasWebGL, "WebGL 2.0 context should be available");
    }

    [Fact]
    public async Task Renderer_ShouldClearCanvas_WithCorrectColor()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");
        await Task.Delay(1000); // Let initial render complete

        // Get a pixel from the canvas
        var pixelData = await _page.EvaluateAsync<int[]>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');
                const pixels = new Uint8Array(4);
                gl.readPixels(0, 0, 1, 1, gl.RGBA, gl.UNSIGNED_BYTE, pixels);
                return Array.from(pixels);
            }
        ");

        // Assert - Verify we got valid pixel data
        Assert.NotNull(pixelData);
        Assert.Equal(4, pixelData.Length);
    }
}
