using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityJSON;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Templates/House", order = 0)]
[Serializable]
public class House : SerializableObject
{
    public int id;
    public string label;
    [JSONNode(NodeOptions.DontSerialize)]
    public Character leader;
    public int tier;
    public int influence;
    [JSONNode(NodeOptions.DontSerialize)]
    public Banner banner;

    public static List<House> All => Game.Houses;

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
    
    /* For serialization */
    [JSONNode] private int leaderId;

    public override void OnSerialization()
    {
        leaderId = leader ? leader.id : -1;
    }

    public override void OnDeserialization()
    {
        if (leaderId != -1) {
            leader = Game.Characters.Find(c => c.id == leaderId);
        }
        var house = Manager.defaultHouses.Find(h => h.id == id);
        banner = house.banner;
    }
    
    #endregion
    
    /*public static float GetCorruption(this Clan clan)
        {
            var corruption = 0f;
            var numFiefsTooMany = clan.GetPermanentFiefs().Count() - clan.Tier;

            if (numFiefsTooMany > 0)
            {
                var factor = numFiefsTooMany > 5 ? 2 : 1;
                corruption = numFiefsTooMany * factor;
            }

            return corruption;
        }

        public static bool HasMaximumFiefs(this Clan clan)
        {
            var numFiefsTooMany = clan.GetPermanentFiefs().Count() - clan.Tier;
            return numFiefsTooMany >= 0;
        }

        public static IEnumerable<Town> GetPermanentFiefs(this Clan clan) => clan.Fiefs.Where(fief => !fief.IsOwnerUnassigned);*/
}