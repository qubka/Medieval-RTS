using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(House))]
public class HouseEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Generate Random Name")) {
            var names = Resources.LoadAll<HouseNames>("Names/");
            ((House) target).GenerateName(names[Random.Range(0, names.Length)]);
            EditorUtility.SetDirty(target);
        }
    }
}