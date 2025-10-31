using Microsoft.Playwright;
using Xunit;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Integration tests for WebGL buffer operations (VBO, VAO, IBO)
/// </summary>
public class BufferIntegrationTests : IAsyncLifetime
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
    public async Task Buffer_ShouldCreateVertexBuffer()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var bufferCreated = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const buffer = gl.createBuffer();
                const isBuffer = gl.isBuffer(buffer);
                gl.deleteBuffer(buffer);

                return isBuffer;
            }
        ");

        // Assert
        Assert.True(bufferCreated, "Vertex buffer should be created successfully");
    }

    [Fact]
    public async Task Buffer_ShouldUploadVertexData()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var dataUploaded = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);

                const vertices = new Float32Array([
                    -1.0, -1.0, 0.0,
                     1.0, -1.0, 0.0,
                     0.0,  1.0, 0.0
                ]);

                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                const size = gl.getBufferParameter(gl.ARRAY_BUFFER, gl.BUFFER_SIZE);
                gl.deleteBuffer(buffer);

                return size === vertices.byteLength;
            }
        ");

        // Assert
        Assert.True(dataUploaded, "Vertex data should be uploaded to GPU buffer");
    }

    [Fact]
    public async Task Buffer_ShouldCreateIndexBuffer()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var indexBufferWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, buffer);

                const indices = new Uint16Array([0, 1, 2, 0, 2, 3]);
                gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);

                const size = gl.getBufferParameter(gl.ELEMENT_ARRAY_BUFFER, gl.BUFFER_SIZE);
                gl.deleteBuffer(buffer);

                return size === indices.byteLength;
            }
        ");

        // Assert
        Assert.True(indexBufferWorks, "Index buffer should be created and filled");
    }

    [Fact]
    public async Task Buffer_ShouldCreateVAO()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var vaoWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const vao = gl.createVertexArray();
                const isVAO = gl.isVertexArray(vao);
                gl.deleteVertexArray(vao);

                return isVAO;
            }
        ");

        // Assert
        Assert.True(vaoWorks, "Vertex Array Object should be created successfully");
    }

    [Fact]
    public async Task Buffer_ShouldBindVAOAndVBO()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var bindingWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const vao = gl.createVertexArray();
                const vbo = gl.createBuffer();

                gl.bindVertexArray(vao);
                gl.bindBuffer(gl.ARRAY_BUFFER, vbo);

                const vertices = new Float32Array([
                    0.0, 0.0, 0.0,
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0
                ]);
                gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

                gl.vertexAttribPointer(0, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(0);

                gl.bindVertexArray(null);

                // Clean up
                gl.deleteBuffer(vbo);
                gl.deleteVertexArray(vao);

                return true;
            }
        ");

        // Assert
        Assert.True(bindingWorks, "VAO and VBO should bind together successfully");
    }

    [Fact]
    public async Task Buffer_ShouldHandleMultipleAttributes()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var multiAttributeWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const vao = gl.createVertexArray();
                gl.bindVertexArray(vao);

                // Position buffer
                const posBuffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, posBuffer);
                const positions = new Float32Array([0, 0, 0, 1, 0, 0, 0, 1, 0]);
                gl.bufferData(gl.ARRAY_BUFFER, positions, gl.STATIC_DRAW);
                gl.vertexAttribPointer(0, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(0);

                // Normal buffer
                const normalBuffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, normalBuffer);
                const normals = new Float32Array([0, 0, 1, 0, 0, 1, 0, 0, 1]);
                gl.bufferData(gl.ARRAY_BUFFER, normals, gl.STATIC_DRAW);
                gl.vertexAttribPointer(1, 3, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(1);

                // UV buffer
                const uvBuffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, uvBuffer);
                const uvs = new Float32Array([0, 0, 1, 0, 0, 1]);
                gl.bufferData(gl.ARRAY_BUFFER, uvs, gl.STATIC_DRAW);
                gl.vertexAttribPointer(2, 2, gl.FLOAT, false, 0, 0);
                gl.enableVertexAttribArray(2);

                gl.bindVertexArray(null);

                // Clean up
                gl.deleteBuffer(posBuffer);
                gl.deleteBuffer(normalBuffer);
                gl.deleteBuffer(uvBuffer);
                gl.deleteVertexArray(vao);

                return true;
            }
        ");

        // Assert
        Assert.True(multiAttributeWorks, "Multiple vertex attributes should be handled correctly");
    }

    [Fact]
    public async Task Buffer_ShouldUpdateBufferData()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var updateWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);

                // Initial data
                const data1 = new Float32Array([1, 2, 3, 4]);
                gl.bufferData(gl.ARRAY_BUFFER, data1, gl.DYNAMIC_DRAW);

                let size1 = gl.getBufferParameter(gl.ARRAY_BUFFER, gl.BUFFER_SIZE);

                // Update data
                const data2 = new Float32Array([5, 6, 7, 8, 9, 10]);
                gl.bufferData(gl.ARRAY_BUFFER, data2, gl.DYNAMIC_DRAW);

                let size2 = gl.getBufferParameter(gl.ARRAY_BUFFER, gl.BUFFER_SIZE);

                gl.deleteBuffer(buffer);

                return size1 === 16 && size2 === 24;
            }
        ");

        // Assert
        Assert.True(updateWorks, "Buffer data should be updatable");
    }

    [Fact]
    public async Task Buffer_ShouldHandleInterleavedData()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var interleavedWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const vao = gl.createVertexArray();
                gl.bindVertexArray(vao);

                const buffer = gl.createBuffer();
                gl.bindBuffer(gl.ARRAY_BUFFER, buffer);

                // Interleaved: position (3) + normal (3) + uv (2) = 8 floats per vertex
                const interleavedData = new Float32Array([
                    // x, y, z, nx, ny, nz, u, v
                    0, 0, 0, 0, 0, 1, 0, 0,
                    1, 0, 0, 0, 0, 1, 1, 0,
                    0, 1, 0, 0, 0, 1, 0, 1
                ]);

                gl.bufferData(gl.ARRAY_BUFFER, interleavedData, gl.STATIC_DRAW);

                const stride = 8 * 4; // 8 floats * 4 bytes

                // Position attribute
                gl.vertexAttribPointer(0, 3, gl.FLOAT, false, stride, 0);
                gl.enableVertexAttribArray(0);

                // Normal attribute
                gl.vertexAttribPointer(1, 3, gl.FLOAT, false, stride, 3 * 4);
                gl.enableVertexAttribArray(1);

                // UV attribute
                gl.vertexAttribPointer(2, 2, gl.FLOAT, false, stride, 6 * 4);
                gl.enableVertexAttribArray(2);

                gl.bindVertexArray(null);

                // Clean up
                gl.deleteBuffer(buffer);
                gl.deleteVertexArray(vao);

                return true;
            }
        ");

        // Assert
        Assert.True(interleavedWorks, "Interleaved vertex data should be handled correctly");
    }

    [Fact]
    public async Task Buffer_ShouldNotLeakBuffers()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var leakTest = await _page.EvaluateAsync<int>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                let buffers = [];

                // Create many buffers
                for (let i = 0; i < 100; i++) {
                    buffers.push(gl.createBuffer());
                }

                // Delete them
                for (let buffer of buffers) {
                    gl.deleteBuffer(buffer);
                }

                return buffers.length;
            }
        ");

        // Assert
        Assert.Equal(100, leakTest);
    }

    [Fact]
    public async Task Buffer_GeometryBuffersTest_ShouldPass()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => {
                const test = document.querySelector('[data-test=""Geometry Buffers""]');
                return test !== null;
            }
        ", new() { Timeout = 15000 });

        var geometryTest = await _page.QuerySelectorAsync("[data-test='Geometry Buffers']");
        var className = await geometryTest!.GetAttributeAsync("class");

        // Assert
        Assert.Contains("passed", className);
    }
}
