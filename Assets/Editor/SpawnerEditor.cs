using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    private int count;
    
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);

        count = EditorGUILayout.IntField("Amount to spawn", count);
        if (GUILayout.Button("Spawn Bandits")) {
            ((Spawner) target).SpawnBandits(count);
            EditorUtility.SetDirty(target);
        }
        
    }
}