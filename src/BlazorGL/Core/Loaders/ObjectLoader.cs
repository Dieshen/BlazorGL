using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;
using System.Text.Json;

namespace BlazorGL.Core.Loaders;

/// <summary>
/// Loader for complete 3D objects and scenes from JSON format
/// Supports hierarchical object structures with geometries and materials
/// </summary>
public class ObjectLoader
{
    private readonly LoadingManager? _manager;
    private readonly MaterialLoader _materialLoader;

    public ObjectLoader(LoadingManager? manager = null)
    {
        _manager = manager;
        _materialLoader = new MaterialLoader(manager);
    }

    /// <summary>
    /// Loads an object hierarchy from JSON
    /// </summary>
    public async Task<Object3D?> LoadAsync(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Load materials first
            Dictionary<string, Material> materials = new();
            if (root.TryGetProperty("materials", out var materialsJson))
            {
                materials = await _materialLoader.LoadAsync(materialsJson.GetRawText());
            }

            // Load geometries
            Dictionary<string, Geometry> geometries = new();
            if (root.TryGetProperty("geometries", out var geometriesJson))
            {
                geometries = ParseGeometries(geometriesJson);
            }

            // Load object hierarchy
            if (root.TryGetProperty("object", out var objectJson))
            {
                return ParseObject(objectJson, geometries, materials);
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load object: {ex.Message}", ex);
        }
    }

    private Dictionary<string, Geometry> ParseGeometries(JsonElement json)
    {
        var geometries = new Dictionary<string, Geometry>();

        if (json.ValueKind == JsonValueKind.Array)
        {
            foreach (var geomJson in json.EnumerateArray())
            {
                if (geomJson.TryGetProperty("uuid", out var uuid))
                {
                    var geometry = ParseGeometry(geomJson);
                    if (geometry != null)
                    {
                        geometries[uuid.GetString() ?? Guid.NewGuid().ToString()] = geometry;
                    }
                }
            }
        }

        return geometries;
    }

    private Geometry? ParseGeometry(JsonElement json)
    {
        if (!json.TryGetProperty("type", out var typeElement))
            return null;

        var type = typeElement.GetString();

        return type switch
        {
            "BoxGeometry" => new BoxGeometry(1, 1, 1),
            "SphereGeometry" => new SphereGeometry(1, 32, 32),
            "PlaneGeometry" => new PlaneGeometry(1, 1),
            "CylinderGeometry" => new CylinderGeometry(1, 1, 1, 32),
            // Add more geometry types as needed
            _ => null
        };
    }

    private Object3D? ParseObject(JsonElement json, Dictionary<string, Geometry> geometries, Dictionary<string, Material> materials)
    {
        if (!json.TryGetProperty("type", out var typeElement))
            return null;

        var type = typeElement.GetString();
        Object3D? obj = null;

        // Create object based on type
        if (type == "Mesh" || type == "SkinnedMesh")
        {
            // Get geometry and material
            Geometry? geometry = null;
            Material? material = null;

            if (json.TryGetProperty("geometry", out var geomUuid))
            {
                var uuid = geomUuid.GetString();
                if (uuid != null && geometries.ContainsKey(uuid))
                {
                    geometry = geometries[uuid];
                }
            }

            if (json.TryGetProperty("material", out var matUuid))
            {
                var uuid = matUuid.GetString();
                if (uuid != null && materials.ContainsKey(uuid))
                {
                    material = materials[uuid];
                }
            }

            if (geometry != null && material != null)
            {
                if (type == "SkinnedMesh")
                {
                    obj = new SkinnedMesh(geometry, material);
                }
                else
                {
                    obj = new Mesh(geometry, material);
                }
            }
        }
        else
        {
            obj = new Object3D();
        }

        if (obj == null)
            return null;

        // Parse common properties
        if (json.TryGetProperty("name", out var name))
            obj.Name = name.GetString() ?? "";

        if (json.TryGetProperty("position", out var position))
            obj.Position = ParseVector3(position);

        if (json.TryGetProperty("rotation", out var rotation))
            obj.Rotation = ParseVector3(rotation);

        if (json.TryGetProperty("scale", out var scale))
            obj.Scale = ParseVector3(scale);

        // Parse children recursively
        if (json.TryGetProperty("children", out var children))
        {
            foreach (var childJson in children.EnumerateArray())
            {
                var child = ParseObject(childJson, geometries, materials);
                if (child != null)
                {
                    obj.AddChild(child);
                }
            }
        }

        return obj;
    }

    private Vector3 ParseVector3(JsonElement json)
    {
        if (json.ValueKind == JsonValueKind.Array)
        {
            var array = json.EnumerateArray().ToArray();
            if (array.Length >= 3)
            {
                return new Vector3(
                    array[0].GetSingle(),
                    array[1].GetSingle(),
                    array[2].GetSingle()
                );
            }
        }
        else if (json.ValueKind == JsonValueKind.Object)
        {
            return new Vector3(
                json.GetProperty("x").GetSingle(),
                json.GetProperty("y").GetSingle(),
                json.GetProperty("z").GetSingle()
            );
        }

        return Vector3.Zero;
    }
}
