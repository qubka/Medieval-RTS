using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct Position : IComponentData
{
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class PositionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithoutBurst()
            .WithAll<Position>()
                .ForEach((Transform transform, ref LocalToWorld localToWorld) => 
                {
                    localToWorld.Value = transform.localToWorldMatrix;
                }
            ).Run();
        
        Entities
            .WithBurst()
            .WithAll<Position>()
                .ForEach((ref Translation translation, ref Rotation rotation, in LocalToWorld localToWorld) => 
                {
                    translation.Value = localToWorld.Position;
                    rotation.Value = localToWorld.Rotation;
                }
            ).ScheduleParallel();
    }
}