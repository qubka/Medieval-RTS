using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Location Config", order = 0)]
[Serializable]
public class Location : ScriptableObject
{
    public string title;
    public bool isCapital;
    public Faction faction;
    public Vector3 position;
    public Vector3 rotation;
    public Character ruler;
    public LocationType type;
    public List<Troop> garrison;
    public List<Army> armies;
}

[Serializable]
public enum LocationType
{
    Town,
    Castle,
    Village
}