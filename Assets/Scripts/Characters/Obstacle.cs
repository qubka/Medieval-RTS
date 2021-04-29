using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Entity entity;
    public Vector3 position;
    
    private void Start()
    {
        // Find entity manager
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Set up achetype
        var obstacle = entityManager.CreateArchetype(typeof(Translation));

        // Add component data to squad
        position = transform.position;
        entity = entityManager.CreateEntity(obstacle);
        entityManager.SetName(entity, "obstacle");
        entityManager.SetComponentData(entity, new Translation { Value = position });
        
        // Add component to the list
        ObstacleTable.Instance.Add(gameObject, this);
    }
}
