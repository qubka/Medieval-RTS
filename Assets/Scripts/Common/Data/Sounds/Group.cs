using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/Group", order = 0)]
[Serializable]
public class Group : ScriptableObject
{
    public AudioClip[] runSounds;
    public AudioClip[] walkSounds;
    public AudioClip[] chargeSounds;
    public AudioClip[] closeFightSounds;
    public AudioClip[] distantFightSounds;
    public AudioClip[] selectSounds;
    public AudioClip[] celebrationSounds;
    public AudioClip[] battleCrySounds;
    public AudioClip[] goSounds;
    public AudioClip[] stopSounds;
}