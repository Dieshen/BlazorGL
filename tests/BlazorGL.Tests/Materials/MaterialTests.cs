using Xunit;
using BlazorGL.Core.Materials;

namespace BlazorGL.Tests.Materials;

public class MaterialTests
{
    [Fact]
    public void BasicMaterial_HasDefaultValues()
    {
        // Arrange & Act
        var material = new BasicMaterial();

        // Assert
        Assert.NotNull(material);
        Assert.Equal(1.0f, material.Opacity);
        Assert.False(material.Transparent);
        Assert.True(material.DepthTest);
        Assert.True(material.DepthWrite);
    }

    [Fact]
    public void PhongMaterial_InitializesShinyness()
    {
        // Arrange & Act
        var material = new PhongMaterial();

        // Assert
        Assert.Equal(30.0f, material.Shininess);
    }

    [Fact]
    public void StandardMaterial_IsPBR()
    {
        // Arrange & Act
        var material = new StandardMaterial();

        // Assert
        Assert.InRange(material.Metalness, 0f, 1f);
        Assert.InRange(material.Roughness, 0f, 1f);
    }

    [Fact]
    public void Material_Opacity_AcceptsValidRange()
    {
        // Arrange
        var material = new BasicMaterial();

        // Act
        material.Opacity = 0.5f;

        // Assert
        Assert.Equal(0.5f, material.Opacity);
    }

    [Fact]
    public void Material_Transparent_CanBeSet()
    {
        // Arrange
        var material = new BasicMaterial();

        // Act
        material.Transparent = true;

        // Assert
        Assert.True(material.Transparent);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Material_DepthSettings_CanBeConfigured(bool depthTest, bool depthWrite)
    {
        // Arrange
        var material = new BasicMaterial();

        // Act
        material.DepthTest = depthTest;
        material.DepthWrite = depthWrite;

        // Assert
        Assert.Equal(depthTest, material.DepthTest);
        Assert.Equal(depthWrite, material.DepthWrite);
    }

    [Fact]
    public void LineBasicMaterial_HasLineWidth()
    {
        // Arrange & Act
        var material = new LineBasicMaterial();

        // Assert
        Assert.Equal(1.0f, material.LineWidth);
    }

    [Fact]
    public void PointsMaterial_HasSize()
    {
        // Arrange & Act
        var material = new PointsMaterial();

        // Assert
        Assert.Equal(1.0f, material.Size);
    }

    [Fact]
    public void Material_Name_CanBeSet()
    {
        // Arrange
        var material = new BasicMaterial();

        // Act
        material.Name = "TestMaterial";

        // Assert
        Assert.Equal("TestMaterial", material.Name);
    }
}
