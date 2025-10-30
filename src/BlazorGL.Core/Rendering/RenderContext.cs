using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Silk.NET.WebGL;
using System.Numerics;
using BlazorGL.Core.Textures;
using BlazorGL.Core.Geometries;

namespace BlazorGL.Core.Rendering;

/// <summary>
/// Wrapper around WebGL context providing rendering capabilities
/// </summary>
public class RenderContext : IDisposable
{
    private bool _disposed;
    private GL _gl = null!;

    /// <summary>
    /// Silk.NET WebGL instance
    /// </summary>
    public GL GL => _gl;

    /// <summary>
    /// Current canvas width
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Current canvas height
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Pixel ratio for high-DPI displays
    /// </summary>
    public float PixelRatio { get; set; } = 1.0f;

    /// <summary>
    /// Buffer caches for geometries
    /// </summary>
    private Dictionary<Geometry, GeometryBuffers> _geometryBuffers = new();

    /// <summary>
    /// Texture cache
    /// </summary>
    private Dictionary<Texture, uint> _textures = new();

    /// <summary>
    /// Initializes the WebGL context
    /// </summary>
    public async Task InitializeAsync(IJSRuntime jsRuntime, ElementReference canvas)
    {
        _gl = await GL.GetFromJSObjectAsync(jsRuntime, canvas);

        if (_gl == null)
            throw new Exception("Failed to get WebGL 2.0 context");

        // Enable basic features
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(CullFaceMode.Back);
        _gl.FrontFace(FrontFaceDirection.Ccw);

        // Enable blending for transparency
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    /// <summary>
    /// Sets the viewport size
    /// </summary>
    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
        _gl.Viewport(0, 0, (uint)width, (uint)height);
    }

    /// <summary>
    /// Clears the framebuffer
    /// </summary>
    public void Clear(bool color = true, bool depth = true, bool stencil = false)
    {
        ClearBufferMask mask = 0;
        if (color) mask |= ClearBufferMask.ColorBufferBit;
        if (depth) mask |= ClearBufferMask.DepthBufferBit;
        if (stencil) mask |= ClearBufferMask.StencilBufferBit;

        _gl.Clear(mask);
    }

    /// <summary>
    /// Sets clear color
    /// </summary>
    public void SetClearColor(Math.Color color, float alpha = 1.0f)
    {
        _gl.ClearColor(color.R, color.G, color.B, alpha);
    }

    /// <summary>
    /// Gets or creates geometry buffers
    /// </summary>
    public GeometryBuffers GetGeometryBuffers(Geometry geometry)
    {
        if (_geometryBuffers.TryGetValue(geometry, out var buffers))
            return buffers;

        buffers = CreateGeometryBuffers(geometry);
        _geometryBuffers[geometry] = buffers;
        return buffers;
    }

    /// <summary>
    /// Creates WebGL buffers for geometry
    /// </summary>
    private GeometryBuffers CreateGeometryBuffers(Geometry geometry)
    {
        var buffers = new GeometryBuffers();

        // Create VAO
        buffers.VAO = _gl.CreateVertexArray();
        _gl.BindVertexArray(buffers.VAO);

        // Vertex buffer
        if (geometry.Vertices.Length > 0)
        {
            buffers.VertexBuffer = _gl.CreateBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.VertexBuffer);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, geometry.Vertices, BufferUsageARB.StaticDraw);
        }

        // Normal buffer
        if (geometry.Normals.Length > 0)
        {
            buffers.NormalBuffer = _gl.CreateBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.NormalBuffer);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, geometry.Normals, BufferUsageARB.StaticDraw);
        }

        // UV buffer
        if (geometry.UVs.Length > 0)
        {
            buffers.UVBuffer = _gl.CreateBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.UVBuffer);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, geometry.UVs, BufferUsageARB.StaticDraw);
        }

        // Index buffer
        if (geometry.Indices.Length > 0)
        {
            buffers.IndexBuffer = _gl.CreateBuffer();
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, buffers.IndexBuffer);
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, geometry.Indices, BufferUsageARB.StaticDraw);
            buffers.IndexCount = geometry.Indices.Length;
        }

        _gl.BindVertexArray(0);

        return buffers;
    }

    /// <summary>
    /// Gets or creates texture on GPU
    /// </summary>
    public uint GetTexture(Texture texture)
    {
        if (!texture.NeedsUpdate && _textures.TryGetValue(texture, out var textureId))
            return textureId;

        if (texture.NeedsUpdate && texture.ImageData != null)
        {
            if (_textures.TryGetValue(texture, out textureId))
            {
                _gl.DeleteTexture(textureId);
            }

            textureId = CreateTexture(texture);
            _textures[texture] = textureId;
            texture.TextureId = textureId;
            texture.NeedsUpdate = false;
        }

        return textureId;
    }

    /// <summary>
    /// Creates a WebGL texture
    /// </summary>
    private unsafe uint CreateTexture(Texture texture)
    {
        uint textureId = _gl.CreateTexture();
        _gl.BindTexture(TextureTarget.Texture2D, textureId);

        if (texture.ImageData != null)
        {
            fixed (byte* dataPtr = texture.ImageData)
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    (int)texture.Format,
                    (uint)texture.Width,
                    (uint)texture.Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    dataPtr
                );
            }
        }

        // Set texture parameters
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)ConvertWrapMode(texture.WrapS));
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)ConvertWrapMode(texture.WrapT));
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)ConvertMinFilter(texture.MinFilter));
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)ConvertMagFilter(texture.MagFilter));

        if (texture.GenerateMipmaps)
        {
            _gl.GenerateMipmap(TextureTarget.Texture2D);
        }

        _gl.BindTexture(TextureTarget.Texture2D, 0);

        return textureId;
    }

    private GLEnum ConvertWrapMode(TextureWrapMode mode) => mode switch
    {
        TextureWrapMode.Repeat => GLEnum.Repeat,
        TextureWrapMode.ClampToEdge => GLEnum.ClampToEdge,
        TextureWrapMode.MirroredRepeat => GLEnum.MirroredRepeat,
        _ => GLEnum.Repeat
    };

    private GLEnum ConvertMinFilter(TextureMinFilter filter) => filter switch
    {
        TextureMinFilter.Nearest => GLEnum.Nearest,
        TextureMinFilter.Linear => GLEnum.Linear,
        TextureMinFilter.NearestMipmapNearest => GLEnum.NearestMipmapNearest,
        TextureMinFilter.LinearMipmapNearest => GLEnum.LinearMipmapNearest,
        TextureMinFilter.NearestMipmapLinear => GLEnum.NearestMipmapLinear,
        TextureMinFilter.LinearMipmapLinear => GLEnum.LinearMipmapLinear,
        _ => GLEnum.Linear
    };

    private GLEnum ConvertMagFilter(TextureMagFilter filter) => filter switch
    {
        TextureMagFilter.Nearest => GLEnum.Nearest,
        TextureMagFilter.Linear => GLEnum.Linear,
        _ => GLEnum.Linear
    };

    /// <summary>
    /// Sets uniform values
    /// </summary>
    public void SetUniform(int location, object value)
    {
        if (location < 0) return;

        switch (value)
        {
            case int i:
                _gl.Uniform1(location, i);
                break;
            case float f:
                _gl.Uniform1(location, f);
                break;
            case bool b:
                _gl.Uniform1(location, b ? 1 : 0);
                break;
            case Vector2 v2:
                _gl.Uniform2(location, v2.X, v2.Y);
                break;
            case Vector3 v3:
                _gl.Uniform3(location, v3.X, v3.Y, v3.Z);
                break;
            case Vector4 v4:
                _gl.Uniform4(location, v4.X, v4.Y, v4.Z, v4.W);
                break;
            case Matrix4x4 mat:
                unsafe
                {
                    _gl.UniformMatrix4(location, 1, false, (float*)&mat);
                }
                break;
            case Texture texture:
                // Texture binding is handled separately
                break;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Clean up buffers
                foreach (var buffers in _geometryBuffers.Values)
                {
                    if (buffers.VAO != 0) _gl.DeleteVertexArray(buffers.VAO);
                    if (buffers.VertexBuffer != 0) _gl.DeleteBuffer(buffers.VertexBuffer);
                    if (buffers.NormalBuffer != 0) _gl.DeleteBuffer(buffers.NormalBuffer);
                    if (buffers.UVBuffer != 0) _gl.DeleteBuffer(buffers.UVBuffer);
                    if (buffers.IndexBuffer != 0) _gl.DeleteBuffer(buffers.IndexBuffer);
                }

                // Clean up textures
                foreach (var textureId in _textures.Values)
                {
                    _gl.DeleteTexture(textureId);
                }

                _gl?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Cached geometry buffers on GPU
/// </summary>
public class GeometryBuffers
{
    public uint VAO { get; set; }
    public uint VertexBuffer { get; set; }
    public uint NormalBuffer { get; set; }
    public uint UVBuffer { get; set; }
    public uint IndexBuffer { get; set; }
    public int IndexCount { get; set; }
}
