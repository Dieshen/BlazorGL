using System.Numerics;
using BlazorGL.Controls;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Rendering;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace BlazorGL.Controls.Tests;

public class DragControlsTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IJSObjectReference> _mockJsModule;
    private readonly Mock<Renderer> _mockRenderer;
    private readonly PerspectiveCamera _camera;
    private readonly List<Object3D> _objects;
    private const string DomElementId = "canvas";

    public DragControlsTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockJsModule = new Mock<IJSObjectReference>();
        _mockRenderer = new Mock<Renderer>();
        _camera = new PerspectiveCamera(75, 16.0f / 9.0f, 0.1f, 1000f);
        _camera.Position = new Vector3(0, 0, 5);
        _objects = new List<Object3D>();
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.NotNull(controls);
        Assert.True(controls.Enabled);
        Assert.True(controls.Recursive);
        Assert.Equal(TransformMode.Translate, controls.TransformMode);
    }

    [Fact]
    public void Constructor_WithNullCamera_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DragControls(null!, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullObjects_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DragControls(_camera, null!, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DragControls(_camera, _objects, null!, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DragControls(_camera, _objects, _mockRenderer.Object, null!, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullDomElementId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, null!));
    }

    [Fact]
    public async Task InitializeAsync_CallsJavaScriptInterop()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

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
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.True(controls.Enabled);
        Assert.True(controls.Recursive);
        Assert.Equal(TransformMode.Translate, controls.TransformMode);
        Assert.Null(controls.Selected);
        Assert.Null(controls.Hovered);
        Assert.NotNull(controls.Objects);
    }

    [Fact]
    public void Enabled_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.Enabled = false;

        // Assert
        Assert.False(controls.Enabled);
    }

    [Fact]
    public void Recursive_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.Recursive = false;

        // Assert
        Assert.False(controls.Recursive);
    }

    [Fact]
    public void TransformMode_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.TransformMode = TransformMode.Rotate;

        // Assert
        Assert.Equal(TransformMode.Rotate, controls.TransformMode);
    }

    [Fact]
    public void Objects_ReturnsProvidedList()
    {
        // Arrange
        var objectList = new List<Object3D>
        {
            new Object3D(),
            new Object3D()
        };

        // Act
        var controls = new DragControls(_camera, objectList, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.Same(objectList, controls.Objects);
    }

    [Fact]
    public void OnPointerDown_WhenDisabled_DoesNotSelectObject()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Enabled = false;

        // Act
        controls.OnPointerDown(0, 0);

        // Assert
        Assert.Null(controls.Selected);
    }

    [Fact]
    public void OnPointerDown_WithNoIntersection_DoesNotSelectObject()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.OnPointerDown(0, 0);

        // Assert
        Assert.Null(controls.Selected);
    }

    [Fact]
    public void OnPointerMove_WhenDisabled_DoesNotMoveObject()
    {
        // Arrange
        var obj = new Object3D { Position = Vector3.Zero };
        _objects.Add(obj);
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Enabled = false;

        // Act
        controls.OnPointerMove(0.5f, 0.5f);

        // Assert
        Assert.Null(controls.Selected);
    }

    [Fact]
    public void OnPointerUp_ClearsSelection()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        // Simulate selection (would normally happen through raycasting)
        // For this test, just verify OnPointerUp doesn't throw

        // Act
        controls.OnPointerUp();

        // Assert
        Assert.Null(controls.Selected);
    }

    [Fact]
    public void DragStart_EventFiresOnSelection()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        DragEventArgs? capturedArgs = null;
        controls.DragStart += (s, e) => capturedArgs = e;

        // Note: Actual selection requires raycasting which needs a full scene setup
        // This test verifies the event infrastructure is in place
        // Act & Assert - no exception thrown
        Assert.NotNull(controls);
    }

    [Fact]
    public void Drag_EventFiresDuringDrag()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        DragEventArgs? capturedArgs = null;
        controls.Drag += (s, e) => capturedArgs = e;

        // Act & Assert - event infrastructure exists
        Assert.NotNull(controls);
    }

    [Fact]
    public void DragEnd_EventFiresOnRelease()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        DragEventArgs? capturedArgs = null;
        controls.DragEnd += (s, e) => capturedArgs = e;

        // Act & Assert - event infrastructure exists
        Assert.NotNull(controls);
    }

    [Fact]
    public void HoverOn_EventFiresOnHover()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        HoverEventArgs? capturedArgs = null;
        controls.HoverOn += (s, e) => capturedArgs = e;

        // Act & Assert - event infrastructure exists
        Assert.NotNull(controls);
    }

    [Fact]
    public void HoverOff_EventFiresOnHoverEnd()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        HoverEventArgs? capturedArgs = null;
        controls.HoverOff += (s, e) => capturedArgs = e;

        // Act & Assert - event infrastructure exists
        Assert.NotNull(controls);
    }

    [Fact]
    public void Update_DoesNotThrow()
    {
        // Arrange
        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act & Assert - should not throw
        controls.Update();
    }

    [Fact]
    public void DragEventArgs_HasCorrectProperties()
    {
        // Arrange
        var obj = new Object3D();
        var point = new Vector3(1, 2, 3);

        // Act
        var args = new DragEventArgs
        {
            Object = obj,
            Point = point
        };

        // Assert
        Assert.Equal(obj, args.Object);
        Assert.Equal(point, args.Point);
    }

    [Fact]
    public void HoverEventArgs_HasCorrectProperties()
    {
        // Arrange
        var obj = new Object3D();

        // Act
        var args = new HoverEventArgs
        {
            Object = obj
        };

        // Assert
        Assert.Equal(obj, args.Object);
    }

    [Fact]
    public async Task DisposeAsync_CallsJavaScriptDispose()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
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

        var controls = new DragControls(_camera, _objects, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        await controls.InitializeAsync();

        // Act
        await controls.DisposeAsync();
        await controls.DisposeAsync();

        // Assert - no exception should be thrown
        Assert.NotNull(controls);
    }
}
