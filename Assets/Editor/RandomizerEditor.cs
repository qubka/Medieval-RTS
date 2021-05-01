using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectRandomizer))]
public class RandomizerEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);

        if (GUILayout.Button("Position")) {
            ((ObjectRandomizer) target).Position();
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("Rotate")) {
            ((ObjectRandomizer) target).Rotate();
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("Scale")) {
            ((ObjectRandomizer) target).Scale();
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("Reset")) {
            ((ObjectRandomizer) target).Reset();
            EditorUtility.SetDirty(target);
        }
    }
}