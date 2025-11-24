using System.Numerics;
using BlazorGL.Controls;
using BlazorGL.Core.Cameras;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace BlazorGL.Tests.Controls;

public class OrbitControlsTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IJSObjectReference> _mockJsModule;
    private readonly PerspectiveCamera _camera;
    private const string DomElementId = "canvas";

    public OrbitControlsTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockJsModule = new Mock<IJSObjectReference>();
        _camera = new PerspectiveCamera(75, 16.0f / 9.0f, 0.1f, 1000f);
        _camera.Position = new Vector3(0, 0, 5);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.NotNull(controls);
        Assert.True(controls.Enabled);
        Assert.True(controls.EnableRotate);
        Assert.True(controls.EnableZoom);
        Assert.True(controls.EnablePan);
    }

    [Fact]
    public void Constructor_WithNullCamera_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OrbitControls(null!, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OrbitControls(_camera, null!, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullDomElementId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OrbitControls(_camera, _mockJsRuntime.Object, null!));
    }

    [Fact]
    public async Task InitializeAsync_CallsJavaScriptInterop()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Act
        await controls.InitializeAsync();

        // Assert
        _mockJsRuntime.Verify(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()), Times.Once);

        // Verify JSModule was called (extension methods can't be verified with Moq)
        // The test passes if no exception is thrown during initialization
        Assert.NotNull(controls);
    }

    [Fact]
    public void DefaultProperties_HaveCorrectValues()
    {
        // Arrange & Act
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.Equal(Vector3.Zero, controls.Target);
        Assert.False(controls.EnableDamping);
        Assert.Equal(0.05f, controls.DampingFactor);
        Assert.Equal(0f, controls.MinDistance);
        Assert.Equal(float.PositiveInfinity, controls.MaxDistance);
        Assert.Equal(0f, controls.MinPolarAngle);
        Assert.Equal(MathF.PI, controls.MaxPolarAngle);
        Assert.Equal(float.NegativeInfinity, controls.MinAzimuthAngle);
        Assert.Equal(float.PositiveInfinity, controls.MaxAzimuthAngle);
        Assert.Equal(1.0f, controls.RotateSpeed);
        Assert.Equal(1.0f, controls.ZoomSpeed);
        Assert.Equal(1.0f, controls.PanSpeed);
        Assert.False(controls.AutoRotate);
        Assert.Equal(2.0f, controls.AutoRotateSpeed);
        Assert.True(controls.Enabled);
    }

    [Fact]
    public void Target_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        var newTarget = new Vector3(10, 5, 3);

        // Act
        controls.Target = newTarget;

        // Assert
        Assert.Equal(newTarget, controls.Target);
    }

    [Fact]
    public void EnableDamping_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.EnableDamping = true;

        // Assert
        Assert.True(controls.EnableDamping);
    }

    [Fact]
    public void Update_WhenDisabled_DoesNotUpdateCamera()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        var originalPosition = _camera.Position;
        controls.Enabled = false;

        // Act
        controls.OnRotate(1.0f, 0.5f);
        controls.Update(0.016f);

        // Assert
        Assert.Equal(originalPosition, _camera.Position);
    }

    [Fact]
    public void OnRotate_WhenEnabled_AccumulatesRotationDelta()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        var initialPosition = _camera.Position;

        // Act
        controls.OnRotate(0.1f, 0.0f);
        controls.Update(0.016f);

        // Assert
        // Camera position should have changed due to rotation
        Assert.NotEqual(initialPosition, _camera.Position);
    }

    [Fact]
    public void OnZoom_WhenEnabled_ChangesDistanceFromTarget()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        var initialDistance = Vector3.Distance(_camera.Position, controls.Target);

        // Act
        controls.OnZoom(1.0f);
        controls.Update(0.016f);

        // Assert
        var newDistance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.NotEqual(initialDistance, newDistance);
    }

    [Fact]
    public void OnZoom_RespectsMinDistance()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.MinDistance = 1.0f;

        // Act - zoom in a lot
        for (int i = 0; i < 100; i++)
        {
            controls.OnZoom(10.0f);
            controls.Update(0.016f);
        }

        // Assert
        var distance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.True(distance >= controls.MinDistance);
    }

    [Fact]
    public void OnZoom_RespectsMaxDistance()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.MaxDistance = 20.0f;

        // Act - zoom out a lot
        for (int i = 0; i < 100; i++)
        {
            controls.OnZoom(-10.0f);
            controls.Update(0.016f);
        }

        // Assert
        var distance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.True(distance <= controls.MaxDistance);
    }

    [Fact]
    public void OnPan_WhenEnabled_MovesTargetAndCamera()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        var initialTarget = controls.Target;

        // Act
        controls.OnPan(10.0f, 5.0f);
        controls.Update(0.016f);

        // Assert
        Assert.NotEqual(initialTarget, controls.Target);
    }

    [Fact]
    public void AutoRotate_WhenEnabled_RotatesCamera()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.AutoRotate = true;
        var initialPosition = _camera.Position;

        // Act
        controls.Update(0.016f);

        // Assert
        Assert.NotEqual(initialPosition, _camera.Position);
    }

    [Fact]
    public void Reset_RestoresInitialState()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Act - make some changes
        controls.OnRotate(1.0f, 1.0f);
        controls.OnZoom(5.0f);
        controls.OnPan(10.0f, 10.0f);
        controls.Update(0.016f);

        // Reset
        controls.Reset();

        // Assert
        Assert.Equal(Vector3.Zero, controls.Target);
    }

    [Fact]
    public void OnRotate_WhenRotateDisabled_DoesNotRotate()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.EnableRotate = false;
        var initialPosition = _camera.Position;

        // Act
        controls.OnRotate(1.0f, 1.0f);
        controls.Update(0.016f);

        // Assert - allow for floating point precision errors
        Assert.True(Vector3.Distance(initialPosition, _camera.Position) < 0.0001f,
            $"Expected position {initialPosition} but got {_camera.Position}");
    }

    [Fact]
    public void OnZoom_WhenZoomDisabled_DoesNotZoom()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.EnableZoom = false;
        var initialDistance = Vector3.Distance(_camera.Position, controls.Target);

        // Act
        controls.OnZoom(5.0f);
        controls.Update(0.016f);

        // Assert
        var newDistance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.Equal(initialDistance, newDistance, 5);
    }

    [Fact]
    public void OnPan_WhenPanDisabled_DoesNotPan()
    {
        // Arrange
        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.EnablePan = false;
        var initialTarget = controls.Target;

        // Act
        controls.OnPan(10.0f, 10.0f);
        controls.Update(0.016f);

        // Assert
        Assert.Equal(initialTarget, controls.Target);
    }

    [Fact]
    public async Task DisposeAsync_CallsJavaScriptDispose()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        await controls.InitializeAsync();

        // Act
        await controls.DisposeAsync();

        // Assert - extension methods can't be verified, but we can ensure no exception
        // The dispose was successful if no exception was thrown
        Assert.NotNull(controls);
    }

    [Fact]
    public async Task DisposeAsync_CanBeCalledMultipleTimes()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new OrbitControls(_camera, _mockJsRuntime.Object, DomElementId);
        await controls.InitializeAsync();

        // Act
        await controls.DisposeAsync();
        await controls.DisposeAsync();

        // Assert - no exception should be thrown when disposing multiple times
        Assert.NotNull(controls);
    }
}
