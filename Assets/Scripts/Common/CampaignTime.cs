using System;
using UnityEngine;
using UnityEngine.UI;
using UnityJSON;

public class CampaignTime : SingletonObject<CampaignTime> {

    [SerializeField] private Text timeLabel;
    [SerializeField] private TimeBar timeBar;
    [SerializeField, Range(0.1f, 24f)] private float hoursInDay = 24f;
    [SerializeField] private int startingYear = 1120;
    [SerializeField] private int startingMonth = 1;
    [SerializeField] private int startingDay = 1;

    private LightManager lightManager;
    private DateTime dateTime;
    private float daysFromStart;
    private float timeDelta;
    
    public DateTime Now => dateTime;

    private void Start()
    {
        lightManager = LightManager.Instance;
        dateTime = new DateTime(startingYear, startingMonth, startingDay);
        timeLabel.text = ToString();
    }

    private void Update() 
    {
        timeDelta += Time.deltaTime;

        lightManager.UpdateLighting(timeDelta / hoursInDay);
        
        if (timeDelta >= hoursInDay) {
            timeDelta = 0f;
            dateTime = dateTime.AddDays(1);
            if (dateTime.DayOfWeek == DayOfWeek.Monday) {
                EconomyManager.BeginNewWeek();
            }
            EconomyManager.BeginNewDay();
            daysFromStart++;
            timeLabel.text = ToString();
        }
    }

    public override string ToString()
    {
        return dateTime.GetDayWithSuffix() + " of " + (Month) dateTime.Month + ", " + dateTime.Year + " AD";
    }
}