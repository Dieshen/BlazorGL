using BlazorGL.Core.Animation;
using System.Numerics;
using System.Text.Json;

namespace BlazorGL.Core.Loaders;

/// <summary>
/// Loader for animation clips from JSON format
/// Supports keyframe animations for position, rotation, scale, and custom properties
/// </summary>
public class AnimationLoader
{
    private readonly LoadingManager? _manager;

    public AnimationLoader(LoadingManager? manager = null)
    {
        _manager = manager;
    }

    /// <summary>
    /// Loads animation clips from JSON
    /// </summary>
    public async Task<List<AnimationClip>> LoadAsync(string json)
    {
        var clips = new List<AnimationClip>();

        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var clipJson in root.EnumerateArray())
                {
                    var clip = ParseAnimationClip(clipJson);
                    if (clip != null)
                    {
                        clips.Add(clip);
                    }
                }
            }
            else if (root.TryGetProperty("animations", out var animationsArray))
            {
                foreach (var clipJson in animationsArray.EnumerateArray())
                {
                    var clip = ParseAnimationClip(clipJson);
                    if (clip != null)
                    {
                        clips.Add(clip);
                    }
                }
            }

            return clips;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load animations: {ex.Message}", ex);
        }
    }

    private AnimationClip? ParseAnimationClip(JsonElement json)
    {
        if (!json.TryGetProperty("name", out var nameElement))
            return null;

        var name = nameElement.GetString() ?? "Animation";
        var duration = 0f;

        if (json.TryGetProperty("duration", out var durationElement))
        {
            duration = durationElement.GetSingle();
        }

        var clip = new AnimationClip(name, duration);

        // Parse tracks
        if (json.TryGetProperty("tracks", out var tracksArray))
        {
            foreach (var trackJson in tracksArray.EnumerateArray())
            {
                var track = ParseTrack(trackJson);
                if (track != null)
                {
                    clip.AddTrack(track);
                }
            }
        }

        return clip;
    }

    private AnimationTrack? ParseTrack(JsonElement json)
    {
        if (!json.TryGetProperty("name", out var nameElement) ||
            !json.TryGetProperty("type", out var typeElement))
            return null;

        var name = nameElement.GetString() ?? "";
        var type = typeElement.GetString() ?? "";

        // Parse times and values
        if (!json.TryGetProperty("times", out var timesElement) ||
            !json.TryGetProperty("values", out var valuesElement))
            return null;

        var times = timesElement.EnumerateArray().Select(e => e.GetSingle()).ToArray();
        var values = valuesElement.EnumerateArray().Select(e => e.GetSingle()).ToArray();

        return type switch
        {
            "vector3" => ParseVector3Track(name, times, values),
            "quaternion" => ParseQuaternionTrack(name, times, values),
            "number" => ParseNumberTrack(name, times, values),
            _ => null
        };
    }

    private AnimationTrack ParseVector3Track(string name, float[] times, float[] values)
    {
        var track = new VectorTrack(name);

        for (int i = 0; i < times.Length; i++)
        {
            var valueIndex = i * 3;
            if (valueIndex + 2 < values.Length)
            {
                var keyframe = new Keyframe<Vector3>(
                    times[i],
                    new Vector3(values[valueIndex], values[valueIndex + 1], values[valueIndex + 2])
                );
                track.AddKeyframe(keyframe);
            }
        }

        return track;
    }

    private AnimationTrack ParseQuaternionTrack(string name, float[] times, float[] values)
    {
        var track = new QuaternionTrack(name);

        for (int i = 0; i < times.Length; i++)
        {
            var valueIndex = i * 4;
            if (valueIndex + 3 < values.Length)
            {
                var keyframe = new Keyframe<Quaternion>(
                    times[i],
                    new Quaternion(values[valueIndex], values[valueIndex + 1], values[valueIndex + 2], values[valueIndex + 3])
                );
                track.AddKeyframe(keyframe);
            }
        }

        return track;
    }

    private AnimationTrack ParseNumberTrack(string name, float[] times, float[] values)
    {
        var track = new NumberTrack(name);

        for (int i = 0; i < times.Length && i < values.Length; i++)
        {
            var keyframe = new Keyframe<float>(times[i], values[i]);
            track.AddKeyframe(keyframe);
        }

        return track;
    }

    /// <summary>
    /// Serializes animation clips to JSON
    /// </summary>
    public string Serialize(List<AnimationClip> clips)
    {
        return JsonSerializer.Serialize(clips.Select(clip => new
        {
            name = clip.Name,
            duration = clip.Duration,
            tracks = clip.Tracks.Select(track => new
            {
                name = track.Name,
                type = GetTrackType(track),
                times = GetTrackTimes(track),
                values = GetTrackValues(track)
            })
        }));
    }

    private string GetTrackType(AnimationTrack track)
    {
        return track switch
        {
            VectorTrack => "vector3",
            QuaternionTrack => "quaternion",
            NumberTrack => "number",
            _ => "unknown"
        };
    }

    private float[] GetTrackTimes(AnimationTrack track)
    {
        // Simplified - would need proper implementation based on track type
        return Array.Empty<float>();
    }

    private float[] GetTrackValues(AnimationTrack track)
    {
        // Simplified - would need proper implementation based on track type
        return Array.Empty<float>();
    }
}
