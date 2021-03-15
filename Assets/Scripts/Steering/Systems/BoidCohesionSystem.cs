
/*
public struct BoidCohesion : IComponentData
{
    public float Weight;
    public float NeighborDist;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class BoidCohesionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var transComp = GetComponentDataFromEntity<Translation>(true);
        Entities
            .WithBurst()
            .WithReadOnly(transComp)
            .WithAll<BoidCohesion>()
                .ForEach((Entity entity, ref Velocity velocity, in BoidCohesion cohesion, in DynamicBuffer<UnitBuffer> units) => 
                {
                    var position = transComp[entity].Value.xz;
                    var sum = new float2();
                    var count = 0;

                    // Iterate through the group of objects
                    for (var i = 0; i < units.Length; i++) {
                        var target = units[i].Value;
                        var pos = transComp[target].Value.xz;
                        var dist = math.length(position - pos);
                        if ((dist > 0f) && (dist < cohesion.NeighborDist)) {
                            sum += pos; // Add position
                            count++;
                        }
                    }

                    // Average the positions of all the objects
                    if (count > 0) {
                        sum /= count;
                        sum -= position;
                        velocity.Angular += (cohesion.Weight * sum);
                    }
                }
        ).ScheduleParallel();
    }
}*/