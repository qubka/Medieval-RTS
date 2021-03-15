using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Avoidance : IComponentData
{
    public float Weight;
    public float LookAhead;
    public float TargetRadius;
}

public struct AvoidanceBuffer : IBufferElementData
{
    public Entity Value;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class AvoidanceSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        var transComp = GetComponentDataFromEntity<Translation>(true);
        Entities
            .WithBurst()
            .WithReadOnly(transComp)
            .WithAll<Avoidance>()
                .ForEach((Entity entity, ref Velocity velocity, in Boid boid, in Avoidance avoid, in DynamicBuffer<AvoidanceBuffer> obstacles) => 
                {
                    var currentPosition = transComp[entity].Value.xz;
                    var prediction = currentPosition + (boid.Velocity * avoid.LookAhead);
                    
                    var mostThreatening = Entity.Null;
                    var threatPosition = float2.zero;

                    // Get closest target
                    for (var i = 0; i < obstacles.Length; i++) {
                        var target = obstacles[i].Value;
                        var targetPosition = transComp[target].Value.xz;
                        if (mostThreatening == Entity.Null || math.distancesq(targetPosition, currentPosition) < math.distancesq(threatPosition, currentPosition)) {
                            mostThreatening = target;
                            threatPosition = targetPosition;
                        }
                    }
                    
                    // Avoid the closest target if necessary
                    if (mostThreatening != Entity.Null) {
                        var targetPosition = ClosestPoint(threatPosition, currentPosition, prediction);
                        var distance = math.distancesq(currentPosition, prediction);
                        
                        // Check if the closest point is in the segment formed by the owner
                        // position and the prediction. Then, check if the owner would
                        // collide with the obstacle if it were at the closest point.
                        if (math.distancesq(targetPosition, currentPosition) < distance && 
                            math.distancesq(targetPosition, prediction) < distance && 
                            math.distance(targetPosition, threatPosition) - avoid.TargetRadius <= avoid.TargetRadius) {
                            var linearVelocity = targetPosition - threatPosition;
                            math.normalizesafe(linearVelocity);
                            linearVelocity *= boid.MaxAccel;
                            velocity.Linear += avoid.Weight * linearVelocity;
                        }
                    }
                }
            ).ScheduleParallel();
    }
    
    private static float2 ClosestPoint(float2 origin, float2 pointA, float2 pointB) 
    {
        // CVector2D that points from pointA to origin.
        var a2o = origin - pointA;
        // CVector2D that points from pointA to pointB.
        var a2b = pointB - pointA;

        a2b = math.normalizesafe(a2b);
        a2b *= math.dot(a2o, a2b);

        return pointA + a2b;
    }
}