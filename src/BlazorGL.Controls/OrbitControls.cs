using System.Numerics;
using BlazorGL.Core.Cameras;
using Microsoft.JSInterop;

namespace BlazorGL.Controls;

/// <summary>
/// OrbitControls allows the camera to orbit around a target point.
/// Handles mouse and touch input for rotating, panning, and zooming.
/// Mirrors the API and behavior of Three.js OrbitControls.
/// </summary>
public sealed class OrbitControls : IAsyncDisposable
{
    private readonly Camera _camera;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _domElementId;
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<OrbitControls>? _dotNetRef;
    private bool _isDisposed;

    // State
    private Vector3 _target = Vector3.Zero;
    private float _sphericalRadius = 10f;
    private float _sphericalTheta = 0f;  // Azimuthal angle (around Y axis)
    private float _sphericalPhi = MathF.PI / 2f;  // Polar angle (from Y axis)

    // Damping
    private Vector2 _rotateDelta = Vector2.Zero;
    private Vector2 _panDelta = Vector2.Zero;
    private float _zoomDelta = 0f;

    // Auto-rotate
    private float _autoRotateAngle = 0f;

    /// <summary>
    /// Creates a new OrbitControls instance
    /// </summary>
    /// <param name="camera">The camera to control</param>
    /// <param name="jsRuntime">JSRuntime for JavaScript interop</param>
    /// <param name="domElementId">ID of the DOM element to attach event listeners to (typically the canvas)</param>
    public OrbitControls(Camera camera, IJSRuntime jsRuntime, string domElementId)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _domElementId = domElementId ?? throw new ArgumentNullException(nameof(domElementId));

        // Initialize spherical coordinates from current camera position
        UpdateSphericalFromCamera();
    }

    #region Properties

    /// <summary>
    /// Target position to orbit around
    /// </summary>
    public Vector3 Target
    {
        get => _target;
        set
        {
            _target = value;
            UpdateSphericalFromCamera();
        }
    }

    /// <summary>
    /// Enable damping (inertia), which can be used to give a sense of weight to the controls
    /// </summary>
    public bool EnableDamping { get; set; } = false;

    /// <summary>
    /// Damping factor (only used if EnableDamping is true)
    /// </summary>
    public float DampingFactor { get; set; } = 0.05f;

    /// <summary>
    /// Minimum distance from target (zoom constraint)
    /// </summary>
    public float MinDistance { get; set; } = 0f;

    /// <summary>
    /// Maximum distance from target (zoom constraint)
    /// </summary>
    public float MaxDistance { get; set; } = float.PositiveInfinity;

    /// <summary>
    /// Minimum polar angle in radians (0 to PI). Restricts vertical rotation.
    /// </summary>
    public float MinPolarAngle { get; set; } = 0f;

    /// <summary>
    /// Maximum polar angle in radians (0 to PI). Restricts vertical rotation.
    /// </summary>
    public float MaxPolarAngle { get; set; } = MathF.PI;

    /// <summary>
    /// Minimum azimuthal angle in radians. Restricts horizontal rotation.
    /// </summary>
    public float MinAzimuthAngle { get; set; } = float.NegativeInfinity;

    /// <summary>
    /// Maximum azimuthal angle in radians. Restricts horizontal rotation.
    /// </summary>
    public float MaxAzimuthAngle { get; set; } = float.PositiveInfinity;

    /// <summary>
    /// Enable or disable camera rotation
    /// </summary>
    public bool EnableRotate { get; set; } = true;

    /// <summary>
    /// Speed of rotation
    /// </summary>
    public float RotateSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Enable or disable camera zoom
    /// </summary>
    public bool EnableZoom { get; set; } = true;

    /// <summary>
    /// Speed of zoom
    /// </summary>
    public float ZoomSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Enable or disable camera panning
    /// </summary>
    public bool EnablePan { get; set; } = true;

    /// <summary>
    /// Speed of panning
    /// </summary>
    public float PanSpeed { get; set; } = 1.0f;

    /// <summary>
    /// Auto-rotate the camera around the target
    /// </summary>
    public bool AutoRotate { get; set; } = false;

    /// <summary>
    /// Speed of auto-rotation (30 seconds per orbit at 60fps if set to 2.0)
    /// </summary>
    public float AutoRotateSpeed { get; set; } = 2.0f;

    /// <summary>
    /// Enable or disable controls completely
    /// </summary>
    public bool Enabled { get; set; } = true;

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the controls by loading the JavaScript module and attaching event listeners
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(OrbitControls));

        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./wwwroot/blazorgl.controls.js");

        _dotNetRef = DotNetObjectReference.Create(this);

        await _jsModule.InvokeVoidAsync("initOrbitControls", _domElementId, _dotNetRef);
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

        // Auto-rotate
        if (AutoRotate)
        {
            _autoRotateAngle = AutoRotateSpeed * deltaTime * (MathF.PI / 180f);
            _rotateDelta.X += _autoRotateAngle;
        }

        // Apply rotation
        if (_rotateDelta != Vector2.Zero)
        {
            _sphericalTheta -= _rotateDelta.X * RotateSpeed;
            _sphericalPhi -= _rotateDelta.Y * RotateSpeed;

            // Constrain angles
            _sphericalPhi = Math.Clamp(_sphericalPhi, MinPolarAngle, MaxPolarAngle);

            if (!float.IsInfinity(MinAzimuthAngle) || !float.IsInfinity(MaxAzimuthAngle))
            {
                _sphericalTheta = Math.Clamp(_sphericalTheta, MinAzimuthAngle, MaxAzimuthAngle);
            }

            // Apply damping or reset
            if (EnableDamping)
            {
                _rotateDelta *= (1f - DampingFactor);
            }
            else
            {
                _rotateDelta = Vector2.Zero;
            }
        }

        // Apply zoom
        if (_zoomDelta != 0f)
        {
            _sphericalRadius *= MathF.Pow(0.95f, _zoomDelta * ZoomSpeed);
            _sphericalRadius = Math.Clamp(_sphericalRadius, MinDistance, MaxDistance);

            // Apply damping or reset
            if (EnableDamping)
            {
                _zoomDelta *= (1f - DampingFactor);
            }
            else
            {
                _zoomDelta = 0f;
            }
        }

        // Apply panning
        if (_panDelta != Vector2.Zero)
        {
            Pan(_panDelta.X, _panDelta.Y);

            // Apply damping or reset
            if (EnableDamping)
            {
                _panDelta *= (1f - DampingFactor);
            }
            else
            {
                _panDelta = Vector2.Zero;
            }
        }

        // Update camera position from spherical coordinates
        UpdateCameraPosition();
    }

    #endregion

    #region Event Handlers (called from JavaScript)

    /// <summary>
    /// Called from JavaScript when user rotates (mouse drag or touch)
    /// </summary>
    [JSInvokable]
    public void OnRotate(float deltaX, float deltaY)
    {
        if (!EnableRotate || !Enabled)
            return;

        _rotateDelta.X += deltaX;
        _rotateDelta.Y += deltaY;
    }

    /// <summary>
    /// Called from JavaScript when user zooms (mouse wheel or pinch)
    /// </summary>
    [JSInvokable]
    public void OnZoom(float delta)
    {
        if (!EnableZoom || !Enabled)
            return;

        _zoomDelta += delta;
    }

    /// <summary>
    /// Called from JavaScript when user pans (right mouse button or two-finger drag)
    /// </summary>
    [JSInvokable]
    public void OnPan(float deltaX, float deltaY)
    {
        if (!EnablePan || !Enabled)
            return;

        _panDelta.X += deltaX;
        _panDelta.Y += deltaY;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate spherical coordinates from current camera position
    /// </summary>
    private void UpdateSphericalFromCamera()
    {
        var offset = _camera.Position - _target;
        _sphericalRadius = offset.Length();

        if (_sphericalRadius > 0)
        {
            _sphericalTheta = MathF.Atan2(offset.X, offset.Z);
            _sphericalPhi = MathF.Acos(Math.Clamp(offset.Y / _sphericalRadius, -1f, 1f));
        }
    }

    /// <summary>
    /// Update camera position from spherical coordinates
    /// </summary>
    private void UpdateCameraPosition()
    {
        // Convert spherical to Cartesian
        var sinPhiRadius = MathF.Sin(_sphericalPhi) * _sphericalRadius;

        var position = new Vector3(
            sinPhiRadius * MathF.Sin(_sphericalTheta),
            MathF.Cos(_sphericalPhi) * _sphericalRadius,
            sinPhiRadius * MathF.Cos(_sphericalTheta)
        );

        _camera.Position = _target + position;
        _camera.LookAt(_target);
        _camera.UpdateMatrix();
    }

    /// <summary>
    /// Pan the camera and target
    /// </summary>
    private void Pan(float deltaX, float deltaY)
    {
        // Get camera right and up vectors
        var offset = _camera.Position - _target;
        var targetDistance = offset.Length();

        // Calculate pan vectors in screen space
        var right = Vector3.Normalize(Vector3.Cross(offset, _camera.Up));
        var up = Vector3.Normalize(Vector3.Cross(right, offset));

        // Scale pan by distance (closer = slower pan)
        var panX = right * (deltaX * targetDistance * PanSpeed * 0.001f);
        var panY = up * (deltaY * targetDistance * PanSpeed * 0.001f);

        var panOffset = panX + panY;
        _target += panOffset;
    }

    /// <summary>
    /// Reset the controls to the initial state
    /// </summary>
    public void Reset()
    {
        _target = Vector3.Zero;
        _rotateDelta = Vector2.Zero;
        _panDelta = Vector2.Zero;
        _zoomDelta = 0f;
        _autoRotateAngle = 0f;
        UpdateSphericalFromCamera();
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
                await _jsModule.InvokeVoidAsync("disposeOrbitControls", _domElementId);
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
