using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Numerics;
using System.Linq;

namespace BlazorGL.Core.WebGL;

/// <summary>
/// Minimal WebGL interop layer backed by a JS module.
/// </summary>
public sealed class GL : IAsyncDisposable, IDisposable
{
    private readonly IJSInProcessObjectReference _module;
    private readonly int _ctxId;
    private bool _disposed;

    private GL(IJSInProcessObjectReference module, int ctxId)
    {
        _module = module;
        _ctxId = ctxId;
    }

    public static async Task<GL> CreateAsync(IJSRuntime jsRuntime, ElementReference canvas)
    {
        if (jsRuntime is not IJSInProcessRuntime inProcess)
        {
            throw new InvalidOperationException("BlazorGL requires IJSInProcessRuntime (WebAssembly) for synchronous WebGL calls.");
        }

        var module = await jsRuntime.InvokeAsync<IJSInProcessObjectReference>(
            "import",
            "./_content/BlazorGL/blazorgl.webgl.js");

        int ctxId = module.Invoke<int>("createContext", canvas);
        return new GL(module, ctxId);
    }

    public void Enable(EnableCap cap) => _module.InvokeVoid("enable", _ctxId, cap.ToString());
    public void Disable(EnableCap cap) => _module.InvokeVoid("disable", _ctxId, cap.ToString());
    public void CullFace(CullFaceMode mode) => _module.InvokeVoid("cullFace", _ctxId, mode.ToString());
    public void FrontFace(FrontFaceDirection dir) => _module.InvokeVoid("frontFace", _ctxId, dir.ToString());
    public void BlendFunc(BlendingFactor src, BlendingFactor dst) => _module.InvokeVoid("blendFunc", _ctxId, src.ToString(), dst.ToString());
    public void Viewport(int x, int y, uint w, uint h) => _module.InvokeVoid("viewport", _ctxId, x, y, w, h);
    public void ClearColor(float r, float g, float b, float a) => _module.InvokeVoid("clearColor", _ctxId, r, g, b, a);
    public void Clear(params ClearBufferMask[] masks)
    {
        var names = masks.Select(m => m.ToString()).ToArray();
        _module.InvokeVoid("clearMultiple", _ctxId, names);
    }

    public uint CreateVertexArray() => _module.Invoke<uint>("createVertexArray", _ctxId);
    public void BindVertexArray(uint vao) => _module.InvokeVoid("bindVertexArray", _ctxId, vao);
    public uint CreateBuffer() => _module.Invoke<uint>("createBuffer", _ctxId);
    public void BindBuffer(BufferTargetARB target, uint buffer) => _module.InvokeVoid("bindBuffer", _ctxId, target.ToString(), buffer);
    public void BufferData(BufferTargetARB target, float[] data, BufferUsageARB usage) => _module.InvokeVoid("bufferDataFloat", _ctxId, target.ToString(), data, usage.ToString());
    public void BufferData(BufferTargetARB target, uint[] data, BufferUsageARB usage) => _module.InvokeVoid("bufferDataUInt", _ctxId, target.ToString(), data, usage.ToString());

    public uint CreateTexture() => _module.Invoke<uint>("createTexture", _ctxId);
    public void BindTexture(TextureTarget target, uint tex) => _module.InvokeVoid("bindTexture", _ctxId, target.ToString(), tex);
    public void TexImage2D(TextureTarget target, int level, InternalFormat internalFormat, uint width, uint height, PixelFormat format, PixelType type, byte[]? data) =>
        _module.InvokeVoid("texImage2D", _ctxId, target.ToString(), level, internalFormat.ToString(), width, height, format.ToString(), type.ToString(), data);
    public void TexParameterI(TextureTarget target, TextureParameterName pname, string value) =>
        _module.InvokeVoid("texParameter", _ctxId, target.ToString(), pname.ToString(), value);
    public void GenerateMipmap(TextureTarget target) => _module.InvokeVoid("generateMipmap", _ctxId, target.ToString());

    public uint CreateFramebuffer() => _module.Invoke<uint>("createFramebuffer", _ctxId);
    public void BindFramebuffer(FramebufferTarget target, uint framebuffer) => _module.InvokeVoid("bindFramebuffer", _ctxId, target.ToString(), framebuffer);
    public void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget texTarget, uint tex, int level) =>
        _module.InvokeVoid("framebufferTexture2D", _ctxId, target.ToString(), attachment.ToString(), texTarget.ToString(), tex, level);

    public uint CreateRenderbuffer() => _module.Invoke<uint>("createRenderbuffer", _ctxId);
    public void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer) => _module.InvokeVoid("bindRenderbuffer", _ctxId, target.ToString(), renderbuffer);
    public void RenderbufferStorage(RenderbufferTarget target, InternalFormat format, uint width, uint height) =>
        _module.InvokeVoid("renderbufferStorage", _ctxId, target.ToString(), format.ToString(), width, height);
    public void FramebufferRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment, RenderbufferTarget renderbufferTarget, uint renderbuffer) =>
        _module.InvokeVoid("framebufferRenderbuffer", _ctxId, target.ToString(), attachment.ToString(), renderbufferTarget.ToString(), renderbuffer);
    public GLEnum CheckFramebufferStatus(FramebufferTarget target) => _module.Invoke<GLEnum>("checkFramebufferStatus", _ctxId, target.ToString());

    public void DeleteTexture(uint tex) => _module.InvokeVoid("deleteTexture", _ctxId, tex);
    public void DeleteFramebuffer(uint fb) => _module.InvokeVoid("deleteFramebuffer", _ctxId, fb);
    public void DeleteRenderbuffer(uint rb) => _module.InvokeVoid("deleteRenderbuffer", _ctxId, rb);
    public void DeleteBuffer(uint buffer) => _module.InvokeVoid("deleteBuffer", _ctxId, buffer);
    public void DeleteVertexArray(uint vao) => _module.InvokeVoid("deleteVertexArray", _ctxId, vao);

    public void ActiveTexture(TextureUnit unit) => _module.InvokeVoid("activeTexture", _ctxId, (int)unit);
    public void DrawElements(PrimitiveType mode, uint count, DrawElementsType type, int offset) =>
        _module.InvokeVoid("drawElements", _ctxId, mode.ToString(), count, type.ToString(), offset);
    public void DrawElementsInstanced(PrimitiveType mode, uint count, DrawElementsType type, int offset, uint instanceCount) =>
        _module.InvokeVoid("drawElementsInstanced", _ctxId, mode.ToString(), count, type.ToString(), offset, instanceCount);
    public void DrawArrays(PrimitiveType mode, int first, uint count) =>
        _module.InvokeVoid("drawArrays", _ctxId, mode.ToString(), first, count);

    public void VertexAttribPointer(uint index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset) =>
        _module.InvokeVoid("vertexAttribPointer", _ctxId, index, size, type.ToString(), normalized, stride, offset);
    public void VertexAttribDivisor(uint index, uint divisor) => _module.InvokeVoid("vertexAttribDivisor", _ctxId, index, divisor);
    public void EnableVertexAttribArray(uint index) => _module.InvokeVoid("enableVertexAttribArray", _ctxId, index);

    public uint CreateShader(ShaderType type) => _module.Invoke<uint>("createShader", _ctxId, type.ToString());
    public void ShaderSource(uint shader, string source) => _module.InvokeVoid("shaderSource", _ctxId, shader, source);
    public void CompileShader(uint shader) => _module.InvokeVoid("compileShader", _ctxId, shader);
    public string GetShaderInfoLog(uint shader) => _module.Invoke<string>("getShaderInfoLog", _ctxId, shader);

    public uint CreateProgram() => _module.Invoke<uint>("createProgram", _ctxId);
    public void AttachShader(uint program, uint shader) => _module.InvokeVoid("attachShader", _ctxId, program, shader);
    public void LinkProgram(uint program) => _module.InvokeVoid("linkProgram", _ctxId, program);
    public string GetProgramInfoLog(uint program) => _module.Invoke<string>("getProgramInfoLog", _ctxId, program);
    public void DeleteShader(uint shader) => _module.InvokeVoid("deleteShader", _ctxId, shader);
    public void DeleteProgram(uint program) => _module.InvokeVoid("deleteProgram", _ctxId, program);
    public void UseProgram(uint program) => _module.InvokeVoid("useProgram", _ctxId, program);
    public int GetProgram(uint program, ProgramPropertyARB property) => _module.Invoke<int>("getProgramParameter", _ctxId, program, property.ToString());
    public string GetActiveUniform(uint program, uint index, out int size, out int type)
    {
        var result = _module.Invoke<ActiveInfo>("getActiveUniform", _ctxId, program, index);
        size = result.Size;
        type = result.Type;
        return result.Name;
    }
    public int GetUniformLocation(uint program, string name) => _module.Invoke<int>("getUniformLocation", _ctxId, program, name);
    public string GetActiveAttrib(uint program, uint index, out int size, out int type)
    {
        var result = _module.Invoke<ActiveInfo>("getActiveAttrib", _ctxId, program, index);
        size = result.Size;
        type = result.Type;
        return result.Name;
    }
    public int GetAttribLocation(uint program, string name) => _module.Invoke<int>("getAttribLocation", _ctxId, program, name);

    public void Uniform1(int location, int value) => _module.InvokeVoid("uniform1i", _ctxId, location, value);
    public void Uniform1(int location, float value) => _module.InvokeVoid("uniform1f", _ctxId, location, value);
    public void Uniform2(int location, float x, float y) => _module.InvokeVoid("uniform2f", _ctxId, location, x, y);
    public void Uniform3(int location, float x, float y, float z) => _module.InvokeVoid("uniform3f", _ctxId, location, x, y, z);
    public void Uniform4(int location, float x, float y, float z, float w) => _module.InvokeVoid("uniform4f", _ctxId, location, x, y, z, w);
    public void UniformMatrix4(int location, bool transpose, float[] values) =>
        _module.InvokeVoid("uniformMatrix4fv", _ctxId, location, transpose, values);

    public void DepthMask(bool flag) => _module.InvokeVoid("depthMask", _ctxId, flag);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        await _module.DisposeAsync();
    }

    public void Dispose()
    {
        if (_disposed) return;
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private record ActiveInfo(string Name, int Size, int Type);
}
