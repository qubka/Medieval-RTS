using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
[InitializeOnLoad]
public class Sounds : ScriptableObject
{
    public List<AudioClip> sounds;
    
    [HideInInspector] public int id;
    
    private void OnEnable()
    {
        id = name.GetHashCode(); 
    }
}
