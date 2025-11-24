using System.Numerics;
using BlazorGL.Core.Cameras;
using BlazorGL.Core.Rendering;
using BlazorGL.Core.Textures;

namespace BlazorGL.Core.Lights;

/// <summary>
/// Cascaded Shadow Maps implementation for DirectionalLight
/// Eliminates perspective aliasing by using multiple shadow maps at different distances
/// </summary>
public class DirectionalLightCSM : IDisposable
{
    /// <summary>
    /// Cascade data structure
    /// </summary>
    public class Cascade
    {
        /// <summary>
        /// Shadow map render target for this cascade
        /// </summary>
        public RenderTarget ShadowMap { get; set; } = null!;

        /// <summary>
        /// Shadow camera for this cascade
        /// </summary>
        public OrthographicCamera ShadowCamera { get; set; } = null!;

        /// <summary>
        /// Split distance for this cascade (view space)
        /// </summary>
        public float SplitDistance { get; set; }

        /// <summary>
        /// View-projection matrix for this cascade
        /// </summary>
        public Matrix4x4 ViewProjectionMatrix { get; set; }

        /// <summary>
        /// Bounding box min in light space
        /// </summary>
        public Vector3 BoundsMin { get; set; }

        /// <summary>
        /// Bounding box max in light space
        /// </summary>
        public Vector3 BoundsMax { get; set; }
    }

    private readonly DirectionalLight _light;
    private readonly List<Cascade> _cascades = new();
    private float[] _splitDistances = Array.Empty<float>();

    /// <summary>
    /// The directional light this CSM belongs to
    /// </summary>
    public DirectionalLight Light => _light;

    /// <summary>
    /// Number of cascade levels (typically 2-4)
    /// </summary>
    public int CascadeCount { get; set; } = 3;

    /// <summary>
    /// Maximum distance for shadow rendering
    /// </summary>
    public float MaxDistance { get; set; } = 1000f;

    /// <summary>
    /// Lambda factor for PSSM (Practical Split Scheme Method)
    /// 0.0 = uniform split, 1.0 = logarithmic split, 0.5 = balanced
    /// </summary>
    public float Lambda { get; set; } = 0.5f;

    /// <summary>
    /// Shadow map resolution per cascade
    /// </summary>
    public int CascadeResolution { get; set; } = 1024;

    /// <summary>
    /// Enable cascade blending for smooth transitions
    /// </summary>
    public bool EnableCascadeBlending { get; set; } = true;

    /// <summary>
    /// Blend range as fraction of cascade distance (0-1)
    /// </summary>
    public float BlendRange { get; set; } = 0.1f;

    /// <summary>
    /// Get all cascades
    /// </summary>
    public IReadOnlyList<Cascade> Cascades => _cascades;

    /// <summary>
    /// Get cascade split distances
    /// </summary>
    public IReadOnlyList<float> SplitDistances => _splitDistances;

    public DirectionalLightCSM(DirectionalLight light, Camera camera)
    {
        _light = light;
        InitializeCascades();
        UpdateCascades(camera);
    }

    /// <summary>
    /// Initialize cascade render targets and cameras
    /// </summary>
    private void InitializeCascades()
    {
        _cascades.Clear();

        for (int i = 0; i < CascadeCount; i++)
        {
            var cascade = new Cascade
            {
                ShadowMap = new RenderTarget(CascadeResolution, CascadeResolution)
                {
                    DepthBuffer = true,
                    StencilBuffer = false
                },
                ShadowCamera = new OrthographicCamera(-10, 10, 10, -10, -100, 100)
            };

            _cascades.Add(cascade);
        }
    }

    /// <summary>
    /// Update all cascades based on camera position and frustum
    /// Should be called each frame before rendering
    /// </summary>
    public void UpdateCascades(Camera camera)
    {
        // Ensure cascades are initialized
        if (_cascades.Count != CascadeCount)
        {
            InitializeCascades();
        }

        // Calculate split distances
        _splitDistances = CalculateCascadeSplits(camera);

        // Update each cascade
        float previousSplit = _splitDistances[0];
        for (int i = 0; i < CascadeCount; i++)
        {
            float nearDist = previousSplit;
            float farDist = _splitDistances[i + 1];

            UpdateCascade(_cascades[i], camera, nearDist, farDist);

            previousSplit = farDist;
        }
    }

    /// <summary>
    /// Calculate cascade split distances using PSSM (Practical Split Scheme Method)
    /// Blends between logarithmic and uniform splitting based on lambda
    /// </summary>
    private float[] CalculateCascadeSplits(Camera camera)
    {
        // Get near plane from camera type
        float near = 0.1f;
        if (camera is PerspectiveCamera perspCamera)
        {
            near = perspCamera.Near;
        }
        else if (camera is OrthographicCamera orthoCamera)
        {
            near = orthoCamera.Near;
        }

        float far = MaxDistance;
        float[] splits = new float[CascadeCount + 1];

        splits[0] = near;
        splits[CascadeCount] = far;

        // Calculate intermediate splits using PSSM
        for (int i = 1; i < CascadeCount; i++)
        {
            float t = (float)i / CascadeCount;

            // Logarithmic split (good for distant detail)
            float logSplit = near * MathF.Pow(far / near, t);

            // Uniform split (good for near detail)
            float uniformSplit = near + (far - near) * t;

            // Blend based on lambda
            splits[i] = Lambda * logSplit + (1.0f - Lambda) * uniformSplit;
        }

        return splits;
    }

    /// <summary>
    /// Update a single cascade
    /// </summary>
    private void UpdateCascade(Cascade cascade, Camera viewCamera, float nearDist, float farDist)
    {
        cascade.SplitDistance = farDist;

        // Get frustum corners for this cascade
        Vector3[] frustumCorners = GetFrustumCorners(viewCamera, nearDist, farDist);

        // Fit shadow camera to frustum
        FitShadowCameraToFrustum(cascade.ShadowCamera, viewCamera, frustumCorners);

        // Update view-projection matrix
        cascade.ShadowCamera.UpdateWorldMatrix(true);
        cascade.ViewProjectionMatrix = cascade.ShadowCamera.ViewMatrix * cascade.ShadowCamera.ProjectionMatrix;
    }

    /// <summary>
    /// Get the 8 corners of the view frustum for a given near and far distance
    /// </summary>
    private Vector3[] GetFrustumCorners(Camera camera, float nearDist, float farDist)
    {
        Vector3[] corners = new Vector3[8];

        // Get camera parameters
        if (camera is PerspectiveCamera perspCamera)
        {
            float aspect = perspCamera.Aspect;
            float fov = perspCamera.Fov;
            float tanHalfFov = MathF.Tan(fov * 0.5f * MathF.PI / 180.0f);

            // Calculate frustum dimensions at near and far planes
            float nearHeight = 2.0f * tanHalfFov * nearDist;
            float nearWidth = nearHeight * aspect;
            float farHeight = 2.0f * tanHalfFov * farDist;
            float farWidth = farHeight * aspect;

            // Near plane corners (view space)
            corners[0] = new Vector3(-nearWidth * 0.5f, -nearHeight * 0.5f, -nearDist);
            corners[1] = new Vector3(nearWidth * 0.5f, -nearHeight * 0.5f, -nearDist);
            corners[2] = new Vector3(nearWidth * 0.5f, nearHeight * 0.5f, -nearDist);
            corners[3] = new Vector3(-nearWidth * 0.5f, nearHeight * 0.5f, -nearDist);

            // Far plane corners (view space)
            corners[4] = new Vector3(-farWidth * 0.5f, -farHeight * 0.5f, -farDist);
            corners[5] = new Vector3(farWidth * 0.5f, -farHeight * 0.5f, -farDist);
            corners[6] = new Vector3(farWidth * 0.5f, farHeight * 0.5f, -farDist);
            corners[7] = new Vector3(-farWidth * 0.5f, farHeight * 0.5f, -farDist);
        }
        else
        {
            // For orthographic cameras (simplified)
            float size = 10.0f;
            corners[0] = new Vector3(-size, -size, -nearDist);
            corners[1] = new Vector3(size, -size, -nearDist);
            corners[2] = new Vector3(size, size, -nearDist);
            corners[3] = new Vector3(-size, size, -nearDist);
            corners[4] = new Vector3(-size, -size, -farDist);
            corners[5] = new Vector3(size, -size, -farDist);
            corners[6] = new Vector3(size, size, -farDist);
            corners[7] = new Vector3(-size, size, -farDist);
        }

        // Transform corners from view space to world space
        Matrix4x4 viewToWorld = camera.WorldMatrix;
        for (int i = 0; i < 8; i++)
        {
            corners[i] = Vector3.Transform(corners[i], viewToWorld);
        }

        return corners;
    }

    /// <summary>
    /// Fit the shadow camera's orthographic bounds to tightly encompass the frustum
    /// This minimizes shadow map wastage and maximizes effective resolution
    /// </summary>
    private void FitShadowCameraToFrustum(OrthographicCamera shadowCamera, Camera viewCamera, Vector3[] frustumCorners)
    {
        // Position shadow camera to look along light direction
        Vector3 lightDir = Vector3.Normalize(_light.Direction);

        // Calculate frustum center in world space
        Vector3 frustumCenter = Vector3.Zero;
        for (int i = 0; i < 8; i++)
        {
            frustumCenter += frustumCorners[i];
        }
        frustumCenter /= 8.0f;

        // Position shadow camera looking at frustum center
        Vector3 cameraPosition = frustumCenter - lightDir * 1000f; // Move far back along light direction
        shadowCamera.Position = cameraPosition;

        // Make camera look in light direction
        shadowCamera.LookAt(frustumCenter, Vector3.UnitY);
        shadowCamera.UpdateWorldMatrix(true);

        // Transform frustum corners to light space
        Matrix4x4 lightView = shadowCamera.ViewMatrix;
        Vector3[] lightSpaceCorners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            lightSpaceCorners[i] = Vector3.Transform(frustumCorners[i], lightView);
        }

        // Calculate AABB in light space
        Vector3 min = lightSpaceCorners[0];
        Vector3 max = lightSpaceCorners[0];
        for (int i = 1; i < 8; i++)
        {
            min = Vector3.Min(min, lightSpaceCorners[i]);
            max = Vector3.Max(max, lightSpaceCorners[i]);
        }

        // Add padding to prevent edge clipping
        float padding = (max.Z - min.Z) * 0.1f;
        min -= new Vector3(padding, padding, padding);
        max += new Vector3(padding, padding, padding);

        // Set orthographic camera bounds
        shadowCamera.Left = min.X;
        shadowCamera.Right = max.X;
        shadowCamera.Bottom = min.Y;
        shadowCamera.Top = max.Y;
        shadowCamera.Near = -max.Z; // Note: inverted Z
        shadowCamera.Far = -min.Z;

        shadowCamera.UpdateProjectionMatrix();
    }

    /// <summary>
    /// Render all cascades
    /// </summary>
    public void RenderCascades(Renderer renderer, Scene scene)
    {
        foreach (var cascade in _cascades)
        {
            // Set render target to this cascade's shadow map
            renderer.SetRenderTarget(cascade.ShadowMap);

            // Clear depth buffer
            renderer.Context.Clear(false, true, false);

            // Render scene from this cascade's camera perspective
            // This would be integrated into the renderer's shadow rendering
            // For now, this is a placeholder showing the structure
        }

        // Restore default framebuffer
        renderer.SetRenderTarget(null);
    }

    /// <summary>
    /// Get the appropriate cascade index for a given view-space Z distance
    /// </summary>
    public int GetCascadeIndex(float viewZ)
    {
        float absViewZ = MathF.Abs(viewZ);
        for (int i = 0; i < CascadeCount - 1; i++)
        {
            if (absViewZ < _splitDistances[i + 1])
            {
                return i;
            }
        }
        return CascadeCount - 1;
    }

    /// <summary>
    /// Calculate blend factor for cascade transitions
    /// </summary>
    public float GetCascadeBlendFactor(float viewZ, int cascadeIndex)
    {
        if (!EnableCascadeBlending || cascadeIndex >= CascadeCount - 1)
        {
            return 0.0f;
        }

        float absViewZ = MathF.Abs(viewZ);
        float cascadeEnd = _splitDistances[cascadeIndex + 1];
        float blendStart = cascadeEnd * (1.0f - BlendRange);

        if (absViewZ < blendStart)
        {
            return 0.0f;
        }

        return (absViewZ - blendStart) / (cascadeEnd - blendStart);
    }

    public void Dispose()
    {
        foreach (var cascade in _cascades)
        {
            cascade.ShadowMap?.Dispose();
        }
        _cascades.Clear();
    }
}
