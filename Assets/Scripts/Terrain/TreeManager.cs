using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TreeManager : MonoBehaviour
{
    [SerializeField] private GameObject treeCollider;
    
    private void Start()
    {
        // Grab the terrain data and store
        var border = Manager.border;
        var terrain = Manager.terrain;
        var trans = terrain.transform;
        var data = terrain.terrainData;
        
        // For every tree on the terrain
        foreach (var tree in data.treeInstances) {
            var position = Vector3.Scale(tree.position, data.size) + trans.position;
            
            // Skip if exceed map limits
            if (border.IsOutsideBorder(position))
                continue;

            // Make connection between tree position and entity
            Instantiate(treeCollider, position, Quaternion.Euler(0f, math.degrees(tree.rotation), 0f));
        }
    }
}
