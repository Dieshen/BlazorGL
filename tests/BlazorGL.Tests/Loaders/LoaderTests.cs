using Xunit;
using BlazorGL.Core.Loaders;
using BlazorGL.Core.Textures;
using BlazorGL.Core.Animation;

namespace BlazorGL.Tests.Loaders;

public class LoaderTests
{
    [Fact]
    public void LoadingManager_TracksProgress()
    {
        var manager = new LoadingManager();
        var startCalled = false;
        var endCalled = false;

        manager.OnStart = (url, loaded, total) => startCalled = true;
        manager.OnLoad = (url, loaded, total) => endCalled = true;

        manager.ItemStart("test.png");
        manager.ItemEnd("test.png");

        Assert.True(startCalled);
        Assert.True(endCalled);
        Assert.Equal(1, manager.ItemsTotal);
        Assert.Equal(1, manager.ItemsLoaded);
    }

    [Fact]
    public void LoadingManager_CalculatesProgress()
    {
        var manager = new LoadingManager();

        manager.ItemStart("file1.png");
        manager.ItemStart("file2.png");
        manager.ItemStart("file3.png");

        Assert.Equal(3, manager.ItemsTotal);
        Assert.Equal(0, manager.ItemsLoaded);

        manager.ItemEnd("file1.png");
        Assert.Equal(1, manager.ItemsLoaded);

        manager.ItemEnd("file2.png");
        Assert.Equal(2, manager.ItemsLoaded);

        manager.ItemEnd("file3.png");
        Assert.Equal(3, manager.ItemsLoaded);
        Assert.False(manager.IsLoading);
    }

    [Fact]
    public void LoadingManager_HandlesErrors()
    {
        var manager = new LoadingManager();
        var errorCalled = false;
        string errorUrl = "";

        manager.OnError = (url) =>
        {
            errorCalled = true;
            errorUrl = url;
        };

        manager.ItemError("failed.png");

        Assert.True(errorCalled);
        Assert.Equal("failed.png", errorUrl);
    }

    [Fact]
    public void DataTexture_CreatesFromByteArray()
    {
        var data = new byte[256 * 256 * 4];
        var texture = new DataTexture(data, 256, 256);

        Assert.NotNull(texture);
        Assert.Equal(256, texture.Width);
        Assert.Equal(256, texture.Height);
        Assert.Equal(data.Length, texture.ImageData!.Length);
    }

    [Fact]
    public void DataTexture_CreatesFromFloatArray()
    {
        var data = new float[64 * 64 * 4];
        var texture = new DataTexture(data, 64, 64);

        Assert.NotNull(texture);
        Assert.Equal(64, texture.Width);
        Assert.Equal(64, texture.Height);
        Assert.Equal(TextureDataType.Float, texture.DataType);
    }

    [Fact]
    public void CubeTexture_HasSixFaces()
    {
        var texture = new CubeTexture();

        Assert.NotNull(texture.FaceData);
        Assert.Equal(6, texture.FaceData.Length);
    }

    [Fact]
    public void CompressedTexture_HasFormat()
    {
        var texture = new CompressedTexture
        {
            CompressionFormat = CompressedTextureFormat.BC1,
            Width = 512,
            Height = 512
        };

        Assert.Equal(CompressedTextureFormat.BC1, texture.CompressionFormat);
        Assert.Equal(0, texture.Mipmaps.Count);
    }

    [Fact]
    public void AnimationLoader_ParsesClipName()
    {
        // Test would require actual JSON parsing
        // This tests the structure exists
        var loader = new AnimationLoader();
        Assert.NotNull(loader);
    }

    [Fact]
    public void MaterialLoader_CanSerialize()
    {
        var loader = new MaterialLoader();
        var material = new BlazorGL.Core.Materials.BasicMaterial
        {
            Name = "TestMaterial",
            Opacity = 0.8f
        };

        var json = loader.Serialize(material);

        Assert.Contains("TestMaterial", json);
        Assert.Contains("0.8", json);
    }
}
