using System.Linq;

public static class FactionManager
{
    public static bool IsAlliedWithFaction(Faction faction1, Faction faction2)
    {
        return faction1.allies.Contains(faction2);
    }

    public static bool IsAtWarAgainstFaction(Faction faction1, Faction faction2)
    {
        return faction1.enemies.Contains(faction2);
    }

    public static void SetNeutral(Faction faction1, Faction faction2)
    {
        faction1.enemies.Remove(faction2);
        faction1.allies.Remove(faction2);
        
        faction2.enemies.Remove(faction1);
        faction2.allies.Remove(faction1);
    }
    
    public static void DeclareAlliance(Faction faction1, Faction faction2)
    {
        faction1.allies.Add(faction2);
        faction2.allies.Add(faction1);
    }
    
    public static void DeclareWar(Faction faction1, Faction faction2)
    {
        faction1.enemies.Add(faction2);
        faction2.enemies.Add(faction1);
    }
}