using Xunit;
using BlazorGL.Core.Textures;

namespace BlazorGL.Tests.Textures;

public class TextureTests
{
    [Fact]
    public void Texture_InitializesWithDefaults()
    {
        var texture = new Texture();

        Assert.NotNull(texture);
        Assert.True(texture.NeedsUpdate);
        Assert.Equal(TextureMinFilter.LinearMipmapLinear, texture.MinFilter);
        Assert.Equal(TextureMagFilter.Linear, texture.MagFilter);
    }

    [Fact]
    public void Texture_CanSetWrapMode()
    {
        var texture = new Texture
        {
            WrapS = TextureWrapMode.Repeat,
            WrapT = TextureWrapMode.ClampToEdge
        };

        Assert.Equal(TextureWrapMode.Repeat, texture.WrapS);
        Assert.Equal(TextureWrapMode.ClampToEdge, texture.WrapT);
    }

    [Fact]
    public void Texture_CanSetFiltering()
    {
        var texture = new Texture
        {
            MinFilter = TextureMinFilter.Nearest,
            MagFilter = TextureMagFilter.Nearest
        };

        Assert.Equal(TextureMinFilter.Nearest, texture.MinFilter);
        Assert.Equal(TextureMagFilter.Nearest, texture.MagFilter);
    }

    [Fact]
    public void Texture_CanDisableMipmaps()
    {
        var texture = new Texture
        {
            GenerateMipmaps = false
        };

        Assert.False(texture.GenerateMipmaps);
    }

    [Fact]
    public void RenderTarget_HasTexture()
    {
        var rt = new RenderTarget(512, 512);

        Assert.Equal(512, rt.Width);
        Assert.Equal(512, rt.Height);
        Assert.NotNull(rt.Texture);
    }

    [Fact]
    public void RenderTarget_CanEnableDepthBuffer()
    {
        var rt = new RenderTarget(256, 256)
        {
            DepthBuffer = true
        };

        Assert.True(rt.DepthBuffer);
    }

    [Fact]
    public void RenderTarget_CanEnableStencilBuffer()
    {
        var rt = new RenderTarget(256, 256)
        {
            StencilBuffer = true
        };

        Assert.True(rt.StencilBuffer);
    }
}
