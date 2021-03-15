using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
[Serializable]
public class Animations : ScriptableObject
{
    public List<AnimationData> idle;
    public List<AnimationData> idleCombat;
    public List<AnimationData> attack;
    public List<AnimationData> attackStep;
    public List<AnimationData> attackLongStep;
    public List<AnimationData> kick;
    public List<AnimationData> rage;
    public List<AnimationData> charge;
    public List<AnimationData> forwardWalk;
    public List<AnimationData> forwardRun;
    public List<AnimationData> backwardWalk;
    public List<AnimationData> backwardRun;
    public List<AnimationData> blockLeftUp;
    public List<AnimationData> blockLeftDown;
    public List<AnimationData> blockRightUp;
    public List<AnimationData> blockRightDown;
    public List<AnimationData> blockShieldUp;
    public List<AnimationData> blockShieldDown;
    public List<AnimationData> counterLeft;
    public List<AnimationData> counterRight;
    public List<AnimationData> counterShield;
    public List<AnimationData> knockdown;
    public List<AnimationData> death;
    public List<AnimationData> hit;
    
    
    public List<AnimationData> GetCounterAnimation(AnimSide side, bool shield, bool counter)
    {
        switch (side) {
            case AnimSide.RightUp:
                return counter ? shield ? counterShield : counterRight : shield && Random.Range(0, 2) == 0 ?  blockShieldUp : blockRightUp;
            case AnimSide.RightDown:
                return counter ? counterRight : shield && Random.Range(0, 2) == 0 ? blockShieldDown : blockRightDown;
            case AnimSide.LeftUp:
                return counter ? counterLeft : blockLeftUp;
            case AnimSide.LeftDown:
                return counter ? counterLeft : blockLeftDown;
            default:
                return null;
        }
    }

    public List<AnimationData> GetAttackAnimation(Weapon weapon, float distance)
    {
        if (distance > weapon.distant) {
            return attackLongStep;
        }

        if (Random.Range(0, 2) == 0 && distance < weapon.kick) {
            return kick;
        }
        
        if (distance < weapon.normal) {
            return attack;
        }
        
        return attackStep;
    }
}

[Serializable]
public class AnimationData
{
    public AnimationClip clip;
    public int frame1;
    public int frame2;
    public AnimSide side1;
    public AnimSide side2;
    public Sounds sound1;
    public Sounds sound2;
    //public float volume = 1f;

    public float Length => clip.length;
    public float FrameRate => clip.frameRate;
}

[Serializable]
public enum AnimSide
{
    None,
    RightUp,
    RightDown,
    LeftUp,
    LeftDown,
}