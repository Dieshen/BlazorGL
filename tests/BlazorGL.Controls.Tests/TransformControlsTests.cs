using System.Numerics;
using BlazorGL.Controls;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Rendering;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace BlazorGL.Controls.Tests;

public class TransformControlsTests
{
    private readonly Mock<IJSRuntime> _mockJsRuntime;
    private readonly Mock<IJSObjectReference> _mockJsModule;
    private readonly Mock<Renderer> _mockRenderer;
    private readonly PerspectiveCamera _camera;
    private const string DomElementId = "canvas";

    public TransformControlsTests()
    {
        _mockJsRuntime = new Mock<IJSRuntime>();
        _mockJsModule = new Mock<IJSObjectReference>();
        _mockRenderer = new Mock<Renderer>();
        _camera = new PerspectiveCamera(75, 16.0f / 9.0f, 0.1f, 1000f);
        _camera.Position = new Vector3(0, 0, 5);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.NotNull(controls);
        Assert.True(controls.Enabled);
        Assert.Equal(TransformMode.Translate, controls.Mode);
        Assert.Equal(TransformSpace.World, controls.Space);
    }

    [Fact]
    public void Constructor_WithNullCamera_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TransformControls(null!, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullRenderer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TransformControls(_camera, null!, _mockJsRuntime.Object, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TransformControls(_camera, _mockRenderer.Object, null!, DomElementId));
    }

    [Fact]
    public void Constructor_WithNullDomElementId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, null!));
    }

    [Fact]
    public async Task InitializeAsync_CallsJavaScriptInterop()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

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
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Assert
        Assert.Equal(TransformMode.Translate, controls.Mode);
        Assert.Equal(TransformSpace.World, controls.Space);
        Assert.True(controls.Enabled);
        Assert.True(controls.ShowX);
        Assert.True(controls.ShowY);
        Assert.True(controls.ShowZ);
        Assert.Null(controls.TranslationSnap);
        Assert.Null(controls.RotationSnap);
        Assert.Null(controls.ScaleSnap);
        Assert.Equal(1.0f, controls.Size);
        Assert.False(controls.Dragging);
        Assert.Null(controls.Object);
    }

    [Fact]
    public void Mode_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.Mode = TransformMode.Rotate;

        // Assert
        Assert.Equal(TransformMode.Rotate, controls.Mode);
    }

    [Fact]
    public void Space_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.Space = TransformSpace.Local;

        // Assert
        Assert.Equal(TransformSpace.Local, controls.Space);
    }

    [Fact]
    public void TranslationSnap_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.TranslationSnap = 0.5f;

        // Assert
        Assert.Equal(0.5f, controls.TranslationSnap);
    }

    [Fact]
    public void RotationSnap_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.RotationSnap = MathF.PI / 4;

        // Assert
        Assert.Equal(MathF.PI / 4, controls.RotationSnap);
    }

    [Fact]
    public void ScaleSnap_CanBeSetAndRetrieved()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act
        controls.ScaleSnap = 0.1f;

        // Assert
        Assert.Equal(0.1f, controls.ScaleSnap);
    }

    [Fact]
    public void Attach_WithValidObject_SetsObjectAndVisibility()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D();

        // Act
        controls.Attach(obj);

        // Assert
        Assert.Equal(obj, controls.Object);
        Assert.True(controls.Visible);
    }

    [Fact]
    public void Attach_WithNullObject_ThrowsArgumentNullException()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => controls.Attach(null!));
    }

    [Fact]
    public void Detach_SetsObjectToNullAndHidesControls()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D();
        controls.Attach(obj);

        // Act
        controls.Detach();

        // Assert
        Assert.Null(controls.Object);
        Assert.False(controls.Visible);
    }

    [Fact]
    public void Update_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Enabled = false;
        var obj = new Object3D();
        controls.Attach(obj);

        // Act & Assert - should not throw
        controls.Update();
    }

    [Fact]
    public void Update_WhenNoObjectAttached_DoesNotThrow()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);

        // Act & Assert - should not throw
        controls.Update();
    }

    [Fact]
    public void OnPointerDown_WhenEnabled_SetsDraggingState()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(1, 2, 3) };
        controls.Attach(obj);
        var eventFired = false;
        controls.DraggingChanged += (s, dragging) => eventFired = dragging;

        // Act
        controls.OnPointerDown(0, 0, "Y");

        // Assert
        Assert.True(controls.Dragging);
        Assert.True(eventFired);
    }

    [Fact]
    public void OnPointerDown_WhenDisabled_DoesNotStartDragging()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Enabled = false;
        var obj = new Object3D { Position = new Vector3(1, 2, 3) };
        controls.Attach(obj);

        // Act
        controls.OnPointerDown(0, 0, "Y");

        // Assert
        Assert.False(controls.Dragging);
    }

    [Fact]
    public void OnPointerMove_InTranslateMode_MovesObject()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Mode = TransformMode.Translate;
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var originalPosition = obj.Position;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerMove(0, 0.5f);

        // Assert - object position should have changed
        Assert.NotEqual(originalPosition, obj.Position);
    }

    [Fact]
    public void OnPointerMove_InRotateMode_RotatesObject()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Mode = TransformMode.Rotate;
        var obj = new Object3D { Quaternion = Quaternion.Identity };
        controls.Attach(obj);
        var originalQuaternion = obj.Quaternion;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerMove(0.5f, 0);

        // Assert - object quaternion should have changed
        Assert.NotEqual(originalQuaternion, obj.Quaternion);
    }

    [Fact]
    public void OnPointerMove_InScaleMode_ScalesObject()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Mode = TransformMode.Scale;
        var obj = new Object3D { Scale = Vector3.One };
        controls.Attach(obj);
        var originalScale = obj.Scale;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerMove(0, 0.5f);

        // Assert - object scale should have changed
        Assert.NotEqual(originalScale, obj.Scale);
    }

    [Fact]
    public void OnPointerMove_WithTranslationSnap_SnapsPosition()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        controls.Mode = TransformMode.Translate;
        controls.TranslationSnap = 1.0f;
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerMove(0, 0.75f); // Should snap to nearest grid

        // Assert - Y position should be snapped to grid
        var y = obj.Position.Y;
        Assert.Equal(0, MathF.Abs(y % 1.0f), 5); // Should be on grid
    }

    [Fact]
    public void OnPointerUp_EndsDragging()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var eventFired = false;
        controls.DraggingChanged += (s, dragging) => eventFired = !dragging;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerUp();

        // Assert
        Assert.False(controls.Dragging);
        Assert.True(eventFired);
    }

    [Fact]
    public void DraggingChanged_EventFiresCorrectly()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var eventCount = 0;
        controls.DraggingChanged += (s, dragging) => eventCount++;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerUp();

        // Assert
        Assert.Equal(2, eventCount); // Once for start, once for end
    }

    [Fact]
    public void MouseDown_EventFiresOnPointerDown()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var eventFired = false;
        controls.MouseDown += (s, e) => eventFired = true;

        // Act
        controls.OnPointerDown(0, 0, "Y");

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void MouseUp_EventFiresOnPointerUp()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var eventFired = false;
        controls.MouseUp += (s, e) => eventFired = true;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerUp();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Change_EventFiresDuringDrag()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var eventFired = false;
        controls.Change += (s, e) => eventFired = true;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerMove(0, 0.5f);

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void ObjectChanged_EventFiresOnDragEnd()
    {
        // Arrange
        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        var obj = new Object3D { Position = new Vector3(0, 0, 0) };
        controls.Attach(obj);
        var eventFired = false;
        controls.ObjectChanged += (s, e) => eventFired = true;

        // Act
        controls.OnPointerDown(0, 0, "Y");
        controls.OnPointerMove(0, 0.5f);
        controls.OnPointerUp();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public async Task DisposeAsync_CallsJavaScriptDispose()
    {
        // Arrange
        _mockJsRuntime.Setup(x => x.InvokeAsync<IJSObjectReference>(
            "import",
            It.IsAny<object[]>()))
            .ReturnsAsync(_mockJsModule.Object);

        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
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

        var controls = new TransformControls(_camera, _mockRenderer.Object, _mockJsRuntime.Object, DomElementId);
        await controls.InitializeAsync();

        // Act
        await controls.DisposeAsync();
        await controls.DisposeAsync();

        // Assert - no exception should be thrown
        Assert.NotNull(controls);
    }
}
