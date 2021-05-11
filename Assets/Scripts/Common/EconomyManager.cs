
public static class EconomyManager
{
    public static void BeginNewDay()
    {
        foreach (var town in TownTable.Instance) {
            town.BeginNewDay();
        }
    }
    
    public static void BeginNewWeek()
    {
        foreach (var town in TownTable.Instance) {
            town.BeginNewWeek();
        }
    }
    
    /*public void BeginNewSeason()
    {
    }*/
}
