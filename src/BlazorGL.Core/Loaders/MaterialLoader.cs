using BlazorGL.Core.Materials;
using BlazorGL.Core.Textures;
using System.Text.Json;

namespace BlazorGL.Core.Loaders;

/// <summary>
/// Loader for materials from JSON format
/// Supports serialization and deserialization of material properties
/// </summary>
public class MaterialLoader
{
    private readonly LoadingManager? _manager;
    private readonly TextureLoader? _textureLoader;

    public MaterialLoader(LoadingManager? manager = null, TextureLoader? textureLoader = null)
    {
        _manager = manager;
        _textureLoader = textureLoader;
    }

    /// <summary>
    /// Loads materials from JSON
    /// </summary>
    public async Task<Dictionary<string, Material>> LoadAsync(string json)
    {
        var materials = new Dictionary<string, Material>();

        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("materials", out var materialsArray))
            {
                foreach (var materialJson in materialsArray.EnumerateArray())
                {
                    var material = await ParseMaterialAsync(materialJson);
                    if (material != null && materialJson.TryGetProperty("uuid", out var uuid))
                    {
                        materials[uuid.GetString() ?? Guid.NewGuid().ToString()] = material;
                    }
                }
            }

            return materials;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse materials: {ex.Message}", ex);
        }
    }

    private async Task<Material?> ParseMaterialAsync(JsonElement json)
    {
        if (!json.TryGetProperty("type", out var typeElement))
            return null;

        var type = typeElement.GetString();
        Material? material = type switch
        {
            "MeshBasicMaterial" => new BasicMaterial(),
            "MeshPhongMaterial" => new PhongMaterial(),
            "MeshStandardMaterial" => new StandardMaterial(),
            "MeshPhysicalMaterial" => new PhysicalMaterial(),
            "LineBasicMaterial" => new LineBasicMaterial(),
            "PointsMaterial" => new PointsMaterial(),
            _ => null
        };

        if (material == null)
            return null;

        // Parse common properties
        if (json.TryGetProperty("name", out var name))
            material.Name = name.GetString() ?? "";

        if (json.TryGetProperty("opacity", out var opacity))
            material.Opacity = opacity.GetSingle();

        if (json.TryGetProperty("transparent", out var transparent))
            material.Transparent = transparent.GetBoolean();

        if (json.TryGetProperty("depthTest", out var depthTest))
            material.DepthTest = depthTest.GetBoolean();

        if (json.TryGetProperty("depthWrite", out var depthWrite))
            material.DepthWrite = depthWrite.GetBoolean();

        // Parse textures
        if (_textureLoader != null && json.TryGetProperty("map", out var mapUrl))
        {
            var url = mapUrl.GetString();
            if (!string.IsNullOrEmpty(url))
            {
                // Would load texture asynchronously
                // material.Map = await _textureLoader.LoadAsync(url);
            }
        }

        return material;
    }

    /// <summary>
    /// Serializes a material to JSON
    /// </summary>
    public string Serialize(Material material)
    {
        // Simple serialization - would need more complete implementation
        return JsonSerializer.Serialize(new
        {
            type = material.GetType().Name,
            uuid = Guid.NewGuid().ToString(),
            name = material.Name,
            opacity = material.Opacity,
            transparent = material.Transparent,
            depthTest = material.DepthTest,
            depthWrite = material.DepthWrite
        });
    }
}
