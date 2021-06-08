using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Seeking : IComponentData
{
    public float Weight;
    public Entity Target;
    public float TargetRadius;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class SeekingSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        var transComp = GetComponentDataFromEntity<Translation>(true);
        Entities
            .WithBurst()
            .WithReadOnly(transComp)
            .WithAll<Seeking>()
                .ForEach((Entity entity, ref Velocity velocity, in Boid boid, in Seeking seeking) => 
                {
                    // If target died, do nothing
                    if (seeking.Target == Entity.Null || !HasComponent<Translation>(seeking.Target)) // Can happens only here, we use it to seek for other units which might die
                        return;
                    
                    var direction = transComp[seeking.Target].Value.xz - transComp[entity].Value.xz;
                    var distance = math.lengthsq(direction);
                    
                    // If we close, do nothing
                    if (distance < seeking.TargetRadius) {
                        return;
                    }

                    direction = math.normalize(direction);
                    direction *= boid.MaxAccel;
                    velocity.Linear += (seeking.Weight * direction);
                }
            ).ScheduleParallel();
    }
}