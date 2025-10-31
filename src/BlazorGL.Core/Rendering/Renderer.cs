using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Numerics;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Textures;
using Silk.NET.WebGL;

namespace BlazorGL.Core.Rendering;

/// <summary>
/// Main rendering class that manages the WebGL context and render loop
/// </summary>
public class Renderer : IDisposable
{
    private bool _disposed;
    private RenderContext _context = null!;
    private RenderState _state = new();
    private IJSRuntime _jsRuntime = null!;

    /// <summary>
    /// WebGL rendering context
    /// </summary>
    public RenderContext Context => _context;

    /// <summary>
    /// Current canvas width
    /// </summary>
    public int Width => _context.Width;

    /// <summary>
    /// Current canvas height
    /// </summary>
    public int Height => _context.Height;

    /// <summary>
    /// Whether to automatically clear the framebuffer before rendering
    /// </summary>
    public bool AutoClear { get; set; } = true;

    /// <summary>
    /// Whether to sort objects before rendering
    /// </summary>
    public bool SortObjects { get; set; } = true;

    /// <summary>
    /// Performance statistics
    /// </summary>
    public PerformanceStats Stats { get; } = new();

    private Math.Color _clearColor = new(0, 0, 0);
    private float _clearAlpha = 1.0f;

    /// <summary>
    /// Initializes the renderer with a canvas element
    /// </summary>
    public async Task InitializeAsync(ElementReference canvas, IJSRuntime? jsRuntime = null)
    {
        if (jsRuntime == null)
        {
            // Try to get JSRuntime from the canvas element's service provider
            // In a real scenario, JSRuntime should be injected
            throw new ArgumentNullException(nameof(jsRuntime), "JSRuntime must be provided");
        }

        _jsRuntime = jsRuntime;
        _context = new RenderContext();
        await _context.InitializeAsync(_jsRuntime, canvas);
    }

    /// <summary>
    /// Sets the viewport size
    /// </summary>
    public void SetSize(int width, int height)
    {
        _context.SetSize(width, height);
    }

    /// <summary>
    /// Sets the clear color
    /// </summary>
    public void SetClearColor(Math.Color color)
    {
        _clearColor = color;
        _context.SetClearColor(color, _clearAlpha);
    }

    /// <summary>
    /// Sets the clear alpha
    /// </summary>
    public void SetClearAlpha(float alpha)
    {
        _clearAlpha = alpha;
        _context.SetClearColor(_clearColor, alpha);
    }

    /// <summary>
    /// Sets the pixel ratio for high-DPI displays
    /// </summary>
    public void SetPixelRatio(float ratio)
    {
        _context.PixelRatio = ratio;
    }

    /// <summary>
    /// Sets the render target for off-screen rendering (null = render to screen)
    /// </summary>
    public void SetRenderTarget(RenderTarget? target)
    {
        _context.BindRenderTarget(target);
    }

    /// <summary>
    /// Renders a scene with a camera
    /// </summary>
    public void Render(Scene scene, Camera camera)
    {
        Stats.BeginFrame();

        // Update scene
        scene.Update(0.016f); // Approximate 60fps deltaTime

        // Clear
        if (AutoClear)
        {
            _context.Clear(true, true, false);
        }

        // Collect render items
        var renderList = new List<RenderItem>();
        CollectRenderItems(scene, renderList);

        // Sort if needed
        if (SortObjects)
        {
            renderList.Sort((a, b) =>
            {
                // Opaque objects first (front to back)
                // Transparent objects last (back to front)
                if (!a.Material.Transparent && b.Material.Transparent)
                    return -1;
                if (a.Material.Transparent && !b.Material.Transparent)
                    return 1;

                if (a.Material.Transparent && b.Material.Transparent)
                {
                    // Back to front for transparent
                    return b.Z.CompareTo(a.Z);
                }
                else
                {
                    // Front to back for opaque
                    return a.Z.CompareTo(b.Z);
                }
            });
        }

        // Collect lights
        var lights = scene.Lights;

        // Render all items
        foreach (var item in renderList)
        {
            if (item.Object is Mesh mesh)
            {
                RenderMesh(mesh, scene, camera, lights);
            }
            else if (item.Object is Line line)
            {
                RenderLine(line, camera, lights);
            }
            else if (item.Object is LineSegments lineSegments)
            {
                RenderLineSegments(lineSegments, camera, lights);
            }
            else if (item.Object is LineLoop lineLoop)
            {
                RenderLineLoop(lineLoop, camera, lights);
            }
            else if (item.Object is Points points)
            {
                RenderPoints(points, camera, lights);
            }
        }

        Stats.EndFrame();
    }

    /// <summary>
    /// Collects renderable objects (meshes, lines, points, etc.)
    /// </summary>
    private void CollectRenderItems(Object3D obj, List<RenderItem> renderList)
    {
        if (!obj.Visible)
            return;

        bool isRenderable = false;

        // Check if object is renderable
        if (obj is Mesh mesh && mesh.Geometry != null && mesh.Material != null)
        {
            isRenderable = true;
        }
        else if (obj is Line line && line.Geometry != null && line.Material != null)
        {
            isRenderable = true;
        }
        else if (obj is LineSegments lineSegments && lineSegments.Geometry != null && lineSegments.Material != null)
        {
            isRenderable = true;
        }
        else if (obj is LineLoop lineLoop && lineLoop.Geometry != null && lineLoop.Material != null)
        {
            isRenderable = true;
        }
        else if (obj is Points points && points.Geometry != null && points.Material != null)
        {
            isRenderable = true;
        }

        if (isRenderable)
        {
            // Calculate distance from camera for sorting
            var worldPos = Vector3.Transform(Vector3.Zero, obj.WorldMatrix);
            float z = worldPos.Z;

            renderList.Add(new RenderItem
            {
                Object = obj,
                Z = z
            });
        }

        foreach (var child in obj.Children)
        {
            CollectRenderItems(child, renderList);
        }
    }

    /// <summary>
    /// Renders a single mesh
    /// </summary>
    private void RenderMesh(Mesh mesh, Scene scene, Camera camera, List<Light> lights)
    {
        var material = mesh.Material;
        var geometry = mesh.Geometry;

        // Compile shader if needed
        if (material.NeedsCompile || !material.Shader.IsCompiled)
        {
            material.OnBeforeCompile(material.Shader);
            material.Shader.Compile(_context.GL);
            material.NeedsCompile = false;
        }

        // Use shader
        if (_state.CurrentShader != material.Shader)
        {
            material.Shader.Use(_context.GL);
            _state.CurrentShader = material.Shader;
        }

        // Update material state
        ApplyMaterialState(material);

        // Get geometry buffers
        var buffers = _context.GetGeometryBuffers(geometry);

        // Bind VAO
        if (_state.CurrentVAO != buffers.VAO)
        {
            _context.GL.BindVertexArray(buffers.VAO);
            _state.CurrentVAO = buffers.VAO;

            // Set up attributes
            SetupAttributes(material.Shader, buffers);
        }

        // Set standard uniforms
        SetStandardUniforms(material.Shader, mesh, camera, lights, scene);

        // Set material uniforms
        material.UpdateUniforms();
        SetMaterialUniforms(material);

        // Draw
        if (buffers.IndexCount > 0)
        {
            _context.GL.DrawElements(PrimitiveType.Triangles, (uint)buffers.IndexCount, DrawElementsType.UnsignedInt, null);
            Stats.DrawCalls++;
            Stats.Triangles += buffers.IndexCount / 3;
        }
    }

    /// <summary>
    /// Renders a continuous line
    /// </summary>
    private void RenderLine(Line line, Camera camera, List<Light> lights)
    {
        var material = line.Material;
        var geometry = line.Geometry;

        // Compile shader if needed
        if (material.NeedsCompile || !material.Shader.IsCompiled)
        {
            material.OnBeforeCompile(material.Shader);
            material.Shader.Compile(_context.GL);
            material.NeedsCompile = false;
        }

        // Use shader
        if (_state.CurrentShader != material.Shader)
        {
            material.Shader.Use(_context.GL);
            _state.CurrentShader = material.Shader;
        }

        // Update material state
        ApplyMaterialState(material);

        // Get geometry buffers
        var buffers = _context.GetGeometryBuffers(geometry);

        // Bind VAO
        if (_state.CurrentVAO != buffers.VAO)
        {
            _context.GL.BindVertexArray(buffers.VAO);
            _state.CurrentVAO = buffers.VAO;

            // Set up attributes
            SetupAttributes(material.Shader, buffers);
        }

        // Set line uniforms
        SetLineUniforms(material.Shader, line, camera);

        // Set material uniforms
        material.UpdateUniforms();
        SetMaterialUniforms(material);

        // Draw as line strip
        int vertexCount = geometry.Vertices.Length / 3;
        if (vertexCount > 0)
        {
            _context.GL.DrawArrays(PrimitiveType.LineStrip, 0, (uint)vertexCount);
            Stats.DrawCalls++;
        }
    }

    /// <summary>
    /// Renders disconnected line segments
    /// </summary>
    private void RenderLineSegments(LineSegments lineSegments, Camera camera, List<Light> lights)
    {
        var material = lineSegments.Material;
        var geometry = lineSegments.Geometry;

        // Compile shader if needed
        if (material.NeedsCompile || !material.Shader.IsCompiled)
        {
            material.OnBeforeCompile(material.Shader);
            material.Shader.Compile(_context.GL);
            material.NeedsCompile = false;
        }

        // Use shader
        if (_state.CurrentShader != material.Shader)
        {
            material.Shader.Use(_context.GL);
            _state.CurrentShader = material.Shader;
        }

        // Update material state
        ApplyMaterialState(material);

        // Get geometry buffers
        var buffers = _context.GetGeometryBuffers(geometry);

        // Bind VAO
        if (_state.CurrentVAO != buffers.VAO)
        {
            _context.GL.BindVertexArray(buffers.VAO);
            _state.CurrentVAO = buffers.VAO;

            // Set up attributes
            SetupAttributes(material.Shader, buffers);
        }

        // Set line uniforms
        SetLineUniforms(material.Shader, lineSegments, camera);

        // Set material uniforms
        material.UpdateUniforms();
        SetMaterialUniforms(material);

        // Draw as separate line segments
        int vertexCount = geometry.Vertices.Length / 3;
        if (vertexCount > 0)
        {
            _context.GL.DrawArrays(PrimitiveType.Lines, 0, (uint)vertexCount);
            Stats.DrawCalls++;
        }
    }

    /// <summary>
    /// Renders a closed line loop
    /// </summary>
    private void RenderLineLoop(LineLoop lineLoop, Camera camera, List<Light> lights)
    {
        var material = lineLoop.Material;
        var geometry = lineLoop.Geometry;

        // Compile shader if needed
        if (material.NeedsCompile || !material.Shader.IsCompiled)
        {
            material.OnBeforeCompile(material.Shader);
            material.Shader.Compile(_context.GL);
            material.NeedsCompile = false;
        }

        // Use shader
        if (_state.CurrentShader != material.Shader)
        {
            material.Shader.Use(_context.GL);
            _state.CurrentShader = material.Shader;
        }

        // Update material state
        ApplyMaterialState(material);

        // Get geometry buffers
        var buffers = _context.GetGeometryBuffers(geometry);

        // Bind VAO
        if (_state.CurrentVAO != buffers.VAO)
        {
            _context.GL.BindVertexArray(buffers.VAO);
            _state.CurrentVAO = buffers.VAO;

            // Set up attributes
            SetupAttributes(material.Shader, buffers);
        }

        // Set line uniforms
        SetLineUniforms(material.Shader, lineLoop, camera);

        // Set material uniforms
        material.UpdateUniforms();
        SetMaterialUniforms(material);

        // Draw as line loop
        int vertexCount = geometry.Vertices.Length / 3;
        if (vertexCount > 0)
        {
            _context.GL.DrawArrays(PrimitiveType.LineLoop, 0, (uint)vertexCount);
            Stats.DrawCalls++;
        }
    }

    /// <summary>
    /// Sets line-specific uniforms
    /// </summary>
    private void SetLineUniforms(Shaders.Shader shader, Object3D lineObject, Camera camera)
    {
        var gl = _context.GL;

        // Matrices
        var modelMatrix = lineObject.WorldMatrix;
        var viewMatrix = camera.ViewMatrix;
        var projectionMatrix = camera.ProjectionMatrix;
        var modelViewMatrix = modelMatrix * viewMatrix;

        _context.SetUniform(shader.GetUniformLocation(gl, "modelMatrix"), modelMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "viewMatrix"), viewMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "projectionMatrix"), projectionMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "modelViewMatrix"), modelViewMatrix);
    }

    /// <summary>
    /// Renders a point cloud
    /// </summary>
    private void RenderPoints(Points points, Camera camera, List<Light> lights)
    {
        var material = points.Material;
        var geometry = points.Geometry;

        // Compile shader if needed
        if (material.NeedsCompile || !material.Shader.IsCompiled)
        {
            material.OnBeforeCompile(material.Shader);
            material.Shader.Compile(_context.GL);
            material.NeedsCompile = false;
        }

        // Use shader
        if (_state.CurrentShader != material.Shader)
        {
            material.Shader.Use(_context.GL);
            _state.CurrentShader = material.Shader;
        }

        // Update material state
        ApplyMaterialState(material);

        // Get geometry buffers
        var buffers = _context.GetGeometryBuffers(geometry);

        // Bind VAO
        if (_state.CurrentVAO != buffers.VAO)
        {
            _context.GL.BindVertexArray(buffers.VAO);
            _state.CurrentVAO = buffers.VAO;

            // Set up attributes
            SetupAttributes(material.Shader, buffers);
        }

        // Set uniforms
        SetLineUniforms(material.Shader, points, camera);

        // Set material uniforms
        material.UpdateUniforms();
        SetMaterialUniforms(material);

        // Draw as points
        int vertexCount = geometry.Vertices.Length / 3;
        if (vertexCount > 0)
        {
            _context.GL.DrawArrays(PrimitiveType.Points, 0, (uint)vertexCount);
            Stats.DrawCalls++;
        }
    }

    /// <summary>
    /// Sets up vertex attributes
    /// </summary>
    private void SetupAttributes(Shaders.Shader shader, GeometryBuffers buffers)
    {
        var gl = _context.GL;

        // Position
        if (shader.Attributes.TryGetValue("position", out uint posLoc) && buffers.VertexBuffer != 0)
        {
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.VertexBuffer);
            gl.EnableVertexAttribArray(posLoc);
            gl.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, null);
        }

        // Normal
        if (shader.Attributes.TryGetValue("normal", out uint normLoc) && buffers.NormalBuffer != 0)
        {
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.NormalBuffer);
            gl.EnableVertexAttribArray(normLoc);
            gl.VertexAttribPointer(normLoc, 3, VertexAttribPointerType.Float, false, 0, null);
        }

        // UV
        if (shader.Attributes.TryGetValue("uv", out uint uvLoc) && buffers.UVBuffer != 0)
        {
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffers.UVBuffer);
            gl.EnableVertexAttribArray(uvLoc);
            gl.VertexAttribPointer(uvLoc, 2, VertexAttribPointerType.Float, false, 0, null);
        }

        // Bind index buffer
        if (buffers.IndexBuffer != 0)
        {
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, buffers.IndexBuffer);
        }
    }

    /// <summary>
    /// Sets standard uniforms (matrices, lights, etc.)
    /// </summary>
    private void SetStandardUniforms(Shaders.Shader shader, Mesh mesh, Camera camera, List<Light> lights, Scene scene)
    {
        var gl = _context.GL;

        // Matrices
        var modelMatrix = mesh.WorldMatrix;
        var viewMatrix = camera.ViewMatrix;
        var projectionMatrix = camera.ProjectionMatrix;
        var modelViewMatrix = modelMatrix * viewMatrix;
        var normalMatrix = Matrix4x4.Transpose(Matrix4x4.Invert(modelMatrix, out var _) ? Matrix4x4.Identity : modelMatrix);

        _context.SetUniform(shader.GetUniformLocation(gl, "modelMatrix"), modelMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "viewMatrix"), viewMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "projectionMatrix"), projectionMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "modelViewMatrix"), modelViewMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "normalMatrix"), normalMatrix);

        // Camera position
        var cameraPos = Vector3.Transform(Vector3.Zero, camera.WorldMatrix);
        _context.SetUniform(shader.GetUniformLocation(gl, "cameraPosition"), cameraPos);

        // Lights
        SetLightUniforms(shader, lights, scene);
    }

    /// <summary>
    /// Sets light uniforms
    /// </summary>
    private void SetLightUniforms(Shaders.Shader shader, List<Light> lights, Scene scene)
    {
        var gl = _context.GL;

        // Ambient light
        var ambientColor = Vector3.Zero;
        var ambientLights = lights.OfType<AmbientLight>();
        foreach (var light in ambientLights)
        {
            ambientColor += light.Color.ToVector3() * light.Intensity;
        }
        _context.SetUniform(shader.GetUniformLocation(gl, "ambientLightColor"), ambientColor);

        // Directional lights
        var dirLights = lights.OfType<DirectionalLight>().Take(4).ToArray();
        _context.SetUniform(shader.GetUniformLocation(gl, "numDirectionalLights"), dirLights.Length);

        for (int i = 0; i < dirLights.Length; i++)
        {
            var light = dirLights[i];
            _context.SetUniform(shader.GetUniformLocation(gl, $"directionalLights[{i}].direction"), light.Direction);
            _context.SetUniform(shader.GetUniformLocation(gl, $"directionalLights[{i}].color"), light.Color.ToVector3());
            _context.SetUniform(shader.GetUniformLocation(gl, $"directionalLights[{i}].intensity"), light.Intensity);
        }

        // Point lights
        var pointLights = lights.OfType<PointLight>().Take(4).ToArray();
        _context.SetUniform(shader.GetUniformLocation(gl, "numPointLights"), pointLights.Length);

        for (int i = 0; i < pointLights.Length; i++)
        {
            var light = pointLights[i];
            var position = Vector3.Transform(Vector3.Zero, light.WorldMatrix);
            _context.SetUniform(shader.GetUniformLocation(gl, $"pointLights[{i}].position"), position);
            _context.SetUniform(shader.GetUniformLocation(gl, $"pointLights[{i}].color"), light.Color.ToVector3());
            _context.SetUniform(shader.GetUniformLocation(gl, $"pointLights[{i}].intensity"), light.Intensity);
            _context.SetUniform(shader.GetUniformLocation(gl, $"pointLights[{i}].distance"), light.Distance);
            _context.SetUniform(shader.GetUniformLocation(gl, $"pointLights[{i}].decay"), light.Decay);
        }

        // Spot lights (similar pattern)
        var spotLights = lights.OfType<SpotLight>().Take(4).ToArray();
        _context.SetUniform(shader.GetUniformLocation(gl, "numSpotLights"), spotLights.Length);

        for (int i = 0; i < spotLights.Length; i++)
        {
            var light = spotLights[i];
            var position = Vector3.Transform(Vector3.Zero, light.WorldMatrix);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].position"), position);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].direction"), light.Direction);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].color"), light.Color.ToVector3());
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].intensity"), light.Intensity);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].distance"), light.Distance);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].angle"), light.Angle);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].penumbra"), light.Penumbra);
            _context.SetUniform(shader.GetUniformLocation(gl, $"spotLights[{i}].decay"), light.Decay);
        }
    }

    /// <summary>
    /// Sets material-specific uniforms
    /// </summary>
    private void SetMaterialUniforms(Material material)
    {
        var gl = _context.GL;
        int textureUnit = 0;

        foreach (var kvp in material.Uniforms)
        {
            int location = material.Shader.GetUniformLocation(gl, kvp.Key);
            if (location < 0) continue;

            if (kvp.Value is Texture texture)
            {
                // Bind texture
                gl.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + textureUnit));
                uint texId = _context.GetTexture(texture);
                gl.BindTexture(TextureTarget.Texture2D, texId);
                gl.Uniform1(location, textureUnit);
                textureUnit++;
            }
            else
            {
                _context.SetUniform(location, kvp.Value);
            }
        }
    }

    /// <summary>
    /// Applies material rendering state
    /// </summary>
    private void ApplyMaterialState(Material material)
    {
        var gl = _context.GL;

        // Depth test
        if (material.DepthTest != _state.DepthTest)
        {
            if (material.DepthTest)
                gl.Enable(EnableCap.DepthTest);
            else
                gl.Disable(EnableCap.DepthTest);
            _state.DepthTest = material.DepthTest;
        }

        // Depth write
        if (material.DepthWrite != _state.DepthWrite)
        {
            gl.DepthMask(material.DepthWrite);
            _state.DepthWrite = material.DepthWrite;
        }

        // Culling
        if (material.CullMode != _state.CurrentCullMode)
        {
            switch (material.CullMode)
            {
                case CullMode.None:
                    gl.Disable(EnableCap.CullFace);
                    break;
                case CullMode.Back:
                    gl.Enable(EnableCap.CullFace);
                    gl.CullFace(CullFaceMode.Back);
                    break;
                case CullMode.Front:
                    gl.Enable(EnableCap.CullFace);
                    gl.CullFace(CullFaceMode.Front);
                    break;
                case CullMode.FrontAndBack:
                    gl.Enable(EnableCap.CullFace);
                    gl.CullFace(CullFaceMode.FrontAndBack);
                    break;
            }
            _state.CurrentCullMode = material.CullMode;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
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
/// Item to be rendered
/// </summary>
internal class RenderItem
{
    public Object3D Object { get; set; } = null!;
    public float Z { get; set; }
}

/// <summary>
/// Performance statistics
/// </summary>
public class PerformanceStats
{
    private DateTime _frameStart;

    public int DrawCalls { get; set; }
    public int Triangles { get; set; }
    public int Vertices { get; set; }
    public float FrameTime { get; private set; }
    public float FPS => FrameTime > 0 ? 1000f / FrameTime : 0;

    public void BeginFrame()
    {
        _frameStart = DateTime.UtcNow;
        DrawCalls = 0;
        Triangles = 0;
        Vertices = 0;
    }

    public void EndFrame()
    {
        FrameTime = (float)(DateTime.UtcNow - _frameStart).TotalMilliseconds;
    }
}
