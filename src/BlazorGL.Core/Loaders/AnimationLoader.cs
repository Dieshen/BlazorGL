using BlazorGL.Core.Animation;
using System.Numerics;
using System.Text.Json;
using System.Linq;

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

        var clip = new AnimationClip
        {
            Name = name,
            Duration = duration
        };

        // Parse tracks
        if (json.TryGetProperty("tracks", out var tracksArray))
        {
            foreach (var trackJson in tracksArray.EnumerateArray())
            {
                var track = ParseTrack(trackJson);
                if (track != null)
                {
                    clip.Tracks.Add(track);
                }
            }
        }

        return clip;
    }

    private KeyframeTrack? ParseTrack(JsonElement json)
    {
        if (!json.TryGetProperty("name", out var nameElement) ||
            !json.TryGetProperty("type", out var typeElement))
            return null;

        var name = nameElement.GetString() ?? "";
        var type = (typeElement.GetString() ?? "").ToLowerInvariant();

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

    private KeyframeTrack ParseVector3Track(string name, float[] times, float[] values)
    {
        var vectors = new List<Vector3>();
        for (int i = 0; i < times.Length; i++)
        {
            var valueIndex = i * 3;
            if (valueIndex + 2 < values.Length)
            {
                vectors.Add(new Vector3(values[valueIndex], values[valueIndex + 1], values[valueIndex + 2]));
            }
        }

        return new KeyframeTrack
        {
            TargetProperty = name,
            Times = times,
            Values = vectors.ToArray()
        };
    }

    private KeyframeTrack ParseQuaternionTrack(string name, float[] times, float[] values)
    {
        var vectors = new List<Vector3>();
        for (int i = 0; i < times.Length; i++)
        {
            var valueIndex = i * 4;
            if (valueIndex + 3 < values.Length)
            {
                var q = new Quaternion(values[valueIndex], values[valueIndex + 1], values[valueIndex + 2], values[valueIndex + 3]);
                vectors.Add(QuaternionToEuler(q));
            }
        }

        return new KeyframeTrack
        {
            TargetProperty = name,
            Times = times,
            Values = vectors.ToArray()
        };
    }

    private KeyframeTrack ParseNumberTrack(string name, float[] times, float[] values)
    {
        var vectors = new List<Vector3>();
        for (int i = 0; i < times.Length && i < values.Length; i++)
        {
            var v = values[i];
            vectors.Add(new Vector3(v, v, v));
        }

        return new KeyframeTrack
        {
            TargetProperty = name,
            Times = times,
            Values = vectors.ToArray()
        };
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
            tracks = clip.Tracks
                .OfType<KeyframeTrack>()
                .Select(track => new
            {
                name = track.TargetProperty,
                type = "vector3",
                times = track.Times,
                values = FlattenVector3(track.Values)
            })
        }));
    }

    private static float[] FlattenVector3(Vector3[] values)
    {
        var result = new float[values.Length * 3];
        for (int i = 0; i < values.Length; i++)
        {
            var idx = i * 3;
            result[idx] = values[i].X;
            result[idx + 1] = values[i].Y;
            result[idx + 2] = values[i].Z;
        }
        return result;
    }

    private static Vector3 QuaternionToEuler(Quaternion q)
    {
        // Convert quaternion to Euler angles (pitch, yaw, roll)
        var sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        var cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        var roll = MathF.Atan2(sinrCosp, cosrCosp);

        var sinp = 2 * (q.W * q.Y - q.Z * q.X);
        float pitch;
        if (MathF.Abs(sinp) >= 1)
            pitch = MathF.CopySign(MathF.PI / 2, sinp);
        else
            pitch = MathF.Asin(sinp);

        var sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        var cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        var yaw = MathF.Atan2(sinyCosp, cosyCosp);

        // Map to Vector3 compatible with CreateFromYawPitchRoll (Y, X, Z order in mixer)
        return new Vector3(pitch, yaw, roll);
    }
}
