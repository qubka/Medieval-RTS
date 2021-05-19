using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Party))]
public class PartyEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Add Random Troop")) {
            var party = ((Party) target);
            var troops = party.leader.faction.troops;
            party.troops.Add(troops[Random.Range(0, troops.Length)].Clone());
            EditorUtility.SetDirty(target);
        }
    }
}