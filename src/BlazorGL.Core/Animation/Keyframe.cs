using System.Collections.Generic;
using System.Numerics;

namespace BlazorGL.Core.Animation;

/// <summary>
/// Represents a value sampled at a specific time along an animation track.
/// </summary>
public readonly struct Keyframe<T>
{
    public float Time { get; }
    public T Value { get; }

    public Keyframe(float time, T value)
    {
        Time = time;
        Value = value;
    }
}

/// <summary>
/// Generic keyframe track with convenience helpers used by tests and tooling.
/// </summary>
public abstract class AnimationTrack<T> : IAnimationTrack
{
    protected AnimationTrack(string targetProperty)
    {
        TargetProperty = targetProperty;
    }

    public string TargetProperty { get; }

    public List<Keyframe<T>> Keyframes { get; } = new();

    public void AddKeyframe(Keyframe<T> keyframe) => Keyframes.Add(keyframe);
}

public sealed class VectorTrack : AnimationTrack<Vector3>
{
    public VectorTrack(string targetProperty) : base(targetProperty) { }
}

public sealed class QuaternionTrack : AnimationTrack<Quaternion>
{
    public QuaternionTrack(string targetProperty) : base(targetProperty) { }
}

public sealed class NumberTrack : AnimationTrack<float>
{
    public NumberTrack(string targetProperty) : base(targetProperty) { }
}
