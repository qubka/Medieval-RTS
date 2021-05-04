using System;
using System.Globalization;
using Unity.Core;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : SingletonObject<TimeManager> {

    [SerializeField] private Text timeLabel;
    [SerializeField] private Slider clockSpin;
    [SerializeField] private Image clockColor;
    [SerializeField] private TimeBar timeBar;
    
    public float TimeDelta { get; set; }
    public int DaysFromStart { get; set; }
    private DateTime Date { get; set; }
    
    public int startingYear = 1120;
    public int startingMonth = 1;
    public int startingDay = 1;
    
    public void UpdateTimeLabel() 
    {
        timeLabel.text = ToString();
        //clockSpin.value = (TimeDelta / DayTime + (Days + Weeks * WeekTime + Months * MonthTime * WeekTime)) / DaysInASeason;
    }

    private void UpdateClockColor()
    {
        clockColor.color = Date.GetSeason() switch {
            Season.Winter => new Color(0, 0, 1),
            Season.Spring => new Color(0, 1, 0),
            Season.Summer => new Color(1, 1, 0),
            Season.Autumn => new Color(1, 0, 0),
            _ => new Color(1, 1, 1)
        };
    }

    public override string ToString() 
    {
        return Date.ToString("d MMMM, yyyy", CultureInfo.InvariantCulture);
    }

    /*public void Load(TimeSave w)
     {
        TimeDelta = w.timeDelta;
        Days = w.days;
        Weeks = w.weeks;
        Months = w.months;
        Years = w.years;
        finances.LoadFinancialReports();
    }*/

    private void Start()
    {
        Date = new DateTime(startingYear, startingMonth, startingDay);
        UpdateTimeLabel();
        UpdateClockColor();
    }

    private void Update() 
    {
        TimeDelta += Time.deltaTime;

        LightManager.Instance.UpdateLighting(TimeDelta / 24f);
        
        if (TimeDelta >= 24f) {
            //convert timedelta to day
            TimeDelta = 0f;
            Date = Date.AddDays(1);
            DaysFromStart++;
            
            UpdateTimeLabel();
            UpdateClockColor();
        }
    }
}