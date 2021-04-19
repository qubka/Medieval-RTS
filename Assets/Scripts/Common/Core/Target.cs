using UnityEngine;

public class Target
{
    public GameObject obj;
    public float? orientation;
    public float? length;

    public Target(GameObject obj, float? orientation = null, float? length = null)
    {
        this.obj = obj;
        this.orientation = orientation;
        this.length = length;
    }
}
