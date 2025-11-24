using BlazorGL.Loaders.Textures;
using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace BlazorGL.Tests.Loaders.Textures;

public class KTX2LoaderTests
{
    [Fact]
    public void Constructor_WithNullJSRuntime_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new KTX2Loader(null!, Mock.Of<HttpClient>());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("jsRuntime");
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act
        Action act = () => new KTX2Loader(Mock.Of<IJSRuntime>(), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public async Task LoadAsync_WithoutInitialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var (loader, _) = CreateLoader();

        // Act
        Func<Task> act = async () => await loader.LoadAsync("http://test.com/test.ktx2");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not initialized*");
    }

    [Fact]
    public async Task InitializeAsync_LoadsJavaScriptModule()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var moduleMock = new Mock<IJSObjectReference>();

        jsRuntimeMock
            .Setup(js => js.InvokeAsync<IJSObjectReference>(
                "import",
                It.IsAny<object[]>()))
            .ReturnsAsync(moduleMock.Object);

        moduleMock
            .Setup(m => m.InvokeAsync<IJSObjectReference>("initialize", It.IsAny<object[]>()))
            .Returns(new ValueTask<IJSObjectReference>((IJSObjectReference)null!));

        var httpClient = new HttpClient();
        var loader = new KTX2Loader(jsRuntimeMock.Object, httpClient);

        // Act
        await loader.InitializeAsync();

        // Assert
        jsRuntimeMock.Verify(
            js => js.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_InitializesOnlyOnce()
    {
        // Arrange
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var moduleMock = new Mock<IJSObjectReference>();

        jsRuntimeMock
            .Setup(js => js.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()))
            .ReturnsAsync(moduleMock.Object);

        moduleMock
            .Setup(m => m.InvokeAsync<IJSObjectReference>("initialize", It.IsAny<object[]>()))
            .Returns(new ValueTask<IJSObjectReference>((IJSObjectReference)null!));

        var httpClient = new HttpClient();
        var loader = new KTX2Loader(jsRuntimeMock.Object, httpClient);

        // Act
        await loader.InitializeAsync();
        await loader.InitializeAsync(); // Second call

        // Assert
        jsRuntimeMock.Verify(
            js => js.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()),
            Times.Once); // Should only be called once
    }

    [Fact]
    public async Task LoadAsync_WithNullUrl_ThrowsArgumentException()
    {
        // Arrange
        var (loader, _) = CreateLoader();
        await loader.InitializeAsync();

        // Act
        Func<Task> act = async () => await loader.LoadAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task LoadAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var (loader, _) = CreateLoader();
        await loader.InitializeAsync();

        // Act
        Func<Task> act = async () => await loader.LoadAsync(string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("url");
    }

    [Fact]
    public async Task DisposeAsync_DisposesJavaScriptModule()
    {
        // Arrange
        var (loader, moduleMock) = CreateLoader();
        await loader.InitializeAsync();

        // Act
        await loader.DisposeAsync();

        // Assert
        moduleMock.Verify(m => m.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_CalledTwice_DisposesModuleOnlyOnce()
    {
        // Arrange
        var (loader, moduleMock) = CreateLoader();
        await loader.InitializeAsync();

        // Act
        await loader.DisposeAsync();
        await loader.DisposeAsync(); // Second call

        // Assert
        moduleMock.Verify(m => m.DisposeAsync(), Times.Once);
    }

    // Helper methods

    private (KTX2Loader loader, Mock<IJSObjectReference> moduleMock) CreateLoader(byte[]? ktx2Data = null)
    {
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var moduleMock = new Mock<IJSObjectReference>();

        jsRuntimeMock
            .Setup(js => js.InvokeAsync<IJSObjectReference>("import", It.IsAny<object[]>()))
            .ReturnsAsync(moduleMock.Object);

        moduleMock
            .Setup(m => m.InvokeAsync<IJSObjectReference>("initialize", It.IsAny<object[]>()))
            .Returns(new ValueTask<IJSObjectReference>((IJSObjectReference)null!));

        moduleMock
            .Setup(m => m.InvokeAsync<TextureCapabilities>("getCapabilities", It.IsAny<object[]>()))
            .ReturnsAsync(new TextureCapabilities
            {
                ASTC = true,
                BC7 = false,
                ETC2 = true,
                PVRTC = false
            });

        moduleMock
            .Setup(m => m.InvokeAsync<KTX2ContainerInfo>("parseKTX2", It.IsAny<object[]>()))
            .ReturnsAsync(new KTX2ContainerInfo
            {
                Width = 512,
                Height = 512,
                Levels = 10,
                IsUASTC = true,
                HasAlpha = true,
                IsSRGB = false
            });

        moduleMock
            .Setup(m => m.InvokeAsync<List<TranscodedMipmap>>("transcode", It.IsAny<object[]>()))
            .ReturnsAsync(new List<TranscodedMipmap>
            {
                new TranscodedMipmap { Data = new byte[64], Width = 512, Height = 512, Level = 0 }
            });

        // Setup HTTP client
        var handlerMock = new Mock<HttpMessageHandler>();

        byte[] responseData = ktx2Data ?? CreateMockKTX2Data();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(responseData)
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var loader = new KTX2Loader(jsRuntimeMock.Object, httpClient);

        return (loader, moduleMock);
    }

    private byte[] CreateMockKTX2Data()
    {
        // Create minimal KTX2 identifier
        return new byte[]
        {
            0xAB, 0x4B, 0x54, 0x58, // Identifier
            0x20, 0x32, 0x30, 0xBB,
            0x0D, 0x0A, 0x1A, 0x0A
        };
    }
}
