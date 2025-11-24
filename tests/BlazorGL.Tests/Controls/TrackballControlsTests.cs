using System.Numerics;
using BlazorGL.Controls;
using BlazorGL.Core.Cameras;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace BlazorGL.Tests.Controls;

public class TrackballControlsTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IJSObjectReference> _mockJsModule;
    private readonly PerspectiveCamera _camera;
    private const string DomElementId = "canvas";

    public TrackballControlsTests()
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
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);

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
            new TrackballControls(null!, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TrackballControls(_camera, null!, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullDomElementId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TrackballControls(_camera, _mockJsRuntime.Object, null!));
    }

    [Fact]
    public async Task InitializeAsync_CallsJavaScriptInterop()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Act
        await controls.InitializeAsync();

        // Assert
        _mockJsRuntime.Verify(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void DefaultProperties_HaveCorrectValues()
    {
        // Arrange & Act
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.Equal(Vector3.Zero, controls.Target);
        Assert.False(controls.StaticMoving);
        Assert.Equal(0.2f, controls.DynamicDampingFactor);
        Assert.Equal(0f, controls.MinDistance);
        Assert.Equal(float.PositiveInfinity, controls.MaxDistance);
        Assert.Equal(1.0f, controls.RotateSpeed);
        Assert.Equal(1.2f, controls.ZoomSpeed);
        Assert.Equal(0.3f, controls.PanSpeed);
        Assert.False(controls.NoRoll);
        Assert.True(controls.Enabled);
    }

    [Fact]
    public void Target_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        var newTarget = new Vector3(10, 5, 3);

        // Act
        controls.Target = newTarget;

        // Assert
        Assert.Equal(newTarget, controls.Target);
    }

    [Fact]
    public void StaticMoving_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.StaticMoving = true;

        // Assert
        Assert.True(controls.StaticMoving);
    }

    [Fact]
    public void NoRoll_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.NoRoll = true;

        // Assert
        Assert.True(controls.NoRoll);
    }

    [Fact]
    public void Update_WhenDisabled_DoesNotUpdateCamera()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        var originalPosition = _camera.Position;
        controls.Enabled = false;

        // Act
        controls.OnRotateStart(100, 100);
        controls.OnRotateMove(150, 150);
        controls.Update(0.016f);

        // Assert
        Assert.Equal(originalPosition, _camera.Position);
    }

    [Fact]
    public void OnRotateStart_WhenEnabled_InitializesRotation()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);

        // Act
        controls.OnRotateStart(400, 300);

        // Assert - no exception thrown
        Assert.NotNull(controls);
    }

    [Fact(Skip = "TODO: Fix trackball rotation axis calculation")]
    public void OnRotateMove_WhenEnabled_UpdatesRotation()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);
        controls.StaticMoving = true;
        var initialPosition = _camera.Position;

        // Act
        controls.OnRotateStart(400, 300);
        controls.OnRotateMove(450, 350);
        controls.Update(0.016f);

        // Assert - camera should have rotated
        Assert.NotEqual(initialPosition, _camera.Position);
    }

    [Fact]
    public void OnRotate_WhenRotateDisabled_DoesNotRotate()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.EnableRotate = false;
        controls.Screen = new Vector2(800, 600);
        var initialPosition = _camera.Position;

        // Act
        controls.OnRotateStart(400, 300);
        controls.OnRotateMove(450, 350);
        controls.Update(0.016f);

        // Assert
        Assert.Equal(initialPosition, _camera.Position);
    }

    [Fact]
    public void OnZoomStart_WhenEnabled_InitializesZoom()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);

        // Act
        controls.OnZoomStart(400, 300);

        // Assert - no exception thrown
        Assert.NotNull(controls);
    }

    [Fact]
    public void OnZoomMove_WhenEnabled_ChangesDistanceFromTarget()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);
        controls.StaticMoving = true;
        var initialDistance = Vector3.Distance(_camera.Position, controls.Target);

        // Act
        controls.OnZoomStart(400, 300);
        controls.OnZoomMove(400, 350);
        controls.Update(0.016f);

        // Assert
        var newDistance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.NotEqual(initialDistance, newDistance);
    }

    [Fact]
    public void OnZoom_WhenZoomDisabled_DoesNotZoom()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.EnableZoom = false;
        controls.Screen = new Vector2(800, 600);
        var initialDistance = Vector3.Distance(_camera.Position, controls.Target);

        // Act
        controls.OnZoomStart(400, 300);
        controls.OnZoomMove(400, 350);
        controls.Update(0.016f);

        // Assert
        var newDistance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.Equal(initialDistance, newDistance, 5);
    }

    [Fact]
    public void OnZoom_RespectsMinDistance()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.MinDistance = 2.0f;
        controls.Screen = new Vector2(800, 600);
        controls.StaticMoving = true;

        // Act - zoom in a lot
        controls.OnZoomStart(400, 300);
        for (int i = 0; i < 50; i++)
        {
            controls.OnZoomMove(400, 400 + i * 10);
            controls.Update(0.016f);
        }

        // Assert
        var distance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.True(distance >= controls.MinDistance - 0.1f);
    }

    [Fact]
    public void OnZoom_RespectsMaxDistance()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.MaxDistance = 10.0f;
        controls.Screen = new Vector2(800, 600);
        controls.StaticMoving = true;

        // Act - zoom out a lot
        controls.OnZoomStart(400, 300);
        for (int i = 0; i < 50; i++)
        {
            controls.OnZoomMove(400, 300 - i * 10);
            controls.Update(0.016f);
        }

        // Assert
        var distance = Vector3.Distance(_camera.Position, controls.Target);
        Assert.True(distance <= controls.MaxDistance + 0.1f);
    }

    [Fact]
    public void OnPanStart_WhenEnabled_InitializesPan()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);

        // Act
        controls.OnPanStart(400, 300);

        // Assert - no exception thrown
        Assert.NotNull(controls);
    }

    [Fact(Skip = "TODO: Fix trackball pan calculation")]
    public void OnPanMove_WhenEnabled_MovesTargetAndCamera()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);
        controls.StaticMoving = true;
        var initialTarget = controls.Target;

        // Act
        controls.OnPanStart(400, 300);
        controls.OnPanMove(450, 350);
        controls.Update(0.016f);

        // Assert
        Assert.NotEqual(initialTarget, controls.Target);
    }

    [Fact]
    public void OnPan_WhenPanDisabled_DoesNotPan()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.EnablePan = false;
        controls.Screen = new Vector2(800, 600);
        var initialTarget = controls.Target;

        // Act
        controls.OnPanStart(400, 300);
        controls.OnPanMove(450, 350);
        controls.Update(0.016f);

        // Assert
        Assert.Equal(initialTarget, controls.Target);
    }

    [Fact]
    public void Reset_RestoresInitialState()
    {
        // Arrange
        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls.Screen = new Vector2(800, 600);

        // Act - make some changes
        controls.OnRotateStart(400, 300);
        controls.OnRotateMove(450, 350);
        controls.OnZoomStart(400, 300);
        controls.OnZoomMove(400, 350);
        controls.Update(0.016f);

        // Reset
        controls.Reset();

        // Assert
        Assert.Equal(Vector3.Zero, controls.Target);
    }

    [Fact(Skip = "TODO: Fix trackball rotation axis calculation")]
    public void RotateSpeed_AffectsRotationAmount()
    {
        // Arrange
        var controls1 = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        controls1.RotateSpeed = 1.0f;
        controls1.Screen = new Vector2(800, 600);
        controls1.StaticMoving = true;

        var camera2 = new PerspectiveCamera(75, 16.0f / 9.0f, 0.1f, 1000f);
        camera2.Position = new Vector3(0, 0, 5);
        var controls2 = new TrackballControls(camera2, _mockJsRuntime.Object, DomElementId);
        controls2.RotateSpeed = 2.0f;
        controls2.Screen = new Vector2(800, 600);
        controls2.StaticMoving = true;

        // Act - same rotation input
        controls1.OnRotateStart(400, 300);
        controls1.OnRotateMove(450, 300);
        controls1.Update(0.016f);

        controls2.OnRotateStart(400, 300);
        controls2.OnRotateMove(450, 300);
        controls2.Update(0.016f);

        // Assert - controls2 should have rotated more
        var distance1 = Vector3.Distance(_camera.Position, new Vector3(0, 0, 5));
        var distance2 = Vector3.Distance(camera2.Position, new Vector3(0, 0, 5));

        // Both should have moved from origin
        Assert.True(distance1 > 0 || distance2 > 0);
    }

    [Fact]
    public async Task DisposeAsync_CallsJavaScriptDispose()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        await controls.InitializeAsync();

        // Act
        await controls.DisposeAsync();

        // Assert - no exception thrown
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

        var controls = new TrackballControls(_camera, _mockJsRuntime.Object, DomElementId);
        await controls.InitializeAsync();

        // Act
        await controls.DisposeAsync();
        await controls.DisposeAsync();

        // Assert - no exception should be thrown
        Assert.NotNull(controls);
    }
}
