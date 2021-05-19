using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityJSON;

public class CampaignTime : SingletonObject<CampaignTime> {

    [SerializeField] private TextMeshProUGUI timeMesh;
    [SerializeField] private TimeBar timeBar;
    [SerializeField] private float timeScale = 1f;
    [SerializeField, Range(0.1f, 24f)] private float hoursInDay = 24f;
    [SerializeField] private int startingYear = 1120;
    [SerializeField] private int startingMonth = 1;
    [SerializeField] private int startingDay = 1;
    [SerializeField] private float startingHour = 12f;

    private LightManager lightManager;
    private DateTime dateTime;
    private float daysFromStart;
    private string timeStamp;
    private float timeDelta;
    
    public DateTime Now => dateTime;
    public float TimeDelta => timeDelta;

    private void Start()
    {
        lightManager = LightManager.Instance;
        dateTime = new DateTime(startingYear, startingMonth, startingDay);
        timeDelta = startingHour;
        timeStamp = ToString();
        timeMesh.text = timeStamp + GetTimeOfDay();
    }

    private void Update()
    {
        timeDelta += Time.deltaTime * timeScale;

        lightManager.UpdateLighting(timeDelta / hoursInDay);

        if (timeDelta >= hoursInDay) {
            timeDelta = 0f;
            dateTime = dateTime.AddDays(1);
            if (dateTime.DayOfWeek == DayOfWeek.Monday) {
                EconomyManager.BeginNewWeek();
            }
            EconomyManager.BeginNewDay();
            daysFromStart++;
            timeStamp = ToString();
        }

        timeMesh.text = timeStamp + Environment.NewLine + GetTimeOfDay();
    }

    public override string ToString()
    {
        return dateTime.ToString("d MMMM, yyyy", CultureInfo.InvariantCulture);
    }

    public TimeOfDay GetTimeOfDay() {
        if (timeDelta >= 0.5f && timeDelta < 5f) {
            return TimeOfDay.Night;
        } else if (timeDelta >= 5f && timeDelta < 7f) {
            return TimeOfDay.EarlyMorning;
        } else if (timeDelta >= 7f && timeDelta < 11.5f) {
            return TimeOfDay.Morning;
        } else if (timeDelta >= 11.5f && timeDelta < 12.5f) {
            return TimeOfDay.Noon;
        } else if (timeDelta >= 12.5f && timeDelta < 17f) {
            return TimeOfDay.Afternoon;
        } else if (timeDelta >= 17f && timeDelta < 19.5f) {
            return TimeOfDay.Evening;
        } else if (timeDelta >= 19.5f && timeDelta < 22f) {
            return TimeOfDay.LateEvening;
        } else if (timeDelta >= 22f && timeDelta < 23.5f) {
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
}