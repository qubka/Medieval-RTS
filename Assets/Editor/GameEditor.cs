/*using BehaviorDesigner.Runtime;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Game))]
public class GameEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Load default data")) {
            ((Game) target).LoadDefaultData();
            EditorUtility.SetDirty(target);
        }
    }
}*/