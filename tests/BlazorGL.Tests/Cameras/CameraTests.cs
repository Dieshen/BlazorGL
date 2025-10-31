using Xunit;
using BlazorGL.Core.Cameras;
using System.Numerics;

namespace BlazorGL.Tests.Cameras;

public class CameraTests
{
    [Fact]
    public void PerspectiveCamera_InitializesWithDefaults()
    {
        // Arrange & Act
        var camera = new PerspectiveCamera();

        // Assert
        Assert.Equal(50f, camera.Fov);
        Assert.Equal(1f, camera.Aspect);
        Assert.Equal(0.1f, camera.Near);
        Assert.Equal(2000f, camera.Far);
    }

    [Theory]
    [InlineData(45, 1.77, 0.5, 1000)]
    [InlineData(60, 1.33, 0.1, 5000)]
    [InlineData(90, 2.0, 1.0, 100)]
    public void PerspectiveCamera_AcceptsCustomParameters(float fov, float aspect, float near, float far)
    {
        // Arrange & Act
        var camera = new PerspectiveCamera(fov, aspect, near, far);

        // Assert
        Assert.Equal(fov, camera.Fov);
        Assert.Equal(aspect, camera.Aspect);
        Assert.Equal(near, camera.Near);
        Assert.Equal(far, camera.Far);
    }

    [Fact]
    public void PerspectiveCamera_UpdateProjectionMatrix_CreatesValidMatrix()
    {
        // Arrange
        var camera = new PerspectiveCamera(60, 1.5f, 0.1f, 1000f);

        // Act
        camera.UpdateProjectionMatrix();

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, camera.ProjectionMatrix);
        // Perspective matrix should have non-zero values
        Assert.NotEqual(0f, camera.ProjectionMatrix.M11);
        Assert.NotEqual(0f, camera.ProjectionMatrix.M22);
    }

    [Fact]
    public void OrthographicCamera_InitializesWithDefaults()
    {
        // Arrange & Act
        var camera = new OrthographicCamera(-10, 10, 10, -10);

        // Assert
        Assert.Equal(-10f, camera.Left);
        Assert.Equal(10f, camera.Right);
        Assert.Equal(10f, camera.Top);
        Assert.Equal(-10f, camera.Bottom);
    }

    [Fact]
    public void OrthographicCamera_UpdateProjectionMatrix_CreatesValidMatrix()
    {
        // Arrange
        var camera = new OrthographicCamera(-100, 100, 100, -100, 0.1f, 1000f);

        // Act
        camera.UpdateProjectionMatrix();

        // Assert
        Assert.NotEqual(Matrix4x4.Identity, camera.ProjectionMatrix);
    }

    [Fact]
    public void Camera_LookAt_UpdatesViewMatrix()
    {
        // Arrange
        var camera = new PerspectiveCamera();
        camera.Position = new Vector3(0, 0, 10);

        // Act
        camera.LookAt(Vector3.Zero);

        // Assert
        // Camera should be looking at origin
        Assert.NotEqual(Matrix4x4.Identity, camera.ViewMatrix);
    }

    [Fact]
    public void Camera_IsObject3D()
    {
        // Arrange & Act
        var camera = new PerspectiveCamera();

        // Assert
        Assert.IsAssignableFrom<Object3D>(camera);
    }

    [Fact]
    public void StereoCamera_CreatesLeftAndRightCameras()
    {
        // Arrange & Act
        var camera = new StereoCamera();

        // Assert
        Assert.NotNull(camera.CameraL);
        Assert.NotNull(camera.CameraR);
    }

    [Fact]
    public void ArrayCamera_HoldsMultipleCameras()
    {
        // Arrange
        var cam1 = new PerspectiveCamera();
        var cam2 = new PerspectiveCamera();
        var cameras = new Camera[] { cam1, cam2 };

        // Act
        var arrayCamera = new ArrayCamera(cameras);

        // Assert
        Assert.Equal(2, arrayCamera.Cameras.Length);
        Assert.Contains(cam1, arrayCamera.Cameras);
        Assert.Contains(cam2, arrayCamera.Cameras);
    }
}
