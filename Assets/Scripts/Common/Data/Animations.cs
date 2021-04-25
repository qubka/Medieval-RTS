using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Medieval/Animations Config", order = 0)]
[Serializable]
public class Animations : ScriptableObject
{
    public AnimationData[] idleNormal;
    public AnimationData[] idleCombat;
    public AnimationData[] idleRange;
    public AnimationData[] idleInjured;
    public AnimationData[] attackNormal;
    public AnimationData[] attackStep;
    public AnimationData[] attackCharge;
    public AnimationData[] rangeStart;
    public AnimationData[] rangeHold;
    public AnimationData[] rangeRelease;
    public AnimationData[] rangeEnd;
    public AnimationData[] reload;
    public AnimationData[] equip;
    public AnimationData[] kick;
    public AnimationData[] rage;
    public AnimationData[] turn;
    public AnimationData[] charge;
    public AnimationData[] forwardWalk;
    public AnimationData[] forwardRun;
    public AnimationData[] backwardWalk;
    public AnimationData[] backwardRun;
    public AnimationData[] blockLeftUp;
    public AnimationData[] blockLeftDown;
    public AnimationData[] blockRightUp;
    public AnimationData[] blockRightDown;
    public AnimationData[] blockShieldUp;
    public AnimationData[] blockShieldDown;
    public AnimationData[] counterLeft;
    public AnimationData[] counterRight;
    public AnimationData[] counterShield;
    public AnimationData[] knockdownNormal;
    public AnimationData[] knockdownCombat;
    public AnimationData[] knockdownRange;
    public AnimationData[] deathNormal;
    public AnimationData[] deathCombat;
    public AnimationData[] deathRange;
    public AnimationData[] hitNormal;
    public AnimationData[] hitCombat;
    public AnimationData[] hitRange;
    public AnimationData[] hitBack;
    public AnimationData[] injuredWalk;
    public AnimationData[] injuredRun;
    public bool hasMultiNormalKnockdown;
    public bool hasMultiCombatKnockdown;
    public bool hasMultiRangeKnockdown;
    public bool hasMount;
    public bool hasDistant;
    public bool hasTurn;
    public bool canKnock;
    public bool canBlock;
    public bool canCounter;
    public bool canKnockdown;
    public bool hasShield;
    [Space]
    public Vector2 knockForce;
    public Vector3 deathPosition;
    public Vector3 deathRotation;

    public AnimationData[] GetCounterAnimation(AnimSide side, bool hasShield, bool isCounter)
    {
        switch (side) {
            case AnimSide.RightUp:
                return isCounter ? hasShield ? counterShield : counterRight : hasShield && Random.Range(0, 2) == 0 ?  blockShieldUp : blockRightUp;
            case AnimSide.RightDown:
                return isCounter ? counterRight : hasShield && Random.Range(0, 2) == 0 ? blockShieldDown : blockRightDown;
            case AnimSide.LeftUp:
                return isCounter ? counterLeft : blockLeftUp;
            case AnimSide.LeftDown:
                return isCounter ? counterLeft : blockLeftDown;
            default:
                return null;
        }
    }

    public AnimationData[] GetAttackAnimation(MeleeWeapon weapon, float distance)
    {
        if (hasDistant && distance > weapon.distant) {
            return attackCharge;
        }

        if (kick.Length > 0 && Random.Range(0, 10) == 0 && distance < weapon.kick) {
            return kick;
        }
        
        return distance > weapon.normal && attackStep.Length > 0 ? attackStep : attackNormal;
    }

    public AnimationData[] GetIdleAnimation(bool isCombat, bool isRange, bool isInjure)
    {
        if (isCombat) {
            if (isRange && idleRange.Length > 0) {
                return idleRange;
            }
            
            if (idleCombat.Length > 0) {
                return idleCombat;
            }
        } else {
            if (isInjure && idleInjured.Length > 0) {
                return idleInjured;
            }
        }

        return idleNormal;
    }

    public AnimationData GetKnockdownAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && knockdownRange.Length > 0) {
                return hasMultiRangeKnockdown ? knockdownRange.GetRandom(1) : knockdownRange.GetRandom();
            }
            
            if (knockdownCombat.Length > 0) {
                return hasMultiCombatKnockdown ? knockdownCombat.GetRandom(1) : knockdownCombat.GetRandom();
            }
        }
        
        return hasMultiNormalKnockdown ? knockdownNormal.GetRandom(1) : knockdownNormal.GetRandom();
    }

    public AnimationData GetHitAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && hitRange.Length > 0) {
                return hitRange.GetRandom();
            }
            
            if (hitCombat.Length > 0) {
                return hitCombat.GetRandom();
            }
        }

        return hitNormal.GetRandom();
    }
    
    public AnimationData GetDeathAnimation(bool isCombat, bool isRange)
    {
        if (isCombat) {
            if (isRange && deathRange.Length > 0) {
                return deathRange.GetRandom();
            }
            
            if (deathCombat.Length > 0) {
                return deathCombat.GetRandom();
            }
        }

        return deathNormal.GetRandom();
    }
    
    public AnimationData GetMoveAnimation(bool isForward, bool isRunning, bool isCombat, bool isRange, bool isInjure)
    {
        if (isCombat) {
            var list = (isForward ? isRunning ? forwardRun : forwardWalk : isRunning ? backwardRun : backwardWalk);
            
            if (isRange && list.Length > 2) {
                return list[2];
            }
        
            if (list.Length > 1) {
                return list[1];
            }
            
            return list[0];
        }

        return (isForward ? isRunning ? isInjure && injuredRun.Length > 0 ? injuredRun : forwardRun : isInjure && injuredWalk.Length > 0 ? injuredWalk : forwardWalk : isRunning ? backwardRun : backwardWalk)[0];
    }

    public AnimationData GetTurnAnimation(Quaternion current, Quaternion desired)
    {
        return turn[Vector.IsRightRotationDirection(current, desired) ? 1 : 0];
    }
    
    public int GetRangeAnimation(RangeWeapon weapon, float distance)
    {
        if (distance > weapon.distant) {
            return 2;
        }

        if (distance < weapon.close) {
            return 0;
        }
        
        return 1;
    }

    public bool IsIdle(AnimationClip current, bool isCombat, bool isRange, bool isInjure)
    {
        if (isCombat) {
            if (isRange && deathRange.Length > 0) {
                return idleRange[0].clip == current;
            }
            
            if (deathCombat.Length > 0) {
                return idleCombat[0].clip == current;
            }
        } else {
            if (isInjure && idleInjured.Length > 0) {
                return idleInjured[0].clip == current;
            }
        }
        
        return idleNormal[0].clip == current;
    }

    public bool IsTurn(AnimationClip current)
    {
        return turn.Length > 0 && (turn[0].clip == current || turn[1].clip == current);
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
    [SerializeField] private AnimationClip[] childList;
    
    public float Length => clip.length;
    public float FrameRate => clip ? clip.frameRate : 30f;
    public bool HasChild => childList.Length > 0;
    public AnimationClip Child => childList.GetRandom();
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