using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(Town))]
public class TownEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Generate Name")) {
            var names = Resources.LoadAll<TownNames>("Names/");
            ((Town) target).GenerateName(names[Random.Range(0, names.Length)]);
            EditorUtility.SetDirty(target);
        }
    }
}