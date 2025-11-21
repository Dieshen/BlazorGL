using BlazorGL.Core.WebGL;

namespace BlazorGL.Core.Shaders;

/// <summary>
/// Represents a compiled shader program
/// </summary>
public class Shader : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// WebGL program handle
    /// </summary>
    public uint ProgramId { get; private set; }

    /// <summary>
    /// Vertex shader source
    /// </summary>
    public string VertexSource { get; }

    /// <summary>
    /// Fragment shader source
    /// </summary>
    public string FragmentSource { get; }

    /// <summary>
    /// Uniform locations cache
    /// </summary>
    public Dictionary<string, int> Uniforms { get; } = new();

    /// <summary>
    /// Attribute locations cache
    /// </summary>
    public Dictionary<string, int> Attributes { get; } = new();

    /// <summary>
    /// Whether the shader is compiled
    /// </summary>
    public bool IsCompiled { get; private set; }

    public Shader(string vertexSource, string fragmentSource)
    {
        VertexSource = vertexSource;
        FragmentSource = fragmentSource;
    }

    /// <summary>
    /// Compiles the shader program
    /// </summary>
    public void Compile(GL gl)
    {
        if (IsCompiled) return;

        // Compile vertex shader
        uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, VertexSource);
        gl.CompileShader(vertexShader);

        string infoLog = gl.GetShaderInfoLog(vertexShader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Vertex shader compilation failed: {infoLog}");
        }

        // Compile fragment shader
        uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, FragmentSource);
        gl.CompileShader(fragmentShader);

        infoLog = gl.GetShaderInfoLog(fragmentShader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            gl.DeleteShader(vertexShader);
            throw new Exception($"Fragment shader compilation failed: {infoLog}");
        }

        // Link program
        ProgramId = gl.CreateProgram();
        gl.AttachShader(ProgramId, vertexShader);
        gl.AttachShader(ProgramId, fragmentShader);
        gl.LinkProgram(ProgramId);

        infoLog = gl.GetProgramInfoLog(ProgramId);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            gl.DeleteShader(vertexShader);
            gl.DeleteShader(fragmentShader);
            gl.DeleteProgram(ProgramId);
            throw new Exception($"Shader program linking failed: {infoLog}");
        }

        // Clean up individual shaders
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        IsCompiled = true;

        // Cache attribute and uniform locations
        CacheLocations(gl);
    }

    /// <summary>
    /// Caches all uniform and attribute locations
    /// </summary>
    private void CacheLocations(GL gl)
    {
        // Get active uniforms
        int uniformCount = gl.GetProgram(ProgramId, ProgramPropertyARB.ActiveUniforms);
        for (uint i = 0; i < uniformCount; i++)
        {
            string name = gl.GetActiveUniform(ProgramId, i, out _, out _);
            int location = gl.GetUniformLocation(ProgramId, name);
            if (location >= 0)
            {
                Uniforms[name] = location;
            }
        }

        // Get active attributes
        int attributeCount = gl.GetProgram(ProgramId, ProgramPropertyARB.ActiveAttributes);
        for (uint i = 0; i < attributeCount; i++)
        {
            string name = gl.GetActiveAttrib(ProgramId, i, out _, out _);
            int location = gl.GetAttribLocation(ProgramId, name);
            Attributes[name] = location;
        }
    }

    /// <summary>
    /// Uses this shader program
    /// </summary>
    public void Use(GL gl)
    {
        gl.UseProgram(ProgramId);
    }

    /// <summary>
    /// Gets a uniform location (from cache or queries GL)
    /// </summary>
    public int GetUniformLocation(GL gl, string name)
    {
        if (Uniforms.TryGetValue(name, out int location))
            return location;

        location = gl.GetUniformLocation(ProgramId, name);
        if (location >= 0)
            Uniforms[name] = location;

        return location;
    }

    /// <summary>
    /// Gets an attribute location (from cache or queries GL)
    /// </summary>
    public int GetAttributeLocation(GL gl, string name)
    {
        if (Attributes.TryGetValue(name, out int location))
            return location;

        location = gl.GetAttribLocation(ProgramId, name);
        Attributes[name] = location;
        return location;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && IsCompiled)
            {
                // Note: GL context must be current when disposing
                // This should be called from renderer cleanup
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
