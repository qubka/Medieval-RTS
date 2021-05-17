using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(Town))]
public class TownEditor : Editor
{
    private InfrastructureType infrastructureType;
    
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Add Random Troop")) {
            var town = ((Town) target);
            var troops = town.data.ruler.faction.troops;
            town.data.garrison.Add(troops[Random.Range(0, troops.Length)]);
            EditorUtility.SetDirty(target);
        }
        
        if (GUILayout.Button("Generate Name")) {
            var names = Resources.LoadAll<TownNames>("Names/");
            ((Town) target).GenerateName(names[Random.Range(0, names.Length)]);
            EditorUtility.SetDirty(target);
        }

        infrastructureType = (InfrastructureType) EditorGUILayout.EnumPopup(infrastructureType);
        
        if (GUILayout.Button("Generate Location")) {
            ((Town) target).GenerateLocation(infrastructureType);
            EditorUtility.SetDirty(target);
        }
    }
}