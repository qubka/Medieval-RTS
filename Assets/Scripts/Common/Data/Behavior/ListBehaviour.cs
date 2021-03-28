
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ListBehaviour<T> : MonoBehaviour, IEnumerable where T : MonoBehaviour
{
    [ReadOnly] public List<T> list;
    
    protected virtual void Awake()
    {
        list = new List<T>();
    }
    
    public void Add(T obj)
    {
        list.Add(obj);
    }
    
    public void Remove(T obj)
    {
        list.Remove(obj);
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}