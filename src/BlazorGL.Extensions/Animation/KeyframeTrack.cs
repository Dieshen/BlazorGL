using System.Numerics;

namespace BlazorGL.Extensions.Animation;

/// <summary>
/// Keyframe animation track for a specific property
/// </summary>
public class KeyframeTrack
{
    public string TargetProperty { get; set; } = string.Empty;
    public float[] Times { get; set; } = Array.Empty<float>();
    public Vector3[] Values { get; set; } = Array.Empty<Vector3>();

    public Vector3 Evaluate(float time)
    {
        if (Times.Length == 0) return Vector3.Zero;
        if (time <= Times[0]) return Values[0];
        if (time >= Times[^1]) return Values[^1];

        for (int i = 0; i < Times.Length - 1; i++)
        {
            if (time >= Times[i] && time <= Times[i + 1])
            {
                float t = (time - Times[i]) / (Times[i + 1] - Times[i]);
                return Vector3.Lerp(Values[i], Values[i + 1], t);
            }
        }

        return Values[^1];
    }
}
