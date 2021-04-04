using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
    public List<AnimationData> rangeEnd;
    public List<AnimationData> reload;
    public List<AnimationData> equip;
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
    public List<AnimationData> knockdownNormal;
    public List<AnimationData> knockdownCombat;
    public List<AnimationData> knockdownRange;
    public List<AnimationData> deathNormal;
    public List<AnimationData> deathCombat;
    public List<AnimationData> deathRange;
    public List<AnimationData> hitNormal;
    public List<AnimationData> hitCombat;
    public List<AnimationData> hitRange;
    public List<AnimationData> hitBack;
    public bool hasMultiNormalKnockdown;
    public bool hasMultiCombatKnockdown;
    public bool hasMultiRangeKnockdown;
    public bool hasAttachment;
    public bool hasDistant;
    [Space]
    public Vector3 deathPosition;
    public Vector3 deathRotation;

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
        if (hasDistant && distance > weapon.distant) {
            return attackCharge;
        }

        if (kick.Count > 0 && Random.Range(0, 2) == 0 && distance < weapon.kick) {
            return kick;
        }
        
        return distance > weapon.normal && attackStep.Count > 0 ? attackStep : attackNormal;
    }

    public List<AnimationData> GetIdleAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && idleRange.Count > 0) {
                return idleRange;
            }
            
            if (idleCombat.Count > 0) {
                return idleCombat;
            }
        }

        return idleNormal;
    }

    public AnimationData GetKnockdownAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && knockdownRange.Count > 0) {
                return hasMultiRangeKnockdown? knockdownRange.GetRandom(1) : knockdownRange.GetRandom();
            }
            
            if (knockdownCombat.Count > 0) {
                return hasMultiCombatKnockdown ? knockdownCombat.GetRandom(1) : knockdownCombat.GetRandom();
            }
        }
        
        return hasMultiNormalKnockdown ? knockdownNormal.GetRandom(1) : knockdownNormal.GetRandom();
    }

    public AnimationData GetHitAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && hitRange.Count > 0) {
                return hitRange.GetRandom();
            }
            
            if (hitCombat.Count > 0) {
                return hitCombat.GetRandom();
            }
        }

        return hitNormal.GetRandom();
    }
    
    public AnimationData GetDeathAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && deathRange.Count > 0) {
                return deathRange.GetRandom();
            }
            
            if (deathCombat.Count > 0) {
                return deathCombat.GetRandom();
            }
        }

        return deathNormal.GetRandom();
    }
    
    public AnimationData GetMoveAnimation(bool isForward, bool isRunning, bool isRun, bool isCombat, bool isRange)
    {
        var list = (isForward ? (isRun || isRunning) ? forwardRun : forwardWalk : (isRun || isRunning) ? backwardRun : backwardWalk);

        if (isCombat) {
            if (isRange && list.Count > 2) {
                return list[2];
            }
        
            if (list.Count > 1) {
                return list[1];
            }
        }

        return list[0];
    }

    public int GetRangeAnimation(Weapon weapon, float distance)
    {
        if (distance > weapon.distant) {
            return 2;
        }

        if (distance < weapon.normal) {
            return 0;
        }
        
        return 1;
    }

    public bool IsIdle(AnimationClip current, bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && deathRange.Count > 0) {
                return idleRange[0].clip == current;
            }
            
            if (deathCombat.Count > 0) {
                return idleCombat[0].clip == current;
            }
        }

        return idleNormal[0].clip == current;
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
    public List<AnimationClip> childList;
    public bool playOnChild;

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