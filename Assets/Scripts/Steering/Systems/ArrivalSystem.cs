using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Arrival : IComponentData
{
    public float Weight;
    public Entity Target;
    public float SlowRadius;
    public float TargetRadius;
    public float TimeToTarget;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class ArrivalSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        var transComp = GetComponentDataFromEntity<Translation>(true);
        Entities
            .WithBurst()
            .WithReadOnly(transComp)
            .WithAll<Arrival>()
                .ForEach((Entity entity, ref Velocity velocity, in Boid boid, in Arrival arrive) => 
                {
                    var direction = transComp[arrive.Target].Value.xz - transComp[entity].Value.xz;
                    var distance = math.length(direction);

                    // If we close, do nothing
                    if (distance < arrive.TargetRadius)
                        return;

                    float targetSpeed;
                    if (distance > arrive.SlowRadius) { 
                        targetSpeed = boid.MaxSpeed;
                    } else {
                        targetSpeed = boid.MaxSpeed * distance / arrive.SlowRadius;
                    }

                    direction = math.normalizesafe(direction);
                    direction *= targetSpeed;
                    direction -= boid.Velocity;
                    direction /= arrive.TimeToTarget;

                    if (math.length(direction) > boid.MaxAccel) {
                        direction = math.normalize(direction);
                        direction *= boid.MaxAccel;
                    }
                    
                    velocity.Linear += (arrive.Weight * direction);
                }
            ).ScheduleParallel();
    }
}