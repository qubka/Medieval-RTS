using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BoidBehaviour : MonoBehaviour
{
    public float2 velocity;
}

public struct Boid : IComponentData
{
    public float MaxSpeed;
    public float MaxAccel;
    public float2 Velocity;
    //public float2 Rotation;
}

public struct Velocity : IComponentData
{
    public float2 Linear;
    //public float2 Angular;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class BoidSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        Entities
            .WithBurst()
            .WithAll<Boid>()
                .ForEach((ref Boid boid, ref Velocity velocity) => 
                {
                    boid.Velocity += velocity.Linear * deltaTime;
                    //boid.Rotation += velocity.Angular * deltaTime;
                
                    if (math.length(boid.Velocity) > boid.MaxSpeed) {
                        boid.Velocity = math.normalize(boid.Velocity);
                        boid.Velocity *= boid.MaxSpeed;
                    }
                    
                    if (math.lengthsq(velocity.Linear) == 0f) {
                        boid.Velocity = float2.zero;
                    }

                    velocity = new Velocity();
                }
            ).ScheduleParallel();
        Entities
            .WithoutBurst()
            .WithAll<Boid>()
                .ForEach((BoidBehaviour mono, in Boid boid) => 
                {
                    mono.velocity = boid.Velocity;
                }
            ).Run();
    }
}