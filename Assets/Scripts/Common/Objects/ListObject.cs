using System.Collections;
using System.Collections.Generic;

public abstract class ListObject<T> : SingletonObject<ListObject<T>>, IEnumerable
{
    [ReadOnly] public readonly List<T> list = new List<T>();
    //public T this[GameObject o] => list[o.GetInstanceID()];

    public int Count => list.Count;
    
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