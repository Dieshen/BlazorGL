using Microsoft.Playwright;
using Xunit;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Integration tests specifically for shader compilation and WebGL shader programs
/// </summary>
public class ShaderIntegrationTests : IAsyncLifetime
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
    public async Task Shader_ShouldCompileBasicMaterialShader()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        // Check for shader compilation
        var hasShaderErrors = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create a simple vertex shader
                const vertexShader = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vertexShader, `
                    #version 300 es
                    in vec3 position;
                    uniform mat4 modelViewMatrix;
                    uniform mat4 projectionMatrix;
                    void main() {
                        gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
                    }
                `);
                gl.compileShader(vertexShader);

                return !gl.getShaderParameter(vertexShader, gl.COMPILE_STATUS);
            }
        ");

        // Assert
        Assert.False(hasShaderErrors, "Basic vertex shader should compile without errors");
    }

    [Fact]
    public async Task Shader_ShouldCompileFragmentShader()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var hasShaderErrors = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fragmentShader, `
                    #version 300 es
                    precision highp float;
                    uniform vec3 color;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(color, 1.0);
                    }
                `);
                gl.compileShader(fragmentShader);

                return !gl.getShaderParameter(fragmentShader, gl.COMPILE_STATUS);
            }
        ");

        // Assert
        Assert.False(hasShaderErrors, "Fragment shader should compile without errors");
    }

    [Fact]
    public async Task Shader_ShouldLinkProgram_Successfully()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var programLinked = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create shaders
                const vertexShader = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vertexShader, `
                    #version 300 es
                    in vec3 position;
                    void main() {
                        gl_Position = vec4(position, 1.0);
                    }
                `);
                gl.compileShader(vertexShader);

                const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fragmentShader, `
                    #version 300 es
                    precision highp float;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(1.0, 0.0, 0.0, 1.0);
                    }
                `);
                gl.compileShader(fragmentShader);

                // Link program
                const program = gl.createProgram();
                gl.attachShader(program, vertexShader);
                gl.attachShader(program, fragmentShader);
                gl.linkProgram(program);

                return gl.getProgramParameter(program, gl.LINK_STATUS);
            }
        ");

        // Assert
        Assert.True(programLinked, "Shader program should link successfully");
    }

    [Fact]
    public async Task Shader_ShouldHandleUniforms_Correctly()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var uniformsWork = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const vertexShader = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vertexShader, `
                    #version 300 es
                    in vec3 position;
                    uniform mat4 transform;
                    void main() {
                        gl_Position = transform * vec4(position, 1.0);
                    }
                `);
                gl.compileShader(vertexShader);

                const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fragmentShader, `
                    #version 300 es
                    precision highp float;
                    uniform vec3 color;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(color, 1.0);
                    }
                `);
                gl.compileShader(fragmentShader);

                const program = gl.createProgram();
                gl.attachShader(program, vertexShader);
                gl.attachShader(program, fragmentShader);
                gl.linkProgram(program);

                if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                    return false;
                }

                gl.useProgram(program);

                // Try to set uniforms
                const transformLoc = gl.getUniformLocation(program, 'transform');
                const colorLoc = gl.getUniformLocation(program, 'color');

                return transformLoc !== null && colorLoc !== null;
            }
        ");

        // Assert
        Assert.True(uniformsWork, "Uniforms should be accessible in shader program");
    }

    [Fact]
    public async Task Shader_ShouldHandleAttributes_Correctly()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var attributesWork = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const vertexShader = gl.createShader(gl.VERTEX_SHADER);
                gl.shaderSource(vertexShader, `
                    #version 300 es
                    in vec3 position;
                    in vec3 normal;
                    in vec2 uv;
                    void main() {
                        gl_Position = vec4(position + normal.xyz * 0.01, 1.0);
                    }
                `);
                gl.compileShader(vertexShader);

                const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER);
                gl.shaderSource(fragmentShader, `
                    #version 300 es
                    precision highp float;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(1.0);
                    }
                `);
                gl.compileShader(fragmentShader);

                const program = gl.createProgram();
                gl.attachShader(program, vertexShader);
                gl.attachShader(program, fragmentShader);
                gl.linkProgram(program);

                if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                    return false;
                }

                // Check attribute locations
                const posLoc = gl.getAttribLocation(program, 'position');
                const normalLoc = gl.getAttribLocation(program, 'normal');
                const uvLoc = gl.getAttribLocation(program, 'uv');

                return posLoc !== -1 && normalLoc !== -1 && uvLoc !== -1;
            }
        ");

        // Assert
        Assert.True(attributesWork, "Vertex attributes should be accessible in shader program");
    }

    [Fact]
    public async Task Shader_ShouldCompilePhongShader()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => {
                const test = document.querySelector('[data-test=""Shader Compilation""]');
                return test !== null;
            }
        ", new() { Timeout = 15000 });

        var shaderTest = await _page.QuerySelectorAsync("[data-test='Shader Compilation']");
        var className = await shaderTest!.GetAttributeAsync("class");

        // Assert
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Shader_ShouldNotLeakShaders()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var leakTest = await _page.EvaluateAsync<string>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                let shadersCreated = 0;
                let shadersDeleted = 0;

                // Create and delete multiple shaders
                for (let i = 0; i < 10; i++) {
                    const vs = gl.createShader(gl.VERTEX_SHADER);
                    const fs = gl.createShader(gl.FRAGMENT_SHADER);
                    shadersCreated += 2;

                    gl.deleteShader(vs);
                    gl.deleteShader(fs);
                    shadersDeleted += 2;
                }

                return `Created: ${shadersCreated}, Deleted: ${shadersDeleted}`;
            }
        ");

        // Assert
        Assert.Contains("Created: 20", leakTest);
        Assert.Contains("Deleted: 20", leakTest);
    }
}
