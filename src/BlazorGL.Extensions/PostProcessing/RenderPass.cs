using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Pass that renders a scene with a camera
/// </summary>
public class RenderPass : Pass
{
    /// <summary>
    /// Scene to render
    /// </summary>
    public Scene Scene { get; set; }

    /// <summary>
    /// Camera to render with
    /// </summary>
    public Camera Camera { get; set; }

    /// <summary>
    /// Material to override all scene materials with (optional)
    /// </summary>
    public Material? OverrideMaterial { get; set; }

    /// <summary>
    /// Whether to clear color buffer before rendering
    /// </summary>
    public bool ClearColor { get; set; } = true;

    /// <summary>
    /// Whether to clear depth buffer before rendering
    /// </summary>
    public bool ClearDepth { get; set; } = true;

    /// <summary>
    /// Whether to clear stencil buffer before rendering
    /// </summary>
    public bool ClearStencil { get; set; } = false;

    private Dictionary<Mesh, Material?>? _originalMaterials;

    public RenderPass(Scene scene, Camera camera)
    {
        Scene = scene;
        Camera = camera;
    }

    public override void Render(Renderer renderer, RenderTarget? writeBuffer, RenderTarget? readBuffer)
    {
        // Set render target
        renderer.SetRenderTarget(writeBuffer);

        // Clear buffers
        if (ClearColor || ClearDepth || ClearStencil)
        {
            renderer.Context.Clear(ClearColor, ClearDepth, ClearStencil);
        }

        // Override materials if needed
        if (OverrideMaterial != null)
        {
            _originalMaterials = SaveAndOverrideMaterials(Scene, OverrideMaterial);
        }

        // Render scene
        renderer.Render(Scene, Camera);

        // Restore original materials
        if (_originalMaterials != null)
        {
            RestoreMaterials(Scene, _originalMaterials);
            _originalMaterials = null;
        }
    }

    private Dictionary<Mesh, Material?> SaveAndOverrideMaterials(Object3D obj, Material overrideMaterial)
    {
        var originalMaterials = new Dictionary<Mesh, Material?>();
        TraverseAndOverride(obj, overrideMaterial, originalMaterials);
        return originalMaterials;
    }

    private void TraverseAndOverride(Object3D obj, Material overrideMaterial, Dictionary<Mesh, Material?> originalMaterials)
    {
        if (obj is Mesh mesh && mesh.Material != null)
        {
            originalMaterials[mesh] = mesh.Material;
            mesh.Material = overrideMaterial;
        }

        foreach (var child in obj.Children)
        {
            TraverseAndOverride(child, overrideMaterial, originalMaterials);
        }
    }

    private void RestoreMaterials(Object3D obj, Dictionary<Mesh, Material?> originalMaterials)
    {
        TraverseAndRestore(obj, originalMaterials);
    }

    private void TraverseAndRestore(Object3D obj, Dictionary<Mesh, Material?> originalMaterials)
    {
        if (obj is Mesh mesh && originalMaterials.TryGetValue(mesh, out var originalMaterial))
        {
            mesh.Material = originalMaterial;
        }

        foreach (var child in obj.Children)
        {
            TraverseAndRestore(child, originalMaterials);
        }
    }
}
