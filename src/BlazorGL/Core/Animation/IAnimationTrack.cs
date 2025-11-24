namespace BlazorGL.Core.Animation;

/// <summary>
/// Common contract for animation tracks stored within an AnimationClip.
/// </summary>
public interface IAnimationTrack
{
    /// <summary>
    /// Property on the target object this track animates (e.g. position, rotation).
    /// </summary>
    string TargetProperty { get; }
}
