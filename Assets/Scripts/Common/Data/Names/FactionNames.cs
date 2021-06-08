using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Names/Faction", order = 0)]
[Serializable]
public class FactionNames : ScriptableObject
{
    public string[] names;

    public string RandomName => names[Random.Range(0, names.Length)];
}