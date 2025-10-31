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
    /// Render target cache
    /// </summary>
    private HashSet<RenderTarget> _initializedRenderTargets = new();

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
    /// Updates instance buffers for instanced mesh
    /// </summary>
    public void UpdateInstanceBuffers(GeometryBuffers buffers, InstancedMesh instancedMesh)
    {
        _gl.BindVertexArray(buffers.VAO);

        // Update instance matrix buffer
        if (instancedMesh.MatricesNeedUpdate && instancedMesh.InstanceMatrices != null)
        {
            if (buffers.InstanceMatrixBuffer == 0)
            {
                buffers.InstanceMatrixBuffer = _gl.CreateBuffer();
                buffers.IsInstanced = true;
            }

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.InstanceMatrixBuffer);

            // Convert matrices to float array (each matrix is 16 floats)
            var matrixData = new float[instancedMesh.Count * 16];
            for (int i = 0; i < instancedMesh.Count; i++)
            {
                var matrix = instancedMesh.InstanceMatrices[i];
                int offset = i * 16;

                matrixData[offset + 0] = matrix.M11;
                matrixData[offset + 1] = matrix.M12;
                matrixData[offset + 2] = matrix.M13;
                matrixData[offset + 3] = matrix.M14;
                matrixData[offset + 4] = matrix.M21;
                matrixData[offset + 5] = matrix.M22;
                matrixData[offset + 6] = matrix.M23;
                matrixData[offset + 7] = matrix.M24;
                matrixData[offset + 8] = matrix.M31;
                matrixData[offset + 9] = matrix.M32;
                matrixData[offset + 10] = matrix.M33;
                matrixData[offset + 11] = matrix.M34;
                matrixData[offset + 12] = matrix.M41;
                matrixData[offset + 13] = matrix.M42;
                matrixData[offset + 14] = matrix.M43;
                matrixData[offset + 15] = matrix.M44;
            }

            _gl.BufferData(BufferTargetARB.ArrayBuffer, matrixData, BufferUsageARB.DynamicDraw);
            instancedMesh.MatricesNeedUpdate = false;
        }

        // Update instance color buffer (optional)
        if (instancedMesh.ColorsNeedUpdate && instancedMesh.InstanceColors != null)
        {
            if (buffers.InstanceColorBuffer == 0)
            {
                buffers.InstanceColorBuffer = _gl.CreateBuffer();
            }

            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.InstanceColorBuffer);

            // Convert colors to float array
            var colorData = new float[instancedMesh.Count * 3];
            for (int i = 0; i < instancedMesh.Count; i++)
            {
                var color = instancedMesh.InstanceColors[i];
                int offset = i * 3;
                colorData[offset + 0] = color.X;
                colorData[offset + 1] = color.Y;
                colorData[offset + 2] = color.Z;
            }

            _gl.BufferData(BufferTargetARB.ArrayBuffer, colorData, BufferUsageARB.DynamicDraw);
            instancedMesh.ColorsNeedUpdate = false;
        }

        _gl.BindVertexArray(0);
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

        // Skin index buffer (for skeletal animation)
        if (geometry.SkinIndices != null && geometry.SkinIndices.Length > 0)
        {
            buffers.SkinIndexBuffer = _gl.CreateBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.SkinIndexBuffer);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, geometry.SkinIndices, BufferUsageARB.StaticDraw);
            buffers.IsSkinned = true;
        }

        // Skin weight buffer (for skeletal animation)
        if (geometry.SkinWeights != null && geometry.SkinWeights.Length > 0)
        {
            buffers.SkinWeightBuffer = _gl.CreateBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.SkinWeightBuffer);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, geometry.SkinWeights, BufferUsageARB.StaticDraw);
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

    /// <summary>
    /// Initializes a render target's framebuffer
    /// </summary>
    public void InitializeRenderTarget(RenderTarget target)
    {
        if (_initializedRenderTargets.Contains(target))
            return;

        // Create framebuffer
        target.FramebufferId = _gl.CreateFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, target.FramebufferId);

        // Create color texture
        uint colorTexture = _gl.CreateTexture();
        _gl.BindTexture(TextureTarget.Texture2D, colorTexture);

        unsafe
        {
            _gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                (int)InternalFormat.Rgba,
                (uint)target.Width,
                (uint)target.Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null
            );
        }

        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        // Attach color texture to framebuffer
        _gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            colorTexture,
            0
        );

        // Store texture in RenderTarget
        target.Texture.TextureId = colorTexture;
        target.Texture.Width = target.Width;
        target.Texture.Height = target.Height;
        _textures[target.Texture] = colorTexture;

        // Create depth buffer if requested
        if (target.DepthBuffer)
        {
            target.DepthBufferId = _gl.CreateRenderbuffer();
            _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, target.DepthBufferId);
            _gl.RenderbufferStorage(
                RenderbufferTarget.Renderbuffer,
                InternalFormat.DepthComponent16,
                (uint)target.Width,
                (uint)target.Height
            );
            _gl.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer,
                target.DepthBufferId
            );
        }

        // Check framebuffer status
        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
        {
            throw new Exception($"Framebuffer is not complete: {status}");
        }

        // Unbind
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        _initializedRenderTargets.Add(target);
    }

    /// <summary>
    /// Binds a render target for rendering (null = screen)
    /// </summary>
    public void BindRenderTarget(RenderTarget? target)
    {
        if (target != null)
        {
            InitializeRenderTarget(target);
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, target.FramebufferId);
            _gl.Viewport(0, 0, (uint)target.Width, (uint)target.Height);
        }
        else
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)Width, (uint)Height);
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

                // Clean up render targets
                foreach (var renderTarget in _initializedRenderTargets)
                {
                    if (renderTarget.FramebufferId != 0) _gl.DeleteFramebuffer(renderTarget.FramebufferId);
                    if (renderTarget.DepthBufferId != 0) _gl.DeleteRenderbuffer(renderTarget.DepthBufferId);
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

    // Instance buffers for instanced rendering
    public uint InstanceMatrixBuffer { get; set; }
    public uint InstanceColorBuffer { get; set; }
    public bool IsInstanced { get; set; }

    // Skinning buffers
    public uint SkinIndexBuffer { get; set; }
    public uint SkinWeightBuffer { get; set; }
    public bool IsSkinned { get; set; }
}
