using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Math;
using BlazorGL.Core.Rendering;
using BlazorGL.Extensions.Raycasting;
using Microsoft.JSInterop;

namespace BlazorGL.Controls;

/// <summary>
/// Event arguments for drag events
/// </summary>
public class DragEventArgs : EventArgs
{
    /// <summary>
    /// The object being dragged
    /// </summary>
    public Object3D Object { get; set; } = null!;

    /// <summary>
    /// The intersection point in world space
    /// </summary>
    public Vector3 Point { get; set; }
}

/// <summary>
/// Event arguments for hover events
/// </summary>
public class HoverEventArgs : EventArgs
{
    /// <summary>
    /// The object being hovered
    /// </summary>
    public Object3D Object { get; set; } = null!;
}

/// <summary>
/// DragControls allows dragging objects in 3D space using raycasting.
/// Provides click-and-drag functionality for interactive object manipulation.
/// Mirrors the API and behavior of Three.js DragControls.
/// </summary>
public sealed class DragControls : IAsyncDisposable
{
    private readonly Camera _camera;
    private readonly Renderer _renderer;
    private readonly List<Object3D> _objects;
    private readonly IJSRuntime _jsRuntime;
    private readonly string _domElementId;
    private readonly Raycaster _raycaster;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<DragControls>? _dotNetRef;
    private bool _isDisposed;

    private Object3D? _selected;
    private Object3D? _hovered;
    private Vector3 _intersection = Vector3.Zero;
    private Vector3 _offset = Vector3.Zero;
    private Vector3 _worldPosition = Vector3.Zero;
    private Plane _plane = new Plane(Vector3.UnitY, 0);

    /// <summary>
    /// Creates a new DragControls instance
    /// </summary>
    /// <param name="camera">The camera for raycasting</param>
    /// <param name="objects">List of objects that can be dragged</param>
    /// <param name="renderer">The renderer for coordinate conversion</param>
    /// <param name="jsRuntime">JSRuntime for JavaScript interop</param>
    /// <param name="domElementId">ID of the DOM element to attach event listeners to</param>
    public DragControls(Camera camera, List<Object3D> objects, Renderer renderer, IJSRuntime jsRuntime, string domElementId)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _domElementId = domElementId ?? throw new ArgumentNullException(nameof(domElementId));
        _raycaster = new Raycaster();
    }

    #region Properties

    /// <summary>
    /// Enable or disable controls completely
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Enable recursive picking (check children)
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Transform mode for dragging
    /// </summary>
    public TransformMode TransformMode { get; set; } = TransformMode.Translate;

    /// <summary>
    /// List of objects that can be dragged
    /// </summary>
    public List<Object3D> Objects => _objects;

    /// <summary>
    /// Currently selected (being dragged) object
    /// </summary>
    public Object3D? Selected => _selected;

    /// <summary>
    /// Currently hovered object
    /// </summary>
    public Object3D? Hovered => _hovered;

    #endregion

    #region Events

    /// <summary>
    /// Fired when drag starts
    /// </summary>
    public event EventHandler<DragEventArgs>? DragStart;

    /// <summary>
    /// Fired during drag
    /// </summary>
    public event EventHandler<DragEventArgs>? Drag;

    /// <summary>
    /// Fired when drag ends
    /// </summary>
    public event EventHandler<DragEventArgs>? DragEnd;

    /// <summary>
    /// Fired when mouse hovers over an object
    /// </summary>
    public event EventHandler<HoverEventArgs>? HoverOn;

    /// <summary>
    /// Fired when mouse leaves an object
    /// </summary>
    public event EventHandler<HoverEventArgs>? HoverOff;

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the controls by loading the JavaScript module and attaching event listeners
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(DragControls));

        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./wwwroot/blazorgl.controls.extended.js");

        _dotNetRef = DotNetObjectReference.Create(this);

        await _jsModule.InvokeVoidAsync("initDragControls", _domElementId, _dotNetRef);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Update the controls. Should be called every frame.
    /// </summary>
    public void Update()
    {
        // Update logic handled by events
    }

    #endregion

    #region Event Handlers (called from JavaScript)

    /// <summary>
    /// Called from JavaScript on pointer down
    /// </summary>
    [JSInvokable]
    public void OnPointerDown(float x, float y)
    {
        if (!Enabled || _isDisposed)
            return;

        var ndc = ScreenToNDC(x, y);
        _raycaster.SetFromCamera(ndc, _camera);

        var intersections = _raycaster.IntersectObjects(_objects, Recursive);

        if (intersections.Count > 0)
        {
            var intersection = intersections[0];
            _selected = intersection.Object;
            _intersection = intersection.Point;

            // Calculate drag plane
            _plane = new Plane(Vector3.Normalize(_camera.Position - _intersection), Vector3.Dot(_camera.Position - _intersection, _intersection));

            if (_selected.Parent != null)
            {
                Matrix4x4.Decompose(_selected.Parent.WorldMatrix, out var parentScale, out var parentRotation, out var parentPosition);
                _offset = _intersection - parentPosition;
            }
            else
            {
                _offset = _intersection - _selected.Position;
            }

            DragStart?.Invoke(this, new DragEventArgs
            {
                Object = _selected,
                Point = _intersection
            });
        }
    }

    /// <summary>
    /// Called from JavaScript on pointer move
    /// </summary>
    [JSInvokable]
    public void OnPointerMove(float x, float y)
    {
        if (!Enabled || _isDisposed)
            return;

        var ndc = ScreenToNDC(x, y);
        _raycaster.SetFromCamera(ndc, _camera);

        if (_selected != null)
        {
            // Dragging
            if (RayIntersectPlane(_raycaster.Ray, _plane, out var point))
            {
                if (_selected.Parent != null)
                {
                    Matrix4x4.Decompose(_selected.Parent.WorldMatrix, out var parentScale, out var parentRotation, out var parentPosition);
                    var localPoint = point - parentPosition;
                    Matrix4x4.Invert(_selected.Parent.WorldMatrix, out var invParent);
                    _selected.Position = Vector3.Transform(localPoint, invParent);
                }
                else
                {
                    _selected.Position = point - _offset;
                }

                Drag?.Invoke(this, new DragEventArgs
                {
                    Object = _selected,
                    Point = point
                });
            }
        }
        else
        {
            // Hovering
            var intersections = _raycaster.IntersectObjects(_objects, Recursive);

            if (intersections.Count > 0)
            {
                var object3D = intersections[0].Object;

                if (_hovered != object3D)
                {
                    if (_hovered != null)
                    {
                        HoverOff?.Invoke(this, new HoverEventArgs { Object = _hovered });
                    }

                    _hovered = object3D;
                    HoverOn?.Invoke(this, new HoverEventArgs { Object = _hovered });
                }
            }
            else
            {
                if (_hovered != null)
                {
                    HoverOff?.Invoke(this, new HoverEventArgs { Object = _hovered });
                    _hovered = null;
                }
            }
        }
    }

    /// <summary>
    /// Called from JavaScript on pointer up
    /// </summary>
    [JSInvokable]
    public void OnPointerUp()
    {
        if (!Enabled || _isDisposed)
            return;

        if (_selected != null)
        {
            DragEnd?.Invoke(this, new DragEventArgs
            {
                Object = _selected,
                Point = _intersection
            });

            _selected = null;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Convert screen coordinates to normalized device coordinates
    /// </summary>
    private Vector2 ScreenToNDC(float x, float y)
    {
        // Assuming screen coordinates are already in normalized form from JS
        return new Vector2(x, y);
    }

    /// <summary>
    /// Test ray intersection with plane
    /// </summary>
    private bool RayIntersectPlane(Ray ray, Plane plane, out Vector3 point)
    {
        var denominator = Vector3.Dot(plane.Normal, ray.Direction);

        if (MathF.Abs(denominator) > 0.0001f)
        {
            var t = (plane.Distance - Vector3.Dot(plane.Normal, ray.Origin)) / denominator;

            if (t >= 0)
            {
                point = ray.Origin + ray.Direction * t;
                return true;
            }
        }

        point = Vector3.Zero;
        return false;
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
                await _jsModule.InvokeVoidAsync("disposeDragControls", _domElementId);
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

/// <summary>
/// Represents a plane in 3D space
/// </summary>
public struct Plane
{
    /// <summary>
    /// Normal vector of the plane
    /// </summary>
    public Vector3 Normal { get; set; }

    /// <summary>
    /// Distance from origin along normal
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// Create a new plane
    /// </summary>
    public Plane(Vector3 normal, float distance)
    {
        Normal = Vector3.Normalize(normal);
        Distance = distance;
    }
}
