namespace BlazorGL.Core.Materials;

/// <summary>
/// Blending mode for transparent materials
/// </summary>
public enum BlendMode
{
    None,
    Normal,
    Additive,
    Subtractive,
    Multiply
}

/// <summary>
/// Face culling mode
/// </summary>
public enum CullMode
{
    None,
    Back,
    Front,
    FrontAndBack
}

/// <summary>
/// Polygon rendering mode
/// </summary>
public enum PolygonMode
{
    Fill,
    Line,
    Point
}

/// <summary>
/// Depth testing function
/// </summary>
public enum DepthFunc
{
    Never,
    Less,
    Equal,
    LessOrEqual,
    Greater,
    NotEqual,
    GreaterOrEqual,
    Always
}

/// <summary>
/// Blend equation for combining source and destination colors
/// </summary>
public enum BlendEquation
{
    Add,
    Subtract,
    ReverseSubtract,
    Min,
    Max
}

/// <summary>
/// Blend factors for source and destination
/// </summary>
public enum BlendFactor
{
    Zero,
    One,
    SrcColor,
    OneMinusSrcColor,
    DstColor,
    OneMinusDstColor,
    SrcAlpha,
    OneMinusSrcAlpha,
    DstAlpha,
    OneMinusDstAlpha,
    ConstantColor,
    OneMinusConstantColor,
    ConstantAlpha,
    OneMinusConstantAlpha,
    SrcAlphaSaturate
}

/// <summary>
/// Stencil test function
/// </summary>
public enum StencilFunc
{
    Never,
    Less,
    Equal,
    LessOrEqual,
    Greater,
    NotEqual,
    GreaterOrEqual,
    Always
}

/// <summary>
/// Stencil operation
/// </summary>
public enum StencilOp
{
    Keep,
    Zero,
    Replace,
    Increment,
    IncrementWrap,
    Decrement,
    DecrementWrap,
    Invert
}
