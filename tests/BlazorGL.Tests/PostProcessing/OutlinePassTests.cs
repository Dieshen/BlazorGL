using System.Numerics;
using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Tests.PostProcessing;

public class OutlinePassTests
{
    [Fact]
    public void OutlinePass_Initialization_Success()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.33f, 0.1f, 1000f);
        int width = 800;
        int height = 600;

        // Act
        var outlinePass = new OutlinePass(scene, camera, width, height);

        // Assert
        Assert.NotNull(outlinePass);
        Assert.Equal(new Vector3(1f, 1f, 0f), outlinePass.OutlineColor);
        Assert.Equal(1.0f, outlinePass.OutlineThickness);
        Assert.Empty(outlinePass.SelectedObjects);
    }

    [Fact]
    public void OutlinePass_SetSize_UpdatesResolution()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.33f, 0.1f, 1000f);
        var outlinePass = new OutlinePass(scene, camera, 800, 600);

        // Act
        outlinePass.SetSize(1024, 768);

        // Assert - no exception thrown
        Assert.NotNull(outlinePass);
    }

    [Fact]
    public void OutlinePass_Properties_CanBeModified()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.33f, 0.1f, 1000f);
        var outlinePass = new OutlinePass(scene, camera, 800, 600);

        // Act
        outlinePass.OutlineColor = new Vector3(1f, 0f, 0f); // Red
        outlinePass.OutlineThickness = 2.0f;

        // Assert
        Assert.Equal(new Vector3(1f, 0f, 0f), outlinePass.OutlineColor);
        Assert.Equal(2.0f, outlinePass.OutlineThickness);
    }

    [Fact]
    public void OutlinePass_SelectedObjects_CanBeManaged()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.33f, 0.1f, 1000f);
        var outlinePass = new OutlinePass(scene, camera, 800, 600);
        var mesh = new Mesh();

        // Act
        outlinePass.SelectedObjects.Add(mesh);

        // Assert
        Assert.Single(outlinePass.SelectedObjects);
        Assert.Contains(mesh, outlinePass.SelectedObjects);
    }
}
