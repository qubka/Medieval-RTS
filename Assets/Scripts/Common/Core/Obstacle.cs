using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Entity entity;
    
    private void Start()
    {
        // Find entity manager
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        // Set up achetype
        var obstacle = entityManager.CreateArchetype(typeof(Translation));

        // Add component data to squad
        entity = entityManager.CreateEntity(obstacle);
        entityManager.SetName(entity, "obstacle");
        entityManager.SetComponentData(entity, new Translation { Value = transform.position });
    }
}
