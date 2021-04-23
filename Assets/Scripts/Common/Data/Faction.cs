using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Faction Config", order = 0)]
[Serializable]
public class Faction : ScriptableObject
{
    public string label;
    public Color color;
    public Character leader;
    //public List<string> relationships;
    //public List<string> locationsOwned;
    //public List<Squadron> factionTroops;

    //[ReadOnly] public List<Character> charaters;
}