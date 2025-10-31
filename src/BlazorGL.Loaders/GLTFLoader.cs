using System.Text.Json;
using BlazorGL.Core;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Cameras;
using System.Numerics;
using System.Text.Json.Serialization;

namespace BlazorGL.Loaders;

/// <summary>
/// Loads GLTF/GLB 3D models with full support for geometry, materials, and node hierarchy
/// </summary>
public class GLTFLoader
{
    private string? _baseUrl;
    private Dictionary<int, byte[]> _bufferCache = new();
    private Dictionary<int, Core.Textures.Texture> _textureCache = new();

    /// <summary>
    /// Loads a GLTF file from a URL
    /// </summary>
    public async Task<GLTFScene> LoadAsync(string url)
    {
        using var httpClient = new HttpClient();
        var data = await httpClient.GetByteArrayAsync(url);

        // Extract base URL
        int lastSlash = url.LastIndexOf('/');
        _baseUrl = lastSlash > 0 ? url.Substring(0, lastSlash + 1) : "";

        // Check if it's a GLB (binary) or GLTF (JSON) file
        if (url.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
        {
            return await LoadFromGLB(data);
        }
        else
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return await LoadFromJSON(json);
        }
    }

    /// <summary>
    /// Loads from byte array (assumes GLB format)
    /// </summary>
    public async Task<GLTFScene> LoadFromBytes(byte[] data)
    {
        return await LoadFromGLB(data);
    }

    private async Task<GLTFScene> LoadFromJSON(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var gltf = JsonSerializer.Deserialize<GLTFRoot>(json, options);
        if (gltf == null)
            throw new Exception("Failed to parse GLTF JSON");

        return await BuildScene(gltf, null);
    }

    private async Task<GLTFScene> LoadFromGLB(byte[] data)
    {
        // GLB format:
        // Header (12 bytes):
        //   - magic (4 bytes): 0x46546C67 ("glTF")
        //   - version (4 bytes): 2
        //   - length (4 bytes): total file length
        // Chunks:
        //   - JSON chunk (chunkLength + chunkType + chunkData)
        //   - BIN chunk (optional)

        if (data.Length < 12)
            throw new Exception("Invalid GLB file: too small");

        uint magic = BitConverter.ToUInt32(data, 0);
        if (magic != 0x46546C67)
            throw new Exception("Invalid GLB file: bad magic number");

        uint version = BitConverter.ToUInt32(data, 4);
        if (version != 2)
            throw new Exception($"Unsupported GLB version: {version}");

        uint length = BitConverter.ToUInt32(data, 8);

        // Read JSON chunk
        int offset = 12;
        uint jsonChunkLength = BitConverter.ToUInt32(data, offset);
        uint jsonChunkType = BitConverter.ToUInt32(data, offset + 4);
        offset += 8;

        if (jsonChunkType != 0x4E4F534A) // "JSON"
            throw new Exception("Invalid GLB file: expected JSON chunk");

        string json = System.Text.Encoding.UTF8.GetString(data, offset, (int)jsonChunkLength);
        offset += (int)jsonChunkLength;

        // Read BIN chunk (if exists)
        byte[]? binaryData = null;
        if (offset < length)
        {
            uint binChunkLength = BitConverter.ToUInt32(data, offset);
            uint binChunkType = BitConverter.ToUInt32(data, offset + 4);
            offset += 8;

            if (binChunkType == 0x004E4942) // "BIN"
            {
                binaryData = new byte[binChunkLength];
                Array.Copy(data, offset, binaryData, 0, binChunkLength);
            }
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var gltf = JsonSerializer.Deserialize<GLTFRoot>(json, options);
        if (gltf == null)
            throw new Exception("Failed to parse GLTF JSON from GLB");

        return await BuildScene(gltf, binaryData);
    }

    private async Task<GLTFScene> BuildScene(GLTFRoot gltf, byte[]? embeddedBinaryData)
    {
        var result = new GLTFScene { Scene = new Scene() };

        // Load buffers
        await LoadBuffers(gltf, embeddedBinaryData);

        // Load textures
        if (gltf.Textures != null && gltf.Images != null)
        {
            await LoadTextures(gltf);
        }

        // Create materials
        var materials = new List<Material>();
        if (gltf.Materials != null)
        {
            foreach (var gltfMat in gltf.Materials)
            {
                materials.Add(CreateMaterial(gltfMat));
            }
        }

        // Create meshes
        var meshObjects = new List<Group>();
        if (gltf.Meshes != null)
        {
            foreach (var gltfMesh in gltf.Meshes)
            {
                var group = new Group { Name = gltfMesh.Name ?? "Mesh" };

                foreach (var primitive in gltfMesh.Primitives ?? Array.Empty<GLTFPrimitive>())
                {
                    var geometry = await CreateGeometry(gltf, primitive);

                    Material material;
                    if (primitive.Material.HasValue && primitive.Material.Value < materials.Count)
                    {
                        material = materials[primitive.Material.Value];
                    }
                    else
                    {
                        material = new StandardMaterial { Color = Core.Math.Color.White };
                    }

                    var mesh = new Mesh(geometry, material);
                    group.Add(mesh);
                }

                meshObjects.Add(group);
            }
        }

        // Build node hierarchy
        if (gltf.Nodes != null)
        {
            var nodeObjects = new Object3D[gltf.Nodes.Length];

            for (int i = 0; i < gltf.Nodes.Length; i++)
            {
                var node = gltf.Nodes[i];
                Object3D obj;

                if (node.Mesh.HasValue && node.Mesh.Value < meshObjects.Count)
                {
                    obj = meshObjects[node.Mesh.Value];
                }
                else
                {
                    obj = new Group();
                }

                obj.Name = node.Name ?? $"Node_{i}";

                // Apply transform
                if (node.Translation != null && node.Translation.Length >= 3)
                {
                    obj.Position = new Vector3(node.Translation[0], node.Translation[1], node.Translation[2]);
                }

                if (node.Rotation != null && node.Rotation.Length >= 4)
                {
                    obj.Rotation = new Quaternion(node.Rotation[0], node.Rotation[1], node.Rotation[2], node.Rotation[3]);
                }

                if (node.Scale != null && node.Scale.Length >= 3)
                {
                    obj.Scale = new Vector3(node.Scale[0], node.Scale[1], node.Scale[2]);
                }

                nodeObjects[i] = obj;
            }

            // Build hierarchy
            for (int i = 0; i < gltf.Nodes.Length; i++)
            {
                var node = gltf.Nodes[i];
                if (node.Children != null)
                {
                    foreach (var childIndex in node.Children)
                    {
                        if (childIndex < nodeObjects.Length)
                        {
                            nodeObjects[i].Add(nodeObjects[childIndex]);
                        }
                    }
                }
            }

            // Add root nodes to scene
            var defaultScene = gltf.Scenes != null && gltf.Scenes.Length > 0 ? gltf.Scenes[0] : null;
            if (defaultScene?.Nodes != null)
            {
                foreach (var nodeIndex in defaultScene.Nodes)
                {
                    if (nodeIndex < nodeObjects.Length)
                    {
                        result.Scene.Add(nodeObjects[nodeIndex]);
                    }
                }
            }
            else
            {
                // Add all root nodes
                for (int i = 0; i < nodeObjects.Length; i++)
                {
                    if (nodeObjects[i].Parent == null)
                    {
                        result.Scene.Add(nodeObjects[i]);
                    }
                }
            }
        }
        else if (meshObjects.Count > 0)
        {
            // No nodes, just add meshes directly
            foreach (var mesh in meshObjects)
            {
                result.Scene.Add(mesh);
            }
        }

        return result;
    }

    private async Task LoadBuffers(GLTFRoot gltf, byte[]? embeddedBinaryData)
    {
        if (gltf.Buffers == null) return;

        for (int i = 0; i < gltf.Buffers.Length; i++)
        {
            var buffer = gltf.Buffers[i];

            if (buffer.Uri == null && embeddedBinaryData != null)
            {
                // Use embedded binary data from GLB
                _bufferCache[i] = embeddedBinaryData;
            }
            else if (buffer.Uri != null)
            {
                if (buffer.Uri.StartsWith("data:"))
                {
                    // Data URI
                    var base64 = buffer.Uri.Substring(buffer.Uri.IndexOf(',') + 1);
                    _bufferCache[i] = Convert.FromBase64String(base64);
                }
                else
                {
                    // External file
                    using var httpClient = new HttpClient();
                    _bufferCache[i] = await httpClient.GetByteArrayAsync(_baseUrl + buffer.Uri);
                }
            }
        }
    }

    private async Task LoadTextures(GLTFRoot gltf)
    {
        if (gltf.Images == null || gltf.Textures == null) return;

        for (int i = 0; i < gltf.Images.Length; i++)
        {
            var image = gltf.Images[i];

            try
            {
                if (image.Uri != null)
                {
                    if (image.Uri.StartsWith("data:"))
                    {
                        // Data URI
                        var base64 = image.Uri.Substring(image.Uri.IndexOf(',') + 1);
                        var imageData = Convert.FromBase64String(base64);
                        _textureCache[i] = TextureLoader.LoadFromBytes(imageData);
                    }
                    else
                    {
                        // External file
                        _textureCache[i] = await TextureLoader.LoadAsync(_baseUrl + image.Uri);
                    }
                }
                else if (image.BufferView.HasValue)
                {
                    // Load from buffer view
                    var bufferView = gltf.BufferViews![image.BufferView.Value];
                    var buffer = _bufferCache[bufferView.Buffer];
                    var imageData = new byte[bufferView.ByteLength];
                    Array.Copy(buffer, bufferView.ByteOffset, imageData, 0, bufferView.ByteLength);
                    _textureCache[i] = TextureLoader.LoadFromBytes(imageData);
                }
            }
            catch
            {
                // Texture loading failed, continue
            }
        }
    }

    private Material CreateMaterial(GLTFMaterial gltfMat)
    {
        var material = new StandardMaterial
        {
            Name = gltfMat.Name ?? "Material"
        };

        if (gltfMat.PbrMetallicRoughness != null)
        {
            var pbr = gltfMat.PbrMetallicRoughness;

            if (pbr.BaseColorFactor != null && pbr.BaseColorFactor.Length >= 3)
            {
                material.Color = new Core.Math.Color(
                    pbr.BaseColorFactor[0],
                    pbr.BaseColorFactor[1],
                    pbr.BaseColorFactor[2],
                    pbr.BaseColorFactor.Length >= 4 ? pbr.BaseColorFactor[3] : 1.0f
                );
            }

            material.Metalness = pbr.MetallicFactor;
            material.Roughness = pbr.RoughnessFactor;

            if (pbr.BaseColorTexture != null && _textureCache.TryGetValue(pbr.BaseColorTexture.Index, out var baseColorTex))
            {
                material.Map = baseColorTex;
            }
        }

        if (gltfMat.NormalTexture != null && _textureCache.TryGetValue(gltfMat.NormalTexture.Index, out var normalTex))
        {
            material.NormalMap = normalTex;
        }

        return material;
    }

    private async Task<Geometry> CreateGeometry(GLTFRoot gltf, GLTFPrimitive primitive)
    {
        if (primitive.Attributes == null)
            return new BoxGeometry(1, 1, 1);

        // Read vertex positions
        float[]? positions = null;
        if (primitive.Attributes.TryGetValue("POSITION", out int posAccessorIndex))
        {
            positions = ReadAccessor<float>(gltf, posAccessorIndex);
        }

        // Read normals
        float[]? normals = null;
        if (primitive.Attributes.TryGetValue("NORMAL", out int normAccessorIndex))
        {
            normals = ReadAccessor<float>(gltf, normAccessorIndex);
        }

        // Read UVs
        float[]? uvs = null;
        if (primitive.Attributes.TryGetValue("TEXCOORD_0", out int uvAccessorIndex))
        {
            uvs = ReadAccessor<float>(gltf, uvAccessorIndex);
        }

        // Read indices
        uint[]? indices = null;
        if (primitive.Indices.HasValue)
        {
            var indicesData = ReadAccessor<ushort>(gltf, primitive.Indices.Value);
            if (indicesData != null)
            {
                indices = indicesData.Select(i => (uint)i).ToArray();
            }
        }

        if (positions == null)
            return new BoxGeometry(1, 1, 1);

        // Create geometry
        var geometry = new CustomGeometry(
            positions,
            indices ?? GenerateIndices(positions.Length / 3)
        );

        if (normals != null)
            geometry.Normals = normals;
        else
            geometry.ComputeNormals();

        if (uvs != null)
            geometry.UVs = uvs;

        geometry.ComputeBoundingBox();
        geometry.ComputeBoundingSphere();

        return geometry;
    }

    private T[]? ReadAccessor<T>(GLTFRoot gltf, int accessorIndex) where T : struct
    {
        if (gltf.Accessors == null || accessorIndex >= gltf.Accessors.Length)
            return null;

        var accessor = gltf.Accessors[accessorIndex];
        var bufferView = gltf.BufferViews![accessor.BufferView];
        var buffer = _bufferCache[bufferView.Buffer];

        int componentSize = GetComponentSize(accessor.ComponentType);
        int componentCount = GetComponentCount(accessor.Type);
        int stride = bufferView.ByteStride ?? (componentSize * componentCount);

        var result = new T[accessor.Count * componentCount];
        int offset = bufferView.ByteOffset + (accessor.ByteOffset ?? 0);

        for (int i = 0; i < accessor.Count; i++)
        {
            for (int j = 0; j < componentCount; j++)
            {
                int byteOffset = offset + i * stride + j * componentSize;

                if (typeof(T) == typeof(float))
                {
                    result[i * componentCount + j] = (T)(object)BitConverter.ToSingle(buffer, byteOffset);
                }
                else if (typeof(T) == typeof(ushort))
                {
                    result[i * componentCount + j] = (T)(object)BitConverter.ToUInt16(buffer, byteOffset);
                }
                else if (typeof(T) == typeof(uint))
                {
                    result[i * componentCount + j] = (T)(object)BitConverter.ToUInt32(buffer, byteOffset);
                }
            }
        }

        return result;
    }

    private int GetComponentSize(int componentType) => componentType switch
    {
        5120 => 1, // BYTE
        5121 => 1, // UNSIGNED_BYTE
        5122 => 2, // SHORT
        5123 => 2, // UNSIGNED_SHORT
        5125 => 4, // UNSIGNED_INT
        5126 => 4, // FLOAT
        _ => 4
    };

    private int GetComponentCount(string type) => type switch
    {
        "SCALAR" => 1,
        "VEC2" => 2,
        "VEC3" => 3,
        "VEC4" => 4,
        "MAT2" => 4,
        "MAT3" => 9,
        "MAT4" => 16,
        _ => 1
    };

    private uint[] GenerateIndices(int vertexCount)
    {
        var indices = new uint[vertexCount];
        for (uint i = 0; i < vertexCount; i++)
            indices[i] = i;
        return indices;
    }
}

/// <summary>
/// GLTF scene container
/// </summary>
public class GLTFScene
{
    public Scene Scene { get; set; } = new();
    public List<Extensions.Animation.AnimationClip> Animations { get; set; } = new();
    public List<Camera> Cameras { get; set; } = new();
}

// GLTF JSON structure
internal class GLTFRoot
{
    public GLTFAsset? Asset { get; set; }
    public GLTFSceneInfo[]? Scenes { get; set; }
    public GLTFNode[]? Nodes { get; set; }
    public GLTFMesh[]? Meshes { get; set; }
    public GLTFAccessor[]? Accessors { get; set; }
    public GLTFBufferView[]? BufferViews { get; set; }
    public GLTFBuffer[]? Buffers { get; set; }
    public GLTFMaterial[]? Materials { get; set; }
    public GLTFTexture[]? Textures { get; set; }
    public GLTFImage[]? Images { get; set; }
}

internal class GLTFAsset
{
    public string Version { get; set; } = "2.0";
    public string? Generator { get; set; }
}

internal class GLTFSceneInfo
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
    public int Mode { get; set; } = 4;
}

internal class GLTFAccessor
{
    public int BufferView { get; set; }
    public int? ByteOffset { get; set; }
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
    public GLTFNormalTextureInfo? NormalTexture { get; set; }
}

internal class GLTFPBRMetallicRoughness
{
    public float[]? BaseColorFactor { get; set; }
    public float MetallicFactor { get; set; } = 1.0f;
    public float RoughnessFactor { get; set; } = 1.0f;
    public GLTFTextureInfo? BaseColorTexture { get; set; }
}

internal class GLTFTextureInfo
{
    public int Index { get; set; }
}

internal class GLTFNormalTextureInfo
{
    public int Index { get; set; }
    public float Scale { get; set; } = 1.0f;
}

internal class GLTFTexture
{
    public int? Source { get; set; }
    public int? Sampler { get; set; }
}

internal class GLTFImage
{
    public string? Uri { get; set; }
    public string? MimeType { get; set; }
    public int? BufferView { get; set; }
}
