using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Morale Attribute Config", order = 0)]
[Serializable]
[InitializeOnLoad]
public class MoraleAttribute : ScriptableObject
{
    public int bonus;
    
    [NonSerialized] public int id;
    
    private void OnEnable()
    {
        id = name.GetHashCode(); 
    }

    public override int GetHashCode()
    {
        return id;
    }
}
