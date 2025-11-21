namespace BlazorGL.Core.WebGL;

/// <summary>
/// Lightweight WebGL enum mirrors used by the renderer.
/// Values are not sent directly to the GPU; they are mapped to WebGL constants inside the JS module.
/// </summary>
public enum EnableCap
{
    DepthTest,
    CullFace,
    Blend
}

public enum CullFaceMode
{
    Back,
    Front,
    FrontAndBack
}

public enum FrontFaceDirection
{
    Ccw
}

public enum BlendingFactor
{
    SrcAlpha,
    OneMinusSrcAlpha
}

public enum ClearBufferMask
{
    ColorBufferBit,
    DepthBufferBit,
    StencilBufferBit
}

public enum BufferTargetARB
{
    ArrayBuffer,
    ElementArrayBuffer
}

public enum BufferUsageARB
{
    StaticDraw,
    DynamicDraw
}

public enum TextureTarget
{
    Texture2D
}

public enum TextureUnit
{
    Texture0
}

public enum TextureParameterName
{
    TextureWrapS,
    TextureWrapT,
    TextureMinFilter,
    TextureMagFilter
}

public enum InternalFormat
{
    Rgba,
    DepthComponent16
}

public enum PixelFormat
{
    Rgba
}

public enum PixelType
{
    UnsignedByte
}

public enum FramebufferTarget
{
    Framebuffer
}

public enum FramebufferAttachment
{
    ColorAttachment0,
    DepthAttachment
}

public enum RenderbufferTarget
{
    Renderbuffer
}

public enum PrimitiveType
{
    Triangles,
    LineStrip,
    Lines,
    LineLoop,
    Points
}

public enum DrawElementsType
{
    UnsignedInt
}

public enum VertexAttribPointerType
{
    Float
}

public enum ShaderType
{
    VertexShader,
    FragmentShader
}

public enum ProgramPropertyARB
{
    ActiveUniforms,
    ActiveAttributes
}

/// <summary>
/// Marker enum for GL status results returned from JS.
/// Values are symbolic and mapped in JS.
/// </summary>
public enum GLEnum
{
    FramebufferComplete = 0x8CD5
}
