using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
[Serializable]
public class Animations : ScriptableObject
{
    public List<AnimationData> idleNormal;
    public List<AnimationData> idleCombat;
    public List<AnimationData> idleRange;
    public List<AnimationData> attackNormal;
    public List<AnimationData> attackStep;
    public List<AnimationData> attackCharge;
    public List<AnimationData> rangeStart;
    public List<AnimationData> rangeHold;
    public List<AnimationData> rangeRelease;
    public List<AnimationData> reload;
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
    public List<AnimationData> knockdownCombat;
    public List<AnimationData> knockdownRange;
    public List<AnimationData> deathNormal;
    public List<AnimationData> deathCombat;
    public List<AnimationData> deathRange;
    public List<AnimationData> hitNormal;
    public List<AnimationData> hitCombat;
    public List<AnimationData> hitRange;
    public List<AnimationData> stateChange;
    public bool hasMultiKnockback;
    

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
            return attackCharge;
        }

        if (Random.Range(0, 2) == 0 && distance < weapon.kick) {
            return kick;
        }
        
        if (distance < weapon.normal) {
            return attackNormal;
        }
        
        return attackStep;
    }

    public List<AnimationData> GetMoveAnimation(bool isForward, bool isRunning, bool run)
    {
        return (isForward ? (run || isRunning) ? forwardRun : forwardWalk : (run || isRunning) ? backwardRun : backwardWalk);
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