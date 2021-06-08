using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Portrait))]
public class PortraitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var portrait = (Portrait) target;

        GUILayout.Space(10);
        
        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Read Image File")) {
            portrait.ReadImageFile();
        }
        EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Random Character!")) {
            portrait.RandomImage();
            // Refresh SceneView when Image is Changed
            EditorUtility.SetDirty(target);
        }

        // Create Buttons for Changing Parts of the Character
        foreach (var image in portrait.characterParts) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(image.name, EditorStyles.label);
            if (GUILayout.Button(" < ", EditorStyles.miniButtonLeft)) {
                portrait.PreviousImage(image);
                // Refresh SceneView when Image is Changed
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button(" > ", EditorStyles.miniButtonRight)) {
                portrait.NextImage(image);
                // Refresh SceneView when Image is Changed
                EditorUtility.SetDirty(target); 
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }
    }
}

    