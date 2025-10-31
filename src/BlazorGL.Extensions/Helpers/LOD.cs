using BlazorGL.Core;

namespace BlazorGL.Extensions.Helpers;

/// <summary>
/// Level of Detail helper for optimizing rendering
/// </summary>
public class LOD : Object3D
{
    private List<LODLevel> _levels = new();

    public void AddLevel(Mesh mesh, float distance)
    {
        _levels.Add(new LODLevel { Mesh = mesh, Distance = distance });
        _levels = _levels.OrderBy(l => l.Distance).ToList();
        Add(mesh);
        mesh.Visible = false;
    }

    public void Update(float cameraDistance)
    {
        foreach (var level in _levels)
        {
            level.Mesh.Visible = false;
        }

        for (int i = _levels.Count - 1; i >= 0; i--)
        {
            if (cameraDistance >= _levels[i].Distance)
            {
                _levels[i].Mesh.Visible = true;
                break;
            }
        }
    }
}

internal class LODLevel
{
    public Mesh Mesh { get; set; } = null!;
    public float Distance { get; set; }
}
