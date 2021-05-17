using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Character))]
public class CharacterEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Generate Random Name")) {
            var names = Resources.LoadAll<CharacterNames>("Names/");
            ((Character) target).GenerateName(names[Random.Range(0, names.Length)]);
            EditorUtility.SetDirty(target);
        }
    }
}