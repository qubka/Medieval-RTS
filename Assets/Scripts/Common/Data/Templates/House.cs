using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Templates/House", order = 0)]
[Serializable]
public class House : ScriptableObject
{
    public int id;
    public string label;
    public Character leader;
    public int tier;
    public int influence;
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
    
    public static House Create(HouseSave save)
    {
        var obj = CreateInstance<House>();
        obj.id = save.id;
        return obj;
    }
    
    public void Load(HouseSave save = null)
    {
        if (save != null) {
            label = save.label;
            if (save.leader != -1) leader = Character.All.First(c => c.id == save.leader);
            tier = save.tier;
            influence = save.influence;
            banner = save.banner;
        } else {
            if (leader) leader = Character.All.First(c => c.id == leader.id);
        }
    }

    public House Clone()
    {
        var obj = Instantiate(this);
        obj.name = obj.name.Replace("(Clone)", "");
        return obj;
    }
}

[Serializable]
public class HouseSave
{
    [HideInInspector] public int id;
    [HideInInspector] public string label;
    [HideInInspector] public int leader;
    [HideInInspector] public int tier;
    [HideInInspector] public int influence;
    [HideInInspector] public Banner banner;

    public HouseSave(House house)
    {
        id = house.id;
        label = house.label;
        leader = house.leader ? house.leader.id : -1;
        tier = house.tier;
        influence = house.influence;
        banner = house.banner;  
    }
}