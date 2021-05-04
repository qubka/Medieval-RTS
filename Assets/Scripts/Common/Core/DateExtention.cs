using System;

public static class DateExtention
{
    public static Season GetSeason(this DateTime date) 
    {
        var value = date.Month + date.Day / 100f;  // <month>.<day(2 digit)>    
        if (value < 3.21 || value >= 12.22) return Season.Winter;   // Winter
        if (value < 6.21) return Season.Spring; // Spring
        if (value < 9.23) return Season.Summer; // Summer
        return Season.Autumn;   // Autumn
    }
    
    /*public static int GetSeason(this DateTime date, bool ofSouthernHemisphere) 
    {
        var hemisphereConst = (ofSouthernHemisphere ? 2 : 0);

        int GetReturn(int northern)
        {
            return (northern + hemisphereConst) % 4;
        }

        var value = date.Month + date.Day / 100f;  // <month>.<day(2 digit)>
        if (value < 3.21 || value >= 12.22) return GetReturn(3);    // 3: Winter
        if (value < 6.21) return GetReturn(0);  // 0: Spring
        if (value < 9.23) return GetReturn(1);  // 1: Summer
        return GetReturn(2);     // 2: Autumn
    }*/
}

public enum Season
{
    Spring, 
    Summer, 
    Autumn, 
    Winter
}