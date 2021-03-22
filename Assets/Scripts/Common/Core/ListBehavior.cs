
using System.Collections.Generic;
using UnityEngine;

public abstract class ListBehavior<T> : MonoBehaviour
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
}