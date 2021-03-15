using UnityEngine;
 
[RequireComponent(typeof(Terrain))]
public class TerrainDistance : MonoBehaviour 
{
    public float drawDistance;

    private void Start () 
    {
        GetComponent<Terrain>().detailObjectDistance = drawDistance;
    }
}