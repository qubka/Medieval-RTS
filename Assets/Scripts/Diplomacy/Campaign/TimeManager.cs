using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityJSON;

public class TimeManager : SingletonObject<TimeManager> {

    [SerializeField] private TextMeshProUGUI timeMesh;
    [SerializeField] private float timeScale = 1f;
    [SerializeField] private Vector3Int startingDate = new Vector3Int(1080, 1, 1);
    [SerializeField] private float startingHour = 12f;
    
    private DateTime dateTime;
    private float daysFromStart;
    private string timeStamp;
    private float timeDelta;

    public DateTime Now => dateTime;
    public float TimeDelta => timeDelta;
    
    private const float hoursInDay = 24f;

    private void Start()
    {
        dateTime = new DateTime(startingDate.x, startingDate.y, startingDate.z);
        timeDelta = startingHour;
        timeStamp = ToString();
        timeMesh.text = timeStamp + StringExtention.GetPrettyName(GetTimeOfDay());
    }

    private void Update()
    {
        timeDelta += Time.deltaTime * timeScale;

        LightManager.Instance.UpdateLighting(timeDelta / hoursInDay);

        if (timeDelta >= hoursInDay) {
            timeDelta = 0f;
            dateTime = dateTime.AddDays(1);
            daysFromStart++;
            timeStamp = ToString();
                
            var events = EventManager.Instance;
            events.OnDailyTickEvent();
            if (dateTime.DayOfWeek == DayOfWeek.Monday) {
                events.OnWeeklyTickEvent();
            }
        }

        timeMesh.text = timeStamp + Environment.NewLine + StringExtention.GetPrettyName(GetTimeOfDay());
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