using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Names/Town", order = 0)]
[Serializable]
public class TownNames : ScriptableObject
{
    [SerializeField] private string[] prefix;
    [SerializeField] private string[] anyfix;

    public string RandomPrefix => prefix[Random.Range(0, prefix.Length)];
    public string RandomAnyfix => anyfix[Random.Range(0, anyfix.Length)];
}
