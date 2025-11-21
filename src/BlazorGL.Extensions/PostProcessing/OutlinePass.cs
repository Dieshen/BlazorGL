using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Materials;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;
using BlazorGL.Extensions.PostProcessing.Shaders;

namespace BlazorGL.Extensions.PostProcessing;

/// <summary>
/// Outline pass for highlighting selected objects with colored edges
/// Uses depth-based edge detection
/// </summary>
public class OutlinePass : Pass
{
    private RenderPass? _depthPass;
    private ShaderPass? _outlineShaderPass;
    private RenderTarget? _depthTarget;
    private Scene _scene;
    private Camera _camera;

    /// <summary>
    /// Color of the outline
    /// </summary>
    public Vector3 OutlineColor { get; set; } = new Vector3(1f, 1f, 0f); // Yellow

    /// <summary>
    /// Thickness of the outline
    /// </summary>
    public float OutlineThickness { get; set; } = 1.0f;

    /// <summary>
    /// Objects to be outlined (if empty, outlines all objects)
    /// </summary>
    public List<Object3D> SelectedObjects { get; set; } = new();

    private int _width;
    private int _height;

    public OutlinePass(Scene scene, Camera camera, int width, int height)
    {
        _scene = scene;
        _camera = camera;
        _width = width;
        _height = height;
        InitializePasses();
    }

    private void InitializePasses()
    {
        // Create depth render target
        _depthTarget = new RenderTarget(_width, _height)
        {
            DepthBuffer = true,
            StencilBuffer = false
        };

        // Create depth-only render pass
        _depthPass = new RenderPass(_scene, _camera)
        {
            ClearColor = true,
            ClearDepth = true
        };

        // Create outline shader pass
        var outlineMaterial = new ShaderMaterial(
            OutlineShader.VertexShader,
            OutlineShader.FragmentShader
        );
        outlineMaterial.Uniforms["resolution"] = new Vector2(_width, _height);
        outlineMaterial.Uniforms["outlineColor"] = OutlineColor;
        outlineMaterial.Uniforms["outlineThickness"] = OutlineThickness;

        _outlineShaderPass = new ShaderPass(outlineMaterial);
    }

    public override void Render(Renderer renderer, RenderTarget? writeBuffer, RenderTarget? readBuffer)
    {
        if (_depthPass == null || _outlineShaderPass == null || _depthTarget == null)
        {
            return;
        }

        // Update uniforms
        var material = _outlineShaderPass._material;
        material.Uniforms["outlineColor"] = OutlineColor;
        material.Uniforms["outlineThickness"] = OutlineThickness;

        // If specific objects are selected, temporarily hide others
        Dictionary<Object3D, bool>? visibilityState = null;
        if (SelectedObjects.Count > 0)
        {
            visibilityState = SetVisibilityForSelected();
        }

        // Step 1: Render scene to depth buffer
        _depthPass.Render(renderer, _depthTarget, null);

        // Restore visibility
        if (visibilityState != null)
        {
            RestoreVisibility(visibilityState);
        }

        // Step 2: Apply outline shader using depth buffer
        material.Uniforms["tDiffuse"] = readBuffer?.Texture;
        material.Uniforms["tDepth"] = _depthTarget.Texture;

        _outlineShaderPass.Render(renderer, writeBuffer, readBuffer);
    }

    private Dictionary<Object3D, bool> SetVisibilityForSelected()
    {
        var visibilityState = new Dictionary<Object3D, bool>();

        // Store current visibility and hide all objects
        TraverseAndHide(_scene, visibilityState);

        // Show only selected objects
        foreach (var obj in SelectedObjects)
        {
            SetVisibility(obj, true);
        }

        return visibilityState;
    }

    private void TraverseAndHide(Object3D obj, Dictionary<Object3D, bool> visibilityState)
    {
        visibilityState[obj] = obj.Visible;
        obj.Visible = false;

        foreach (var child in obj.Children)
        {
            TraverseAndHide(child, visibilityState);
        }
    }

    private void SetVisibility(Object3D obj, bool visible)
    {
        obj.Visible = visible;

        foreach (var child in obj.Children)
        {
            SetVisibility(child, visible);
        }
    }

    private void RestoreVisibility(Dictionary<Object3D, bool> visibilityState)
    {
        foreach (var kvp in visibilityState)
        {
            kvp.Key.Visible = kvp.Value;
        }
    }

    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;

        // Dispose old target
        _depthTarget?.Dispose();

        // Reinitialize
        InitializePasses();
    }
}
