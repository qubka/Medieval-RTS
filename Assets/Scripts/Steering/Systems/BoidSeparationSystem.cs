
/*
public struct BoidSeparation : IComponentData
{
    public float Weight;
    public float DesiredSeparation;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class BoidSeparationSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        var transComp = GetComponentDataFromEntity<Translation>(true);
        Entities
            .WithBurst()
            .WithReadOnly(transComp)
            .WithAll<BoidSeparation>()
                .ForEach((Entity entity, ref Velocity velocity, in BoidSeparation separation, in DynamicBuffer<UnitBuffer> units) => 
                {
                    var position = transComp[entity].Value.xz;
                    var sum = new float2();
                    var count = 0;
                    
                    // For every boid in the system, check if it's too close
                    for (var i = 0; i < units.Length; i++) {
                        var target = units[i].Value;
                        var diff = position - transComp[target].Value.xz;
                        var dist = math.length(diff);

                        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
                        if ((dist > 0f) && (dist < separation.DesiredSeparation)) {
                            // Calculate vector pointing away from neighbor
                            diff = math.normalize(diff);
                            diff /= dist; // Weight by distance
                            sum += diff;
                            count++; // Keep track of how many
                        }
                    }

                    // Average -- divide by how many
                    if (count > 0) {
                        sum /= count;
                        velocity.Angular += (separation.Weight * sum);
                    }
                }
        ).ScheduleParallel();
    }
}*/