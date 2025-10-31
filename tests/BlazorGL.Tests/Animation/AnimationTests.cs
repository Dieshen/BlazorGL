using Xunit;
using BlazorGL.Core.Animation;
using System.Numerics;

namespace BlazorGL.Tests.Animation;

public class AnimationTests
{
    [Fact]
    public void AnimationClip_CreatesWithNameAndDuration()
    {
        // Arrange & Act
        var clip = new AnimationClip("TestClip", 5.0f);

        // Assert
        Assert.Equal("TestClip", clip.Name);
        Assert.Equal(5.0f, clip.Duration);
    }

    [Fact]
    public void AnimationClip_CanAddTrack()
    {
        // Arrange
        var clip = new AnimationClip("TestClip", 5.0f);
        var track = new VectorTrack("position");

        // Act
        clip.AddTrack(track);

        // Assert
        Assert.Contains(track, clip.Tracks);
    }

    [Fact]
    public void VectorTrack_CanAddKeyframes()
    {
        // Arrange
        var track = new VectorTrack("position");
        var keyframe1 = new Keyframe<Vector3>(0f, Vector3.Zero);
        var keyframe2 = new Keyframe<Vector3>(1f, Vector3.One);

        // Act
        track.AddKeyframe(keyframe1);
        track.AddKeyframe(keyframe2);

        // Assert
        Assert.Equal(2, track.Keyframes.Count);
    }

    [Fact]
    public void Keyframe_StoresTimeAndValue()
    {
        // Arrange
        var time = 2.5f;
        var value = new Vector3(1, 2, 3);

        // Act
        var keyframe = new Keyframe<Vector3>(time, value);

        // Assert
        Assert.Equal(time, keyframe.Time);
        Assert.Equal(value, keyframe.Value);
    }

    [Fact]
    public void AnimationMixer_CanCreateAction()
    {
        // Arrange
        var clip = new AnimationClip("TestClip", 5.0f);
        var mixer = new AnimationMixer();

        // Act
        var action = mixer.ClipAction(clip);

        // Assert
        Assert.NotNull(action);
        Assert.Equal(clip, action.Clip);
    }

    [Fact]
    public void AnimationAction_StartsAsPaused()
    {
        // Arrange
        var clip = new AnimationClip("TestClip", 5.0f);
        var mixer = new AnimationMixer();

        // Act
        var action = mixer.ClipAction(clip);

        // Assert
        Assert.False(action.IsRunning);
    }

    [Fact]
    public void AnimationAction_CanBePlayed()
    {
        // Arrange
        var clip = new AnimationClip("TestClip", 5.0f);
        var mixer = new AnimationMixer();
        var action = mixer.ClipAction(clip);

        // Act
        action.Play();

        // Assert
        Assert.True(action.IsRunning);
    }

    [Fact]
    public void AnimationAction_CanBeStopped()
    {
        // Arrange
        var clip = new AnimationClip("TestClip", 5.0f);
        var mixer = new AnimationMixer();
        var action = mixer.ClipAction(clip);
        action.Play();

        // Act
        action.Stop();

        // Assert
        Assert.False(action.IsRunning);
    }

    [Fact]
    public void QuaternionTrack_CanAddKeyframes()
    {
        // Arrange
        var track = new QuaternionTrack("rotation");
        var keyframe = new Keyframe<Quaternion>(0f, Quaternion.Identity);

        // Act
        track.AddKeyframe(keyframe);

        // Assert
        Assert.Single(track.Keyframes);
    }

    [Fact]
    public void NumberTrack_CanAddKeyframes()
    {
        // Arrange
        var track = new NumberTrack("opacity");
        var keyframe = new Keyframe<float>(0f, 1.0f);

        // Act
        track.AddKeyframe(keyframe);

        // Assert
        Assert.Single(track.Keyframes);
    }
}
