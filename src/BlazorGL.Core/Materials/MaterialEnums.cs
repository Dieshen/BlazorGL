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
