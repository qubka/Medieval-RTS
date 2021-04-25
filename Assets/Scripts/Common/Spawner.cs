using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public GameObject army;
    public Vector3 center;
    public float radius;
    
    public void SpawnBandits(int count)
    {
        for (var i = 0; i < count; i++) {
            var pos = Vector.GetRandomNavMeshPositionNearLocation(center, radius);
            Instantiate(army, pos, Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
        }
    }
    
    /*public void SpawnBandits()
    {
        
    }*/
}
