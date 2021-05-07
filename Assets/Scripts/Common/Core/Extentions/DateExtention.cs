using System;

public static class DateExtention
{
    public static Season GetSeason(this DateTime date) 
    {
        var value = date.Month + date.Day / 100f;  // <month>.<day(2 digit)>    
        if (value < 3.21f || value >= 12.22f) return Season.Winter;   // Winter
        if (value < 6.21f) return Season.Spring; // Spring
        if (value < 9.23f) return Season.Summer; // Summer
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
        if (value < 3.21f || value >= 12.22f) return GetReturn(3);    // 3: Winter
        if (value < 6.21f) return GetReturn(0);  // 0: Spring
        if (value < 9.23f) return GetReturn(1);  // 1: Summer
        return GetReturn(2);     // 2: Autumn
    }*/

    public static string GetDayWithSuffix(this DateTime date)
    {
        var now = date.Day;
        return now + ((now % 10 == 1 && now % 100 != 11) ? "st" : (now % 10 == 2 && now % 100 != 12) ? "nd" : (now % 10 == 3 && now % 100 != 13) ? "rd" : "th");
    }
}

public enum Season
{
    Spring, 
    Summer, 
    Autumn, 
    Winter
}

public enum Month
{
    January = 1, 
    February,
    March, 
    April, 
    May, 
    June, 
    July, 
    August, 
    September, 
    October, 
    November,
    December
}