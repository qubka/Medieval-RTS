using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<Pool> pools;
    private Dictionary<int, Queue<GameObject>> poolTable;
    private Transform worldTransform;
    
    [Serializable]
    public struct Pool
    {
        public GameObject prefab;
        public int size;
    }
    
    private void Awake()
    {
        worldTransform = transform;
        poolTable = new Dictionary<int, Queue<GameObject>>();

        foreach (var pool in pools) {
            var objectPool = new Queue<GameObject>();
            for (var i = 0; i < pool.size; i++) {
                var obj = Instantiate(pool.prefab, worldTransform, false);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolTable.Add(pool.prefab.name.GetHashCode(), objectPool);
        }
    }

    public GameObject SpawnFromPool(int id, Vector3? pos = null, Quaternion? rot = null)
    {
        var queue = poolTable[id];
        if (queue.Count == 0) {
            foreach (var pool in pools) {
                if (pool.prefab.name.GetHashCode() == id) {
                    return Instantiate(pool.prefab, pos ?? Vector3.zero, rot ?? Quaternion.identity);
                }
            }
            throw new Exception("No such tag!");
        }
        
        var objectToSpawn = queue.Dequeue();
        
        objectToSpawn.SetActive(true);
        
        var trans =  objectToSpawn.transform;
        trans.SetParent(null);
        
        if (pos.HasValue && rot.HasValue) {
            trans.SetPositionAndRotation(pos.Value, rot.Value);
        } else if (pos.HasValue) {
            trans.position = pos.Value;
        } else if (rot.HasValue) {
            trans.rotation = rot.Value;
        }

        return objectToSpawn;
    }

    public void ReturnToPool(int id, GameObject obj)
    {
        var trans = obj.transform;
        trans.SetParent(worldTransform, false);
        trans.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        
        obj.SetActive(false);
        poolTable[id].Enqueue(obj);
    }
}