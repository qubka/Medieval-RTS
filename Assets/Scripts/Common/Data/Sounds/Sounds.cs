using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/List", order = 0)]
[Serializable]
[InitializeOnLoad]
public class Sounds : ScriptableObject
{
    [SerializeField] private List<AudioClip> sounds;

    public AudioClip Clip => sounds.GetRandom();
    
    [HideInInspector] public int id;
    
    private void OnEnable()
    {
        id = name.GetHashCode(); 
    }
}
