using BlazorGL.Core.Shaders;
using System.Numerics;

namespace BlazorGL.Core.Materials;

/// <summary>
/// Material that renders distance from a reference point (typically a light)
/// Used for shadow mapping with point lights
/// </summary>
public class DistanceMaterial : Material
{
    /// <summary>
    /// Reference position to measure distance from
    /// </summary>
    public Vector3 ReferencePosition { get; set; } = Vector3.Zero;

    /// <summary>
    /// Near distance
    /// </summary>
    public float Near { get; set; } = 0.1f;

    /// <summary>
    /// Far distance
    /// </summary>
    public float Far { get; set; } = 1000f;

    public DistanceMaterial()
    {
        InitializeShader();
    }

    public override void InitializeShader()
    {
        Shader = new Shader(ShaderLibrary.Distance.VertexShader, ShaderLibrary.Distance.FragmentShader);
        NeedsCompile = true;
    }

    public override void UpdateUniforms()
    {
        Uniforms["opacity"] = Opacity;
        Uniforms["referencePosition"] = ReferencePosition;
        Uniforms["near"] = Near;
        Uniforms["far"] = Far;
    }
}
