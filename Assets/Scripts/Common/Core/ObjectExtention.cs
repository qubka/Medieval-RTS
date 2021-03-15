
using UnityEngine;

public static class ObjectExtention
{
    public static void DestroyIfNamed(GameObject gameObject, string name)
    {
        if (gameObject && gameObject.name == name) {
            Object.Destroy(gameObject);
        }
    }
}
