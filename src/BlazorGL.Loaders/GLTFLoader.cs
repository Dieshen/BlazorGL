using System.Text.Json;
using BlazorGL.Core;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Cameras;
using System.Numerics;

namespace BlazorGL.Loaders;

/// <summary>
/// Loads GLTF/GLB 3D models
/// </summary>
public class GLTFLoader
{
    /// <summary>
    /// Loads a GLTF file from a URL
    /// </summary>
    public async Task<GLTFScene> LoadAsync(string url)
    {
        using var httpClient = new HttpClient();
        var data = await httpClient.GetByteArrayAsync(url);

        // Check if it's a GLB (binary) or GLTF (JSON) file
        if (url.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
        {
            return LoadFromGLB(data);
        }
        else
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return LoadFromJSON(json, url);
        }
    }

    /// <summary>
    /// Loads from byte array
    /// </summary>
    public GLTFScene LoadFromBytes(byte[] data)
    {
        // Assume GLB format for binary data
        return LoadFromGLB(data);
    }

    private GLTFScene LoadFromJSON(string json, string baseUrl)
    {
        // Parse GLTF JSON
        var gltf = JsonSerializer.Deserialize<GLTFRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (gltf == null)
            throw new Exception("Failed to parse GLTF JSON");

        return BuildScene(gltf, null, baseUrl);
    }

    private GLTFScene LoadFromGLB(byte[] data)
    {
        // GLB format:
        // - 12 byte header
        // - JSON chunk
        // - Optional BIN chunk

        // This is a simplified implementation
        // A full implementation would parse the binary format properly
        throw new NotImplementedException("GLB loading is not yet fully implemented. Please use GLTF JSON format for now.");
    }

    private GLTFScene BuildScene(GLTFRoot gltf, byte[]? binaryData, string baseUrl)
    {
        var result = new GLTFScene
        {
            Scene = new Scene()
        };

        // Load meshes
        if (gltf.Meshes != null)
        {
            foreach (var gltfMesh in gltf.Meshes)
            {
                var group = new Group { Name = gltfMesh.Name ?? "Mesh" };

                foreach (var primitive in gltfMesh.Primitives ?? Array.Empty<GLTFPrimitive>())
                {
                    // Create geometry from primitive
                    var geometry = CreateGeometry(gltf, primitive, binaryData);

                    // Create material
                    var material = new StandardMaterial
                    {
                        Color = Core.Math.Color.White
                    };

                    var mesh = new Mesh(geometry, material);
                    group.Add(mesh);
                }

                result.Scene.Add(group);
            }
        }

        return result;
    }

    private Geometry CreateGeometry(GLTFRoot gltf, GLTFPrimitive primitive, byte[]? binaryData)
    {
        // This is a simplified implementation
        // A full implementation would read from accessors/bufferViews/buffers

        // For now, create a placeholder box geometry
        return new BoxGeometry(1, 1, 1);
    }
}

/// <summary>
/// GLTF scene container
/// </summary>
public class GLTFScene
{
    public Scene Scene { get; set; } = new();
    public List<Animation> Animations { get; set; } = new();
    public List<Camera> Cameras { get; set; } = new();
}

/// <summary>
/// Animation clip (placeholder for now)
/// </summary>
public class Animation
{
    public string Name { get; set; } = string.Empty;
    public float Duration { get; set; }
}

// GLTF JSON structure (simplified)
internal class GLTFRoot
{
    public GLTFAsset? Asset { get; set; }
    public GLTFScene[]? Scenes { get; set; }
    public GLTFNode[]? Nodes { get; set; }
    public GLTFMesh[]? Meshes { get; set; }
    public GLTFAccessor[]? Accessors { get; set; }
    public GLTFBufferView[]? BufferViews { get; set; }
    public GLTFBuffer[]? Buffers { get; set; }
    public GLTFMaterial[]? Materials { get; set; }
    public GLTFTexture[]? Textures { get; set; }
}

internal class GLTFAsset
{
    public string Version { get; set; } = "2.0";
    public string? Generator { get; set; }
}

internal class GLTFScene
{
    public string? Name { get; set; }
    public int[]? Nodes { get; set; }
}

internal class GLTFNode
{
    public string? Name { get; set; }
    public int? Mesh { get; set; }
    public float[]? Translation { get; set; }
    public float[]? Rotation { get; set; }
    public float[]? Scale { get; set; }
    public int[]? Children { get; set; }
}

internal class GLTFMesh
{
    public string? Name { get; set; }
    public GLTFPrimitive[]? Primitives { get; set; }
}

internal class GLTFPrimitive
{
    public Dictionary<string, int>? Attributes { get; set; }
    public int? Indices { get; set; }
    public int? Material { get; set; }
    public int Mode { get; set; } = 4; // TRIANGLES
}

internal class GLTFAccessor
{
    public int BufferView { get; set; }
    public int ComponentType { get; set; }
    public int Count { get; set; }
    public string Type { get; set; } = "SCALAR";
    public float[]? Min { get; set; }
    public float[]? Max { get; set; }
}

internal class GLTFBufferView
{
    public int Buffer { get; set; }
    public int ByteOffset { get; set; }
    public int ByteLength { get; set; }
    public int? ByteStride { get; set; }
}

internal class GLTFBuffer
{
    public int ByteLength { get; set; }
    public string? Uri { get; set; }
}

internal class GLTFMaterial
{
    public string? Name { get; set; }
    public GLTFPBRMetallicRoughness? PbrMetallicRoughness { get; set; }
}

internal class GLTFPBRMetallicRoughness
{
    public float[]? BaseColorFactor { get; set; }
    public float MetallicFactor { get; set; } = 1.0f;
    public float RoughnessFactor { get; set; } = 1.0f;
}

internal class GLTFTexture
{
    public int? Source { get; set; }
    public int? Sampler { get; set; }
}
