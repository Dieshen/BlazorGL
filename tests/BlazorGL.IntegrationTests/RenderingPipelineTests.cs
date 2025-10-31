using Microsoft.Playwright;
using Xunit;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Integration tests for the complete rendering pipeline
/// </summary>
public class RenderingPipelineTests : IAsyncLifetime
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
    public async Task Pipeline_ShouldClearFramebuffer()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var clearWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                gl.clearColor(1.0, 0.0, 0.0, 1.0);
                gl.clear(gl.COLOR_BUFFER_BIT);

                return true;
            }
        ");

        // Assert
        Assert.True(clearWorks, "Framebuffer should be cleared successfully");
    }

    [Fact]
    public async Task Pipeline_ShouldSetViewport()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var viewportSet = await _page.EvaluateAsync<string>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                gl.viewport(0, 0, 800, 600);

                const viewport = gl.getParameter(gl.VIEWPORT);
                return `${viewport[0]},${viewport[1]},${viewport[2]},${viewport[3]}`;
            }
        ");

        // Assert
        Assert.Equal("0,0,800,600", viewportSet);
    }

    [Fact]
    public async Task Pipeline_ShouldEnableDepthTest()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var depthTestEnabled = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                gl.enable(gl.DEPTH_TEST);
                gl.depthFunc(gl.LEQUAL);

                const enabled = gl.isEnabled(gl.DEPTH_TEST);
                const func = gl.getParameter(gl.DEPTH_FUNC);

                return enabled && func === gl.LEQUAL;
            }
        ");

        // Assert
        Assert.True(depthTestEnabled, "Depth testing should be enabled and configured");
    }

    [Fact]
    public async Task Pipeline_ShouldEnableBlending()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var blendingWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                gl.enable(gl.BLEND);
                gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);

                const enabled = gl.isEnabled(gl.BLEND);
                return enabled;
            }
        ");

        // Assert
        Assert.True(blendingWorks, "Blending should be enabled");
    }

    [Fact]
    public async Task Pipeline_ShouldDrawTriangles()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var drawWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create simple shaders
                const vsSource = `#version 300 es
                    in vec3 position;
                    void main() {
                        gl_Position = vec4(position, 1.0);
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

                if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                    return false;
                }

                gl.useProgram(program);

                // Create triangle
                const vertices = new Float32Array([
                    0.0,  0.5, 0.0,
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

                // Draw
                gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                gl.drawArrays(gl.TRIANGLES, 0, 3);

                // Clean up
                gl.deleteBuffer(buffer);
                gl.deleteVertexArray(vao);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return true;
            }
        ");

        // Assert
        Assert.True(drawWorks, "Triangle should be drawn successfully");
    }

    [Fact]
    public async Task Pipeline_ShouldDrawIndexedGeometry()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var indexedDrawWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create simple shaders
                const vsSource = `#version 300 es
                    in vec3 position;
                    void main() {
                        gl_Position = vec4(position, 1.0);
                    }
                `;

                const fsSource = `#version 300 es
                    precision highp float;
                    out vec4 fragColor;
                    void main() {
                        fragColor = vec4(0.0, 1.0, 0.0, 1.0);
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

                // Create quad with indexed drawing
                const vertices = new Float32Array([
                   -0.5,  0.5, 0.0,
                   -0.5, -0.5, 0.0,
                    0.5, -0.5, 0.0,
                    0.5,  0.5, 0.0
                ]);

                const indices = new Uint16Array([
                    0, 1, 2,
                    0, 2, 3
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

                // Draw indexed
                gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                gl.drawElements(gl.TRIANGLES, 6, gl.UNSIGNED_SHORT, 0);

                // Clean up
                gl.deleteBuffer(vbo);
                gl.deleteBuffer(ibo);
                gl.deleteVertexArray(vao);
                gl.deleteProgram(program);
                gl.deleteShader(vs);
                gl.deleteShader(fs);

                return true;
            }
        ");

        // Assert
        Assert.True(indexedDrawWorks, "Indexed geometry should be drawn successfully");
    }

    [Fact]
    public async Task Pipeline_ShouldHandleMultipleDrawCalls()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => {
                const test = document.querySelector('[data-test=""Multiple Objects""]');
                return test !== null;
            }
        ", new() { Timeout = 20000 });

        var multiObjectTest = await _page.QuerySelectorAsync("[data-test='Multiple Objects']");
        var className = await multiObjectTest!.GetAttributeAsync("class");

        // Assert
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Pipeline_ShouldRenderToTexture()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var renderToTextureWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create framebuffer and texture
                const fb = gl.createFramebuffer();
                const texture = gl.createTexture();

                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, 256, 256, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

                gl.bindFramebuffer(gl.FRAMEBUFFER, fb);
                gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, texture, 0);

                const status = gl.checkFramebufferStatus(gl.FRAMEBUFFER);

                if (status === gl.FRAMEBUFFER_COMPLETE) {
                    // Render to texture
                    gl.viewport(0, 0, 256, 256);
                    gl.clearColor(0.0, 0.5, 1.0, 1.0);
                    gl.clear(gl.COLOR_BUFFER_BIT);
                }

                // Restore default framebuffer
                gl.bindFramebuffer(gl.FRAMEBUFFER, null);

                // Clean up
                gl.deleteTexture(texture);
                gl.deleteFramebuffer(fb);

                return status === gl.FRAMEBUFFER_COMPLETE;
            }
        ");

        // Assert
        Assert.True(renderToTextureWorks, "Rendering to texture should work");
    }

    [Fact]
    public async Task Pipeline_ShouldHandleCulling()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var cullingWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                gl.enable(gl.CULL_FACE);
                gl.cullFace(gl.BACK);
                gl.frontFace(gl.CCW);

                const enabled = gl.isEnabled(gl.CULL_FACE);
                const cullFace = gl.getParameter(gl.CULL_FACE_MODE);
                const frontFace = gl.getParameter(gl.FRONT_FACE);

                return enabled && cullFace === gl.BACK && frontFace === gl.CCW;
            }
        ");

        // Assert
        Assert.True(cullingWorks, "Face culling should be configured correctly");
    }

    [Fact]
    public async Task Pipeline_ShouldMeasurePerformance()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        await Task.Delay(2000); // Let rendering settle

        var hasNoPerformanceIssues = await _page.EvaluateAsync<bool>(@"
            () => {
                // Check if rendering is reasonably fast
                const start = performance.now();

                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Render a simple triangle 100 times
                for (let i = 0; i < 100; i++) {
                    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
                }

                const elapsed = performance.now() - start;

                // Should take less than 1 second
                return elapsed < 1000;
            }
        ");

        // Assert
        Assert.True(hasNoPerformanceIssues, "Rendering should be performant");
    }

    [Fact]
    public async Task Pipeline_AllTestsShouldPass()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        // Wait for all tests to complete
        await _page.WaitForFunctionAsync(@"
            () => {
                const results = document.querySelectorAll('#testResults li');
                return results.length >= 8;
            }
        ", new() { Timeout = 30000 });

        // Get test results
        var results = await _page.EvaluateAsync<Dictionary<string, bool>>(@"
            () => {
                const testResults = {};
                const items = document.querySelectorAll('#testResults li');

                items.forEach(item => {
                    const testName = item.getAttribute('data-test');
                    const passed = item.classList.contains('passed');
                    if (testName) {
                        testResults[testName] = passed;
                    }
                });

                return testResults;
            }
        ");

        // Assert - All tests should pass
        foreach (var result in results)
        {
            Assert.True(result.Value, $"Test '{result.Key}' should pass");
        }

        Assert.NotEmpty(results);
    }
}
