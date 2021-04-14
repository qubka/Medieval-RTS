using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
[InitializeOnLoad]
public class MoraleAttribute : ScriptableObject
{
    public int bonus;
    
    [HideInInspector] public int id;
    
    private void OnEnable()
    {
        id = name.GetHashCode(); 
    }
}
