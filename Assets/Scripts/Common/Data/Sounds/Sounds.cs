using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/List", order = 0)]
[Serializable]
public class Sounds : ScriptableObject
{
    [SerializeField] private AudioClip[] sounds;

    public AudioClip Clip => sounds.GetRandom();
}
