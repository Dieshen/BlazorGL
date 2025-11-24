using System.Numerics;
using BlazorGL.Core.Cameras;
using Microsoft.JSInterop;

namespace BlazorGL.Controls;

/// <summary>
/// TrackballControls allows free 360° camera rotation without gimbal lock.
/// Uses quaternion-based rotation for smooth, constraint-free camera movement.
/// Mirrors the API and behavior of Three.js TrackballControls.
/// </summary>
public sealed class TrackballControls : IAsyncDisposable
{
    private readonly Camera _camera;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _domElementId;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<TrackballControls>? _dotNetRef;
    private bool _isDisposed;

    // State
    private Vector3 _target = Vector3.Zero;
    private Vector3 _lastPosition;
    private Quaternion _lastRotation;

    // Movement state
    private Vector3 _eye = Vector3.Zero;
    private Vector3 _rotateStart = Vector3.Zero;
    private Vector3 _rotateEnd = Vector3.Zero;
    private Vector2 _zoomStart = Vector2.Zero;
    private Vector2 _zoomEnd = Vector2.Zero;
    private Vector2 _panStart = Vector2.Zero;
    private Vector2 _panEnd = Vector2.Zero;

    /// <summary>
    /// Creates a new TrackballControls instance
    /// </summary>
    /// <param name="camera">The camera to control</param>
    /// <param name="jsRuntime">JSRuntime for JavaScript interop</param>
    /// <param name="domElementId">ID of the DOM element to attach event listeners to</param>
    public TrackballControls(Camera camera, IJSRuntime jsRuntime, string domElementId)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _domElementId = domElementId ?? throw new ArgumentNullException(nameof(domElementId));

        _lastPosition = _camera.Position;
        _lastRotation = _camera.Quaternion;
    }

    #region Properties

    /// <summary>
    /// Target position to rotate around
    /// </summary>
    public Vector3 Target
    {
        get => _target;
        set => _target = value;
    }

    /// <summary>
    /// Enable or disable controls completely
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Enable or disable camera rotation
    /// </summary>
    public bool EnableRotate { get; set; } = true;

    /// <summary>
    /// Enable or disable camera zoom
    /// </summary>
    public bool EnableZoom { get; set; } = true;

    /// <summary>
    /// Enable or disable camera panning
    /// </summary>
    public bool EnablePan { get; set; } = true;

    /// <summary>
    /// Speed of rotation
    /// </summary>
    public float RotateSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Speed of zoom
    /// </summary>
    public float ZoomSpeed { get; set; } = 1.2f;

    /// <summary>
    /// Speed of panning
    /// </summary>
    public float PanSpeed { get; set; } = 0.3f;

    /// <summary>
    /// Enable static moving (no momentum/damping)
    /// </summary>
    public bool StaticMoving { get; set; } = false;

    /// <summary>
    /// Dynamic damping factor (only used if StaticMoving is false)
    /// </summary>
    public float DynamicDampingFactor { get; set; } = 0.2f;

    /// <summary>
    /// Prevent camera from rolling (maintain up vector)
    /// </summary>
    public bool NoRoll { get; set; } = false;

    /// <summary>
    /// Minimum distance from target (zoom constraint)
    /// </summary>
    public float MinDistance { get; set; } = 0f;

    /// <summary>
    /// Maximum distance from target (zoom constraint)
    /// </summary>
    public float MaxDistance { get; set; } = float.PositiveInfinity;

    /// <summary>
    /// Screen size for calculations (width, height)
    /// </summary>
    public Vector2 Screen { get; set; } = new Vector2(1920, 1080);

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the controls by loading the JavaScript module and attaching event listeners
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(TrackballControls));

        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./wwwroot/blazorgl.controls.extended.js");

        _dotNetRef = DotNetObjectReference.Create(this);

        await _jsModule.InvokeVoidAsync("initTrackballControls", _domElementId, _dotNetRef);
    }

    #endregion

    #region Update

    /// <summary>
    /// Update the controls. Must be called every frame in the render loop.
    /// </summary>
    /// <param name="deltaTime">Time since last frame in seconds</param>
    public void Update(float deltaTime)
    {
        if (!Enabled || _isDisposed)
            return;

        _eye = _camera.Position - _target;

        // Rotation
        if (!StaticMoving)
        {
            _rotateEnd = _rotateEnd + (_rotateStart - _rotateEnd) * DynamicDampingFactor;
        }

        if (_rotateStart != _rotateEnd)
        {
            RotateCamera();
        }

        // Zoom
        if (!StaticMoving)
        {
            _zoomEnd = _zoomEnd + (_zoomStart - _zoomEnd) * DynamicDampingFactor;
        }

        if (_zoomStart.LengthSquared() > 0 || _zoomEnd.LengthSquared() > 0)
        {
            ZoomCamera();
        }

        // Pan
        if (!StaticMoving)
        {
            _panEnd = _panEnd + (_panStart - _panEnd) * DynamicDampingFactor;
        }

        if (_panStart.LengthSquared() > 0)
        {
            PanCamera();
        }

        // Update camera
        _camera.Position = _target + _eye;

        // Check for changes
        if (Vector3.DistanceSquared(_lastPosition, _camera.Position) > 0.0001f ||
            Quaternion.Dot(_lastRotation, _camera.Quaternion) < 0.9999f)
        {
            _camera.LookAt(_target);
            _lastPosition = _camera.Position;
            _lastRotation = _camera.Quaternion;
            _camera.UpdateMatrix();
        }
    }

    #endregion

    #region Event Handlers (called from JavaScript)

    /// <summary>
    /// Called from JavaScript when user starts rotation
    /// </summary>
    [JSInvokable]
    public void OnRotateStart(float x, float y)
    {
        if (!EnableRotate || !Enabled)
            return;

        _rotateStart = _rotateEnd = GetMouseOnCircle(x, y);
    }

    /// <summary>
    /// Called from JavaScript when user rotates
    /// </summary>
    [JSInvokable]
    public void OnRotateMove(float x, float y)
    {
        if (!EnableRotate || !Enabled)
            return;

        _rotateEnd = GetMouseOnCircle(x, y);
    }

    /// <summary>
    /// Called from JavaScript when user starts zoom
    /// </summary>
    [JSInvokable]
    public void OnZoomStart(float x, float y)
    {
        if (!EnableZoom || !Enabled)
            return;

        _zoomStart = _zoomEnd = GetMouseOnScreen(x, y);
    }

    /// <summary>
    /// Called from JavaScript when user zooms
    /// </summary>
    [JSInvokable]
    public void OnZoomMove(float x, float y)
    {
        if (!EnableZoom || !Enabled)
            return;

        _zoomEnd = GetMouseOnScreen(x, y);
    }

    /// <summary>
    /// Called from JavaScript when user starts panning
    /// </summary>
    [JSInvokable]
    public void OnPanStart(float x, float y)
    {
        if (!EnablePan || !Enabled)
            return;

        _panStart = _panEnd = GetMouseOnScreen(x, y);
    }

    /// <summary>
    /// Called from JavaScript when user pans
    /// </summary>
    [JSInvokable]
    public void OnPanMove(float x, float y)
    {
        if (!EnablePan || !Enabled)
            return;

        _panEnd = GetMouseOnScreen(x, y);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Project mouse coordinates onto a virtual trackball sphere
    /// </summary>
    private Vector3 GetMouseOnCircle(float pageX, float pageY)
    {
        var x = (pageX - Screen.X * 0.5f) / (Screen.X * 0.5f);
        var y = (Screen.Y * 0.5f - pageY) / (Screen.Y * 0.5f);

        var length = x * x + y * y;
        var z = 0f;

        if (length <= 1.0f)
        {
            z = MathF.Sqrt(1.0f - length);
        }
        else
        {
            // Normalize to circle edge
            var norm = 1.0f / MathF.Sqrt(length);
            x *= norm;
            y *= norm;
        }

        var result = new Vector3(x, y, z);

        // Transform to camera space
        _eye = _camera.Position - _target;
        var projection = Vector3.Dot(_camera.Up, _eye) / _eye.Length();
        var up = Vector3.Normalize(_camera.Up * projection);
        var right = Vector3.Normalize(Vector3.Cross(_camera.Up, _eye));

        return (up * result.Y) + (right * result.X) + (Vector3.Normalize(_eye) * result.Z);
    }

    /// <summary>
    /// Get normalized screen coordinates
    /// </summary>
    private Vector2 GetMouseOnScreen(float pageX, float pageY)
    {
        return new Vector2(
            (pageX - Screen.X * 0.5f) / (Screen.X * 0.5f),
            (pageY - Screen.Y * 0.5f) / (Screen.Y * 0.5f)
        );
    }

    /// <summary>
    /// Rotate camera using quaternion rotation
    /// </summary>
    private void RotateCamera()
    {
        var angle = Vector3.Distance(_rotateStart, _rotateEnd);

        if (angle > 0)
        {
            _eye = _camera.Position - _target;

            var axis = Vector3.Cross(_rotateEnd, _rotateStart);
            axis = Vector3.Normalize(axis);

            angle *= RotateSpeed;
            var quaternion = Quaternion.CreateFromAxisAngle(axis, angle);

            _eye = Vector3.Transform(_eye, quaternion);
            _camera.Up = Vector3.Transform(_camera.Up, quaternion);

            if (StaticMoving)
            {
                _rotateStart = _rotateEnd;
            }
        }

        // No roll correction
        if (NoRoll)
        {
            var right = Vector3.Cross(_camera.Up, _eye);
            right = Vector3.Normalize(right);
            _camera.Up = Vector3.Cross(_eye, right);
            _camera.Up = Vector3.Normalize(_camera.Up);
        }
    }

    /// <summary>
    /// Zoom camera by moving along eye vector
    /// </summary>
    private void ZoomCamera()
    {
        var factor = 1.0f + (_zoomEnd.Y - _zoomStart.Y) * ZoomSpeed;

        if (factor != 1.0f && factor > 0.0f)
        {
            _eye *= factor;

            // Apply distance constraints
            var distance = _eye.Length();
            if (distance < MinDistance)
            {
                _eye = Vector3.Normalize(_eye) * MinDistance;
            }
            else if (distance > MaxDistance)
            {
                _eye = Vector3.Normalize(_eye) * MaxDistance;
            }

            if (StaticMoving)
            {
                _zoomStart = _zoomEnd;
            }
            else
            {
                _zoomStart.Y += (_zoomEnd.Y - _zoomStart.Y) * DynamicDampingFactor;
            }
        }
    }

    /// <summary>
    /// Pan camera in screen space
    /// </summary>
    private void PanCamera()
    {
        var mouseChange = _panEnd - _panStart;

        if (mouseChange.LengthSquared() > 0)
        {
            mouseChange *= _eye.Length() * PanSpeed;

            var pan = Vector3.Cross(_eye, _camera.Up);
            pan = Vector3.Normalize(pan) * mouseChange.X;
            pan += Vector3.Normalize(_camera.Up) * mouseChange.Y;

            _camera.Position += pan;
            _target += pan;

            if (StaticMoving)
            {
                _panStart = _panEnd;
            }
            else
            {
                _panStart += (_panEnd - _panStart) * DynamicDampingFactor;
            }
        }
    }

    /// <summary>
    /// Reset the controls to the initial state
    /// </summary>
    public void Reset()
    {
        _rotateStart = _rotateEnd = Vector3.Zero;
        _zoomStart = _zoomEnd = Vector2.Zero;
        _panStart = _panEnd = Vector2.Zero;
        _target = Vector3.Zero;
        _lastPosition = _camera.Position;
        _lastRotation = _camera.Quaternion;
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
                await _jsModule.InvokeVoidAsync("disposeTrackballControls", _domElementId);
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
