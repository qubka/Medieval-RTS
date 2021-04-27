using UnityEngine;
using UnityJSON;

public abstract class SerializableObject : ScriptableObject, ISerializationListener
{
    protected int hash { set; get; }
    
    public void OnSerializationWillBegin(Serializer serializer)
    {
        OnSerialization();
    }

    public void OnSerializationSucceeded(Serializer serializer)
    {
    }

    public void OnSerializationFailed(Serializer serializer)
    {
    }

    public abstract void OnSerialization();

    public abstract void OnDeserialization();
    
    public static bool operator &(SerializableObject x, SerializableObject y)
    {
        return x.hash == y.hash;
    }
}