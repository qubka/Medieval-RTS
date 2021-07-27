using System;
using UnityEngine;

[Serializable]
public class Model
{
    [Header("Models")]
    public GameObject primary;
    public GameObject secondary;
    [Header("Sounds")]
    public Sounds footstepSounds;
    public float interval;
}