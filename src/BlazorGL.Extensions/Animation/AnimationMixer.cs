using BlazorGL.Core;

namespace BlazorGL.Extensions.Animation;

/// <summary>
/// Manages and plays animations on an object
/// </summary>
public class AnimationMixer
{
    private Object3D _target;
    private AnimationClip? _clip;
    private float _time;
    private bool _playing;
    private bool _loop = true;

    public AnimationMixer(Object3D target)
    {
        _target = target;
    }

    public void Play(AnimationClip clip)
    {
        _clip = clip;
        _time = 0;
        _playing = true;
    }

    public void Stop()
    {
        _playing = false;
    }

    public void Update(float deltaTime)
    {
        if (!_playing || _clip == null) return;

        _time += deltaTime;

        if (_time > _clip.Duration)
        {
            if (_loop)
                _time = 0;
            else
            {
                _time = _clip.Duration;
                _playing = false;
            }
        }

        foreach (var track in _clip.Tracks)
        {
            var value = track.Evaluate(_time);

            switch (track.TargetProperty.ToLower())
            {
                case "position":
                    _target.Position = value;
                    break;
                case "scale":
                    _target.Scale = value;
                    break;
            }
        }
    }

    public bool Loop
    {
        get => _loop;
        set => _loop = value;
    }
}
