using BlazorGL.Core;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;
using System.Numerics;

namespace BlazorGL.Loaders;

/// <summary>
/// Loads Wavefront OBJ 3D models with material (.mtl) support
/// </summary>
public class OBJLoader
{
    private string? _baseUrl;

    /// <summary>
    /// Loads an OBJ file from a URL
    /// </summary>
    public async Task<Group> LoadAsync(string url)
    {
        using var httpClient = new HttpClient();
        var data = await httpClient.GetStringAsync(url);

        // Extract base URL for loading materials and textures
        int lastSlash = url.LastIndexOf('/');
        _baseUrl = lastSlash > 0 ? url.Substring(0, lastSlash + 1) : "";

        return await ParseAsync(data);
    }

    /// <summary>
    /// Loads an OBJ file from string content
    /// </summary>
    public async Task<Group> LoadFromStringAsync(string content, string? baseUrl = null)
    {
        _baseUrl = baseUrl;
        return await ParseAsync(content);
    }

    private async Task<Group> ParseAsync(string content)
    {
        var positions = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();

        var objects = new Dictionary<string, OBJObject>();
        OBJObject? currentObject = null;
        OBJGroup? currentGroup = null;
        string? currentMaterialName = null;
        var materials = new Dictionary<string, Material>();

        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "v": // Vertex position
                    if (parts.Length >= 4)
                    {
                        positions.Add(new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        ));
                    }
                    break;

                case "vn": // Vertex normal
                    if (parts.Length >= 4)
                    {
                        normals.Add(new Vector3(
                            float.Parse(parts[1]),
                            float.Parse(parts[2]),
                            float.Parse(parts[3])
                        ));
                    }
                    break;

                case "vt": // Texture coordinate
                    if (parts.Length >= 3)
                    {
                        uvs.Add(new Vector2(
                            float.Parse(parts[1]),
                            float.Parse(parts[2])
                        ));
                    }
                    break;

                case "f": // Face
                    if (currentObject == null)
                    {
                        currentObject = new OBJObject { Name = "default" };
                        objects["default"] = currentObject;
                    }

                    if (currentGroup == null)
                    {
                        currentGroup = new OBJGroup { MaterialName = currentMaterialName };
                        currentObject.Groups.Add(currentGroup);
                    }

                    ParseFace(parts, currentGroup, positions.Count, normals.Count, uvs.Count);
                    break;

                case "o": // Object
                case "g": // Group
                    if (parts.Length >= 2)
                    {
                        string name = string.Join(" ", parts.Skip(1));
                        if (!objects.TryGetValue(name, out currentObject))
                        {
                            currentObject = new OBJObject { Name = name };
                            objects[name] = currentObject;
                        }
                        currentGroup = null;
                    }
                    break;

                case "usemtl": // Use material
                    if (parts.Length >= 2)
                    {
                        currentMaterialName = parts[1];
                        currentGroup = null; // Start new group for new material
                    }
                    break;

                case "mtllib": // Material library
                    if (parts.Length >= 2 && !string.IsNullOrEmpty(_baseUrl))
                    {
                        string mtlFile = parts[1];
                        var loadedMaterials = await LoadMTLAsync(_baseUrl + mtlFile);
                        foreach (var kvp in loadedMaterials)
                        {
                            materials[kvp.Key] = kvp.Value;
                        }
                    }
                    break;
            }
        }

        // Build scene
        var rootGroup = new Group { Name = "OBJ_Root" };

        foreach (var obj in objects.Values)
        {
            var objGroup = new Group { Name = obj.Name };

            foreach (var group in obj.Groups)
            {
                var geometry = BuildGeometry(group, positions, normals, uvs);

                Material material;
                if (group.MaterialName != null && materials.TryGetValue(group.MaterialName, out var mat))
                {
                    material = mat;
                }
                else
                {
                    material = new StandardMaterial
                    {
                        Color = Core.Math.Color.White,
                        Roughness = 0.5f
                    };
                }

                var mesh = new Mesh
                {
                    Geometry = geometry,
                    Material = material
                };

                objGroup.Add(mesh);
            }

            rootGroup.Add(objGroup);
        }

        return rootGroup;
    }

    private void ParseFace(string[] parts, OBJGroup group, int posCount, int normCount, int uvCount)
    {
        var faceVertices = new List<OBJVertex>();

        for (int i = 1; i < parts.Length; i++)
        {
            var indices = parts[i].Split('/');
            var vertex = new OBJVertex();

            if (indices.Length >= 1 && !string.IsNullOrEmpty(indices[0]))
            {
                int idx = int.Parse(indices[0]);
                vertex.PositionIndex = idx < 0 ? posCount + idx : idx - 1;
            }

            if (indices.Length >= 2 && !string.IsNullOrEmpty(indices[1]))
            {
                int idx = int.Parse(indices[1]);
                vertex.UVIndex = idx < 0 ? uvCount + idx : idx - 1;
            }

            if (indices.Length >= 3 && !string.IsNullOrEmpty(indices[2]))
            {
                int idx = int.Parse(indices[2]);
                vertex.NormalIndex = idx < 0 ? normCount + idx : idx - 1;
            }

            faceVertices.Add(vertex);
        }

        // Triangulate face (assumes convex polygons)
        for (int i = 1; i < faceVertices.Count - 1; i++)
        {
            group.Vertices.Add(faceVertices[0]);
            group.Vertices.Add(faceVertices[i]);
            group.Vertices.Add(faceVertices[i + 1]);
        }
    }

    private Geometry BuildGeometry(OBJGroup group, List<Vector3> positions, List<Vector3> normals, List<Vector2> uvs)
    {
        var vertexData = new List<float>();
        var normalData = new List<float>();
        var uvData = new List<float>();
        var indexData = new List<uint>();
        var vertexMap = new Dictionary<string, uint>();

        foreach (var vertex in group.Vertices)
        {
            string key = $"{vertex.PositionIndex}_{vertex.NormalIndex}_{vertex.UVIndex}";

            if (!vertexMap.TryGetValue(key, out uint index))
            {
                index = (uint)(vertexData.Count / 3);
                vertexMap[key] = index;

                // Position
                var pos = positions[vertex.PositionIndex];
                vertexData.Add(pos.X);
                vertexData.Add(pos.Y);
                vertexData.Add(pos.Z);

                // Normal
                if (vertex.NormalIndex >= 0 && vertex.NormalIndex < normals.Count)
                {
                    var norm = normals[vertex.NormalIndex];
                    normalData.Add(norm.X);
                    normalData.Add(norm.Y);
                    normalData.Add(norm.Z);
                }
                else
                {
                    normalData.Add(0);
                    normalData.Add(1);
                    normalData.Add(0);
                }

                // UV
                if (vertex.UVIndex >= 0 && vertex.UVIndex < uvs.Count)
                {
                    var uv = uvs[vertex.UVIndex];
                    uvData.Add(uv.X);
                    uvData.Add(1.0f - uv.Y); // Flip V coordinate
                }
                else
                {
                    uvData.Add(0);
                    uvData.Add(0);
                }
            }

            indexData.Add(index);
        }

        var geometry = new CustomGeometry(vertexData.ToArray(), indexData.ToArray());
        geometry.Normals = normalData.ToArray();
        geometry.UVs = uvData.ToArray();

        // If no normals were provided, compute them
        if (normals.Count == 0)
        {
            geometry.ComputeNormals();
        }

        geometry.ComputeBoundingBox();
        geometry.ComputeBoundingSphere();

        return geometry;
    }

    private async Task<Dictionary<string, Material>> LoadMTLAsync(string url)
    {
        var materials = new Dictionary<string, Material>();

        try
        {
            using var httpClient = new HttpClient();
            var content = await httpClient.GetStringAsync(url);

            StandardMaterial? currentMaterial = null;
            string? currentName = null;

            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                    continue;

                var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "newmtl":
                        if (currentMaterial != null && currentName != null)
                        {
                            materials[currentName] = currentMaterial;
                        }
                        currentName = parts.Length > 1 ? parts[1] : "default";
                        currentMaterial = new StandardMaterial();
                        break;

                    case "Kd": // Diffuse color
                        if (currentMaterial != null && parts.Length >= 4)
                        {
                            currentMaterial.Color = new Core.Math.Color(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])
                            );
                        }
                        break;

                    case "Ns": // Shininess (convert to roughness)
                        if (currentMaterial != null && parts.Length >= 2)
                        {
                            float shininess = float.Parse(parts[1]);
                            currentMaterial.Roughness = 1.0f - (shininess / 1000.0f);
                        }
                        break;

                    case "d": // Dissolve (opacity)
                    case "Tr": // Transparency
                        if (currentMaterial != null && parts.Length >= 2)
                        {
                            currentMaterial.Opacity = float.Parse(parts[1]);
                            currentMaterial.Transparent = currentMaterial.Opacity < 1.0f;
                        }
                        break;

                    case "map_Kd": // Diffuse texture
                        if (currentMaterial != null && parts.Length >= 2 && !string.IsNullOrEmpty(_baseUrl))
                        {
                            string texturePath = parts[1];
                            try
                            {
                                currentMaterial.Map = await TextureLoader.LoadAsync(_baseUrl + texturePath);
                            }
                            catch
                            {
                                // Texture loading failed, continue without it
                            }
                        }
                        break;
                }
            }

            if (currentMaterial != null && currentName != null)
            {
                materials[currentName] = currentMaterial;
            }
        }
        catch
        {
            // MTL file loading failed, return empty materials
        }

        return materials;
    }
}

internal class OBJObject
{
    public string Name { get; set; } = "";
    public List<OBJGroup> Groups { get; set; } = new();
}

internal class OBJGroup
{
    public string? MaterialName { get; set; }
    public List<OBJVertex> Vertices { get; set; } = new();
}

internal struct OBJVertex
{
    public int PositionIndex;
    public int NormalIndex;
    public int UVIndex;
}
