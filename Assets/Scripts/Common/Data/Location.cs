using System;
using UnityEngine;
using UnityJSON;

[CreateAssetMenu(menuName = "Medieval/Location Config", order = 0)]
[Serializable]
public class Location : SerializableObject
{
    [Header("General")]
    public int id;
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
    
    [JSONNode] private int factionId;
    [JSONNode] private int rulerId;

    public override void OnSerialization()
    {
        factionId = faction ? faction.id : -1;
        rulerId = ruler ? ruler.id : -1;
    }

    public override void OnDeserialization()
    {
        var game = SaveLoadManager.Instance.current;
        if (factionId != -1) {
            faction = game.factions.Find(f => f.id == factionId);
        }
        if (rulerId != -1) {
            ruler = game.characters.Find(c => c.id == rulerId);
        }
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