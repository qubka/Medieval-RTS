using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Names/Character", order = 0)]
[Serializable]
public class CharacterNames : ScriptableObject
{
    public List<string> titles;
    public List<string> names;
    public Gender gender;
    
    //public string this[int index] => names[index];
    //public int Count => names.Count;
    public string RandomName => names[Random.Range(0, names.Count)];
}