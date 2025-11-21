using Xunit;
using BlazorGL.Core;
using BlazorGL.Core.Math;
using BlazorGL.Core.Lights;
using BlazorGL.Core.Geometries;
using BlazorGL.Core.Materials;

namespace BlazorGL.Tests.Core;

public class SceneTests
{
    [Fact]
    public void Scene_InitializesEmpty()
    {
        var scene = new Scene();

        Assert.NotNull(scene);
        Assert.Empty(scene.Children);
        Assert.Empty(scene.Lights);
    }

    [Fact]
    public void Scene_CanAddObjects()
    {
        var scene = new Scene();
        var mesh = new Mesh(new BoxGeometry(), new BasicMaterial());

        scene.AddChild(mesh);

        Assert.Contains(mesh, scene.Children);
    }

    [Fact]
    public void Scene_CollectsLights()
    {
        var scene = new Scene();
        var light = new DirectionalLight();

        scene.AddChild(light);

        Assert.Single(scene.Lights);
        Assert.Contains(light, scene.Lights);
    }

    [Fact]
    public void Scene_CollectsNestedLights()
    {
        var scene = new Scene();
        var group = new Object3D();
        var light1 = new PointLight();
        var light2 = new SpotLight();

        group.AddChild(light1);
        group.AddChild(light2);
        scene.AddChild(group);

        Assert.Equal(2, scene.Lights.Count);
    }

    [Fact]
    public void Scene_Background_CanBeSet()
    {
        var scene = new Scene();
        var color = new Color(0.2f, 0.3f, 0.4f);

        scene.Background = color;

        Assert.Equal(color, scene.Background);
    }

    [Fact]
    public void Scene_Update_UpdatesAllChildren()
    {
        var scene = new Scene();
        var obj1 = new Object3D();
        var obj2 = new Object3D();

        scene.AddChild(obj1);
        scene.AddChild(obj2);

        // Should not throw
        scene.Update(0.016f);
    }

    [Fact]
    public void Scene_IsObject3D()
    {
        var scene = new Scene();

        Assert.IsAssignableFrom<Object3D>(scene);
    }
}
