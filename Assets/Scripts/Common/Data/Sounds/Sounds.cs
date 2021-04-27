using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/List", order = 0)]
[Serializable]
[InitializeOnLoad]
public class Sounds : ScriptableObject
{
    [SerializeField] private AudioClip[] sounds;

    public AudioClip Clip => sounds.GetRandom();
}
