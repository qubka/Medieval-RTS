using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/House (Family) Config", order = 0)]
[Serializable]
[InitializeOnLoad]
public class House : SerializableObject
{
    public int id;
    public string label;
    //public int prestige;
    [JSONNode(NodeOptions.DontSerialize)]
    public Banner banner;

#if UNITY_EDITOR
    public void GenerateName(HouseNames names)
    {
        id = Resources.LoadAll<House>("Houses/").Length;
        var instance = GetInstanceID();
        var newName = names.RandomName();
        var assetPath = AssetDatabase.GetAssetPath(instance);
        AssetDatabase.RenameAsset(assetPath, newName + "_" + id);
        name = newName;
        label = newName;
        banner = Resources.LoadAll<Banner>("Banners/")[id - 1];
    }
#endif
    
    #region Serialization
    
    public override void OnSerialization()
    {
    }

    public override void OnDeserialization()
    {
        var house = Manager.defaultHouses.Find(h => h.id == id);
        banner = house.banner;
    }
    
    #endregion
}