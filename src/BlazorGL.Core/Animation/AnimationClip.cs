using System;
using System.Collections.Generic;

namespace BlazorGL.Core.Animation;

/// <summary>
/// Animation clip containing keyframe tracks
/// </summary>
public class AnimationClip
{
    public AnimationClip() : this("Animation", 0f) { }

    public AnimationClip(string name, float duration)
    {
        Name = name;
        Duration = duration;
    }

    public string Name { get; set; } = string.Empty;
    public float Duration { get; set; }
    public List<IAnimationTrack> Tracks { get; } = new();

    public void AddTrack(IAnimationTrack track)
    {
        if (track == null) throw new ArgumentNullException(nameof(track));
        Tracks.Add(track);
    }
}
