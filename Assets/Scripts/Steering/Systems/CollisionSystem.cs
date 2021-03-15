/*using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Colliding : IComponentData
{
}

public struct Collisional : IComponentData
{
    //public int Obstacles;
    public bool HasInFront;
    public bool HasInLeft;
    public bool HasInRight;
}

public struct ColliderBuffer : IBufferElementData
{
    public Entity Value;
}

[UpdateInGroup(typeof(TransformSystemGroup))]
public class CollisionSystem : SystemBase
{
    private static float Cone = math.cos(math.radians(90f));
    private static float3 Forward = new float3(0f, 0f, 1f);
    private static float3 Left = new float3(-1f, 0f, 0f);
    private static float3 Right = new float3(1f, 0f, 0f);
    
    protected override void OnUpdate() 
    {
        var transComp = GetComponentDataFromEntity<Translation>(true);
        Entities
            .WithBurst()
            .WithReadOnly(transComp)
            .WithAll<Colliding>()
                .ForEach((Entity entity, ref Collisional collisional, in Rotation rotation, in DynamicBuffer<ColliderBuffer> collision) => 
                {
                    var position = transComp[entity].Value;
                    var forward = math.mul(rotation.Value, Forward);
                    var left = math.mul(rotation.Value, Left);
                    var right = math.mul(rotation.Value, Right);

                    var hasInFront = false;
                    var hasInLeft = false;
                    var hasInRight = false;
                    
                    for (var i = 0; i < collision.Length; i++) {
                        var neighbour = collision[i].Value;
                        var direction = transComp[neighbour].Value - position;
                        
                        if (math.dot(forward,direction) > Cone) {
                            hasInFront = true;
                        }
                        if (math.dot(left,direction) > Cone) {
                            hasInLeft = true;
                        }
                        if (math.dot(right, direction) > Cone) {
                            hasInRight = true;
                        }
                    }

                    collisional.HasInFront = hasInFront;
                    collisional.HasInLeft = hasInLeft;
                    collisional.HasInRight = hasInRight;
                }
            ).ScheduleParallel();
    }
}*/