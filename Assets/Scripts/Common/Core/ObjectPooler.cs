using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolTable;
    
    [Serializable]
    public struct Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
    private void Awake()
    {
        poolTable = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools) {
            var objectPool = new Queue<GameObject>();
            for (var i = 0; i < pool.size; i++) {
                var gameObject = Instantiate(pool.prefab);
                gameObject.SetActive(false);
                objectPool.Enqueue(gameObject);
            }
            poolTable.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        var queue = poolTable[tag];
        if (queue.Count == 0) {
            foreach (var pool in pools) {
                if (pool.tag == tag) {
                    return Instantiate(pool.prefab, position, rotation);
                }
            }
            throw new Exception("No such tag!");
        }
        
        var objectToSpawn = queue.Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetPositionAndRotation(position, rotation);

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject gameObject)
    {
        gameObject.SetActive(false);
        poolTable[tag].Enqueue(gameObject);
    }
}