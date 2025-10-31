using Xunit;
using BlazorGL.Core.Math;
using System.Numerics;

namespace BlazorGL.Tests.Math;

public class MathTests
{
    [Fact]
    public void Color_InitializesWithRGB()
    {
        var color = new Color(0.5f, 0.7f, 0.9f);

        Assert.Equal(0.5f, color.R);
        Assert.Equal(0.7f, color.G);
        Assert.Equal(0.9f, color.B);
    }

    [Fact]
    public void Color_White_IsCorrect()
    {
        var white = Color.White;

        Assert.Equal(1.0f, white.R);
        Assert.Equal(1.0f, white.G);
        Assert.Equal(1.0f, white.B);
    }

    [Fact]
    public void Color_Black_IsCorrect()
    {
        var black = Color.Black;

        Assert.Equal(0.0f, black.R);
        Assert.Equal(0.0f, black.G);
        Assert.Equal(0.0f, black.B);
    }

    [Fact]
    public void Color_Red_IsCorrect()
    {
        var red = Color.Red;

        Assert.Equal(1.0f, red.R);
        Assert.Equal(0.0f, red.G);
        Assert.Equal(0.0f, red.B);
    }

    [Fact]
    public void Color_ToVector3_Converts()
    {
        var color = new Color(0.1f, 0.2f, 0.3f);
        var vec = color.ToVector3();

        Assert.Equal(0.1f, vec.X);
        Assert.Equal(0.2f, vec.Y);
        Assert.Equal(0.3f, vec.Z);
    }

    [Fact]
    public void BoundingBox_ContainsPoint()
    {
        var bbox = new BoundingBox
        {
            Min = new Vector3(-1, -1, -1),
            Max = new Vector3(1, 1, 1)
        };

        Assert.True(bbox.ContainsPoint(Vector3.Zero));
        Assert.True(bbox.ContainsPoint(new Vector3(0.5f, 0.5f, 0.5f)));
        Assert.False(bbox.ContainsPoint(new Vector3(2, 0, 0)));
    }

    [Fact]
    public void BoundingBox_ExpandByPoint()
    {
        var bbox = new BoundingBox
        {
            Min = new Vector3(0, 0, 0),
            Max = new Vector3(1, 1, 1)
        };

        bbox.ExpandByPoint(new Vector3(2, 3, 4));

        Assert.Equal(new Vector3(0, 0, 0), bbox.Min);
        Assert.Equal(new Vector3(2, 3, 4), bbox.Max);
    }

    [Fact]
    public void BoundingSphere_ContainsPoint()
    {
        var sphere = new BoundingSphere
        {
            Center = Vector3.Zero,
            Radius = 5.0f
        };

        Assert.True(sphere.ContainsPoint(Vector3.Zero));
        Assert.True(sphere.ContainsPoint(new Vector3(3, 0, 0)));
        Assert.False(sphere.ContainsPoint(new Vector3(10, 0, 0)));
    }

    [Fact]
    public void BoundingSphere_ExpandByPoint()
    {
        var sphere = new BoundingSphere
        {
            Center = Vector3.Zero,
            Radius = 1.0f
        };

        sphere.ExpandByPoint(new Vector3(5, 0, 0));

        Assert.True(sphere.Radius >= 5.0f);
    }
}
