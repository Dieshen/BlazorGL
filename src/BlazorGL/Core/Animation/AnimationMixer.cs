using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorGL.Core.Animation;

/// <summary>
/// Lightweight mixer used by unit tests and data tooling to manage clip playback state.
/// </summary>
public class AnimationMixer
{
    private readonly List<AnimationAction> _actions = new();

    public AnimationAction ClipAction(AnimationClip clip)
    {
        if (clip == null) throw new ArgumentNullException(nameof(clip));

        var existing = _actions.FirstOrDefault(a => a.Clip == clip);
        if (existing != null)
            return existing;

        var action = new AnimationAction(clip);
        _actions.Add(action);
        return action;
    }

    /// <summary>
    /// Advances all running actions.
    /// </summary>
    public void Update(float deltaTime)
    {
        foreach (var action in _actions)
        {
            action.Update(deltaTime);
        }
    }
}

public class AnimationAction
{
    private float _time;

    internal AnimationAction(AnimationClip clip)
    {
        Clip = clip;
    }

    public AnimationClip Clip { get; }
    public bool IsRunning { get; private set; }

    public void Play()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
        _time = 0;
    }

    internal void Update(float deltaTime)
    {
        if (!IsRunning || Clip.Duration <= 0)
            return;

        _time += deltaTime;
        if (_time >= Clip.Duration)
        {
            _time = Clip.Duration;
            IsRunning = false;
        }
    }
}
