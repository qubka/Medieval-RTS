using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Global))]
public class ScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var nameProperty = serializedObject.FindProperty(nameof(target.name));
        if (nameProperty != null) EditorGUILayout.PropertyField(nameProperty);
        
        DrawDefaultInspector();
    }
}