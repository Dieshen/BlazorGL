using BlazorGL.Core;
using BlazorGL.Core.Cameras;
using BlazorGL.Extensions.PostProcessing;
using Xunit;

namespace BlazorGL.Tests.PostProcessing;

public class RenderPassTests
{
    [Fact]
    public void RenderPass_Initialization_Success()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.33f, 0.1f, 1000f);

        // Act
        var renderPass = new RenderPass(scene, camera);

        // Assert
        Assert.NotNull(renderPass);
        Assert.Same(scene, renderPass.Scene);
        Assert.Same(camera, renderPass.Camera);
        Assert.True(renderPass.ClearColor);
        Assert.True(renderPass.ClearDepth);
        Assert.False(renderPass.ClearStencil);
        Assert.Null(renderPass.OverrideMaterial);
    }

    [Fact]
    public void RenderPass_Properties_CanBeModified()
    {
        // Arrange
        var scene = new Scene();
        var camera = new PerspectiveCamera(75, 1.33f, 0.1f, 1000f);
        var renderPass = new RenderPass(scene, camera);

        // Act
        renderPass.ClearColor = false;
        renderPass.ClearDepth = false;
        renderPass.ClearStencil = true;

        // Assert
        Assert.False(renderPass.ClearColor);
        Assert.False(renderPass.ClearDepth);
        Assert.True(renderPass.ClearStencil);
    }
}
