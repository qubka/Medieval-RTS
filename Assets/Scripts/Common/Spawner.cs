using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public List<GameObject> bandits;
    public Vector3 center;
    public float radius;
    public int areaMask = 1;
    
    public void SpawnBandits(int count)
    {
        for (var i = 0; i < count; i++) {
            var pos = Vector.GetRandomNavMeshPositionNearLocation(center, radius, areaMask);
            Instantiate(bandits[Random.Range(0, bandits.Count)], pos, Quaternion.Euler(0f, Random.Range(0, 360f), 0f));
        }
    }
    
    /*public void SpawnBandits()
    {
        
    }*/
}
