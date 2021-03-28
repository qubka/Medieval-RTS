using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TableBehaviour<T> : MonoBehaviour, IEnumerable where T : MonoBehaviour
{
    [ReadOnly] public Dictionary<int, T> table;
    public T this[GameObject o] => table[o.GetInstanceID()];
    
    protected virtual void Awake()
    {
        table = new Dictionary<int, T>();
    }
    
    public void Add(GameObject o, T obj)
    {
        table.Add(o.GetInstanceID(), obj);
    }
    
    public void Remove(GameObject o)
    {
        table.Remove(o.GetInstanceID());
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return table.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}