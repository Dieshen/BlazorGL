using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Rendering;
using Microsoft.JSInterop;

namespace BlazorGL.Controls;

/// <summary>
/// Mode for transform operations
/// </summary>
public enum TransformMode
{
    /// <summary>Translate objects in 3D space</summary>
    Translate,
    /// <summary>Rotate objects around axes</summary>
    Rotate,
    /// <summary>Scale objects along axes</summary>
    Scale
}

/// <summary>
/// Space for transform operations
/// </summary>
public enum TransformSpace
{
    /// <summary>World space coordinates</summary>
    World,
    /// <summary>Local space coordinates</summary>
    Local
}

/// <summary>
/// TransformControls provides interactive 3D gizmos for manipulating objects.
/// Allows translation, rotation, and scaling with visual feedback.
/// Mirrors the API and behavior of Three.js TransformControls.
/// </summary>
public sealed class TransformControls : Object3D, IAsyncDisposable
{
    private readonly Camera _camera;
    private readonly Renderer _renderer;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _domElementId;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<TransformControls>? _dotNetRef;
    private bool _isDisposed;

    private Object3D? _object;
    private bool _dragging;
    private string? _axis;
    private Vector3 _worldPosition = Vector3.Zero;
    private Quaternion _worldQuaternion = Quaternion.Identity;
    private Vector3 _worldScale = Vector3.One;
    private Vector2 _pointerStart = Vector2.Zero;
    private Vector3 _positionStart = Vector3.Zero;
    private Quaternion _quaternionStart = Quaternion.Identity;
    private Vector3 _scaleStart = Vector3.One;

    /// <summary>
    /// Creates a new TransformControls instance
    /// </summary>
    /// <param name="camera">The camera for view calculations</param>
    /// <param name="renderer">The renderer for raycasting</param>
    /// <param name="jsRuntime">JSRuntime for JavaScript interop</param>
    /// <param name="domElementId">ID of the DOM element to attach event listeners to</param>
    public TransformControls(Camera camera, Renderer renderer, IJSRuntime jsRuntime, string domElementId)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _domElementId = domElementId ?? throw new ArgumentNullException(nameof(domElementId));
    }

    #region Properties

    /// <summary>
    /// Current transform mode
    /// </summary>
    public TransformMode Mode { get; set; } = TransformMode.Translate;

    /// <summary>
    /// Current transform space
    /// </summary>
    public TransformSpace Space { get; set; } = TransformSpace.World;

    /// <summary>
    /// Enable or disable controls completely
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Show X axis gizmo
    /// </summary>
    public bool ShowX { get; set; } = true;

    /// <summary>
    /// Show Y axis gizmo
    /// </summary>
    public bool ShowY { get; set; } = true;

    /// <summary>
    /// Show Z axis gizmo
    /// </summary>
    public bool ShowZ { get; set; } = true;

    /// <summary>
    /// Translation snap increment (null = no snap)
    /// </summary>
    public float? TranslationSnap { get; set; } = null;

    /// <summary>
    /// Rotation snap increment in radians (null = no snap)
    /// </summary>
    public float? RotationSnap { get; set; } = null;

    /// <summary>
    /// Scale snap increment (null = no snap)
    /// </summary>
    public float? ScaleSnap { get; set; } = null;

    /// <summary>
    /// Size of the gizmo
    /// </summary>
    public float Size { get; set; } = 1.0f;

    /// <summary>
    /// Whether a drag operation is in progress
    /// </summary>
    public bool Dragging => _dragging;

    /// <summary>
    /// The currently attached object
    /// </summary>
    public Object3D? Object => _object;

    #endregion

    #region Events

    /// <summary>
    /// Fired when dragging state changes
    /// </summary>
    public event EventHandler<bool>? DraggingChanged;

    /// <summary>
    /// Fired when the controlled object changes
    /// </summary>
    public event EventHandler? Change;

    /// <summary>
    /// Fired when object transformation is complete
    /// </summary>
    public event EventHandler? ObjectChanged;

    /// <summary>
    /// Fired when mouse button is pressed on gizmo
    /// </summary>
    public event EventHandler? MouseDown;

    /// <summary>
    /// Fired when mouse button is released
    /// </summary>
    public event EventHandler? MouseUp;

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the controls by loading the JavaScript module and attaching event listeners
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(TransformControls));

        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./wwwroot/blazorgl.controls.extended.js");

        _dotNetRef = DotNetObjectReference.Create(this);

        await _jsModule.InvokeVoidAsync("initTransformControls", _domElementId, _dotNetRef);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Attach controls to an object
    /// </summary>
    /// <param name="obj">The object to control</param>
    public void Attach(Object3D obj)
    {
        _object = obj ?? throw new ArgumentNullException(nameof(obj));
        Visible = true;
    }

    /// <summary>
    /// Detach controls from the current object
    /// </summary>
    public void Detach()
    {
        _object = null;
        Visible = false;
    }

    /// <summary>
    /// Update the controls. Must be called every frame in the render loop.
    /// </summary>
    public void Update()
    {
        if (!Enabled || _isDisposed || _object == null)
            return;

        // Update gizmo position to match object
        UpdateGizmoPosition();
        UpdateGizmoRotation();
    }

    #endregion

    #region Event Handlers (called from JavaScript)

    /// <summary>
    /// Called from JavaScript when user presses mouse on gizmo
    /// </summary>
    [JSInvokable]
    public void OnPointerDown(float x, float y, string axis)
    {
        if (!Enabled || _object == null)
            return;

        _dragging = true;
        _axis = axis;
        _pointerStart = new Vector2(x, y);
        _positionStart = _object.Position;
        _quaternionStart = _object.Quaternion;
        _scaleStart = _object.Scale;

        DraggingChanged?.Invoke(this, true);
        MouseDown?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called from JavaScript when user moves mouse during drag
    /// </summary>
    [JSInvokable]
    public void OnPointerMove(float x, float y)
    {
        if (!_dragging || !Enabled || _object == null || _axis == null)
            return;

        var pointer = new Vector2(x, y);
        var delta = pointer - _pointerStart;

        switch (Mode)
        {
            case TransformMode.Translate:
                ApplyTranslation(delta);
                break;
            case TransformMode.Rotate:
                ApplyRotation(delta);
                break;
            case TransformMode.Scale:
                ApplyScale(delta);
                break;
        }

        Change?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called from JavaScript when user releases mouse
    /// </summary>
    [JSInvokable]
    public void OnPointerUp()
    {
        if (_dragging)
        {
            _dragging = false;
            _axis = null;

            DraggingChanged?.Invoke(this, false);
            MouseUp?.Invoke(this, EventArgs.Empty);
            ObjectChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Update gizmo position to match object position
    /// </summary>
    private void UpdateGizmoPosition()
    {
        if (_object == null)
            return;

        _object.UpdateWorldMatrix(true, false);
        Matrix4x4.Decompose(_object.WorldMatrix, out _worldScale, out _worldQuaternion, out _worldPosition);
        Position = _worldPosition;
    }

    /// <summary>
    /// Update gizmo rotation based on space mode
    /// </summary>
    private void UpdateGizmoRotation()
    {
        if (_object == null)
            return;

        if (Space == TransformSpace.Local)
        {
            this.Quaternion = _worldQuaternion;
        }
        else
        {
            this.Quaternion = System.Numerics.Quaternion.Identity;
        }
    }

    /// <summary>
    /// Apply translation based on mouse movement
    /// </summary>
    private void ApplyTranslation(Vector2 delta)
    {
        if (_object == null || _axis == null)
            return;

        var movement = GetAxisMovement(_axis, delta);

        if (TranslationSnap.HasValue)
        {
            movement = SnapToGrid(movement, TranslationSnap.Value);
        }

        _object.Position = _positionStart + movement;
    }

    /// <summary>
    /// Apply rotation based on mouse movement
    /// </summary>
    private void ApplyRotation(Vector2 delta)
    {
        if (_object == null || _axis == null)
            return;

        var angle = delta.Length() * 0.01f;

        if (RotationSnap.HasValue)
        {
            angle = MathF.Round(angle / RotationSnap.Value) * RotationSnap.Value;
        }

        var axis = GetRotationAxis(_axis);
        var rotation = Quaternion.CreateFromAxisAngle(axis, angle);
        _object.Quaternion = _quaternionStart * rotation;
    }

    /// <summary>
    /// Apply scale based on mouse movement
    /// </summary>
    private void ApplyScale(Vector2 delta)
    {
        if (_object == null || _axis == null)
            return;

        var scaleFactor = 1.0f + delta.Y * 0.01f;

        if (ScaleSnap.HasValue)
        {
            scaleFactor = MathF.Round(scaleFactor / ScaleSnap.Value) * ScaleSnap.Value;
        }

        var scale = GetAxisScale(_axis, scaleFactor);
        _object.Scale = _scaleStart * scale;
    }

    /// <summary>
    /// Get movement vector for axis
    /// </summary>
    private Vector3 GetAxisMovement(string axis, Vector2 delta)
    {
        var movement = Vector3.Zero;
        var factor = delta.Y * 0.01f;

        switch (axis.ToUpper())
        {
            case "X":
                movement = new Vector3(factor, 0, 0);
                break;
            case "Y":
                movement = new Vector3(0, factor, 0);
                break;
            case "Z":
                movement = new Vector3(0, 0, factor);
                break;
            case "XY":
                movement = new Vector3(delta.X * 0.01f, delta.Y * 0.01f, 0);
                break;
            case "YZ":
                movement = new Vector3(0, delta.X * 0.01f, delta.Y * 0.01f);
                break;
            case "XZ":
                movement = new Vector3(delta.X * 0.01f, 0, delta.Y * 0.01f);
                break;
        }

        if (Space == TransformSpace.Local && _object != null)
        {
            movement = Vector3.Transform(movement, _object.Quaternion);
        }

        return movement;
    }

    /// <summary>
    /// Get rotation axis vector
    /// </summary>
    private Vector3 GetRotationAxis(string axis)
    {
        return axis.ToUpper() switch
        {
            "X" => Vector3.UnitX,
            "Y" => Vector3.UnitY,
            "Z" => Vector3.UnitZ,
            _ => Vector3.UnitY
        };
    }

    /// <summary>
    /// Get scale vector for axis
    /// </summary>
    private Vector3 GetAxisScale(string axis, float factor)
    {
        return axis.ToUpper() switch
        {
            "X" => new Vector3(factor, 1, 1),
            "Y" => new Vector3(1, factor, 1),
            "Z" => new Vector3(1, 1, factor),
            "XYZ" => new Vector3(factor, factor, factor),
            _ => Vector3.One
        };
    }

    /// <summary>
    /// Snap vector to grid
    /// </summary>
    private Vector3 SnapToGrid(Vector3 vector, float gridSize)
    {
        return new Vector3(
            MathF.Round(vector.X / gridSize) * gridSize,
            MathF.Round(vector.Y / gridSize) * gridSize,
            MathF.Round(vector.Z / gridSize) * gridSize
        );
    }

    #endregion

    #region Disposal

    /// <summary>
    /// Dispose of the controls and clean up JavaScript resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("disposeTransformControls", _domElementId);
                await _jsModule.DisposeAsync();
            }
            catch
            {
                // Ignore errors during disposal
            }
        }

        _dotNetRef?.Dispose();
    }

    #endregion
}
