using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolTable;
    private Transform worldTransform;
    
    [Serializable]
    public struct Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
    private void Awake()
    {
        worldTransform = transform;
        poolTable = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools) {
            var objectPool = new Queue<GameObject>();
            for (var i = 0; i < pool.size; i++) {
                var obj = Instantiate(pool.prefab, worldTransform, false);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolTable.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3? pos = null, Quaternion? rot = null)
    {
        var queue = poolTable[tag];
        if (queue.Count == 0) {
            foreach (var pool in pools) {
                if (pool.tag == tag) {
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

    public void ReturnToPool(string tag, GameObject obj)
    {
        var trans = obj.transform;
        trans.SetParent(worldTransform, false);
        trans.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        
        obj.SetActive(false);
        poolTable[tag].Enqueue(obj);
    }
}