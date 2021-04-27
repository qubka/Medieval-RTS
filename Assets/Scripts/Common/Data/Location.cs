using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Location Config", order = 0)]
[Serializable]
public class Location : SerializableObject
{
    public string label;
    public bool isCapital;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Faction faction;
    public Vector3 position;
    public Quaternion rotation;
    [JSONNode(NodeOptions.DontSerialize)] 
    public Character ruler;
    public Infrastructure type;
    //public List<Troop> garrison;
    //public List<Party> armies;
    
    [JSONNode] 
    private string factionName;
    [JSONNode] 
    private string rulerName;

    public void OnEnable()
    {
        hash = label.GetHashCode();
    }
    
    public override void OnSerialization()
    {
        factionName = faction.label;
        rulerName = ruler.surname;
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        faction = game.factions.Find(f => f.label.Equals(factionName));
        ruler = game.characters.Find(c => c.surname.Equals(rulerName));
    }
}

[Serializable]
[JSONEnum(format = JSONEnumMemberFormating.Lowercased)]
public enum Infrastructure
{
    Town,
    Castle,
    Village
}