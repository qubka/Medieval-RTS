using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TableObject<T> : SingletonObject<TableObject<T>>, IEnumerable
{
    private readonly Dictionary<int, T> table = new Dictionary<int, T>();
    
	public T this[GameObject o] {
		get {
			table.TryGetValue(o.GetInstanceID(), out var output);
			return output;
		}
	}
	public int Count => table.Count;
    public Dictionary<int, T>.ValueCollection Values => table.Values;
    
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