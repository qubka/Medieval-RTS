using System;
using UnityJSON;

[Serializable]
[JSONObject(ObjectOptions.TupleFormat)]
public class Pack<T1, T2>
{
    [JSONNode(NodeOptions.SerializeNull)]
    public T1 item1;

    [JSONNode(NodeOptions.SerializeNull)]
    public T2 item2;

    [JSONConstructor]
    public Pack(T1 item1, T2 item2)
    {
        this.item1 = item1;
        this.item2 = item2;
    }
}