
public static class EconomyManager
{
    public static void BeginNewDay()
    {
        foreach (var town in TownTable.Instance) {
            town.BeginNewDay();
        }
    }
    
    public static void BeginNewQuarter()
    {
        foreach (var town in TownTable.Instance) {
            town.BeginNewQuarter();
        }
    }
    
    /*public void BeginNewSeason()
    {
    }*/
}
