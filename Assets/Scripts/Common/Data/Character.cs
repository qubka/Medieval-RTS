using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Character Config", order = 0)]
[Serializable]
public class Character : ScriptableObject
{
    [Header("Primary")]
    public string surname;
    public string title;
    public Gender gender;
    public int age;
    public int renown;
    public int honor;

    [Header("Game")] 
    public bool isNoble;
    //public bool isCompanion;
    //public List<string> locationsOwned;

    [Header("Data")] 
    public Faction faction;
    public Army army;
    public Image portrait;
    public Banner banner;

#if UNITY_EDITOR    
    public void GenerateName(CharacterNames names)
    {
        var instance = GetInstanceID();
        var newName = names.RandomName;
        var assetPath = UnityEditor.AssetDatabase.GetAssetPath(instance);
        UnityEditor.AssetDatabase.RenameAsset(assetPath, newName + instance);
        name = newName;
        surname = newName;
        age = Random.Range(17, 50);
        renown = Random.Range(0, 1000);
        honor = Random.Range(-100, 100);
    }
#endif    
}