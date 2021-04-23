using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/Group", order = 0)]
[Serializable]
public class Group : ScriptableObject
{
    public List<AudioClip> runSounds;
    public List<AudioClip> walkSounds;
    public List<AudioClip> chargeSounds;
    public List<AudioClip> closeFightSounds;
    public List<AudioClip> distantFightSounds;
    public List<AudioClip> selectSounds;
    public List<AudioClip> celebrationSounds;
    public List<AudioClip> battleCrySounds;
    public List<AudioClip> goSounds;
    public List<AudioClip> stopSounds;
}