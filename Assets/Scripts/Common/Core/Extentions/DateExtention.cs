using System;
using System.ComponentModel;

public static class CampaignTime
{
    public static Season GetSeason(this DateTime date) 
    {
        var value = date.Month + date.Day / 100f;  // <month>.<day(2 digit)>    
        if (value < 3.21f || value >= 12.22f) return Season.Winter;   // Winter
        if (value < 6.21f) return Season.Spring; // Spring
        if (value < 9.23f) return Season.Summer; // Summer
        return Season.Autumn;   // Autumn
    }
    
    public static string GetDayWithSuffix(this DateTime date)
    {
        var value = date.Day;
        return value + ((value % 10 == 1 && value % 100 != 11) ? "st" : (value % 10 == 2 && value % 100 != 12) ? "nd" : (value % 10 == 3 && value % 100 != 13) ? "rd" : "th");
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

    public static float GetHourAndMinutes(this DateTime date)
    {
        return date.Hour + date.Minute / 60f;
    }
    
    public static float GetDayPercent(this DateTime date)
    {
        return date.GetHourAndMinutes() / 24f;
    }
    
    public static TimeOfDay GetTimeOfDay(this DateTime date)
    {
        var value = date.Hour + (date.Minute / 60f);
        if (value >= 0.5f && value < 5f) {
            return TimeOfDay.Night;
        } else if (value >= 5f && value < 7f) {
            return TimeOfDay.EarlyMorning;
        } else if (value >= 7f && value < 11.5f) {
            return TimeOfDay.Morning;
        } else if (value >= 11.5f && value < 12.5f) {
            return TimeOfDay.Noon;
        } else if (value >= 12.5f && value < 17f) {
            return TimeOfDay.Afternoon;
        } else if (value >= 17f && value < 19.5f) {
            return TimeOfDay.Evening;
        } else if (value >= 19.5f && value < 22f) {
            return TimeOfDay.LateEvening;
        } else if (value >= 22f && value < 23.5f) {
            return TimeOfDay.Twilight;
        }
        return TimeOfDay.Midnight;
    }
    
    [Serializable]
    public enum TimeOfDay
    {
        Midnight,
        Twilight,
        [Description("Late Evening")]
        LateEvening,
        Evening,
        Afternoon,
        Noon,
        Morning,
        [Description("Early Morning")]
        EarlyMorning,
        Night
    }
    
    [Serializable]
    public enum Season
    {
        Spring, 
        Summer, 
        Autumn, 
        Winter
    }

    [Serializable]
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
}