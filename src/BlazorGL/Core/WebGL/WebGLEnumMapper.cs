using BlazorGL.Core.Materials;

namespace BlazorGL.Core.WebGL;

/// <summary>
/// Maps BlazorGL material enums to WebGL enums
/// </summary>
public static class WebGLEnumMapper
{
    /// <summary>
    /// Maps BlendEquation to WebGL BlendEquationMode
    /// </summary>
    public static BlendEquationMode ToWebGL(this Materials.BlendEquation equation)
    {
        return equation switch
        {
            Materials.BlendEquation.Add => BlendEquationMode.FuncAdd,
            Materials.BlendEquation.Subtract => BlendEquationMode.FuncSubtract,
            Materials.BlendEquation.ReverseSubtract => BlendEquationMode.FuncReverseSubtract,
            Materials.BlendEquation.Min => BlendEquationMode.Min,
            Materials.BlendEquation.Max => BlendEquationMode.Max,
            _ => BlendEquationMode.FuncAdd
        };
    }

    /// <summary>
    /// Maps BlendFactor to WebGL BlendingFactor
    /// </summary>
    public static BlendingFactor ToWebGL(this Materials.BlendFactor factor)
    {
        return factor switch
        {
            Materials.BlendFactor.Zero => BlendingFactor.Zero,
            Materials.BlendFactor.One => BlendingFactor.One,
            Materials.BlendFactor.SrcColor => BlendingFactor.SrcColor,
            Materials.BlendFactor.OneMinusSrcColor => BlendingFactor.OneMinusSrcColor,
            Materials.BlendFactor.DstColor => BlendingFactor.DstColor,
            Materials.BlendFactor.OneMinusDstColor => BlendingFactor.OneMinusDstColor,
            Materials.BlendFactor.SrcAlpha => BlendingFactor.SrcAlpha,
            Materials.BlendFactor.OneMinusSrcAlpha => BlendingFactor.OneMinusSrcAlpha,
            Materials.BlendFactor.DstAlpha => BlendingFactor.DstAlpha,
            Materials.BlendFactor.OneMinusDstAlpha => BlendingFactor.OneMinusDstAlpha,
            Materials.BlendFactor.ConstantColor => BlendingFactor.ConstantColor,
            Materials.BlendFactor.OneMinusConstantColor => BlendingFactor.OneMinusConstantColor,
            Materials.BlendFactor.ConstantAlpha => BlendingFactor.ConstantAlpha,
            Materials.BlendFactor.OneMinusConstantAlpha => BlendingFactor.OneMinusConstantAlpha,
            Materials.BlendFactor.SrcAlphaSaturate => BlendingFactor.SrcAlphaSaturate,
            _ => BlendingFactor.SrcAlpha
        };
    }

    /// <summary>
    /// Maps StencilFunc to WebGL StencilFunction
    /// </summary>
    public static StencilFunction ToWebGL(this Materials.StencilFunc func)
    {
        return func switch
        {
            Materials.StencilFunc.Never => StencilFunction.Never,
            Materials.StencilFunc.Less => StencilFunction.Less,
            Materials.StencilFunc.Equal => StencilFunction.Equal,
            Materials.StencilFunc.LessOrEqual => StencilFunction.LessEqual,
            Materials.StencilFunc.Greater => StencilFunction.Greater,
            Materials.StencilFunc.NotEqual => StencilFunction.NotEqual,
            Materials.StencilFunc.GreaterOrEqual => StencilFunction.GreaterEqual,
            Materials.StencilFunc.Always => StencilFunction.Always,
            _ => StencilFunction.Always
        };
    }

    /// <summary>
    /// Maps StencilOp to WebGL StencilOperation
    /// </summary>
    public static StencilOperation ToWebGL(this Materials.StencilOp op)
    {
        return op switch
        {
            Materials.StencilOp.Keep => StencilOperation.Keep,
            Materials.StencilOp.Zero => StencilOperation.Zero,
            Materials.StencilOp.Replace => StencilOperation.Replace,
            Materials.StencilOp.Increment => StencilOperation.Increment,
            Materials.StencilOp.IncrementWrap => StencilOperation.IncrementWrap,
            Materials.StencilOp.Decrement => StencilOperation.Decrement,
            Materials.StencilOp.DecrementWrap => StencilOperation.DecrementWrap,
            Materials.StencilOp.Invert => StencilOperation.Invert,
            _ => StencilOperation.Keep
        };
    }
}
