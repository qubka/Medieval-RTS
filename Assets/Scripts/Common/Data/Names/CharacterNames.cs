using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Names/Character", order = 0)]
[Serializable]
public class CharacterNames : ScriptableObject
{
    public string[] titles;
    public string[] names;
    public Gender gender;
    
    //public string this[int index] => names[index];
    //public int Count => names.Count;
    public string RandomName => names[Random.Range(0, names.Length)];
}