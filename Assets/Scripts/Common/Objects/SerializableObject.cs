using UnityEngine;
using UnityJSON;

public abstract class SerializableObject : ScriptableObject, ISerializationListener
{
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
}