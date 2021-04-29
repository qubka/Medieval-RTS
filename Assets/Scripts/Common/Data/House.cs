using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityJSON;

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
    
    /* For serialization */
    [JSONNode] private string bannerAsset;
    
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
    }
#endif
    
    public override void OnSerialization()
    {
        bannerAsset = banner ? Path.GetFileName(AssetDatabase.GetAssetPath(banner)) : "";
    }

    public override void OnDeserialization()
    {
        if (bannerAsset.Length > 0) {
            banner = AssetDatabase.LoadAssetAtPath<Banner>("Assets/Resources/Banners/" + bannerAsset);
            bannerAsset = "";
        }
    }
}