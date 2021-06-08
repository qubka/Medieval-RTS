using System;
using UnityEngine;

public class Stats 
{
    public event EventHandler OnStatsChanged;
    private static int STAT_MIN = 0;
    private static int STAT_MAX = 200;

    public enum Type
    {
        Attack,
        Defence,
        Speed,
        Morale,
        Health,
    }

    private SingleStat attackStat;
    private SingleStat defenceStat;
    private SingleStat speedStat;
    private SingleStat moraleStat;
    private SingleStat healthStat;

    public Stats(int attack, int defence, int speed, int morale, int health)
    {
        attackStat = new SingleStat(attack);
        defenceStat = new SingleStat(defence);
        speedStat = new SingleStat(speed);
        moraleStat = new SingleStat(morale);
        healthStat = new SingleStat(health);
    }
    
    private SingleStat GetSingleStat(Type statType)
    {
        switch (statType) {
            default:
            case Type.Attack: return attackStat;
            case Type.Defence: return defenceStat;
            case Type.Speed: return speedStat;
            case Type.Morale: return moraleStat;
            case Type.Health: return healthStat;
        }
    }

    public void SetStatAmount(Type statType, int statAmount)
    {
        GetSingleStat(statType).SetStatAmount(statAmount);
        OnStatsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void IncreaseStatAmount(Type statType)
    {
        SetStatAmount(statType, GetStatAmount(statType) + 1);
    }

    public void DecreaseStatAmount(Type statType)
    {
        SetStatAmount(statType, GetStatAmount(statType) - 1);
    }

    public int GetStatAmount(Type statType)
    {
        return GetSingleStat(statType).GetStatAmount();
    }

    public float GetStatAmountNormalized(Type statType)
    {
        return GetSingleStat(statType).GetStatAmountNormalized();
    }

    /*
     * Represents a Single Stat of any Type
     * */
    private class SingleStat
    {
        private int stat;

        public SingleStat(int statAmount)
        {
            SetStatAmount(statAmount);
        }

        public void SetStatAmount(int statAmount)
        {
            stat = Mathf.Clamp(statAmount, STAT_MIN, STAT_MAX);
        }

        public int GetStatAmount()
        {
            return stat;
        }

        public float GetStatAmountNormalized()
        {
            return (float) stat / STAT_MAX;
        }
    }
}
