using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Character))]
public class CharacterEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10f);
        
        if (GUILayout.Button("Generate Name")) {
            var names = Resources.LoadAll<CharacterNames>("Names/");
            ((Character) target).GenerateName(names[Random.Range(0, names.Length)]);
            EditorUtility.SetDirty(target);
        }
    }
}
/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Faction))]
public class FactionEditor : Editor
{
    private CharacterNames names;
    private Gender gender;
    private string title;
    private int amount;

    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();
        
        GUILayout.Space(10);
        GUILayout.Label("Characters Generator");
        names = (CharacterNames) EditorGUILayout.ObjectField("Character names", names,  typeof(CharacterNames), false);
        gender = (Gender) EditorGUILayout.EnumPopup("Gender", gender);
        title = EditorGUILayout.TextField("Title", title);
        amount = EditorGUILayout.IntField("Amount", amount);
        if (GUILayout.Button("Generate Character") && amount > 0 && names) {
            var banner = Resources.LoadAll<Banner>("Banners/");
            for (var i = 0; i < amount; i++) {
                var obj = (Character) CreateInstance(typeof(Character));
                obj.names = names.RandomName;
                obj.title = title;
                obj.gender = gender;
                obj.banner = banner[i];
                AssetDatabase.CreateAsset(obj, obj.Path);
                //charaters.Add(obj);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(target);
        }
    }
}*/