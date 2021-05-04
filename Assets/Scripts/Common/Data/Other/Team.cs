using System;
using UnityEngine;

[Serializable]
public enum Team
{
    Self,
    Allied,
    Enemy
}

public static class TeamUtils 
{
    public static Color GetColor(this Team team)
    {
        switch (team) {
            case Team.Self:
                return Color.green;
            case Team.Allied:
                return Color.cyan;
            case Team.Enemy:
                return Color.red;
            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, "Team not exist!");
        }
    }
}