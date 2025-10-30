using System.Numerics;

namespace BlazorGL.Core;

/// <summary>
/// Base class for all objects in the 3D scene graph
/// </summary>
public abstract class Object3D
{
    private Vector3 _position = Vector3.Zero;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;
    private Matrix4x4 _localMatrix = Matrix4x4.Identity;
    private Matrix4x4 _worldMatrix = Matrix4x4.Identity;
    private bool _matrixNeedsUpdate = true;
    private Object3D? _parent;

    /// <summary>
    /// Unique identifier for this object
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Name of the object
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the object is visible in the scene
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// User-defined data attached to this object
    /// </summary>
    public object? UserData { get; set; }

    /// <summary>
    /// Local position relative to parent
    /// </summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _matrixNeedsUpdate = true;
        }
    }

    /// <summary>
    /// Local rotation as a quaternion
    /// </summary>
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _matrixNeedsUpdate = true;
        }
    }

    /// <summary>
    /// Local scale
    /// </summary>
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _matrixNeedsUpdate = true;
        }
    }

    /// <summary>
    /// Local transformation matrix
    /// </summary>
    public Matrix4x4 LocalMatrix
    {
        get
        {
            if (_matrixNeedsUpdate)
            {
                UpdateLocalMatrix();
            }
            return _localMatrix;
        }
    }

    /// <summary>
    /// World transformation matrix (includes parent transforms)
    /// </summary>
    public Matrix4x4 WorldMatrix
    {
        get
        {
            UpdateWorldMatrix();
            return _worldMatrix;
        }
    }

    /// <summary>
    /// Parent object in the scene graph
    /// </summary>
    public Object3D? Parent
    {
        get => _parent;
        set
        {
            if (_parent == value) return;

            _parent?.Children.Remove(this);
            _parent = value;
            _parent?.Children.Add(this);
        }
    }

    /// <summary>
    /// Child objects
    /// </summary>
    public List<Object3D> Children { get; } = new();

    /// <summary>
    /// Adds a child object
    /// </summary>
    public void Add(Object3D child)
    {
        if (child.Parent != null)
        {
            child.Parent.Remove(child);
        }
        child._parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Removes a child object
    /// </summary>
    public void Remove(Object3D child)
    {
        if (Children.Remove(child))
        {
            child._parent = null;
        }
    }

    /// <summary>
    /// Removes all children
    /// </summary>
    public void Clear()
    {
        foreach (var child in Children)
        {
            child._parent = null;
        }
        Children.Clear();
    }

    /// <summary>
    /// Finds a child by name (recursive)
    /// </summary>
    public T? GetObjectByName<T>(string name) where T : Object3D
    {
        if (this is T obj && Name == name)
            return obj;

        foreach (var child in Children)
        {
            var result = child.GetObjectByName<T>(name);
            if (result != null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Gets all objects of a specific type (recursive)
    /// </summary>
    public List<T> GetObjectsOfType<T>() where T : Object3D
    {
        var results = new List<T>();

        if (this is T obj)
            results.Add(obj);

        foreach (var child in Children)
        {
            results.AddRange(child.GetObjectsOfType<T>());
        }

        return results;
    }

    /// <summary>
    /// Makes the object look at a target position
    /// </summary>
    public void LookAt(Vector3 target)
    {
        var direction = Vector3.Normalize(target - Position);

        // Calculate rotation to look at target
        // Assuming default forward is -Z and up is +Y
        var forward = -Vector3.UnitZ;
        var up = Vector3.UnitY;

        // Handle edge case when looking straight up or down
        if (MathF.Abs(Vector3.Dot(direction, up)) > 0.999f)
        {
            up = Vector3.UnitX;
        }

        // Create rotation from current forward to target direction
        var rotationAxis = Vector3.Cross(forward, direction);
        if (rotationAxis.LengthSquared() < 1e-6f)
        {
            // Already looking at target or opposite direction
            _rotation = Vector3.Dot(forward, direction) > 0
                ? Quaternion.Identity
                : Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI);
        }
        else
        {
            var angle = MathF.Acos(Vector3.Dot(forward, direction));
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(rotationAxis), angle);
        }

        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Rotates around the X axis
    /// </summary>
    public void RotateX(float angle)
    {
        _rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, angle);
        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Rotates around the Y axis
    /// </summary>
    public void RotateY(float angle)
    {
        _rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Rotates around the Z axis
    /// </summary>
    public void RotateZ(float angle)
    {
        _rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Translates along the X axis
    /// </summary>
    public void TranslateX(float distance)
    {
        _position += new Vector3(distance, 0, 0);
        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Translates along the Y axis
    /// </summary>
    public void TranslateY(float distance)
    {
        _position += new Vector3(0, distance, 0);
        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Translates along the Z axis
    /// </summary>
    public void TranslateZ(float distance)
    {
        _position += new Vector3(0, 0, distance);
        _matrixNeedsUpdate = true;
    }

    /// <summary>
    /// Called every frame before rendering (override for custom behavior)
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        // Update all children
        foreach (var child in Children)
        {
            child.Update(deltaTime);
        }
    }

    /// <summary>
    /// Called just before rendering this object (override for custom behavior)
    /// </summary>
    public virtual void OnBeforeRender(Rendering.Renderer renderer, Scene scene, Cameras.Camera camera)
    {
    }

    /// <summary>
    /// Traverses the scene graph and calls action on this and all descendants
    /// </summary>
    public void Traverse(Action<Object3D> action)
    {
        action(this);
        foreach (var child in Children)
        {
            child.Traverse(action);
        }
    }

    /// <summary>
    /// Updates the local transformation matrix from position, rotation, and scale
    /// </summary>
    private void UpdateLocalMatrix()
    {
        _localMatrix = Matrix4x4.CreateScale(_scale) *
                      Matrix4x4.CreateFromQuaternion(_rotation) *
                      Matrix4x4.CreateTranslation(_position);
        _matrixNeedsUpdate = false;
    }

    /// <summary>
    /// Updates the world transformation matrix
    /// </summary>
    private void UpdateWorldMatrix()
    {
        if (_matrixNeedsUpdate)
        {
            UpdateLocalMatrix();
        }

        if (_parent != null)
        {
            _worldMatrix = LocalMatrix * _parent.WorldMatrix;
        }
        else
        {
            _worldMatrix = LocalMatrix;
        }
    }

    public override string ToString() => $"{GetType().Name}(Name: {Name}, Position: {Position})";
}
