namespace BlazorGL.Extensions.Animation;

/// <summary>
/// Animation clip containing keyframe tracks
/// </summary>
public class AnimationClip
{
    public string Name { get; set; } = string.Empty;
    public float Duration { get; set; }
    public List<KeyframeTrack> Tracks { get; set; } = new();
}
