using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Medieval/Sounds/Commander", order = 0)]
[Serializable]
public class Commander : ScriptableObject
{
    public AudioClip[] forKing;
    public AudioClip[] forGlory;
    public AudioClip[] forRealm;
    public AudioClip[] forVictory;
    public AudioClip[] fromLeftFlank;
    public AudioClip[] fromRightFlank;
    public AudioClip[] noneShallStopUs;
    public AudioClip[] halt;
    public AudioClip[] hold;
    public AudioClip[] steady;
    public AudioClip[] charge;
    public AudioClip[] standInLine;
    public AudioClip[] standBack;
    public AudioClip[] forward;
    public AudioClip[] regroup;
    public AudioClip[] fire;
    public AudioClip[] move;
    public AudioClip[] lego;
    public AudioClip[] underfire;
    public AudioClip[] dismiss;
    public AudioClip[] prepare;
    public AudioClip[] retreat;
    public AudioClip[] saveYourLives;
    public AudioClip[] longLiveTheKing;
    public AudioClip[] enemyApproaching;
    public AudioClip[] enemyIncoming;
    public AudioClip[] takeNoPrisoners;
    public AudioClip[] formTheOrder;
    public AudioClip[] takeYourPosition;
    public AudioClip[] toTheRightFlank;
    public AudioClip[] toTheLeftFlank;
    public AudioClip[] theyComeFromBehind;
    public AudioClip[] comeBackCowards;
    public AudioClip[] inTheNameOfLord;
    public AudioClip[] fightUntilYouDie;
    public AudioClip[] killThemAll;
    public AudioClip[] braceYourselves;
    public AudioClip[] toArms;
    public AudioClip[] victoryIsOurs;
}