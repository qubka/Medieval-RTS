using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Town))]
public class TownEditor : Editor {

    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate")) {
            ((Town) target).GenerateName();
            EditorUtility.SetDirty(target);
        }
    }
}