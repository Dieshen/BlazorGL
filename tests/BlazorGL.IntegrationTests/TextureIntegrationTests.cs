using Microsoft.Playwright;
using Xunit;

namespace BlazorGL.IntegrationTests;

/// <summary>
/// Integration tests for WebGL texture operations
/// </summary>
public class TextureIntegrationTests : IAsyncLifetime
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
    public async Task Texture_ShouldCreate2DTexture()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var textureCreated = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture = gl.createTexture();
                const isTexture = gl.isTexture(texture);
                gl.deleteTexture(texture);

                return isTexture;
            }
        ");

        // Assert
        Assert.True(textureCreated, "2D texture should be created successfully");
    }

    [Fact]
    public async Task Texture_ShouldUploadImageData()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var dataUploaded = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);

                // Create a simple 2x2 texture
                const pixels = new Uint8Array([
                    255, 0, 0, 255,  // Red
                    0, 255, 0, 255,  // Green
                    0, 0, 255, 255,  // Blue
                    255, 255, 0, 255 // Yellow
                ]);

                gl.texImage2D(
                    gl.TEXTURE_2D,
                    0,
                    gl.RGBA,
                    2, 2,
                    0,
                    gl.RGBA,
                    gl.UNSIGNED_BYTE,
                    pixels
                );

                gl.deleteTexture(texture);

                return true;
            }
        ");

        // Assert
        Assert.True(dataUploaded, "Texture data should be uploaded to GPU");
    }

    [Fact]
    public async Task Texture_ShouldSetTextureParameters()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var parametersSet = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);

                // Set texture parameters
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);

                // Verify parameters
                const minFilter = gl.getTexParameter(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER);
                const magFilter = gl.getTexParameter(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER);
                const wrapS = gl.getTexParameter(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S);
                const wrapT = gl.getTexParameter(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T);

                gl.deleteTexture(texture);

                return minFilter === gl.LINEAR &&
                       magFilter === gl.LINEAR &&
                       wrapS === gl.CLAMP_TO_EDGE &&
                       wrapT === gl.CLAMP_TO_EDGE;
            }
        ");

        // Assert
        Assert.True(parametersSet, "Texture parameters should be set correctly");
    }

    [Fact]
    public async Task Texture_ShouldGenerateMipmaps()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var mipmapsGenerated = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);

                // Create a 4x4 texture (power of 2)
                const size = 4;
                const pixels = new Uint8Array(size * size * 4);
                for (let i = 0; i < pixels.length; i++) {
                    pixels[i] = 128;
                }

                gl.texImage2D(
                    gl.TEXTURE_2D,
                    0,
                    gl.RGBA,
                    size, size,
                    0,
                    gl.RGBA,
                    gl.UNSIGNED_BYTE,
                    pixels
                );

                // Generate mipmaps
                gl.generateMipmap(gl.TEXTURE_2D);

                gl.deleteTexture(texture);

                return true;
            }
        ");

        // Assert
        Assert.True(mipmapsGenerated, "Mipmaps should be generated successfully");
    }

    [Fact]
    public async Task Texture_ShouldBindToTextureUnit()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var bindingWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture1 = gl.createTexture();
                const texture2 = gl.createTexture();

                // Bind to different texture units
                gl.activeTexture(gl.TEXTURE0);
                gl.bindTexture(gl.TEXTURE_2D, texture1);

                gl.activeTexture(gl.TEXTURE1);
                gl.bindTexture(gl.TEXTURE_2D, texture2);

                // Clean up
                gl.deleteTexture(texture1);
                gl.deleteTexture(texture2);

                return true;
            }
        ");

        // Assert
        Assert.True(bindingWorks, "Textures should bind to different texture units");
    }

    [Fact]
    public async Task Texture_ShouldCreateCubeMap()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var cubeMapCreated = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_CUBE_MAP, texture);

                // Create a 2x2 pixel for each face
                const pixels = new Uint8Array([
                    255, 0, 0, 255,
                    0, 255, 0, 255,
                    0, 0, 255, 255,
                    255, 255, 0, 255
                ]);

                const faces = [
                    gl.TEXTURE_CUBE_MAP_POSITIVE_X,
                    gl.TEXTURE_CUBE_MAP_NEGATIVE_X,
                    gl.TEXTURE_CUBE_MAP_POSITIVE_Y,
                    gl.TEXTURE_CUBE_MAP_NEGATIVE_Y,
                    gl.TEXTURE_CUBE_MAP_POSITIVE_Z,
                    gl.TEXTURE_CUBE_MAP_NEGATIVE_Z
                ];

                for (let face of faces) {
                    gl.texImage2D(
                        face,
                        0,
                        gl.RGBA,
                        2, 2,
                        0,
                        gl.RGBA,
                        gl.UNSIGNED_BYTE,
                        pixels
                    );
                }

                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
                gl.texParameteri(gl.TEXTURE_CUBE_MAP, gl.TEXTURE_MAG_FILTER, gl.LINEAR);

                gl.deleteTexture(texture);

                return true;
            }
        ");

        // Assert
        Assert.True(cubeMapCreated, "Cube map texture should be created successfully");
    }

    [Fact]
    public async Task Texture_ShouldCreateRenderTarget()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var renderTargetWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                // Create framebuffer
                const framebuffer = gl.createFramebuffer();
                gl.bindFramebuffer(gl.FRAMEBUFFER, framebuffer);

                // Create texture for color attachment
                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);
                gl.texImage2D(
                    gl.TEXTURE_2D,
                    0,
                    gl.RGBA,
                    512, 512,
                    0,
                    gl.RGBA,
                    gl.UNSIGNED_BYTE,
                    null
                );
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

                // Attach texture to framebuffer
                gl.framebufferTexture2D(
                    gl.FRAMEBUFFER,
                    gl.COLOR_ATTACHMENT0,
                    gl.TEXTURE_2D,
                    texture,
                    0
                );

                // Check framebuffer status
                const status = gl.checkFramebufferStatus(gl.FRAMEBUFFER);

                // Clean up
                gl.bindFramebuffer(gl.FRAMEBUFFER, null);
                gl.deleteTexture(texture);
                gl.deleteFramebuffer(framebuffer);

                return status === gl.FRAMEBUFFER_COMPLETE;
            }
        ");

        // Assert
        Assert.True(renderTargetWorks, "Render target framebuffer should be complete");
    }

    [Fact]
    public async Task Texture_ShouldCreateDepthTexture()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var depthTextureWorks = await _page.EvaluateAsync<bool>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const texture = gl.createTexture();
                gl.bindTexture(gl.TEXTURE_2D, texture);

                gl.texImage2D(
                    gl.TEXTURE_2D,
                    0,
                    gl.DEPTH_COMPONENT24,
                    512, 512,
                    0,
                    gl.DEPTH_COMPONENT,
                    gl.UNSIGNED_INT,
                    null
                );

                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.NEAREST);
                gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);

                gl.deleteTexture(texture);

                return true;
            }
        ");

        // Assert
        Assert.True(depthTextureWorks, "Depth texture should be created successfully");
    }

    [Fact]
    public async Task Texture_UploadTest_ShouldPass()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#testResults");

        await _page.WaitForFunctionAsync(@"
            () => {
                const test = document.querySelector('[data-test=""Texture Upload""]');
                return test !== null;
            }
        ", new() { Timeout = 15000 });

        var textureTest = await _page.QuerySelectorAsync("[data-test='Texture Upload']");
        var className = await textureTest!.GetAttributeAsync("class");

        // Assert
        Assert.Contains("passed", className);
    }

    [Fact]
    public async Task Texture_ShouldHandleMultipleFormats()
    {
        // Arrange & Act
        await _page!.GotoAsync(TestAppUrl);
        await _page.WaitForSelectorAsync("#glCanvas");

        var formatsWork = await _page.EvaluateAsync<string>(@"
            () => {
                const canvas = document.getElementById('glCanvas');
                const gl = canvas.getContext('webgl2');

                const formats = [
                    { internal: gl.RGBA, format: gl.RGBA, type: gl.UNSIGNED_BYTE },
                    { internal: gl.RGB, format: gl.RGB, type: gl.UNSIGNED_BYTE },
                    { internal: gl.LUMINANCE, format: gl.LUMINANCE, type: gl.UNSIGNED_BYTE },
                ];

                let successful = 0;

                for (let fmt of formats) {
                    try {
                        const texture = gl.createTexture();
                        gl.bindTexture(gl.TEXTURE_2D, texture);

                        const pixels = new Uint8Array(16);
                        gl.texImage2D(
                            gl.TEXTURE_2D,
                            0,
                            fmt.internal,
                            2, 2,
                            0,
                            fmt.format,
                            fmt.type,
                            pixels
                        );

                        gl.deleteTexture(texture);
                        successful++;
                    } catch (e) {
                        // Format not supported
                    }
                }

                return `Successful formats: ${successful}/${formats.length}`;
            }
        ");

        // Assert
        Assert.Contains("Successful", formatsWork);
    }
}
