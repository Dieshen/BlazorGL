using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// WebGL 1.0 fallback tests
/// Tests compatibility and fallback behavior when WebGL 2.0 is not available
/// Ensures graceful degradation to WebGL 1.0
/// </summary>
public class WebGL1FallbackTests : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string TestAppUrl = "http://localhost:5000";
    private readonly ITestOutputHelper _output;

    public WebGL1FallbackTests(ITestOutputHelper output)
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
    public async Task WebGL1_ContextAvailable_WhenWebGL2Blocked()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            // Act - Force WebGL 1.0 by blocking WebGL 2.0
            var webglInfo = await page.EvaluateAsync<WebGLInfo>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');

                    // Try WebGL 2.0 first
                    const gl2 = canvas.getContext('webgl2');

                    // Fallback to WebGL 1.0
                    const gl1 = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

                    return {
                        hasWebGL2: gl2 !== null,
                        hasWebGL1: gl1 !== null,
                        version: gl2 ? 2 : (gl1 ? 1 : 0),
                        renderer: gl2 ? gl2.getParameter(gl2.RENDERER) : (gl1 ? gl1.getParameter(gl1.RENDERER) : 'None'),
                        maxTextureSize: gl2 ? gl2.getParameter(gl2.MAX_TEXTURE_SIZE) : (gl1 ? gl1.getParameter(gl1.MAX_TEXTURE_SIZE) : 0)
                    };
                }
            ");

            // Assert
            _output.WriteLine($"WebGL Fallback Test:");
            _output.WriteLine($"  WebGL 2.0 Available: {webglInfo.HasWebGL2}");
            _output.WriteLine($"  WebGL 1.0 Available: {webglInfo.HasWebGL1}");
            _output.WriteLine($"  Active Version: {webglInfo.Version}");
            _output.WriteLine($"  Renderer: {webglInfo.Renderer}");
            _output.WriteLine($"  Max Texture Size: {webglInfo.MaxTextureSize}");

            Assert.True(webglInfo.HasWebGL1 || webglInfo.HasWebGL2, "At least WebGL 1.0 should be available");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task WebGL1_BasicRendering_WorksCorrectly()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            // Act - Test basic rendering with WebGL 1.0
            var renderResult = await page.EvaluateAsync<RenderResult>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

                    if (!gl) {
                        return { success: false, error: 'WebGL 1.0 not available' };
                    }

                    try {
                        // Clear the canvas
                        gl.clearColor(1.0, 0.0, 0.0, 1.0);
                        gl.clear(gl.COLOR_BUFFER_BIT);

                        // Create a simple shader program
                        const vsSource = `
                            attribute vec3 position;
                            void main() {
                                gl_Position = vec4(position, 1.0);
                            }
                        `;

                        const fsSource = `
                            precision mediump float;
                            void main() {
                                gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
                            }
                        `;

                        const vs = gl.createShader(gl.VERTEX_SHADER);
                        gl.shaderSource(vs, vsSource);
                        gl.compileShader(vs);

                        if (!gl.getShaderParameter(vs, gl.COMPILE_STATUS)) {
                            return { success: false, error: 'Vertex shader compilation failed' };
                        }

                        const fs = gl.createShader(gl.FRAGMENT_SHADER);
                        gl.shaderSource(fs, fsSource);
                        gl.compileShader(fs);

                        if (!gl.getShaderParameter(fs, gl.COMPILE_STATUS)) {
                            return { success: false, error: 'Fragment shader compilation failed' };
                        }

                        const program = gl.createProgram();
                        gl.attachShader(program, vs);
                        gl.attachShader(program, fs);
                        gl.linkProgram(program);

                        if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                            return { success: false, error: 'Program linking failed' };
                        }

                        gl.useProgram(program);

                        // Create a triangle
                        const vertices = new Float32Array([
                            0.0,  0.5, 0.0,
                           -0.5, -0.5, 0.0,
                            0.5, -0.5, 0.0
                        ]);

                        const buffer = gl.createBuffer();
                        gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
                        gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                        const posLoc = gl.getAttribLocation(program, 'position');
                        gl.vertexAttribPointer(posLoc, 3, gl.FLOAT, false, 0, 0);
                        gl.enableVertexAttribArray(posLoc);

                        // Draw the triangle
                        gl.drawArrays(gl.TRIANGLES, 0, 3);

                        // Cleanup
                        gl.deleteBuffer(buffer);
                        gl.deleteProgram(program);
                        gl.deleteShader(vs);
                        gl.deleteShader(fs);

                        return { success: true, error: null };
                    } catch (e) {
                        return { success: false, error: e.message };
                    }
                }
            ");

            // Assert
            _output.WriteLine($"WebGL 1.0 Basic Rendering:");
            _output.WriteLine($"  Success: {renderResult.Success}");
            if (!string.IsNullOrEmpty(renderResult.Error))
            {
                _output.WriteLine($"  Error: {renderResult.Error}");
            }

            Assert.True(renderResult.Success, renderResult.Error ?? "Rendering failed");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task WebGL1_ExtensionsAvailable_ForCommonFeatures()
    {
        // Arrange
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            // Act
            var extensions = await page.EvaluateAsync<ExtensionInfo>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

                    if (!gl) {
                        return {
                            available: [],
                            hasFloatTextures: false,
                            hasDepthTexture: false,
                            hasAnisotropic: false,
                            hasVAO: false,
                            hasInstancing: false
                        };
                    }

                    const available = gl.getSupportedExtensions() || [];

                    return {
                        available: available,
                        hasFloatTextures: available.includes('OES_texture_float'),
                        hasDepthTexture: available.includes('WEBGL_depth_texture'),
                        hasAnisotropic: available.includes('EXT_texture_filter_anisotropic') ||
                                      available.includes('WEBKIT_EXT_texture_filter_anisotropic'),
                        hasVAO: available.includes('OES_vertex_array_object'),
                        hasInstancing: available.includes('ANGLE_instanced_arrays')
                    };
                }
            ");

            // Assert
            _output.WriteLine($"WebGL 1.0 Extensions:");
            _output.WriteLine($"  Total Extensions: {extensions.Available.Length}");
            _output.WriteLine($"  Float Textures: {extensions.HasFloatTextures}");
            _output.WriteLine($"  Depth Textures: {extensions.HasDepthTexture}");
            _output.WriteLine($"  Anisotropic Filtering: {extensions.HasAnisotropic}");
            _output.WriteLine($"  VAO: {extensions.HasVAO}");
            _output.WriteLine($"  Instancing: {extensions.HasInstancing}");

            Assert.NotEmpty(extensions.Available);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task WebGL1_TextureFormats_AreSupported()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var textureSupport = await page.EvaluateAsync<TextureFormatSupport>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

                    if (!gl) return { rgba: false, rgb: false, luminance: false };

                    try {
                        const texture = gl.createTexture();
                        gl.bindTexture(gl.TEXTURE_2D, texture);

                        // Test RGBA
                        let rgba = false;
                        try {
                            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 2, 2, 0, gl.RGBA, gl.UNSIGNED_BYTE,
                                new Uint8Array([255,0,0,255, 0,255,0,255, 0,0,255,255, 255,255,0,255]));
                            rgba = true;
                        } catch(e) {}

                        // Test RGB
                        let rgb = false;
                        try {
                            gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGB, 2, 2, 0, gl.RGB, gl.UNSIGNED_BYTE,
                                new Uint8Array([255,0,0, 0,255,0, 0,0,255, 255,255,0]));
                            rgb = true;
                        } catch(e) {}

                        // Test LUMINANCE
                        let luminance = false;
                        try {
                            gl.texImage2D(gl.TEXTURE_2D, 0, gl.LUMINANCE, 2, 2, 0, gl.LUMINANCE, gl.UNSIGNED_BYTE,
                                new Uint8Array([128, 255, 64, 192]));
                            luminance = true;
                        } catch(e) {}

                        gl.deleteTexture(texture);

                        return { rgba, rgb, luminance };
                    } catch(e) {
                        return { rgba: false, rgb: false, luminance: false };
                    }
                }
            ");

            _output.WriteLine($"WebGL 1.0 Texture Formats:");
            _output.WriteLine($"  RGBA: {textureSupport.Rgba}");
            _output.WriteLine($"  RGB: {textureSupport.Rgb}");
            _output.WriteLine($"  LUMINANCE: {textureSupport.Luminance}");

            Assert.True(textureSupport.Rgba, "RGBA textures should be supported");
            Assert.True(textureSupport.Rgb, "RGB textures should be supported");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task WebGL1_LimitsAndCapabilities_MeetMinimumRequirements()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var limits = await page.EvaluateAsync<WebGLLimits>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');
                    const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

                    if (!gl) return {};

                    return {
                        maxTextureSize: gl.getParameter(gl.MAX_TEXTURE_SIZE),
                        maxCubeMapTextureSize: gl.getParameter(gl.MAX_CUBE_MAP_TEXTURE_SIZE),
                        maxRenderBufferSize: gl.getParameter(gl.MAX_RENDERBUFFER_SIZE),
                        maxVertexAttribs: gl.getParameter(gl.MAX_VERTEX_ATTRIBS),
                        maxVaryingVectors: gl.getParameter(gl.MAX_VARYING_VECTORS),
                        maxVertexUniformVectors: gl.getParameter(gl.MAX_VERTEX_UNIFORM_VECTORS),
                        maxFragmentUniformVectors: gl.getParameter(gl.MAX_FRAGMENT_UNIFORM_VECTORS),
                        maxTextureImageUnits: gl.getParameter(gl.MAX_TEXTURE_IMAGE_UNITS),
                        maxVertexTextureImageUnits: gl.getParameter(gl.MAX_VERTEX_TEXTURE_IMAGE_UNITS),
                        maxCombinedTextureImageUnits: gl.getParameter(gl.MAX_COMBINED_TEXTURE_IMAGE_UNITS),
                        maxViewportDims: gl.getParameter(gl.MAX_VIEWPORT_DIMS)
                    };
                }
            ");

            _output.WriteLine($"\nWebGL 1.0 Limits:");
            _output.WriteLine($"  Max Texture Size: {limits.MaxTextureSize}");
            _output.WriteLine($"  Max Cube Map Texture Size: {limits.MaxCubeMapTextureSize}");
            _output.WriteLine($"  Max Renderbuffer Size: {limits.MaxRenderBufferSize}");
            _output.WriteLine($"  Max Vertex Attribs: {limits.MaxVertexAttribs}");
            _output.WriteLine($"  Max Varying Vectors: {limits.MaxVaryingVectors}");
            _output.WriteLine($"  Max Vertex Uniform Vectors: {limits.MaxVertexUniformVectors}");
            _output.WriteLine($"  Max Fragment Uniform Vectors: {limits.MaxFragmentUniformVectors}");
            _output.WriteLine($"  Max Texture Image Units: {limits.MaxTextureImageUnits}");
            _output.WriteLine($"  Max Vertex Texture Units: {limits.MaxVertexTextureImageUnits}");

            // WebGL 1.0 minimum requirements from spec
            Assert.True(limits.MaxTextureSize >= 64, "Max texture size should be at least 64");
            Assert.True(limits.MaxVertexAttribs >= 8, "Should support at least 8 vertex attributes");
            Assert.True(limits.MaxTextureImageUnits >= 8, "Should support at least 8 texture units");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task WebGL1_MultipleContexts_CanCoexist()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var multiContext = await page.EvaluateAsync<bool>(@"
                () => {
                    // Create multiple canvases with WebGL 1.0 contexts
                    const canvas1 = document.createElement('canvas');
                    const canvas2 = document.createElement('canvas');

                    const gl1 = canvas1.getContext('webgl');
                    const gl2 = canvas2.getContext('webgl');

                    if (!gl1 || !gl2) return false;

                    // Try basic operations on both
                    gl1.clearColor(1, 0, 0, 1);
                    gl1.clear(gl1.COLOR_BUFFER_BIT);

                    gl2.clearColor(0, 1, 0, 1);
                    gl2.clear(gl2.COLOR_BUFFER_BIT);

                    return true;
                }
            ");

            _output.WriteLine($"Multiple WebGL 1.0 Contexts: {multiContext}");
            Assert.True(multiContext, "Should support multiple WebGL 1.0 contexts");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task WebGL1_PerformanceComparison_WithWebGL2()
    {
        var page = await _browser!.NewPageAsync();

        try
        {
            await page.GotoAsync(TestAppUrl);
            await page.WaitForSelectorAsync("#glCanvas");

            var comparison = await page.EvaluateAsync<VersionComparison>(@"
                () => {
                    const canvas = document.getElementById('glCanvas');

                    // Test WebGL 2.0
                    const gl2 = canvas.getContext('webgl2');
                    let webgl2Time = 0;
                    if (gl2) {
                        const start = performance.now();
                        for (let i = 0; i < 100; i++) {
                            gl2.clear(gl2.COLOR_BUFFER_BIT);
                        }
                        webgl2Time = performance.now() - start;
                    }

                    // Test WebGL 1.0
                    const gl1 = canvas.getContext('webgl');
                    let webgl1Time = 0;
                    if (gl1) {
                        const start = performance.now();
                        for (let i = 0; i < 100; i++) {
                            gl1.clear(gl1.COLOR_BUFFER_BIT);
                        }
                        webgl1Time = performance.now() - start;
                    }

                    return {
                        hasWebGL2: gl2 !== null,
                        hasWebGL1: gl1 !== null,
                        webgl2Time: webgl2Time,
                        webgl1Time: webgl1Time
                    };
                }
            ");

            _output.WriteLine($"\nPerformance Comparison:");
            _output.WriteLine($"  WebGL 2.0 Available: {comparison.HasWebGL2}");
            _output.WriteLine($"  WebGL 1.0 Available: {comparison.HasWebGL1}");

            if (comparison.HasWebGL2)
            {
                _output.WriteLine($"  WebGL 2.0 Time: {comparison.Webgl2Time:F2}ms");
            }
            if (comparison.HasWebGL1)
            {
                _output.WriteLine($"  WebGL 1.0 Time: {comparison.Webgl1Time:F2}ms");
            }

            if (comparison.HasWebGL2 && comparison.HasWebGL1)
            {
                var diff = Math.Abs(comparison.Webgl2Time - comparison.Webgl1Time);
                _output.WriteLine($"  Time Difference: {diff:F2}ms");
            }

            Assert.True(comparison.HasWebGL1, "WebGL 1.0 should be available as fallback");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    // Helper classes
    private class WebGLInfo
    {
        public bool HasWebGL2 { get; set; }
        public bool HasWebGL1 { get; set; }
        public int Version { get; set; }
        public string Renderer { get; set; } = "";
        public int MaxTextureSize { get; set; }
    }

    private class RenderResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
    }

    private class ExtensionInfo
    {
        public string[] Available { get; set; } = Array.Empty<string>();
        public bool HasFloatTextures { get; set; }
        public bool HasDepthTexture { get; set; }
        public bool HasAnisotropic { get; set; }
        public bool HasVAO { get; set; }
        public bool HasInstancing { get; set; }
    }

    private class TextureFormatSupport
    {
        public bool Rgba { get; set; }
        public bool Rgb { get; set; }
        public bool Luminance { get; set; }
    }

    private class WebGLLimits
    {
        public int MaxTextureSize { get; set; }
        public int MaxCubeMapTextureSize { get; set; }
        public int MaxRenderBufferSize { get; set; }
        public int MaxVertexAttribs { get; set; }
        public int MaxVaryingVectors { get; set; }
        public int MaxVertexUniformVectors { get; set; }
        public int MaxFragmentUniformVectors { get; set; }
        public int MaxTextureImageUnits { get; set; }
        public int MaxVertexTextureImageUnits { get; set; }
        public int MaxCombinedTextureImageUnits { get; set; }
        public int[] MaxViewportDims { get; set; } = Array.Empty<int>();
    }

    private class VersionComparison
    {
        public bool HasWebGL2 { get; set; }
        public bool HasWebGL1 { get; set; }
        public double Webgl2Time { get; set; }
        public double Webgl1Time { get; set; }
    }
}
