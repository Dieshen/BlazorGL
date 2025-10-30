using System.Numerics;
using BlazorGL.Core;

namespace BlazorGL.Extensions.Particles;

/// <summary>
/// Basic particle system
/// </summary>
public class ParticleSystem : Object3D
{
    private Particle[] _particles;
    public int ParticleCount => _particles.Length;

    public ParticleSystem(int count)
    {
        _particles = new Particle[count];
        for (int i = 0; i < count; i++)
        {
            _particles[i] = new Particle();
        }
    }

    public override void Update(float deltaTime)
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            ref var p = ref _particles[i];

            p.Velocity += new Vector3(0, -9.81f, 0) * deltaTime;
            p.Position += p.Velocity * deltaTime;
            p.Life -= deltaTime * 0.1f;

            if (p.Life <= 0)
            {
                p.Position = Vector3.Zero;
                p.Velocity = RandomVelocity();
                p.Life = 1.0f;
            }
        }

        base.Update(deltaTime);
    }

    private static Vector3 RandomVelocity()
    {
        var random = new Random();
        return new Vector3(
            (float)(random.NextDouble() * 2 - 1),
            (float)(random.NextDouble() * 2),
            (float)(random.NextDouble() * 2 - 1)
        );
    }
}

public struct Particle
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Life;
}
